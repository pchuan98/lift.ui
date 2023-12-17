using System.Windows;

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
        if (window == null)
        {
            return;
        }

        var left = window.Left + (window.Width - src.Width) / 2;
        var top = window.Top + (window.Height - src.Height) / 2;

        src.Left = left;
        src.Top = top;

        active(src);
    }

}
