using System;
using System.Globalization;
using System.Windows.Data;

namespace Ymf825MidiDriver
{
    [ValueConversion(typeof(object), typeof(bool))]
    internal class BoolSelectedValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
