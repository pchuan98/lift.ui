namespace Lift.UI.Core.Converters;

/// <summary>
/// 单值转换器基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseValueConverter<T>
    : MarkupExtension, IValueConverter
    where T : class, new()
{
    /// <inheritdoc/>
    public abstract object Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    /// <inheritdoc/>
    public virtual object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    /// <summary>
    /// 单例模式，节省xaml开销（但是有一定概率会出问题）
    /// </summary>
    private static T? _instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
        => _instance ??= new T();
}
