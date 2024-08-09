using System.Diagnostics;
using System.Windows;
using DependencyPropertyGenerator;

namespace Lift.UI.Controls.Panel;

// todo： 这里的需求还没有整理好，我应该写一个综合性质的控件，他的功能应该有
// 1. 控件之间的间隔设置
// 2. 我可以固定某一个控件填充所有部分，而其他填充指定像素宽度和高度，但是不用和dockpanel一样复杂
// 3. 该容器的总的行为应该是UniformGrid，但是如果我不给Cols和Rows的时候，他应该是stackpanel+dockpanel

/// <summary>
/// 容器中的元素有固定的间隔
/// </summary>
[
    DependencyProperty<double>(
        "Spacing",
        DefaultValue = 0,
        PropertyXmlDocumentation = "<summary>\nThe spacing between two elements\n</summary>")
]
public partial class AutoPanel : System.Windows.Controls.Panel;

/// <summary>
/// 用来测试的部分，之后要记得改回去，这里测试只写横向
///
/// 特性：两元素中一个填充，要给刚好自己的大小，另外，两个控件都会填充整个布局
///
/// NTOE 后面迁移到AutoPanel要记得改
/// </summary>
[DependencyProperty<bool>("IsSwitch", DefaultValue = false, AffectsArrange = true)] // 默认为 Auto - Filling,为true后反过来
[DependencyProperty<double>("Spacing", DefaultValue = 0, AffectsArrange = true)] // 两个元素之间的间隔
public partial class TwoAutoPanelTest1 : System.Windows.Controls.Panel
{
    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count != 2)
            throw new ArgumentOutOfRangeException(nameof(Children.Count), Children.Count, "Count must equl 2");

        var head = Children[0];
        var tail = Children[1];

        head.Measure(availableSize);
        tail.Measure(availableSize);

        return availableSize;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count != 2)
            throw new ArgumentOutOfRangeException(nameof(Children.Count), Children.Count, "Count must equl 2");

        var head = Children[0];
        var tail = Children[1];

        try
        {
            var headRect = new Rect(0, 0,
                IsSwitch ? finalSize.Width - tail.DesiredSize.Width - Spacing : head.DesiredSize.Width, finalSize.Height);
            head.Arrange(headRect);

            tail.Arrange(new Rect(headRect.Width + Spacing, 0, finalSize.Width - Spacing - headRect.Width, finalSize.Height));
        }
        catch (ArgumentException e)
        {
            Debug.WriteLine(e.Message);
        }

        return finalSize;
    }
}
