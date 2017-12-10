using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ymf825MidiDriver
{
    [ValueConversion(typeof(bool), typeof(string))]
    internal class ServerConnectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool connected))
                return Binding.DoNothing;

            return connected ? "YMF825 接続済" : "YMF825 未接続";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(bool), typeof(ImageSource))]
    internal class ServerConnectionImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool boolValue))
                return Binding.DoNothing;

            return new BitmapImage(new Uri(boolValue ? "Resources/server_green.png" : "Resources/server.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
