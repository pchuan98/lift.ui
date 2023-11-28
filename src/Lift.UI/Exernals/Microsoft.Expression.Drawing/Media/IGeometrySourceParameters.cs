﻿using System.Windows.Media;

namespace Lift.UI.Expression.Media;

public interface IGeometrySourceParameters
{
    Stretch Stretch { get; }

    Brush Stroke { get; }

    double StrokeThickness { get; }
}
