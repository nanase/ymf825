using System;
using System.Globalization;
using System.Windows.Data;

namespace Ymf825MidiDriver
{
    [ValueConversion(typeof(object[]), typeof(string))]
    internal class ToneNumberToStringConveter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 4)
                return Binding.DoNothing;

            if (!(values[0] is bool programNumberAssigned) || !(values[2] is bool percussionNumberAssigned) ||
                !(values[1] is int programNumber) || !(values[3] is int percussionNumber))
                return Binding.DoNothing;
            
            return programNumberAssigned ?
                $"prog: #{programNumber}" + (percussionNumberAssigned ? $", perc: #{percussionNumber}" : "") :
                "prog: #--";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new[] { Binding.DoNothing, Binding.DoNothing };
        }
    }
}
