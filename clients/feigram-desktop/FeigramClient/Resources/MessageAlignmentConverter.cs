using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FeigramClient.Resources
{
    public class MessageAlignmentConverter : IValueConverter
    {
        public string CurrentUserId { get; set; } = "";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fromId = value as string;
            return fromId == CurrentUserId ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
