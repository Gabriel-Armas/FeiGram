﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FeigramClient.Resources
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;

            if (value is bool b)
                flag = b;
            else if (value is string str)
                flag = string.IsNullOrWhiteSpace(str);

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}
