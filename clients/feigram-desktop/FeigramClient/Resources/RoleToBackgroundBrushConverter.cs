using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FeigramClient.Resources
{
    public class RoleToBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string role = value as string;

            if (targetType == typeof(Brush))
            {
                if (role == "Banned")
                    return Brushes.LightPink;
                return Brushes.White;
            }

            if (targetType == typeof(bool))
            {
                return role != "Banned";
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
