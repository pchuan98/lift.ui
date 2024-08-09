using System.Windows;
using System.Windows.Media;
using DependencyPropertyGenerator;

namespace Lift.UI.Controls.Panel;

/// <summary>
/// 顺时针-圆-排布，第一个元素永远在控件最上方
///
/// The actual width or height is radius + max(height in children)
/// </summary>
[
    DependencyProperty<double>("Radius",
        DefaultValue = 20,
        AffectsArrange = true,
        PropertyXmlDocumentation = "<summary>\nChild control clockwise rotation angle\n</summary>"),
    DependencyProperty<double>(
        "Rotate",
        AffectsMeasure = true,
        PropertyXmlDocumentation = "<summary>\nChild control clockwise rotation angle\n</summary>"),
]
public partial class CirclePanel : System.Windows.Controls.Panel
{
    /// <summary>
    /// <inheritdoc />
    /// NOTE: 遍历子元素，根据子元素判断当前控件需要的尺寸 [Available >= DesiredSize >= ElementsSize]
    /// </summary>
    /// <param name="availableSize"></param>
    /// <returns></returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var count = Children.Count;
        var maxHeight = 0.0;

        if (count == 1)
        {
            Children[0].Measure(availableSize);
            var desired = Children[0].DesiredSize;

            return new Size(Math.Max(availableSize.Width, desired.Width), Math.Max(availableSize.Height, desired.Height));
        }

        for (var i = 0; i < count; i++)
        {
            Children[i].Measure(availableSize);
            var desired = Children[i].DesiredSize;

            maxHeight = Math.Max(maxHeight, desired.Height);
        }

        var diameter = Radius * 2 + maxHeight;
        var vertex = 1.414214 * diameter * Math.Max(
            Math.Cos((Rotate + 45) / 180.0 * Math.PI),
            Math.Sin((Rotate + 45) / 180.0 * Math.PI));
        vertex = Math.Abs(vertex);

        return new Size(vertex, vertex);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var count = Children.Count;
        var perAngle = 360.0 / count;

        var ry = finalSize.Height / 2;
        var rx = finalSize.Width / 2;

        // todo when count==1

        for (var i = 0; i < count; i++)
        {
            var child = Children[i];
            var desired = child.DesiredSize;

            var center = (desired.Width / 2, desired.Height / 2);
            var angle = perAngle * i + Rotate;

            var transform = new RotateTransform()
            {
                CenterX = center.Item1,
                CenterY = center.Item2,
                Angle = angle
            };

            child.RenderTransform = transform;

            var x = rx + Radius * Math.Sin(angle / 180.0 * Math.PI);
            var y = ry - Radius * Math.Cos(angle / 180.0 * Math.PI);

            child.Arrange(new Rect(x - center.Item1, y - center.Item2, desired.Width, desired.Height));
        }

        return finalSize;
    }
}
