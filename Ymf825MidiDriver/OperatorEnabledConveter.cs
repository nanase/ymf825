using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ymf825MidiDriver
{
    [ValueConversion(typeof(int), typeof(bool))]
    internal class OperatorEnabledConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int intValue))
                return Binding.DoNothing;

            return intValue > 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

    }
}
