using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FeigramClient.Resources
{
    public class MessageAlignmentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return false;

            string fromId = values[0]?.ToString();
            string currentUserId = values[1]?.ToString();

            return fromId == currentUserId;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}