using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using MidiUtils.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ymf825;
using Ymf825Server;

namespace Ymf825MidiDriver
{
    /// <inheritdoc cref="System.Windows.Window" />
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        #region -- Public Fields --

        public static readonly RoutedCommand NewToneCommand = new RoutedCommand();
        public static readonly RoutedCommand DeleteToneCommand = new RoutedCommand();
        public static readonly RoutedCommand DuplicateToneCommand = new RoutedCommand();
        public static readonly RoutedCommand ServerConnectionToggleCommand = new RoutedCommand();
        public static readonly RoutedCommand MidiConnectionToggleCommand = new RoutedCommand();
        public static readonly RoutedCommand ToneExportCommand = new RoutedCommand();
        public static readonly RoutedCommand NewEqualizerCommand = new RoutedCommand();
        public static readonly RoutedCommand DeleteEqualizerCommand = new RoutedCommand();
        public static readonly RoutedCommand DuplicateEqualizerCommand = new RoutedCommand();

        public static readonly DependencyProperty SpiConnectedProperty = DependencyProperty.Register(nameof(SpiConnected), typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty MidiConnectedProperty = DependencyProperty.Register(nameof(MidiConnected), typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty SelectedToneItemProperty = DependencyProperty.Register(nameof(SelectedToneItem), typeof(ToneItem), typeof(MainWindow), new PropertyMetadata(default(ToneItem)));
        public static readonly DependencyProperty SelectedEqualizerItemProperty = DependencyProperty.Register(nameof(SelectedEqualizerItem), typeof(EqualizerItem), typeof(MainWindow), new PropertyMetadata(default(EqualizerItem)));

        #endregion

        #region -- Private Fields --

        private readonly ObservableCollection<ToneItem> toneItems;
        private readonly ObservableCollection<EqualizerItem> equalizerItems;
        private readonly ObservableCollection<string> midiDevices;

        private readonly Project project = new Project();

        private MidiIn midiIn;
        private MidiDriver midiDriver;

        private string filePath;
        private string fileName;
        private bool fileChanged;
        private bool isMonitoringChanging;

        #endregion

        #region -- Public Properties --

        public ToneItem SelectedToneItem
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (ToneItem)GetValue(SelectedToneItemProperty);
            set => SetValue(SelectedToneItemProperty, value);
        }

        public EqualizerItem SelectedEqualizerItem
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (EqualizerItem)GetValue(SelectedEqualizerItemProperty);
            set => SetValue(SelectedEqualizerItemProperty, value);
        }

