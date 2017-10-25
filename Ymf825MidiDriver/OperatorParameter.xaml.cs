using System.Windows;

namespace Ymf825MidiDriver
{
    /// <inheritdoc cref="System.Windows.Controls.UserControl" />
    /// <summary>
    /// OperatorParameter.xaml の相互作用ロジック
    /// </summary>
    public partial class OperatorParameter
    {
        #region -- Public Fields --

        public static readonly RoutedEvent TonePropertyChangedEvent = EventManager.RegisterRoutedEvent("TonePropertyChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OperatorParameter));
        public static readonly DependencyProperty SelectedOperatorProperty = DependencyProperty.Register("SelectedOperator", typeof(Ymf825.OperatorParameter), typeof(OperatorParameter), new PropertyMetadata(default(Ymf825.OperatorParameter)));

        #endregion

        #region -- Public Propeties --

        public int? OperatorNumber { get; set; }

        public Ymf825.OperatorParameter SelectedOperator
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (Ymf825.OperatorParameter)GetValue(SelectedOperatorProperty);
            set => SetValue(SelectedOperatorProperty, value);
        }

        public bool HasFeedback { get; set; }
        #endregion
        
        #region -- Public Events --

        public event RoutedEventHandler TonePropertyChanged
        {
            add => AddHandler(TonePropertyChangedEvent, value);
            remove => RemoveHandler(TonePropertyChangedEvent, value);
        }

        #endregion

        #region -- Constructors --

        public OperatorParameter()
        {
            InitializeComponent();
        }

        #endregion

        #region -- Private Methods --

        private void OnTonePropertyChanged(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(TonePropertyChangedEvent, sender));
        }

        private void EnvelopeGraph_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            EnvelopeGraph.UpdateEnvelopeLines();
            e.Handled = true;
        }

        private void SliderEnvelope_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            switch (((FrameworkElement)sender).Name)
            {
                case nameof(SliderTotalLevel):
                case nameof(LabelTotalLevel):
                    EnvelopeGraph.ShowAttackArea = true;
                    EnvelopeGraph.ShowDecayArea = true;
                    EnvelopeGraph.ShowSustainArea = true;
                    EnvelopeGraph.ShowReleaseArea = true;
                    break;

                case nameof(SliderAttackRate):
                case nameof(LabelAttackRate):
                    EnvelopeGraph.ShowAttackArea = true;
                    break;

                case nameof(SliderDecayRate):
                case nameof(LabelDecayRate):
                    EnvelopeGraph.ShowDecayArea = true;
                    break;

                case nameof(SliderSustainRate):
                case nameof(SliderSustainLevel):
                case nameof(LabelSustainRate):
                case nameof(LabelSustainLevel):
                    EnvelopeGraph.ShowSustainArea = true;
                    break;

                case nameof(SliderReleaseRate):
                case nameof(LabelReleaseRate):
                    EnvelopeGraph.ShowReleaseArea = true;
                    break;
            }

            EnvelopeGraph.UpdateEnvelopeArea();
            e.Handled = true;
        }

        private void SliderEnvelope_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            switch (((FrameworkElement)sender).Name)
            {
                case nameof(SliderTotalLevel):
                case nameof(LabelTotalLevel):
                    EnvelopeGraph.ShowAttackArea = false;
                    EnvelopeGraph.ShowDecayArea = false;
                    EnvelopeGraph.ShowSustainArea = false;
                    EnvelopeGraph.ShowReleaseArea = false;
                    break;

                case nameof(SliderAttackRate):
                case nameof(LabelAttackRate):
                    EnvelopeGraph.ShowAttackArea = false;
                    break;

                case nameof(SliderDecayRate):
                case nameof(LabelDecayRate):
                    EnvelopeGraph.ShowDecayArea = false;
                    break;

                case nameof(SliderSustainRate):
                case nameof(SliderSustainLevel):
                case nameof(LabelSustainRate):
                case nameof(LabelSustainLevel):
                    EnvelopeGraph.ShowSustainArea = false;
                    break;

                case nameof(SliderReleaseRate):
                case nameof(LabelReleaseRate):
                    EnvelopeGraph.ShowReleaseArea = false;
                    break;
            }

            EnvelopeGraph.UpdateEnvelopeArea();
            e.Handled = true;
        }

        #endregion
    }
}
