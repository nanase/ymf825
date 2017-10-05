using System;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using RegisterMap;
using Ymf825;

namespace Ymf825Server
{
    internal partial class MainForm : Form
    {
        #region -- Private Fields --

        private readonly MapRenderer registerMap;
        private Spi spiDevice;
        private Spi.ChannelConfig spiChannelConfig;
        private Ymf825Client spiService;
        private ServiceHost serviceHost;
        private bool connected;

        #endregion

        #region -- Constructors --

        public MainForm()
        {
            InitializeComponent();

            registerMap = new MapRenderer
            {
                EnableUnusedMarking = true,
                DecayMode = true
            };
            pictureBox1.BackgroundImage = registerMap.Bitmap;

            UpdateControlByConnected();
            RefreshDeviceList();
        }

        #endregion

        #region -- Event Handlers --

        private void timer_registerMap_Tick(object sender, EventArgs e)
        {
            registerMap.Draw();
            pictureBox1.Refresh();
        }

        private void timer_stat_Tick(object sender, EventArgs e)
        {
            label_writeBytes.Text = spiService.WriteBytesTotal.ToString("N0");
            label_burstWriteBytes.Text = spiService.BurstWriteBytesTotal.ToString("N0");
            label_readBytes.Text = spiService.ReadBytesTotal.ToString("N0");

            label_writeCommands.Text = spiService.WriteCommandsTotal.ToString("N0");
            label_burstWriteCommands.Text = spiService.BurstWriteCommandsTotal.ToString("N0");
            label_readCommands.Text = spiService.ReadCommandsTotal.ToString("N0");

            label_failedWriteBytes.Text = spiService.FailedWriteBytesTotal.ToString("N0");
            label_failedBurstWriteBytes.Text = spiService.FailedBurstWriteBytesTotal.ToString("N0");
            label_failedReadBytes.Text = spiService.FailedReadBytesTotal.ToString("N0");

            label_writeError.Text = spiService.WriteErrorTotal.ToString("N0");
            label_burstWriteError.Text = spiService.BurstWriteErrorTotal.ToString("N0");
            label_readError.Text = spiService.ReadErrorTotal.ToString("N0");
        }

        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            if (connected)
                return;

            RefreshDeviceList();
        }

        private void toolStripButton_connect_Click(object sender, EventArgs e)
        {
            if (connected)
                return;

            ConnectDevice();
            UpdateSpiDeviceInfoLabel();
            UpdateControlByConnected();
        }

        private void toolStripButton_disconnect_Click(object sender, EventArgs e)
        {
            if (!connected)
                return;

            DisconnectDevice();
            UpdateControlByConnected();
            UpdateSpiDeviceInfoLabel(true);
        }

        private void toolStripButton_reset_Click(object sender, EventArgs e)
        {
            if (!connected)
                return;

            spiService.SendReset();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectDevice();
        }

        #endregion

        #region -- Private Methods --

        private void ConnectDevice()
        {
            spiChannelConfig = new Spi.ChannelConfig(10000000,
                latencyTimer: 1,
                configOptions: Spi.ChannelConfigOptions.CsActiveLow | Spi.ChannelConfigOptions.CsDbus7 |
                               Spi.ChannelConfigOptions.Mode0);
            spiDevice = new Spi(toolStripComboBox_deviceList.SelectedIndex, spiChannelConfig);
            spiService = new Ymf825Client(spiDevice);
            spiService.DataWrote += (sender, args) =>
            {
                registerMap.SetData(args.Address, args.Data);
            };
            spiService.DataBurstWrote += (sender, args) =>
            {
                if (args.Data.Count > 0)
                    registerMap.SetData(args.Address, args.Data.Last());
            };
            serviceHost = new ServiceHost(spiService, new Uri(Ymf825Client.ServiceUri));

            try
            {
                serviceHost.AddServiceEndpoint(typeof(IYmf825Client), new NetNamedPipeBinding(), Ymf825Client.ServiceName);
                serviceHost.Open();
            }
            catch (AddressAlreadyInUseException)
            {
                MessageBox.Show("既にサービスは起動しています。");
                DisconnectDevice();
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show("不明なエラーが発生しました。\n\n" + e);
                DisconnectDevice();
                return;
            }

            connected = true;
            spiService.SendReset();
        }

        private void DisconnectDevice()
        {
            if (serviceHost != null && serviceHost.State == CommunicationState.Opened)
                serviceHost.Close();

            serviceHost = null;
            spiService = null;

            try
            {
                spiDevice?.Dispose();
            }
            catch (InvalidOperationException)
            {
                // もみ消して破棄できたことにする
            }

            spiDevice = null;
            connected = false;
        }

        private void UpdateControlByConnected()
        {
            toolStripButton_refresh.Enabled = !connected;
            toolStripComboBox_deviceList.Enabled = !connected;
            toolStripButton_connect.Enabled = !connected;
            toolStripButton_disconnect.Enabled = connected;
            toolStripButton_reset.Enabled = connected;

            timer_stat.Enabled = connected;
        }

        private void UpdateSpiDeviceInfoLabel(bool reset = false)
        {
            if (reset)
            {
                label_spiClock.Text = "";
                label_ssPinout.Text = "";
                label_ssActiveOutput.Text = "";
                return;
            }

            label_spiClock.Text = spiChannelConfig.ClockRate.ToString("N3");

            switch ((Spi.ChannelConfigOptions)((int)spiChannelConfig.ConfigOptions & 0b011100))
            {
                case Spi.ChannelConfigOptions.CsDbus3:
                    label_ssPinout.Text = "D3";
                    break;
                case Spi.ChannelConfigOptions.CsDbus4:
                    label_ssPinout.Text = "D4";
                    break;
                case Spi.ChannelConfigOptions.CsDbus5:
                    label_ssPinout.Text = "D5";
                    break;
                case Spi.ChannelConfigOptions.CsDbus6:
                    label_ssPinout.Text = "D6";
                    break;
                case Spi.ChannelConfigOptions.CsDbus7:
                    label_ssPinout.Text = "D7";
                    break;
                default:
                    label_ssPinout.Text = "unknown";
                    break;
            }

            switch ((Spi.ChannelConfigOptions)((int)spiChannelConfig.ConfigOptions & 0b100000))
            {
                case Spi.ChannelConfigOptions.CsActiveHigh:
                    label_ssActiveOutput.Text = "H (5V)";
                    break;
                case Spi.ChannelConfigOptions.CsActiveLow:
                    label_ssActiveOutput.Text = "L (GND)";
                    break;
                default:
                    label_ssActiveOutput.Text = "unknown";
                    break;
            }
        }

        private void RefreshDeviceList()
        {
            toolStripComboBox_deviceList.Items.Clear();
            var deviceInfoList = Spi.GetDeviceInfoList();
            var deviceInfoArray = deviceInfoList as Spi.DeviceInfo[] ?? deviceInfoList.ToArray();

            foreach (var deviceInfo in deviceInfoArray)
            {
                toolStripComboBox_deviceList.Items.Add(deviceInfo);
            }

            if (deviceInfoArray.Length > 0)
            {
                toolStripComboBox_deviceList.Enabled = true;
                toolStripButton_connect.Enabled = true;
            }
            else
            {
                toolStripComboBox_deviceList.Items.Add("デバイスが見つかりません");
                toolStripComboBox_deviceList.Enabled = false;
                toolStripButton_connect.Enabled = false;
            }

            toolStripComboBox_deviceList.SelectedIndex = 0;
        }

        #endregion
    }
}
