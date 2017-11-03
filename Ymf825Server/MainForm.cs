using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using RegisterMap;
using Ymf825;

namespace Ymf825Server
{
    internal partial class MainForm : Form
    {
        #region -- Private Fields --

        private readonly MapRenderer registerMap;
        private readonly MapRenderer[] toneParameterRegisterMap = new MapRenderer[16];
        private readonly PictureBox[] tonePrameterPictureBoxes;
        private SerialPort serialPort;
        private Ymf825Client ymf825Client;
        private ServiceHost serviceHost;
        private bool connected;

        #endregion

        #region -- Constructors --

        public MainForm()
        {
            InitializeComponent();

            registerMap = new MapRenderer(0x81)
            {
                EnableUnusedMarking = true,
                DecayMode = true
            };
            pictureBox1.BackgroundImage = registerMap.Bitmap;

            tonePrameterPictureBoxes = new[]
            {
                pictureBox_tone1,
                pictureBox_tone2,
                pictureBox_tone3,
                pictureBox_tone4,
                pictureBox_tone5,
                pictureBox_tone6,
                pictureBox_tone7,
                pictureBox_tone8,
                pictureBox_tone9,
                pictureBox_tone10,
                pictureBox_tone11,
                pictureBox_tone12,
                pictureBox_tone13,
                pictureBox_tone14,
                pictureBox_tone15,
                pictureBox_tone16,
            };

            for (var i = 0; i < 16; i++)
            {
                toneParameterRegisterMap[i] = new MapRenderer(30)
                {
                    EnableUnusedMarking = true,
                    DecayMode = true
                };

                tonePrameterPictureBoxes[i].BackgroundImage = toneParameterRegisterMap[i].Bitmap;
            }

            UpdateControlByConnected();
            RefreshDeviceList();
        }

        #endregion

        #region -- Event Handlers --

        private void timer_registerMap_Tick(object sender, EventArgs e)
        {
            registerMap.Draw();
            pictureBox1.Refresh();

            for (var i = 0; i < 16; i++)
            {
                toneParameterRegisterMap[i].Draw();
                tonePrameterPictureBoxes[i].Refresh();
            }
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

            registerMap.ClearAll();

            for (var i = 0; i < 16; i++)
                toneParameterRegisterMap[i].ClearAll();

            ymf825Client.ResetHardware();
        }

