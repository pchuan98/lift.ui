﻿using System.Windows;

namespace Lift.UI.Interactivity;

public interface IAttachedObject
{
    void Attach(DependencyObject dependencyObject);
    void Detach();

    DependencyObject AssociatedObject { get; }
}
