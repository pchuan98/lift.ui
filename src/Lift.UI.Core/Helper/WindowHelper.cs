using System.Windows;
using System.Windows.Interop;

namespace Lift.UI.Core.Helper;

/// <summary>
/// 
/// </summary>
public static class WindowHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="src"></param>
    /// <param name="window"></param>
    /// <param name="active"></param>
    public static void CenterTo(this Window src, Window window, Action<Window> active)
    {
        var obj = src;
        var windowHandle = new WindowInteropHelper(window).Handle;
        var screen = Screen.FromHandle(windowHandle);

        var top = window.WindowState == WindowState.Maximized
            ? screen.Bounds.Top + (screen.Bounds.Height - obj.Height) / 2
            : window.Top + (window.ActualHeight - obj.Height) / 2;

        var left = window.WindowState == WindowState.Maximized
            ? screen.Bounds.Left + (screen.Bounds.Width - obj.Width) / 2
            : window.Left - (obj.Width - window.ActualWidth) / 2;

        obj.Top = top;
        obj.Left = left;
    }

}
