using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ymf825MidiDriver
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// ToneExportWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ToneExportWindow
    {
        public ToneItem ToneItem { get; set; }

        public ToneExportWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HexRadioButton_Checked(sender, e);
        }

        private void HexRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ToneItem == null)
                return;
            
            var buffer = new byte[30];
            ToneItem.ToneParameter.Export(buffer, 0);
            var data = buffer.Select(b => b.ToString("x2")).ToArray();
            DataText.Text = $"{string.Join(" ", data.Take(2))}\n" +
                            $"{string.Join(" ", data.Skip(2).Take(7))}\n" +
                            $"{string.Join(" ", data.Skip(9).Take(7))}\n" +
                            $"{string.Join(" ", data.Skip(16).Take(7))}\n" +
                            $"{string.Join(" ", data.Skip(23).Take(7))}";
        }

        private void DecRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ToneItem == null)
                return;

            var buffer = new byte[30];
            ToneItem.ToneParameter.Export(buffer, 0);
            var data = buffer.Select(b => $"{b,3:d}").ToArray();
            DataText.Text = $"{string.Join(" ", data.Take(2))}\n" +
                            $"{string.Join(" ", data.Skip(2).Take(7))}\n" +
                            $"{string.Join(" ", data.Skip(9).Take(7))}\n" +
                            $"{string.Join(" ", data.Skip(16).Take(7))}\n" +
                            $"{string.Join(" ", data.Skip(23).Take(7))}";
        }

        private void Base64RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ToneItem == null)
                return;

            var buffer = new byte[30];
            ToneItem.ToneParameter.Export(buffer, 0);
            DataText.Text = Convert.ToBase64String(buffer, Base64FormattingOptions.None);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