        public bool MidiConnected
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (bool)GetValue(MidiConnectedProperty);
            set => SetValue(MidiConnectedProperty, value);
        }

        public bool SpiConnected
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (bool)GetValue(SpiConnectedProperty);
            set => SetValue(SpiConnectedProperty, value);
        }

        public ServerWindow ServerWindow { get; }

        #endregion

        #region -- Constructors --

        public MainWindow()
        {
            InitializeComponent();
            ServerWindow = new ServerWindow();
            ServerWindow.Connected += (sender, args) => SpiConnected = true;
            ServerWindow.Disconnected += (sender, args) => SpiConnected = false;

            toneItems = new ObservableCollection<ToneItem>();
            equalizerItems = new ObservableCollection<EqualizerItem>();
            midiDevices = new ObservableCollection<string>();

            ToneListBox.DataContext = toneItems;
            EqualizerListBox.DataContext = equalizerItems;
            ComboBoxMidiDevice.DataContext = midiDevices;

            project.Tones = toneItems;
            project.Equalizers = equalizerItems;

            toneItems.CollectionChanged += ToneItemsOnCollectionChanged;
            UpdateWindowTitle();
            UpdateComboBoxMidiDevice();
        }

        #endregion

        #region -- Event Handlers --

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isMonitoringChanging = true;
            ServerWindow.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!fileChanged)
            {
                midiDriver?.Stop();
                midiDriver = null;
                midiIn?.Dispose();
                midiIn = null;
                ServerWindow.Close();
                return;
            }

            var confirm = ConfirmDiscordChanging();

            switch (confirm)
            {
                case true:
                    if (filePath == null)
                    {
                        if (ShowSaveDialog())
                            SaveProject();
                        else
                            e.Cancel = true;
                    }
                    else
                        SaveProject();
                    break;

                case null:
                    e.Cancel = true;
                    return;
            }

            midiDriver?.Stop();
            midiDriver = null;
            midiIn?.Dispose();
            midiIn = null;
            ServerWindow.Close();
        }

        private void ToneItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!isMonitoringChanging || fileChanged)
                return;

            fileChanged = true;
            UpdateWindowTitle();
        }

        private void TonePropertyChanged(object sender, RoutedEventArgs e)
        {
            if (isMonitoringChanging)
                midiDriver?.NotifyChangeTone(SelectedToneItem);

            if (isMonitoringChanging && !fileChanged)
            {
                fileChanged = true;
                UpdateWindowTitle();
            }

            e.Handled = true;
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;

            if (toolBar?.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
                overflowGrid.Visibility = Visibility.Collapsed;

            if (toolBar?.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
                mainPanelBorder.Margin = new Thickness(0);
        }

        private void ComboBoxMidiDevice_DropDownOpened(object sender, EventArgs e)
        {
            UpdateComboBoxMidiDevice();
        }

        #region Commands

        private void Command_CanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewToneCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            toneItems.Add(new ToneItem());
            ToneListBox.SelectedIndex = toneItems.Count - 1;
        }

        private void DeleteToneCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = ToneListBox.SelectedIndex;
            toneItems.RemoveAt(index);

            if (toneItems.Count == index)
                index--;

            ToneListBox.SelectedIndex = index;
        }

        private void DuplicateToneCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = ToneListBox.SelectedIndex;
            var clone = toneItems[index].DeepClone();
            clone.ProgramNumber = 0;
            clone.ProgramNumberAssigned = false;
            clone.PercussionNumber = 0;
            clone.PercussionNumberAssigned = false;
            toneItems.Add(clone);
            ToneListBox.SelectedIndex = toneItems.Count - 1;
        }

        private void NewEqualizerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            equalizerItems.Add(new EqualizerItem());
            EqualizerListBox.SelectedIndex = equalizerItems.Count - 1;
        }

        private void DeleteEqualizerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = EqualizerListBox.SelectedIndex;
            equalizerItems.RemoveAt(index);

            if (equalizerItems.Count == index)
                index--;

            EqualizerListBox.SelectedIndex = index;
        }

        private void DuplicateEqualizerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = EqualizerListBox.SelectedIndex;
            var clone = equalizerItems[index].DeepClone();
            clone.ProgramNumber = 0;
            clone.ProgramNumberAssigned = false;
            equalizerItems.Add(clone);
            EqualizerListBox.SelectedIndex = equalizerItems.Count - 1;
        }

        private void SaveCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (filePath == null)
            {
                if (ShowSaveDialog())
                    SaveProject();
            }
            else
                SaveProject();
        }

        private void SaveAsCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ShowSaveDialog())
                SaveProject();
        }

        private void OpenCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (fileChanged)
            {
                var confirm = ConfirmDiscordChanging();

                switch (confirm)
                {
                    case true:
                        if (filePath == null)
                        {
                            if (ShowSaveDialog())
                                SaveProject();
                            else
                                return;
                        }
                        else
                            SaveProject();
                        break;

                    case null:
                        return;
                }
            }

            if (ShowOpenDialog())
                LoadProject();
        }

        private void NewCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (fileChanged)
            {
                var confirm = ConfirmDiscordChanging();

                switch (confirm)
                {
                    case true:
                        if (filePath == null)
                        {
                            if (ShowSaveDialog())
                                SaveProject();
                            else
                                return;
                        }
                        else
                            SaveProject();
                        break;

                    case null:
                        return;
                }
            }

            // reset tones
            isMonitoringChanging = false;
            toneItems.Clear();
            equalizerItems.Clear();
            filePath = null;
            fileName = null;
            fileChanged = false;
            UpdateWindowTitle();
            isMonitoringChanging = true;
        }

        private void CloseCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void ServerConnectionToggleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ServerWindow.Visible = !ServerWindow.Visible;
        }

        private void MidiConnectionToggleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ComboBoxMidiDevice.SelectedIndex == -1)
                return;

            if (MidiConnected)
            {
                midiDriver.Stop();
                midiDriver = null;
                midiIn.Dispose();
                midiIn = null;

                UpdateComboBoxMidiDevice();
            }
            else
            {
                try
                {
                    midiIn = new MidiIn(ComboBoxMidiDevice.SelectedIndex);
                    midiDriver = new MidiDriver(toneItems, equalizerItems, midiIn, ServerWindow.Driver);
                    midiDriver.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"MIDIデバイス {midiDevices[ComboBoxMidiDevice.SelectedIndex]} に接続できませんでした。\n{ex.Message}",
                        "YMF825 MIDI Driver", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }
            }

            MidiConnected = !MidiConnected;
        }

        private void ToneExportCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var toneExportWindow = new ToneExportWindow { ToneItem = SelectedToneItem, Owner = this };
            toneExportWindow.ShowDialog();
        }

        private void ButtonEqualizerApply_Click(object sender, RoutedEventArgs e)
        {
            double[] coefficients;
            Equalizer equalizer;

            switch (ComboBoxFilterType.SelectedIndex)
            {
                // Lowpass
                case 0:
                    coefficients = FilterCoefficients.Lowpass((double)NumericUpDownCutoff.Value,
                        (double)NumericUpDownQ.Value);
                    break;

                // Highpass
                case 1:
                    coefficients = FilterCoefficients.Highpass((double)NumericUpDownCutoff.Value,
                        (double)NumericUpDownQ.Value);
                    break;

                // Bandpass
                case 2:
                    coefficients = FilterCoefficients.Bandpass((double)NumericUpDownCutoff.Value,
                        (double)NumericUpDownBandWidth.Value, (double)NumericUpDownQ.Value);
                    break;

                // Bandstop
                case 3:
                    coefficients = FilterCoefficients.Bandstop((double)NumericUpDownCutoff.Value,
                        (double)NumericUpDownBandWidth.Value);
                    break;

                // Lowshelf
                case 4:
                    coefficients = FilterCoefficients.LowShelf((double)NumericUpDownCutoff.Value,
                        (double)NumericUpDownGain.Value, (double)NumericUpDownQ.Value);
                    break;

                // Highshelf
                case 5:
                    coefficients = FilterCoefficients.HighShelf((double)NumericUpDownCutoff.Value,
                        (double)NumericUpDownGain.Value, (double)NumericUpDownQ.Value);
                    break;

                // Peaking
                case 6:
                    coefficients = FilterCoefficients.Peaking((double)NumericUpDownCutoff.Value,
                        (double)NumericUpDownBandWidth.Value, (double)NumericUpDownGain.Value);
                    break;

                // Allpass
                case 7:
                    coefficients = FilterCoefficients.Allpass((double)NumericUpDownCutoff.Value,
                        (double)NumericUpDownQ.Value);
                    break;

                default:
                    coefficients = new[] { 1.0, 0.0, 0.0, 0.0, 0.0 };
                    break;
            }

            switch (int.Parse(((Button)sender).Tag.ToString()))
            {
                case 0:
                    equalizer = SelectedEqualizerItem.Equalizer0;
                    break;

                case 1:
                    equalizer = SelectedEqualizerItem.Equalizer1;
                    break;

                case 2:
                    equalizer = SelectedEqualizerItem.Equalizer2;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            for (var i = 0; i < 5; i++)
                equalizer[i] = coefficients[i];

            var index = EqualizerListBox.SelectedIndex;
            EqualizerListBox.SelectedIndex = -1;
            EqualizerListBox.SelectedIndex = index;
        }

        #endregion

        #endregion

        #region -- Private Methods --

        private void UpdateWindowTitle()
        {
            Title = $"{(fileChanged ? "*" : "")}{fileName ?? "untitled"} ― YMF825 MIDI Driver";
        }

        private bool ShowSaveDialog()
        {
            var dialog = new SaveFileDialog()
            {
                Title = "名前を付けて保存",
                Filter = "トーンプロジェクト (*.json)|*.json"
            };

            if (dialog.ShowDialog() != true)
                return false;

            filePath = dialog.FileName;
            fileName = Path.GetFileName(filePath);
            return true;
        }

        private bool ShowOpenDialog()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "開く",
                Filter = "トーンプロジェクト (*.json)|*.json"
            };

            if (dialog.ShowDialog() != true)
                return false;

            filePath = dialog.FileName;
            fileName = Path.GetFileName(filePath);
            return true;
        }

        private void SaveProject()
        {
            var serializeObject = JsonConvert.SerializeObject(project, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            File.WriteAllText(filePath, serializeObject);

            fileChanged = false;
            UpdateWindowTitle();
        }

        private void LoadProject()
        {
            var serializeText = File.ReadAllText(filePath);
            var importedProject = JsonConvert.DeserializeObject<Project>(serializeText);

            isMonitoringChanging = false;

            toneItems.Clear();

            foreach (var item in importedProject.Tones)
                toneItems.Add(item);

            equalizerItems.Clear();

            foreach (var item in importedProject.Equalizers)
                equalizerItems.Add(item);

            importedProject.Tones = toneItems;
            importedProject.Equalizers = equalizerItems;

            fileChanged = false;
            UpdateWindowTitle();
            isMonitoringChanging = true;
        }

        private bool? ConfirmDiscordChanging()
        {
            var result = MessageBox.Show(
                $"{fileName ?? "untitled"} への変更内容を保存しますか?",
                "YMF825 MIDI Driver",
                MessageBoxButton.YesNoCancel, MessageBoxImage.None, MessageBoxResult.Yes);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    return true;

                case MessageBoxResult.No:
                    return false;

                default:
                    return null;
            }
        }

        private void UpdateComboBoxMidiDevice()
        {
            midiDevices.Clear();

            foreach (var midiDevice in MidiIn.InputDeviceNames)
                midiDevices.Add(midiDevice);

            if (midiDevices.Count > 0)
                ComboBoxMidiDevice.SelectedIndex = 0;
        }

        #endregion
    }
}
