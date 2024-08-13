using System.Windows.Input;

namespace Lift.UI.Commands;

/// <summary>
/// All control commands in this lib
/// </summary>
public static class ControlCommands
{
    /// <summary>
    /// 切换任意两个有且仅有两个可切换的对象
    /// </summary>
    public static RoutedCommand Toggle { get; } = new(nameof(Toggle), typeof(ControlCommands));

}
