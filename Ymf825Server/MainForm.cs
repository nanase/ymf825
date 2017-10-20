using System;
using System.IO.Ports;
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
        private SerialPort serialPort;
        private Ymf825Client ymf825Client;
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
            label_writeBytes.Text = ymf825Client.WriteBytesTotal.ToString("N0");
            label_burstWriteBytes.Text = ymf825Client.BurstWriteBytesTotal.ToString("N0");
            label_readBytes.Text = ymf825Client.ReadBytesTotal.ToString("N0");

            label_writeCommands.Text = ymf825Client.WriteCommandsTotal.ToString("N0");
            label_burstWriteCommands.Text = ymf825Client.BurstWriteCommandsTotal.ToString("N0");
            label_readCommands.Text = ymf825Client.ReadCommandsTotal.ToString("N0");

            label_failedWriteBytes.Text = ymf825Client.FailedWriteBytesTotal.ToString("N0");
            label_failedBurstWriteBytes.Text = ymf825Client.FailedBurstWriteBytesTotal.ToString("N0");
            label_failedReadBytes.Text = ymf825Client.FailedReadBytesTotal.ToString("N0");

            label_writeError.Text = ymf825Client.WriteErrorTotal.ToString("N0");
            label_burstWriteError.Text = ymf825Client.BurstWriteErrorTotal.ToString("N0");
            label_readError.Text = ymf825Client.ReadErrorTotal.ToString("N0");
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

            ymf825Client.ResetHardware();
            ymf825Client.ResetSoftware();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectDevice();
        }

        #endregion

        #region -- Private Methods --

        private void ConnectDevice()
        {
            serialPort = new SerialPort(toolStripComboBox_deviceList.SelectedItem.ToString(), 1000000, Parity.None);
            serialPort.Open();

            ymf825Client = new Ymf825Client(serialPort);
            ymf825Client.DataWrote += (sender, args) =>
            {
                registerMap.SetData(args.Address, args.Data);
            };
            ymf825Client.DataBurstWrote += (sender, args) =>
            {
                if (args.Data.Count > 0)
                    registerMap.SetData(args.Address, args.Data.Last());
            };
            serviceHost = new ServiceHost(ymf825Client, new Uri(Ymf825Client.ServiceUri));

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
            ymf825Client.SetTarget(TargetDevice.Ymf825Board0 | TargetDevice.Ymf825Board1);
        }

        private void DisconnectDevice()
        {
            if (serviceHost != null && serviceHost.State == CommunicationState.Opened)
                serviceHost.Close();

            serviceHost = null;
            ymf825Client = null;

            serialPort?.Dispose();
            serialPort = null;
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
                label_serialPortName.Text = "";
                label_serialPortBaudRate.Text = "";
                return;
            }

            label_serialPortName.Text = serialPort.PortName;
            label_serialPortBaudRate.Text = serialPort.BaudRate.ToString("N0");
        }

        private void RefreshDeviceList()
        {
            toolStripComboBox_deviceList.Items.Clear();
            var deviceInfoArray = SerialPort.GetPortNames();

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
                toolStripComboBox_deviceList.Items.Add("ポートが見つかりません");
                toolStripComboBox_deviceList.Enabled = false;
                toolStripButton_connect.Enabled = false;
            }

            toolStripComboBox_deviceList.SelectedIndex = 0;
        }

        #endregion
    }
}
