using System.Windows;
using Ymf825;

namespace Ymf825MidiDriver
{
    /// <inheritdoc cref="System.Windows.Controls.UserControl" />
    /// <summary>
    /// EnvelopeGraph.xaml の相互作用ロジック
    /// </summary>
    public partial class EnvelopeGraph
    {
        private const double GraphPadding = 5.0;

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
            get => (int)GetValue(AttackRateProperty);
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

        public bool ShowAttackArea { get; set; }

        public bool ShowDecayArea { get; set; }

        public bool ShowSustainArea { get; set; }

        public bool ShowReleaseArea { get; set; }

        public EnvelopeGraph()
        {
            InitializeComponent();
        }

        public void UpdateEnvelopeLines()
        {
            var (rates, ratesXy) = CalcRates();
            var (attackRate, decayRate, _, _, _) = rates;
            var (attackRateX, attackRateY, decayRateX, decayRateY, sustainRateX, sustainRateY) = ratesXy;

            var ghostAttackRateY = 70.0 - (70.0 - GraphPadding);
            var ghostDecayRateY = 70.0 - (1.0 - SustainLevel / 15.0) * (70.0 - GraphPadding);
            var ghostSustainRateY = 70.0 - (1.0 - SustainLevel / 15.0) * 0.5 * (70.0 - GraphPadding);

            // Rate Line
            AttackRateLine.Points.Clear();
            AttackRateLine.Points.Add(new Point(5.0, 70.0));
            AttackRateLine.Points.Add(new Point(attackRateX, attackRateY));

            DecayRateLine.Points.Clear();
            DecayRateLine.Points.Add(new Point(attackRateX, attackRateY));
            DecayRateLine.Points.Add(new Point(decayRateX, decayRateY));

            SustainRateLine.Points.Clear();
            SustainRateLine.Points.Add(new Point(decayRateX, decayRateY));
            SustainRateLine.Points.Add(new Point(sustainRateX, sustainRateY));

            ReleaseRateLine.Points.Clear();
            ReleaseRateLine.Points.Add(new Point(sustainRateX, sustainRateY));
            ReleaseRateLine.Points.Add(new Point(ActualWidth - GraphPadding, 70.0));

            // Ghost Rate Line
            GhostAttackRateLine.Points.Clear();
            GhostAttackRateLine.Points.Add(new Point(5.0, 70.0));
            GhostAttackRateLine.Points.Add(new Point(attackRateX, ghostAttackRateY));

            GhostDecayRateLine.Points.Clear();
            GhostDecayRateLine.Points.Add(new Point(attackRateX, ghostAttackRateY));
            GhostDecayRateLine.Points.Add(new Point(decayRateX, ghostDecayRateY));

            GhostSustainRateLine.Points.Clear();
            GhostSustainRateLine.Points.Add(new Point(decayRateX, ghostDecayRateY));
            GhostSustainRateLine.Points.Add(new Point(sustainRateX, ghostSustainRateY));

            GhostReleaseRateLine.Points.Clear();
            GhostReleaseRateLine.Points.Add(new Point(sustainRateX, ghostSustainRateY));
            GhostReleaseRateLine.Points.Add(new Point(ActualWidth - GraphPadding, 70.0));

            // Time Label
            SustainTime.Content = ((attackRate + decayRate) * 1000.0).ToString("f2");
            SustainTime.Margin = new Thickness(decayRateX + GraphPadding, 0, 0, -2);
            SustainTime.HorizontalAlignment = HorizontalAlignment.Left;

            if (decayRateX + GraphPadding * 2.0 + SustainTime.ActualWidth > ActualWidth)
            {
                SustainTime.Margin = new Thickness(0, 0, GraphPadding + 3.0, -2);
                SustainTime.HorizontalAlignment = HorizontalAlignment.Right;
            }

            // Time Line
            DecayTimeLine.Points.Clear();
            DecayTimeLine.Points.Add(new Point(attackRateX, GraphPadding));
            DecayTimeLine.Points.Add(new Point(attackRateX, 70.0));

            SustainTimeLine.Points.Clear();
            SustainTimeLine.Points.Add(new Point(decayRateX, GraphPadding));
            SustainTimeLine.Points.Add(new Point(decayRateX, 80.0));

            ReleaseTimeLine.Points.Clear();
            ReleaseTimeLine.Points.Add(new Point(sustainRateX, GraphPadding));
            ReleaseTimeLine.Points.Add(new Point(sustainRateX, 70.0));

            UpdateEnvelopeArea();
        }

