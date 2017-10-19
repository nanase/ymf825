using System.Windows.Controls;

namespace Ymf825MidiDriver
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// OperatorParameter.xaml の相互作用ロジック
    /// </summary>
    public partial class OperatorParameter
    {
        public int? OperatorNumber { get; set; }

        public bool HasFeedback { get; set; }

        public int BasicOctave
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(MainWindow.BasicOctaveProperty);
            set => SetValue(MainWindow.BasicOctaveProperty, value);
        }

        public OperatorParameter()
        {
            InitializeComponent();
        }
    }
}
