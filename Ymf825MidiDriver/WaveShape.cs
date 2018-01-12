using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Math;

namespace Ymf825MidiDriver
{
    internal class WaveShape
    {
        private const double Pi2 = PI * 2.0;
        private const double ClampThreshold = 0.7;

        private static double Clamp(double value)
        {
            return value > ClampThreshold ? ClampThreshold :
                   value < -ClampThreshold ? -ClampThreshold : value;
        }

        private static double Triangle(double x)
        {
            return x < 0.25 ? x * 4.0 :
                   x < 0.75 ? 2.0 - x * 4.0 : -4.0 + x * 4.0;
        }

        private static double Saw(double x)
        {
            return x < 0.5 ? x * 2.0 : -2.0 + x * 2.0;
        }

        public static readonly Func<double, double>[] ShapeFunctions =
        {
            x => Sin(x * Pi2),
            x => x < 0.5 ? Sin(x * Pi2) : 0.0,
            x => Abs(Sin(x * Pi2)),
            x => x % 0.5 < 0.25 ? Abs(Sin(x * Pi2)) : 0.0,

            x => x < 0.5 ? Sin(2.0 * x * Pi2) : 0.0,
            x => x < 0.5 ? Abs(Sin(2.0 * x * Pi2)) : 0.0,
            x => x < 0.5 ? 1.0 : -1.0,
            x => x < 0.5 ? 1.0 - Sin(0.5 * x * Pi2) : Sin(0.5 * x * Pi2) - 1.0,

            x => Clamp(Sin(x * Pi2)),
            x => x < 0.5 ? Clamp(Sin(x * Pi2)) : 0.0,
            x => Clamp(Abs(Sin(x * Pi2))),
            x => x % 0.5 < 0.25 ? Clamp(Abs(Sin(x * Pi2))) : 0.0,

            x => x < 0.5 ? Clamp(Sin(2.0 * x * Pi2)) : 0.0,
            x => x < 0.5 ? Clamp(Abs(Sin(2.0 * x * Pi2))) : 0.0,
            x => x <= 0.5 ? 1.0 : 0.0,
            x => 0.0,

            Triangle,
            x => x < 0.5 ? Triangle(x) : 0.0,
            x => Abs(Triangle(x)),
            x => x % 0.5 < 0.25 ? Abs(Triangle(x)) : 0.0,

            x => x < 0.5 ? Triangle(2.0 * x) : 0.0,
            x => x < 0.5 ? Abs(Triangle(2.0 * x)) : 0.0,
            x => x % 0.5 < 0.25 ? 1.0 : 0.0,
            x => 0.0,

            Saw,
            x => x < 0.5 ? Saw(x) : 0.0,
            x => Saw(x % 0.5),
            x => x % 0.5 < 0.25 ? Abs(Saw(x % 0.5)) : 0.0,

            x => x < 0.5 ? Saw(2.0 * x) : 0.0,
            x => x < 0.5 ? Abs(Saw(2.0 * x % 0.5)) : 0.0,
            x => x < 0.25 ? 1.0 : 0.0,
            x => 0.0,
        };
    }

    [ValueConversion(typeof(string), typeof(Grid))]
    internal class WaveShapeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const double centerLine = 22.0;
            const double width = 100.0;
            const double offsetX = 1.0;
            const double offsetY = 1.0;
            const int resolution = 80;

            if (value == null)
                return Binding.DoNothing;

            var values = value.ToString().Split(',').Select(v => int.Parse(v.Trim())).ToArray();
            var polylines = new Grid[values.Length];

            for (var j = 0; j < values.Length; j++)
            {
                var gridLine = new Polyline
                {
                    Stroke = new SolidColorBrush(Color.FromRgb(0xc0, 0xc0, 0xc0)),
                    Height = centerLine * 2.0 + offsetY * 2.0,
                    Width = width + offsetX * 2.0
                };
                gridLine.Points.Add(new Point(offsetX, centerLine + offsetY));
                gridLine.Points.Add(new Point(width + offsetX, centerLine + offsetY));

                var shapeLine = new Polyline
                {
                    Stroke = Brushes.Black,
                    Height = centerLine * 2.0 + offsetY * 2.0,
                    Width = width + offsetX * 2.0
                };

                var shape = WaveShape.ShapeFunctions[values[j]];
                {
                    shapeLine.Points.Add(new Point(offsetX, centerLine + offsetY));

                    for (var i = 1; i < resolution; i++)
                        shapeLine.Points.Add(new Point(
                            0.5 / resolution * i * width + offsetX,
                            centerLine - centerLine * shape(0.5 / resolution * i) + offsetY));

                    for (var i = 1; i < resolution; i++)
                        shapeLine.Points.Add(new Point(
                            (0.5 + 0.5 / resolution * i) * width + offsetX,
                            centerLine - centerLine * shape(0.5 + 0.5 / resolution * i) + offsetY));

                    shapeLine.Points.Add(new Point(width + offsetX, centerLine + offsetY));
                }

                polylines[j] = new Grid();
                polylines[j].Children.Add(gridLine);
                polylines[j].Children.Add(shapeLine);
            }

            return polylines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }

    [ValueConversion(typeof(int), typeof(int))]
    internal class WaveShapeToSelectedIndexConverter : IValueConverter
    {
        private static readonly int[] WaveShapeValueMap =
        {
            // SelectedIndex -> WaveShapeValue
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 24, 25, 26, 27, 28, 29, 30
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
                return Binding.DoNothing;

            var intValue = (int)value;
            var index = Array.IndexOf(WaveShapeValueMap, intValue);

            return index == -1 ? Binding.DoNothing : index;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
                return Binding.DoNothing;

            var intValue = (int)value;

            if (intValue < 0 || WaveShapeValueMap.Length <= intValue)
                return Binding.DoNothing;

            return WaveShapeValueMap[intValue];
        }

    }
}