        public void UpdateEnvelopeArea()
        {
            var (_, ratesXy) = CalcRates();
            var (attackRateX, attackRateY, decayRateX, decayRateY, sustainRateX, sustainRateY) = ratesXy;
            
            AttackArea.Points.Clear();
            if (ShowAttackArea)
            {
                AttackArea.Points.Add(new Point(5.0, 70.0));
                AttackArea.Points.Add(new Point(attackRateX, attackRateY));
                AttackArea.Points.Add(new Point(attackRateX, 70.0));
            }

            DecayArea.Points.Clear();
            if (ShowDecayArea)
            {
                DecayArea.Points.Add(new Point(attackRateX, attackRateY));
                DecayArea.Points.Add(new Point(decayRateX, decayRateY));
                DecayArea.Points.Add(new Point(decayRateX, 70.0));
                DecayArea.Points.Add(new Point(attackRateX, 70.0));
            }

            SustainArea.Points.Clear();
            if (ShowSustainArea)
            {
                SustainArea.Points.Add(new Point(decayRateX, decayRateY));
                SustainArea.Points.Add(new Point(sustainRateX, sustainRateY));
                SustainArea.Points.Add(new Point(sustainRateX, 70.0));
                SustainArea.Points.Add(new Point(decayRateX, 70.0));
            }

            ReleaseArea.Points.Clear();
            if (ShowReleaseArea)
            {
                ReleaseArea.Points.Add(new Point(sustainRateX, sustainRateY));
                ReleaseArea.Points.Add(new Point(ActualWidth - GraphPadding, 70.0));
                ReleaseArea.Points.Add(new Point(sustainRateX, 70.0));
            }
        }

        private
            ((double attackRate, double decayRate, double sustainRate, double releaseRate, double totalRate),
             (double attackRateX, double attackRateY, double decayRateX, double decayRateY, double sustainRateX, double sustainRateY))
            CalcRates()
        {
            Ymf825Driver.GetFnumAndBlock(Key, out var fnum, out var block, out var _);
            var rof = Ymf825Driver.CalcRof(EnableKsr, block, BasicOctave, (int)fnum);

            var attackRate = Ymf825Driver.CalcAttackRateTime(AttackRate, rof);
            var decayRate = Ymf825Driver.CalcEnvelopeRateTime(DecayRate, rof);
            var sustainRate = Ymf825Driver.CalcEnvelopeRateTime(SustainRate, rof);
            var releaseRate = Ymf825Driver.CalcEnvelopeRateTime(ReleaseRate, rof);
            var totalRate = attackRate + decayRate + sustainRate + releaseRate;

            var attackRateX = (attackRate / totalRate) * (ActualWidth - GraphPadding * 2) + GraphPadding;
            var attackRateY = 70.0 - (1.0 - TotalLevel / 63.0) * (70.0 - GraphPadding);
            var decayRateX = attackRateX + (decayRate / totalRate) * (ActualWidth - GraphPadding * 2);
            var decayRateY = 70.0 - (1.0 - TotalLevel / 63.0) * (1.0 - SustainLevel / 15.0) * (70.0 - GraphPadding);
            var sustainRateX = decayRateX + (sustainRate / totalRate) * (ActualWidth - GraphPadding * 2);
            var sustainRateY = 70.0 - (1.0 - TotalLevel / 63.0) * (1.0 - SustainLevel / 15.0) * 0.5 * (70.0 - GraphPadding);

            if (double.IsNaN(attackRateX))
                attackRateX = double.PositiveInfinity;

            if (double.IsNaN(decayRateX))
                decayRateX = double.PositiveInfinity;

            if (double.IsNaN(sustainRateX))
                sustainRateX = double.PositiveInfinity;

            return ((attackRate, decayRate, sustainRate, releaseRate, totalRate),
                    (attackRateX, attackRateY, decayRateX, decayRateY, sustainRateX, sustainRateY));
        }
    }
}
