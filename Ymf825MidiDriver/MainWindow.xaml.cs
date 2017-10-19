using System.Windows;

namespace Ymf825MidiDriver
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        public static readonly DependencyProperty BasicOctaveProperty = DependencyProperty.Register("BasicOctave", typeof(int), typeof(MainWindow), new PropertyMetadata(1));

        public int BasicOctave
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(BasicOctaveProperty);
            set => SetValue(BasicOctaveProperty, value);
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
