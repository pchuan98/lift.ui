using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Lift.UI.Test.Controls.PropertyGrid;

// note: 目标是所有方法都应该依托于反射来做

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

public class PropertyItem : ListBoxItem
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(object), typeof(PropertyItem), new PropertyMetadata(default(object)));

    public object Value
    {
        get => (object) GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
        nameof(DisplayName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string DisplayName
    {
        get => (string) GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
        nameof(PropertyName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string PropertyName
    {
        get => (string) GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
        nameof(PropertyType), typeof(Type), typeof(PropertyItem), new PropertyMetadata(default(Type)));

    public Type PropertyType
    {
        get => (Type) GetValue(PropertyTypeProperty);
        set => SetValue(PropertyTypeProperty, value);
    }

    public static readonly DependencyProperty PropertyTypeNameProperty = DependencyProperty.Register(
        nameof(PropertyTypeName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string PropertyTypeName
    {
        get => (string) GetValue(PropertyTypeNameProperty);
        set => SetValue(PropertyTypeNameProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string Description
    {
        get => (string) GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
        nameof(ReadOnly), typeof(bool), typeof(PropertyItem), new PropertyMetadata(default(bool)));

    public bool ReadOnly
    {
        get => (bool) GetValue(ReadOnlyProperty);
        set => SetValue(ReadOnlyProperty, value);
    }

    public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
        nameof(Category), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string Category
    {
        get => (string) GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }
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

public abstract class PropertyEditorBase : DependencyObject
{
    public abstract FrameworkElement CreateElement(PropertyItem propertyItem);

    public virtual BindingMode GetBindingMode(PropertyItem propertyItem)
        => propertyItem.ReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
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

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PropertyGridAttribute : Attribute
{
    /// <summary>
    /// 组名
    /// </summary>
    public string? GroupName { get; set; }

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
    public string? Editor { get; set; }

    /// <summary>
    /// 是否跳过该数据对象
    /// </summary>
    public bool Ignore { get; set; }

}


[TemplatePart(Name = ElementItems, Type = typeof(ItemsControl))]
[TemplatePart(Name = ElementHeader, Type = typeof(Control))]
public class PropertyGrid : Control
{
    public const string ElementItems = "PART_Items";

    public const string ElementHeader = "PART_Header";

    public PropertyGrid()
    {

    }

    private ItemsControl? _itemsControl;

    private ICollectionView _dataView;


    #region propdp

    public static readonly DependencyProperty MinTitleWidthProperty = DependencyProperty.Register(
        nameof(MinTitleWidth), typeof(double), typeof(PropertyGrid), new PropertyMetadata(default(double)));

    public double MinTitleWidth
    {
        get => (double) GetValue(MinTitleWidthProperty);
        set => SetValue(MinTitleWidthProperty, value);
    }

    public static readonly DependencyProperty MaxTitleWidthProperty = DependencyProperty.Register(
        nameof(MaxTitleWidth), typeof(double), typeof(PropertyGrid), new PropertyMetadata(default(double)));

    public double MaxTitleWidth
    {
        get => (double) GetValue(MaxTitleWidthProperty);
        set => SetValue(MaxTitleWidthProperty, value);
    }

    public static readonly DependencyProperty ShowSortButtonProperty = DependencyProperty.Register(
        nameof(ShowSortButton), typeof(bool), typeof(PropertyGrid), new PropertyMetadata(default(bool)));

    public bool ShowSortButton
    {
        get => (bool) GetValue(ShowSortButtonProperty);
        set => SetValue(ShowSortButtonProperty, value);
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


    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();


        _itemsControl = GetTemplateChild(ElementItems) as ItemsControl;
        UpdateItems(SelectedObject);
    }

    void UpdateItems(object? obj)
    {
        if (obj == null || _itemsControl == null) return;


        _dataView = CollectionViewSource.GetDefaultView(obj);
        _itemsControl.ItemsSource = new List<string>
        {
            "Item 1",
            "Item 2",
            "Item 3",
            "Item 4",
            "Item 5"
        }; ;



    }


}
