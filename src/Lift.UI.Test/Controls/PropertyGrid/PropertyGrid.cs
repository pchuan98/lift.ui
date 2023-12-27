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

namespace Lift.UI.Test.Controls.PropertyGrid;

// note: 目标是所有方法都应该依托于反射来做


#region Editor

/// <summary>
/// 基础的编辑对象
/// </summary>
public abstract class PropertyEditorBase : DependencyObject
{
    /// <summary>
    /// 使用PropertyItem去创建一个Framework
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <returns></returns>
    public abstract FrameworkElement CreateElement(PropertyItem propertyItem);

    /// <summary>
    /// 手动绑定属性
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <param name="element"></param>
    public virtual void CreateBinding(PropertyItem propertyItem, DependencyObject element) =>
        BindingOperations.SetBinding(element, GetDependencyProperty(),
            new Binding($"{propertyItem.PropertyName}")
            {
                Source = propertyItem.Value,
                Mode = GetBindingMode(propertyItem),
                UpdateSourceTrigger = GetUpdateSourceTrigger(propertyItem),
                Converter = GetConverter(propertyItem)
            });

    /// <summary>
    /// 用来自定义该对象使用什么控件来操作
    /// </summary>
    /// <returns></returns>
    public abstract DependencyProperty GetDependencyProperty();

    /// <summary>
    /// 确定绑定方式
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <returns></returns>
    public virtual BindingMode GetBindingMode(PropertyItem propertyItem) =>
        propertyItem.PropertyGridAttribute?.ReadOnly is false ? BindingMode.OneWay : BindingMode.TwoWay;

    /// <summary>
    /// 默认当属性改变的时候更新数据源
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <returns></returns>
    public virtual UpdateSourceTrigger GetUpdateSourceTrigger(PropertyItem propertyItem) =>
        UpdateSourceTrigger.PropertyChanged;

    /// <summary>
    /// 默认不进行数据转换
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <returns></returns>
    protected virtual IValueConverter? GetConverter(PropertyItem propertyItem)
        => null;
}

public class ReadOnlyEditor : PropertyEditorBase
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem) => new System.Windows.Controls.TextBox()
    {
        IsReadOnly = true
    };


    public override DependencyProperty GetDependencyProperty()
        => System.Windows.Controls.TextBox.TextProperty;

    protected override IValueConverter GetConverter(PropertyItem propertyItem) =>
        ResourceHelper.GetResource<IValueConverter>("Object2StringConverter");
}

#endregion

/// <summary>
/// 专门针对ViewModel的反射帮助类
/// </summary>
internal static class ViewModelReflectionHelper
{
    /// <summary>
    /// 
    /// </summary>
    private const string MvvmToolkitsClassName = "ObservableObject";

    /// <summary>
    /// 配合CommunicateMvvmToolkits
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string Field2Prop(this string name)
        => $"{char.ToUpper(name.Replace("_", "")[0])}{name.Replace("_", "")[1..]}";

    /// <summary>
    /// 判断有没有继承ObservableObject
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsObservableObject(this object obj)
        => obj.GetType().BaseType?.Name == MvvmToolkitsClassName;

    /// <summary>
    /// 新的GetProperties方式，这里面
    /// </summary>
    /// <returns></returns>
    public static MemberInfo[] GetMembers(this object obj)
        => obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(member => member.MemberType is MemberTypes.Field or MemberTypes.Property)
            .Where(member => !member.IsNeedSkip(obj)).ToArray();

    /// <summary>
    /// 当前这个成员是否需要跳过不看
    /// 尤其是自动生成这一部分
    /// </summary>
    /// <param name="info"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsNeedSkip(this MemberInfo info, object obj)
        => obj.IsObservableObject() && (info.IsGeneratedCode() || info.IsDebuggerBrowsable());

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static PropertyGridAttribute? GetPropertyGridAttribute(this MemberInfo info)
        => info.GetCustomAttribute<PropertyGridAttribute>();

    /// <summary>
    /// 判断是不是GeneratedCodeAttribute数据对象
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsGeneratedCode(this MemberInfo info)
        => info.GetCustomAttribute<System.CodeDom.Compiler.GeneratedCodeAttribute>() is not null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsDebuggerBrowsable(this MemberInfo info)
        => info.GetCustomAttribute<System.Diagnostics.DebuggerBrowsableAttribute>() is not null;

    /// <summary>
    /// 获取绑定使用的名称
    /// </summary>
    /// <param name="info"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetBindingName(this MemberInfo info, object obj)
        => obj.IsObservableObject() ? info.Name.Field2Prop() : info.Name;
}

/// <summary>
/// 
/// </summary>
public static class ControlCommands
{
    /// <summary>
    /// 搜索
    /// </summary>
    public static RoutedCommand Search { get; } = new(nameof(Search), typeof(ControlCommands));

    /// <summary>
    /// 按照类别排序
    /// </summary>
    public static RoutedCommand SortByCategory { get; } = new(nameof(SortByCategory), typeof(ControlCommands));

    /// <summary>
    /// 按照名称排序
    /// </summary>
    public static RoutedCommand SortByName { get; } = new(nameof(SortByName), typeof(ControlCommands));
}

