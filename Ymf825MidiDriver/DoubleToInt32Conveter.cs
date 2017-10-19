using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ymf825MidiDriver
{
    [ValueConversion(typeof(double), typeof(int))]
    internal class DoubleToInt32Conveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double doubleValue))
                return Binding.DoNothing;

            return (int)Math.Round(doubleValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int intValue))
                return Binding.DoNothing;

            return (double)intValue;
        }

    }
}
