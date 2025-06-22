using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FeigramClient.Resources
{
    public class MessageBubbleColorConverter : IValueConverter
    {
        public string CurrentUserId { get; set; } = "";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fromId = value as string;
            return fromId == CurrentUserId
                ? new SolidColorBrush(Color.FromRgb(200, 255, 200))
                : new SolidColorBrush(Color.FromRgb(200, 220, 255));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
