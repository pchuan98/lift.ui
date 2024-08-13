using System.Windows;
using System.Windows.Markup;

[assembly: ThemeInfo(ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly)]

// attach
[assembly: XmlnsDefinition("https://github.com/pchuan98/lift.ui", "Lift.UI.Attach")]

// controls
[assembly: XmlnsDefinition("https://github.com/pchuan98/lift.ui", "Lift.UI.Controls.Panel")]

[assembly: XmlnsPrefix("https://github.com/pchuan98/lift.ui", "lift")]
