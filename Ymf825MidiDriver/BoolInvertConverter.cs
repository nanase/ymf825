using System;
using System.Globalization;
using System.Windows.Data;

namespace Ymf825MidiDriver
{
    [ValueConversion(typeof(bool), typeof(bool))]
    internal class BoolInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool boolValue))
                return Binding.DoNothing;

            return !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool boolValue))
                return Binding.DoNothing;

            return !boolValue;
        }
    }
}
