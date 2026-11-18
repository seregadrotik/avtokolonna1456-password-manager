using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Avtokolonna1456.PasswordManager.Models;

namespace Avtokolonna1456.PasswordManager.Converters
{
    public sealed class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value is PasswordStatus s ? s : PasswordStatus.Ok;
            string key;
            switch (status)
            {
                case PasswordStatus.Warning: key = "Brush.StatusWarning"; break;
                case PasswordStatus.Urgent: key = "Brush.StatusUrgent"; break;
                case PasswordStatus.Today: key = "Brush.StatusToday"; break;
                case PasswordStatus.Expired: key = "Brush.StatusExpired"; break;
                default: key = "Brush.StatusOk"; break;
            }

            if (Application.Current?.TryFindResource(key) is Brush brush)
            {
                return brush;
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
