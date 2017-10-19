using System.Windows;

namespace Ymf825MidiDriver
{
    /// <inheritdoc cref="System.Windows.Controls.UserControl" />
    /// <summary>
    /// EnvelopeGraph.xaml の相互作用ロジック
    /// </summary>
    public partial class EnvelopeGraph
    {
        public static readonly DependencyProperty AttackRateProperty = DependencyProperty.Register("AttackRate", typeof(int), typeof(EnvelopeGraph), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty DecayRateProperty = DependencyProperty.Register("DecayRate", typeof(int), typeof(EnvelopeGraph), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty SustainRateProperty = DependencyProperty.Register("SustainRate", typeof(int), typeof(EnvelopeGraph), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty SustainLevelProperty = DependencyProperty.Register("SustainLevel", typeof(int), typeof(EnvelopeGraph), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty ReleaseRateProperty = DependencyProperty.Register("ReleaseRate", typeof(int), typeof(EnvelopeGraph), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty TotalLevelProperty = DependencyProperty.Register("TotalLevel", typeof(int), typeof(EnvelopeGraph), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(int), typeof(EnvelopeGraph), new PropertyMetadata(64));
        public static readonly DependencyProperty EnableKsrProperty = DependencyProperty.Register("EnableKsr", typeof(bool), typeof(EnvelopeGraph), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty BasicOctaveProperty = DependencyProperty.Register("BasicOctave", typeof(int), typeof(EnvelopeGraph), new PropertyMetadata(default(int)));
        
        public int AttackRate
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int) GetValue(AttackRateProperty);
            set => SetValue(AttackRateProperty, value);
        }

        public int DecayRate
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(DecayRateProperty);
            set => SetValue(DecayRateProperty, value);
        }

        public int SustainRate
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(SustainRateProperty);
            set => SetValue(SustainRateProperty, value);
        }

        public int SustainLevel
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(SustainLevelProperty);
            set => SetValue(SustainLevelProperty, value);
        }

        public int ReleaseRate
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(ReleaseRateProperty);
            set => SetValue(ReleaseRateProperty, value);
        }

        public int TotalLevel
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(TotalLevelProperty);
            set => SetValue(TotalLevelProperty, value);
        }
        
        public int Key
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public bool EnableKsr
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (bool)GetValue(EnableKsrProperty);
            set => SetValue(EnableKsrProperty, value);
        }

        public int BasicOctave
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(BasicOctaveProperty);
            set => SetValue(BasicOctaveProperty, value);
        }

        public EnvelopeGraph()
        {
            InitializeComponent();
        }
    }
}
