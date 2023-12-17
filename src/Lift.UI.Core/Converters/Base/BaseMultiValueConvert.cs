namespace Lift.UI.Core.Converters;

/// <summary>
/// 多值转换器基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseMultiValueConvert<T>
    : MarkupExtension, IMultiValueConverter
     where T : class, new()
{
    /// <inheritdoc/>
    public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

    /// <inheritdoc/>
    public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
      => throw new NotImplementedException();

    /// <summary>
    /// 单例模式，节省xaml开销（但是有一定概率会出问题）
    /// </summary>
    private static T? _Instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
        => _Instance ??= new T();
}
