using System;
using System.Globalization;
using System.Windows.Data;

namespace Ymf825MidiDriver
{
    [ValueConversion(typeof(object[]), typeof(string))]
    internal class EqualizerNumberToStringConveter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return Binding.DoNothing;

            if (!(values[0] is bool programNumberAssigned) || !(values[1] is int programNumber))
                return Binding.DoNothing;

            return programNumberAssigned ?
                $"prog: #{programNumber}" : "prog: #--";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new[] { Binding.DoNothing, Binding.DoNothing };
        }
    }
}
