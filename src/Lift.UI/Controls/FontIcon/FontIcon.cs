using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DependencyPropertyGenerator;

namespace Lift.UI.Controls;

/// <summary>
/// 
/// </summary>
[DependencyProperty<Icons>("Icon", DefaultValue = Icons.None)]
[DependencyProperty<string>("IconString", DefaultValue = "", DefaultBindingMode = DefaultBindingMode.OneWay)]
[DependencyProperty<IconFontFamily>("IconFamily", DefaultValue = IconFontFamily.Filled)]
[TemplatePart(Name = ElementIconLabel, Type = typeof(Label))]
public partial class FontIcon : ContentControl
{
    private const string ElementIconLabel = "PART_IconLabel";

    private Label? _iconLabel;

    partial void OnIconFamilyChanged(IconFontFamily newValue)
    {
        FontFamily = newValue switch
        {
            IconFontFamily.Filled => (FontFamily) FindResource("FluentSystemIconsFilled"),
            IconFontFamily.Regular => (FontFamily) FindResource("FluentSystemIconsFilled"),
            _ => throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null)
        };
    }



    partial void OnIconChanged(Icons newValue)
    {
        if (newValue == Icons.None) IconString = string.Empty;

        if (!IconsMap.TryGetValue(Icon.ToString(), out var keys)) return;

        FontFamily = IconFamily switch
        {
            IconFontFamily.Filled => (FontFamily) FindResource("FluentSystemIconsFilled"),
            IconFontFamily.Regular => (FontFamily) FindResource("FluentSystemIconsFilled"),
            _ => throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null)
        };

        var map = FontFamily.ToString().Contains("FluentSystemIcons-Filled")
            ? FilledMap : FontFamily.ToString().Contains("FluentSystemIcons-Regular") ? RegularMap
            : throw new ArgumentOutOfRangeException(nameof(FontFamily), FontFamily.ToString(), "Not support font.");

        var name = FontFamily.ToString().Contains("FluentSystemIcons-Filled")
            ? "filled" : FontFamily.ToString().Contains("FluentSystemIcons-Regular") ? "regular"
                : throw new ArgumentOutOfRangeException(nameof(FontFamily), FontFamily.ToString(), "Not support font.");

        var fontsize = FontSize switch
        {
            <= 12 => 12,
            > 12 and <= 16 => 16,
            > 16 and <= 20 => 20,
            > 20 and <= 24 => 24,
            > 24 and <= 28 => 28,
            > 28 and <= 32 => 32,
            > 32 and <= 48 => 48,
            > 48 => 48,
            _ => throw new ArgumentException("Not valid fontsize.")
        };

        var keyBestMatch = keys.FirstOrDefault(s => s.Contains(fontsize.ToString()) && s.Contains(name));
        var keyMatch = keys.FirstOrDefault(s => s.Contains(name));

        if (keyBestMatch is null && keyMatch is null) return;

        IconString = keyBestMatch is not null
            ? ((char) map[keyBestMatch]).ToString()
            : ((char) map[keyMatch!]).ToString();
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _iconLabel = (Label?) GetTemplateChild(ElementIconLabel);
        ArgumentNullException.ThrowIfNull(_iconLabel);
    }
}

/// <summary>
/// 辅助创建ICON枚举和枚举反射字典
///
/// https://raw.githubusercontent.com/microsoft/fluentui-system-icons/main/icons_regular.md
/// https://raw.githubusercontent.com/microsoft/fluentui-system-icons/main/icons_filled.md
/// </summary>
internal static class FontIconHelper
{
    /// <summary>
    /// 
    /// </summary>
    private static readonly HttpClient Client = new();

    private static string _regular = "";
    private static string _filled = "";

    /// <summary>
    /// 
    /// </summary>
    private static string RegualrPath => Path.Join(Path.GetTempPath(), "__regular.md");

    /// <summary>
    /// 
    /// </summary>
    private static string FilledPath => Path.Join(Path.GetTempPath(), "__filled.md");

    /// <summary>
    /// 
    /// </summary>
    private static readonly string RegularMdUrl = @"https://raw.githubusercontent.com/microsoft/fluentui-system-icons/main/icons_regular.md";

    /// <summary>
    /// 
    /// </summary>
    private static readonly string FilledMdUrl = @"https://raw.githubusercontent.com/microsoft/fluentui-system-icons/main/icons_filled.md";

    /// <summary>
    /// |Name|Icon|iOS|Android|
    /// </summary>
    private static readonly Regex CellRegex = new(@"\|(.*)\|.*?\|.*?\|(.*)\|");

    /// <summary>
    /// 
    /// </summary>
    private static readonly Regex KeysRegex = new(@"`(.*?)`");

    private static (string, IEnumerable<string>)[] TFilled
        => CellRegex
            .Matches(_filled)
            .Skip(2)
            .Select(m => (m.Groups[1].Value.Replace(" ", "").ToUpper(),
                KeysRegex.Matches(m.Groups[2].Value).Select(k => k.Groups[1].Value)))
            .ToArray();

    private static (string, IEnumerable<string>)[] TRegular
        => CellRegex
            .Matches(_regular)
            .Skip(2)
            .Select(m => (m.Groups[1].Value.Replace(" ", "").ToUpper(),
                KeysRegex.Matches(m.Groups[2].Value).Select(k => k.Groups[1].Value)))
            .ToArray();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isReDownload"></param>
    public static async Task Download(bool isReDownload = false)
    {
        if (isReDownload || !File.Exists(RegualrPath))
            await File.WriteAllTextAsync(RegualrPath, await Client.GetStringAsync(RegularMdUrl));
        if (isReDownload || !File.Exists(FilledPath))
            await File.WriteAllTextAsync(FilledPath, await Client.GetStringAsync(FilledMdUrl));

        _regular = await File.ReadAllTextAsync(RegualrPath);
        _filled = await File.ReadAllTextAsync(FilledPath);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetEnumString()
    {
        var filled = TFilled;
        var regular = TRegular;

        var filledHash = filled.Select(item => item.Item1).ToHashSet();
        var regularHash = regular.Select(item => item.Item1).ToHashSet();

        var strs = (from key in filledHash.Union(regularHash).OrderBy(k => k)
                    let val1 = filled.FirstOrDefault(item => item.Item1.ToUpper() == key).Item2
                    let val2 = regular.FirstOrDefault(item => item.Item1.ToUpper() == key).Item2
                    select $"""
                    /// <summary>
                    {((val1 is null ? "" : "/// " + string.Join("\t", val1) +
                                                    (val2 is null ? "" : Environment.NewLine)) +
                              (val2 is null ? "" : "/// " + string.Join("\t", val2)))}
                    /// </summary>
                    {key},

                    """).ToList();

        return string.Join("", strs);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetDictString()
    {
        var filled = TFilled;
        var regular = TRegular;

        var filledHash = filled.Select(item => item.Item1).ToHashSet();
        var regularHash = regular.Select(item => item.Item1).ToHashSet();

        var strs = (from key in filledHash.Union(regularHash).OrderBy(k => k)
                    let val1 = filled.FirstOrDefault(item => item.Item1.ToUpper() == key).Item2
                    let val2 = regular.FirstOrDefault(item => item.Item1.ToUpper() == key).Item2
                    select $$"""
                     {"{{key}}",[{{string.Join(",", (val1 ?? new List<string>())
                         .Concat(val2 ?? new List<string>())
                         .Select(item => "\"" + item + "\""))}}]},
                     """).ToList();

        return string.Join("\n", strs);
    }
}
