// reference: https://github.com/HandyOrg/HandyControl/blob/master/src/Shared/HandyControl_Shared/Controls/Block/ToggleBlock.cs

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DependencyPropertyGenerator;
using Lift.UI.Commands;

namespace Lift.UI.Controls.Block;

/// <summary>
/// 用来切换展示两个元素的占位符
/// </summary>
[DependencyProperty<bool?>("IsChecked", DefaultValueExpression = "false", Journal = true, TypeConverter = typeof(NullableBoolConverter))]
[DependencyProperty<object>("CheckedContent")]
[DependencyProperty<object>("UnCheckedContent")]
[DependencyProperty<object>("NullContent")]
public partial class ToggleBlock : Control
{
    /// <summary>
    /// 
    /// </summary>
    public ToggleBlock()
    {
        CommandBindings.Add(new CommandBinding(ControlCommands.Toggle,
            (_, _) => SetCurrentValue(IsCheckedProperty, IsChecked != true)));
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        SetCurrentValue(IsCheckedProperty, !IsChecked);
    }
}
