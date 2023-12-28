using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Lift.UI.Controls;
using Lift.UI.Tools;
using Lift.UI.Tools.Extension;

namespace Lift.UI.V2.Controls.PropertyGrid;

/// <summary>
/// 属性网格，用来做常规网格处理
/// </summary>
[TemplatePart(Name = ElementItems, Type = typeof(PropertyItemsControl))]
public class PropertyGrid : Control
{
    public const string ElementItems = "PART_Items";

    private PropertyItemsControl? _itemsControl;

    private ICollectionView? _dataView;

    #region propdp

    public static readonly DependencyProperty MinTitleWidthProperty = DependencyProperty.Register(
        nameof(MinTitleWidth), typeof(double), typeof(PropertyGrid), new PropertyMetadata(default(double)));

    /// <summary>
    /// 最小的Title宽度
    /// </summary>
    public double MinTitleWidth
    {
        get => (double) GetValue(MinTitleWidthProperty);
        set => SetValue(MinTitleWidthProperty, value);
    }

    public static readonly DependencyProperty MaxTitleWidthProperty = DependencyProperty.Register(
        nameof(MaxTitleWidth), typeof(double), typeof(PropertyGrid), new PropertyMetadata(default(double)));

    /// <summary>
    /// 最大的Title宽度
    /// </summary>
    public double MaxTitleWidth
    {
        get => (double) GetValue(MaxTitleWidthProperty);
        set => SetValue(MaxTitleWidthProperty, value);
    }

    public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(
        nameof(ItemMargin), typeof(Thickness), typeof(PropertyGrid), new PropertyMetadata(default(Thickness)));

    public Thickness ItemMargin
    {
        get => (Thickness) GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }
    #endregion

    #region SelectedObject

    public static readonly RoutedEvent SelectedObjectChangedEvent =
        EventManager.RegisterRoutedEvent("SelectedObjectChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<object>), typeof(PropertyGrid));

    public event RoutedPropertyChangedEventHandler<object> SelectedObjectChanged
    {
        add => AddHandler(SelectedObjectChangedEvent, value);
        remove => RemoveHandler(SelectedObjectChangedEvent, value);
    }

    public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
        nameof(SelectedObject), typeof(object), typeof(PropertyGrid), new PropertyMetadata(default(object), OnSelectedObjectChanged));

