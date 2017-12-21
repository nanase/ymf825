using System.Windows;
using System.Windows.Controls;

namespace Ymf825MidiDriver.EqualizerControl
{
    /// <inheritdoc cref="System.Windows.Controls.UserControl" />
    /// <summary>
    /// EqualizerValueControl.xaml の相互作用ロジック
    /// </summary>
    public partial class EqualizerValueControl
    {
        #region -- Private Fields --

        public static readonly DependencyProperty EqualizerProperty = DependencyProperty.Register(nameof(Equalizer), typeof(Equalizer), typeof(EqualizerValueControl), new PropertyMetadata(default(Equalizer)));
        
        #endregion

        #region -- Public Properties --

        public int EqualizerIndex { get; set; }

        public Equalizer Equalizer
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (Equalizer)GetValue(EqualizerProperty);
            set => SetValue(EqualizerProperty, value);
        }

        #endregion

        #region -- Constructors --

        public EqualizerValueControl()
        {
            InitializeComponent();
        }

        #endregion

        #region -- Event Handlers --

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            B0.Value = 1.0m;
            B1.Value = 0.0m;
            B2.Value = 0.0m;
            A1.Value = 0.0m;
            A2.Value = 0.0m;
        }

        #endregion


    }
}
