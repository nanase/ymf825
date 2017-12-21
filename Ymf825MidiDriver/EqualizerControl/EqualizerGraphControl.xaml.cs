using System.Windows;
using System.Windows.Controls;

namespace Ymf825MidiDriver.EqualizerControl
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// EqualizerGraphControl.xaml の相互作用ロジック
    /// </summary>
    public partial class EqualizerGraphControl
    {
        #region -- Private Fields --

        public static readonly DependencyProperty EqualizerItemProperty = DependencyProperty.Register(nameof(EqualizerItem), typeof(EqualizerItem), typeof(EqualizerGraphControl), new PropertyMetadata(default(EqualizerItem)));

        #endregion

        #region -- Public Properties --

        public EqualizerItem EqualizerItem
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (EqualizerItem)GetValue(EqualizerItemProperty);
            set => SetValue(EqualizerItemProperty, value);
        }

        #endregion

        #region -- Constructors --

        public EqualizerGraphControl()
        {
            InitializeComponent();
        }

        #endregion
    }
}
