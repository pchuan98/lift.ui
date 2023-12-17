using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Lift.UI.Core.Helper;

/// <summary>
/// The helper class for VisualTree
/// </summary>
public static class VisualTreeHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="child"></param>
    /// <returns></returns>
    public static T? FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
        var parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);

        return parentObject switch
        {
            null => null,
            T parent => parent,
            _ => FindParent<T>(parentObject)
        };
    }
}
