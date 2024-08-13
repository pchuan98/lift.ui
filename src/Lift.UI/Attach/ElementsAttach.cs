using System.Windows;
using DependencyPropertyGenerator;

namespace Lift.UI.Attach;

/// <summary>
/// 
/// </summary>
[AttachedDependencyProperty<int>("Int32", DefaultValue = 0, Inherits = true)]
[AttachedDependencyProperty<double>("Double", DefaultValue = 0, Inherits = true)]
public partial class Element;

/// <summary>
/// 
/// </summary>
[AttachedDependencyProperty<CornerRadius>("CornerRadius", Inherits = true)]
public partial class BorderElement;


