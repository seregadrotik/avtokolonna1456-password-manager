using System;
using System.Globalization;
using System.Windows.Data;

namespace Avtokolonna1456.PasswordManager.Converters
{
    public sealed class PasswordMaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string ?? string.Empty;
            return new string('•', Math.Max(8, text.Length));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
