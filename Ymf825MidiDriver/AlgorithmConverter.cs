using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Ymf825MidiDriver.AlgorithmControl;

namespace Ymf825MidiDriver
{
    [ValueConversion(typeof(string), typeof(UserControl))]
    internal class AlgorithmConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            var values = value.ToString().Split(',').Select(v => int.Parse(v.Trim())).ToArray();
            var controls = new UserControl[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                switch (values[i])
                {
                    case 0:
                        controls[i] = new Algorithm0 { Width = 180, Height = 70 };
                        break;

                    case 1:
                        controls[i] = new Algorithm1 { Width = 180, Height = 70 };
                        break;

                    case 2:
                        controls[i] = new Algorithm2 { Width = 180, Height = 70 };
                        break;

                    case 3:
                        controls[i] = new Algorithm3 { Width = 180, Height = 70 };
                        break;

                    case 4:
                        controls[i] = new Algorithm4 { Width = 180, Height = 70 };
                        break;

                    case 5:
                        controls[i] = new Algorithm5 { Width = 180, Height = 70 };
                        break;

                    case 6:
                        controls[i] = new Algorithm6 { Width = 180, Height = 70 };
                        break;

                    case 7:
                        controls[i] = new Algorithm7 { Width = 180, Height = 70 };
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }

                controls[i].Tag = i;
            }

            return controls;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
