using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ymf825MidiDriver
{
    [ValueConversion(typeof(bool), typeof(ImageSource))]
    internal class MidiConnectionImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool boolValue))
                return Binding.DoNothing;

            return new BitmapImage(new Uri(boolValue ? "Resources/connect.png" : "Resources/disconnect.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