/// <summary>
/// 这个应该和Attribute相关联
/// </summary>
public class PropertyItem : ListBoxItem
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(object), typeof(PropertyItem), new PropertyMetadata(default(object)));

    /// <summary>
    /// ViewModel原始对象
    /// </summary>
    public object Value
    {
        get => (object) GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
        nameof(PropertyName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    /// <summary>
    /// 我当前需要关注的属性名称
    /// </summary>
    public string PropertyName
    {
        get => (string) GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
        nameof(DisplayName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string DisplayName
    {
        get => (string) GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public static readonly DependencyProperty EditorProperty = DependencyProperty.Register(
        nameof(Editor), typeof(PropertyEditorBase), typeof(PropertyItem), new PropertyMetadata(null));

    /// <summary>
    /// 数据类型对应使用的编辑窗口类型
    /// </summary>
    public PropertyEditorBase? Editor
    {
        get => (PropertyEditorBase?) GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    public static readonly DependencyProperty EditorElementProperty = DependencyProperty.Register(
        nameof(EditorElement), typeof(FrameworkElement), typeof(PropertyItem), new PropertyMetadata(default(FrameworkElement)));

    /// <summary>
    /// 有编辑窗口衍生的实际WPF控件元素
    /// </summary>
    public FrameworkElement EditorElement
    {
        get => (FrameworkElement) GetValue(EditorElementProperty);
        set => SetValue(EditorElementProperty, value);
    }

    public static readonly DependencyProperty PropertyGridAttributeProperty = DependencyProperty.Register(
        nameof(PropertyGridAttribute), typeof(PropertyGridAttribute), typeof(PropertyItem), new PropertyMetadata(default(PropertyGridAttribute)));

    public static readonly DependencyProperty TipsProperty = DependencyProperty.Register(
        nameof(Tips), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string Tips
    {
        get => (string) GetValue(TipsProperty);
        set => SetValue(TipsProperty, value);
    }

    /// <summary>
    /// 属性的外围修饰
    /// </summary>
    public PropertyGridAttribute? PropertyGridAttribute
    {
        get => (PropertyGridAttribute?) GetValue(PropertyGridAttributeProperty);
        set => SetValue(PropertyGridAttributeProperty, value);
    }

    /// <summary>
    /// 初始化显示使用的控件元素
    /// </summary>
    public virtual void InitElement()
    {
        if (Editor == null) return;
        EditorElement = Editor.CreateElement(this);
        Editor.CreateBinding(this, EditorElement);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="vm"></param>
    /// <returns></returns>
    public static PropertyItem FromMemberInfo(MemberInfo info, object vm) => new PropertyItem()
    {
        Value = vm,
        PropertyName = info.GetBindingName(vm),
        PropertyGridAttribute = info.GetPropertyGridAttribute() ?? new PropertyGridAttribute(),
        Editor = null,
        DisplayName = info.GetPropertyGridAttribute()?.Alias ?? info.GetBindingName(vm),
    };
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PropertyGridAttribute : Attribute
{
    #region Base prop

    /// <summary>
    /// 组名
    /// </summary>
    public string? GroupName { get; set; } = "DEFAULT";

    /// <summary>
    /// 别名
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// 变量名称用的Tips
    /// </summary>
    public string? Tips { get; set; }

    /// <summary>
    /// 是否变量只读
    /// </summary>
    public bool ReadOnly { get; set; } = true;

    /// <summary>
    /// 对应的控件类型，使用string是为了方便扩展
    /// </summary>
    public string Editor { get; set; } = "ReadOnlyDefault";

    /// <summary>
    /// 是否跳过该数据对象
    /// </summary>
    public bool Ignore { get; set; } = false;

    #endregion

    // todo 附属属性
    // note 附属属性和控件强相关，所以应该放到自定义控件那一部分
}

public class PropertyItemsControl : ListBox
{
    /// <summary>
    /// <inheritdoc />
    /// <para>
    /// 这个方法的作用是告诉 ItemsControl 是否应该使用指定的元素作为项目的容器，而不是默认的容器。
    /// </para>
    /// <para>
    /// 这里将它直接映到了 <see cref="PropertyItem"/> 类型。
    /// </para>
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is PropertyItem;

    public PropertyItemsControl()
    {
        // 用于控制在分组数据时是否启用虚拟化。

#if !NET40
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(this, true);
        VirtualizingPanel.SetScrollUnit(this, ScrollUnit.Pixel);
#else
        System.Windows.Controls.ScrollViewer.SetCanContentScroll(this, false);
#endif
    }
}

public enum PropertyEditorType
{
    TextBox,
    ComboBox,
    CheckBox,
    DateTimePicker,
    ColorPicker,
    Slider,
    PasswordBox,
    Custom
}

/// <summary>
/// 属性网格，用来做常规网格处理
/// </summary>
[TemplatePart(Name = ElementItems, Type = typeof(ItemsControl))]
public class PropertyGrid : Control
{
    public const string ElementItems = "PART_Items";

    private ItemsControl? _itemsControl;

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

    #endregion

    #region search bar

    // 假如这里有search相关的东西

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

        _itemsControl = GetTemplateChild(ElementItems) as ItemsControl;
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
            .Do(item => item.Editor = Get(item.PropertyGridAttribute?.Editor ?? "ReadOnlyDefault"))
            .Do(item => item.InitElement()));

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
        {"ReadOnlyDefault",typeof(ReadOnlyEditor)}
    };

    /// <summary>
    /// 添加某个关键字
    /// </summary>
    /// <param name="key"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool AddEditor(string key, Type type)
    {
        if (type.BaseType != typeof(PropertyEditorBase))
            throw new Exception("The editor type must inherit the PropertyEditorBase");

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

    internal PropertyEditorBase? Get(string key)
        => EditorDict.TryGetValue(key, out var type)
            ? Activator.CreateInstance(type) as PropertyEditorBase
            : null;

    #endregion
}