        private void toolStripButton_softReset_Click(object sender, EventArgs e)
        {
            if (!connected)
                return;

            registerMap.ClearAll();

            for (var i = 0; i < 16; i++)
                toneParameterRegisterMap[i].ClearAll();

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
            var baudRate = int.Parse(toolStripComboBox_baudRate.SelectedItem.ToString());
            serialPort = new SerialPort(toolStripComboBox_deviceList.SelectedItem.ToString(), baudRate, Parity.None);
            serialPort.Open();

            ymf825Client = new Ymf825Client(serialPort);
            ymf825Client.DataWrote += (sender, args) =>
            {
                registerMap.SetData(args.Address, args.Data);
            };
            ymf825Client.DataBurstWrote += (sender, args) =>
            {
                if (args.Data.Count <= 0)
                    return;

                registerMap.SetData(args.Address, args.Data.Last());

                var toneNumber = args.Data[0] - 0x80;

                if (toneNumber < 0 || toneNumber > 16 || args.Data.Count < toneNumber * 30 + 5)
                {
                    Console.WriteLine($"Invalid BurstWrite Data - Tone Number: {toneNumber}, Data Size: {args.Data.Count} (required {toneNumber * 30 + 5})");
                    return;
                }

                for (var i = 0; i < toneNumber; i++)
                    for (var j = 0; j < 30; j++)
                        toneParameterRegisterMap[i].SetData(j, args.Data[i * 30 + j + 1]);
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
            toolStripComboBox_baudRate.Enabled = !connected;
            toolStripButton_connect.Enabled = !connected;
            toolStripButton_disconnect.Enabled = connected;
            toolStripButton_reset.Enabled = connected;
            toolStripButton_softReset.Enabled = connected;
            button_CommTest.Enabled = connected;

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
            toolStripComboBox_baudRate.SelectedItem = "115200";
        }

        private void communicationTest_Click(object sender, EventArgs e)
        {
            button_CommTest.Enabled = false;
            var driver = new Ymf825Driver(ymf825Client);
            textBox1.Text = string.Empty;
            var logger = new ControlWriter(textBox1);
            logger.WriteLine("Start testing.");
            FillZeroToComm(driver, logger);
            SequenceToComm(driver, logger);
            RandomToComm(driver, logger);
            logger.WriteLine("Finished.");
            button_CommTest.Enabled = true;
        }


        private static void FillZeroToComm(Ymf825Driver driver, TextWriter logger)
        {
            var commTest = new CommTest(256, driver);
            logger.Write("  Zero Fill ... ");

            for (var i = 0; i < 256; i++)
                commTest.Test(0);

            commTest.RemoveEvent();
            var writeLossCount = commTest.WriteLog.Count(v => v != 0);
            var readLossCount = commTest.ReadLog.Count(v => v != 0);

            if (commTest.WriteCount != 256 && writeLossCount > 0)
            {
                logger.WriteLine("NG");
                logger.WriteLine($"    -> Write Error - count: {commTest.WriteCount}, loss: {writeLossCount}");
                return;
            }

            if (commTest.ReadCount != 512 && readLossCount > 0)
            {
                logger.WriteLine("NG");
                logger.WriteLine($"    -> Read Error - count: {commTest.ReadCount}, loss: {readLossCount}");
                return;
            }

            logger.WriteLine("OK");
        }

        private static void SequenceToComm(Ymf825Driver driver, TextWriter logger)
        {
            var commTest = new CommTest(256, driver);
            logger.Write("  Sequence  ... ");

            for (var i = 0; i < 256; i++)
                commTest.Test((byte)i);

            commTest.RemoveEvent();
            var writeLossCount = commTest.WriteLog.Select((v, i) => v == i).Count(c => !c);
            var readLossCount = commTest.ReadLog.Select((v, i) => v == (int)Math.Floor(i / 2.0)).Count(c => !c);

            if (commTest.WriteCount != 256 && writeLossCount > 0)
            {
                logger.WriteLine("NG");
                logger.WriteLine($"    -> Write Error - count: {commTest.WriteCount}, loss: {writeLossCount}");
                return;
            }

            if (commTest.ReadCount != 512 && readLossCount > 0)
            {
                logger.WriteLine("NG");
                logger.WriteLine($"    -> Read Error - count: {commTest.ReadCount}, loss: {readLossCount}");
                return;
            }

            logger.WriteLine("OK");
        }

        private static void RandomToComm(Ymf825Driver driver, TextWriter logger)
        {
            var commTest = new CommTest(1024, driver);
            var randomLog = new byte[1024];
            var random = new Random();
            logger.Write("  Random    ... ");

            for (var i = 0; i < 1024; i++)
            {
                var value = (byte)random.Next(0, 256);
                randomLog[i] = value;
                commTest.Test(value);
            }

            commTest.RemoveEvent();
            var writeLossCount = commTest.WriteLog.Select((v, i) => v == randomLog[i]).Count(c => !c);
            var readLossCount = commTest.ReadLog.Select((v, i) => v == randomLog[(int)Math.Floor(i / 2.0)]).Count(c => !c);

            if (commTest.WriteCount != 1024 && writeLossCount > 0)
            {
                logger.WriteLine("NG");
                logger.WriteLine($"    -> Write Error - count: {commTest.WriteCount}, loss: {writeLossCount}");
                return;
            }

            if (commTest.ReadCount != 2048 && readLossCount > 0)
            {
                logger.WriteLine("NG");
                logger.WriteLine($"    -> Read Error - count: {commTest.ReadCount}, loss: {readLossCount}");
                return;
            }

            logger.WriteLine("OK");
        }

        #endregion

        private class CommTest
        {
            private readonly byte[] writeLog;
            private readonly byte[] readLog;
            private readonly EventHandler<DataTransferedEventArgs> writeCheck;
            private readonly EventHandler<DataTransferedEventArgs> readCheck;
            private readonly Ymf825Driver driver;

            public IEnumerable<byte> WriteLog => writeLog;
            public IEnumerable<byte> ReadLog => readLog;
            public int WriteCount { get; private set; }
            public int ReadCount { get; private set; }

            public CommTest(int testCount, Ymf825Driver driver)
            {
                writeLog = new byte[testCount];
                readLog = new byte[testCount * 2];

                writeCheck = (sender, args) =>
                {
                    writeLog[WriteCount] = args.Data;
                    WriteCount++;
                };
                readCheck = (sender, args) =>
                {
                    readLog[ReadCount] = args.Data;
                    ReadCount++;
                };

                this.driver = driver;
                driver.Client.DataWrote += writeCheck;
                driver.Client.DataRead += readCheck;
            }

            public void RemoveEvent()
            {
                driver.Client.DataWrote -= writeCheck;
                driver.Client.DataRead -= readCheck;
            }

            public void Test(byte value)
            {
                driver.SetSoftwareTestCommunication(value);
                driver.GetSoftwareTestCommunication(TargetDevice.Ymf825Board0);
                driver.GetSoftwareTestCommunication(TargetDevice.Ymf825Board1);
            }
        }
        
        private class ControlWriter : TextWriter
        {
            private readonly Control textbox;

            public override Encoding Encoding => Encoding.UTF8;

            public ControlWriter(Control textbox)
            {
                this.textbox = textbox;
            }

            public override void Write(char value)
            {
                textbox.Text += value;
            }

            public override void Write(string value)
            {
                textbox.Text += value;
            }
        }
    }
}
