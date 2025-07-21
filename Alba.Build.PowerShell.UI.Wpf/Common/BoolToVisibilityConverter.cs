using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Alba.Build.PowerShell.UI.Wpf.Common;

[ValueConversion(typeof(bool), typeof(Visibility))]
internal class BoolToVisibilityConverter(Visibility @true, Visibility @false) : MarkupExtension, IValueConverter
{
    public Visibility True { get; set; } = @true;
    public Visibility False { get; set; } = @false;

    public BoolToVisibilityConverter() : this(Visibility.Visible, Visibility.Collapsed) { }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (ReferenceEquals(value, DependencyProperty.UnsetValue))
            return DependencyProperty.UnsetValue;

        var visibility = value switch {
            null => false,
            bool b => b,
            int i => i > 0,
            _ => true,
        };
        return visibility ? True : False;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Visibility visibility)
            return Binding.DoNothing;
        if (visibility == True)
            return true;
        else if (visibility == False)
            return false;
        return Binding.DoNothing;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}