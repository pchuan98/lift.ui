// reference: https://github.com/HandyOrg/HandyControl/blob/master/src/Shared/HandyControl_Shared/Controls/Panel/SimplePanel.cs

using System.Data;
using System.Windows;

namespace Lift.UI.Controls.Panel;

/// <summary>
/// 取代Grid的轻量化控件
/// </summary>
public class SimplePanel : System.Windows.Controls.Panel
{
    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var maxSize = new Size();

        foreach (UIElement child in InternalChildren)
        {
            // HACK: 这里居然会有空？不信，在这里加个判断看看
            if (child == null) throw new Exception("UNBELIVABLE！This element is null.");
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (child == null) continue;

            child.Measure(availableSize);
            maxSize.Width = Math.Max(maxSize.Width, child.DesiredSize.Width);
            maxSize.Height = Math.Max(maxSize.Height, child.DesiredSize.Height);
        }

        return maxSize;

    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (UIElement child in InternalChildren)
            child?.Arrange(new Rect(finalSize));

        return finalSize;
    }
}
