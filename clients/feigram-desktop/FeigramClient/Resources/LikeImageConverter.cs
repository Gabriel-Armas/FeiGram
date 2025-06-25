using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FeigramClient.Resources
{
    public class LikeImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLiked = value is bool b && b;

            var imagePath = isLiked ? "/Resources/megustaActivo.png" : "/Resources/megusta.png";
            return new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