    private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (PropertyGrid) d;
        ctl.OnSelectedObjectChanged(e.OldValue, e.NewValue);
    }

    protected virtual void OnSelectedObjectChanged(object oldValue, object newValue)
    {
        UpdateItems(newValue);
        RaiseEvent(new RoutedPropertyChangedEventArgs<object>(oldValue, newValue, SelectedObjectChangedEvent));
    }

    public object SelectedObject
    {
        get => (object) GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    #endregion

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _itemsControl = GetTemplateChild(ElementItems) as PropertyItemsControl;
        UpdateItems(SelectedObject);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        // 全局右侧长度
        ResizeTitleWidth();
    }

    /// <summary>
    /// 更新当前的显示内容，后面删选要用
    /// </summary>
    /// <param name="obj"></param>
    void UpdateItems(object? obj)
    {
        if (obj == null || _itemsControl == null) return;

        _dataView = CollectionViewSource.GetDefaultView(obj.GetMembers()
            .Select(s => PropertyItem.FromMemberInfo(s, obj))
            .Where(item => !item.PropertyGridAttribute?.Ignore ?? true)
            .Do(item => item.Editor = GetEditor(item))
            .Do(item => item.InitElement()));

        var s = obj.GetMembers()
            .Select(s => PropertyItem.FromMemberInfo(s, obj))
            .Where(item => !item.PropertyGridAttribute?.Ignore ?? true);

        var sfae = s.ToArray()[0].BindingType.ToString().ToLower();

        _dataView.GroupDescriptions.Add(new PropertyGroupDescription("PropertyGridAttribute.GroupName"));

        _itemsControl.ItemsSource = _dataView;
    }

    #region Extension Function

    /// <summary>
    /// 重新计算长度和宽度
    /// </summary>
    void ResizeTitleWidth() => TitleElement.SetTitleWidth(this,
        new GridLength(Math.Max(MinTitleWidth, Math.Min(MaxTitleWidth, ActualWidth / 3))));


    /// <summary>
    /// 按照名字排序，清除分组
    /// </summary>
    /// <param name="reverse">是否反向排序</param>
    public void SortByName(bool reverse = false)
    {
        if (_dataView == null) return;
        using (_dataView.DeferRefresh())
        {
            _dataView.GroupDescriptions.Clear();
            _dataView.SortDescriptions.Clear();
            _dataView.SortDescriptions.Add(new SortDescription(PropertyItem.DisplayNameProperty.Name,
                reverse is false ? ListSortDirection.Ascending : ListSortDirection.Descending));
        }
    }

    /// <summary>
    /// 按照组来排序
    /// </summary>
    /// <param name="prop">分组依据，一般不改</param>
    public void SortByGroup(string prop = "PropertyGridAttribute.GroupName")
    {
        if (_dataView == null) return;

        using (_dataView.DeferRefresh())
        {
            _dataView.GroupDescriptions.Clear();
            _dataView.SortDescriptions.Clear();

            _dataView.GroupDescriptions.Add(new PropertyGroupDescription(prop));
        }
    }

    /// <summary>
    /// 查找关键字，隐藏无关词条
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isCase">是否大小写敏感</param>
    public void Search(string name, bool isCase = false)
    {
        if (_dataView == null) return;

        _dataView.Filter = o => isCase is false
            ? ((PropertyItem) o).DisplayName.ToLower().Contains(name.ToLower())
            : ((PropertyItem) o).DisplayName.Contains(name);
    }

    #endregion

    #region EditorDict

    /// <summary>
    /// 用于反射使用的属性解析器，所以这里除了内置的可以很方便的通过添加key和value的方式来扩展当前默认属性表
    /// </summary>
    private Dictionary<string, Type> EditorDict { get; set; } = new()
    {
        {EditorDictKeys.ReadOnlyWithTextBox,typeof(ReadOnlyWithTextBoxEditor)},
        {EditorDictKeys.ReadOnlyWithTextBlock,typeof(ReadOnlyWithTextBlockEditor)},
        {EditorDictKeys.SwitchProperty,typeof(SwitchPropertyEditor)},
    };

    /// <summary>
    /// 
    /// </summary>
    public static class EditorDictKeys
    {
        /// <summary>
        /// 
        /// </summary>
        public const string ReadOnlyWithTextBox = "ReadOnlyWithTextBoxEditor";

        /// <summary>
        /// 
        /// </summary>
        public const string ReadOnlyWithTextBlock = "ReadOnlyWithTextBlockEditor";

        /// <summary>
        /// 
        /// </summary>
        public const string SwitchProperty = "SwitchPropertyEditor";
    }

    /// <summary>
    /// 添加某个关键字
    /// </summary>
    /// <param name="key"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool AddEditor(string key, Type type)
    {
        if (type.BaseType != typeof(BasePropertyEditor))
            throw new Exception("The editor type must inherit the BasePropertyEditor");

        if (EditorDict.Keys.Contains(key))
            throw new Exception($"The key: {key} has already been registered");

        return EditorDict.TryAdd(key, type);
    }

    /// <summary>
    /// 移除某个关键字
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool RemoveEditor(string key)
        => EditorDict.Remove(key);

    /// <summary>
    /// 从PropertyItem生成合法的数据对象
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal BasePropertyEditor? GetEditor(PropertyItem item)
        => GetEditor(item.PropertyGridAttribute.Editor ?? "") ?? item.BindingType.ToString() switch
        {
            TypeUtils.Int => GetEditor(EditorDictKeys.ReadOnlyWithTextBox),
            TypeUtils.Bool => GetEditor(EditorDictKeys.SwitchProperty),
            TypeUtils.String => GetEditor(EditorDictKeys.ReadOnlyWithTextBox),
            _ => GetEditor(EditorDictKeys.ReadOnlyWithTextBlock)
        };


    internal BasePropertyEditor? GetEditor(string key)
        => (EditorDict.TryGetValue(key ?? "", out var type))
            ? Activator.CreateInstance(type) as BasePropertyEditor : null;

    #endregion
}
