namespace Ymf825Server
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripComboBox_deviceList = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButton_connect = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_disconnect = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_reset = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_readError = new System.Windows.Forms.Label();
            this.label_burstWriteError = new System.Windows.Forms.Label();
            this.label_writeError = new System.Windows.Forms.Label();
            this.label_failedReadBytes = new System.Windows.Forms.Label();
            this.label_failedBurstWriteBytes = new System.Windows.Forms.Label();
            this.label_failedWriteBytes = new System.Windows.Forms.Label();
            this.label_readCommands = new System.Windows.Forms.Label();
            this.label_burstWriteCommands = new System.Windows.Forms.Label();
            this.label_writeCommands = new System.Windows.Forms.Label();
            this.label_readBytes = new System.Windows.Forms.Label();
            this.label_burstWriteBytes = new System.Windows.Forms.Label();
            this.label_writeBytes = new System.Windows.Forms.Label();
            this.label_serialPortBaudRate = new System.Windows.Forms.Label();
            this.label_serialPortName = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timer_registerMap = new System.Windows.Forms.Timer(this.components);
            this.timer_stat = new System.Windows.Forms.Timer(this.components);
            this.toolStripComboBox_baudRate = new System.Windows.Forms.ToolStripComboBox();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_refresh,
            this.toolStripComboBox_deviceList,
            this.toolStripComboBox_baudRate,
            this.toolStripButton_connect,
            this.toolStripButton_disconnect,
            this.toolStripSeparator1,
            this.toolStripButton_reset});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(509, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_refresh
            // 
            this.toolStripButton_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_refresh.Image = global::Ymf825Server.Properties.Resources.arrow_refresh;
            this.toolStripButton_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refresh.Name = "toolStripButton_refresh";
            this.toolStripButton_refresh.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_refresh.Text = "デバイスリストを更新";
            this.toolStripButton_refresh.Click += new System.EventHandler(this.toolStripButton_refresh_Click);
            // 
            // toolStripComboBox_deviceList
            // 
            this.toolStripComboBox_deviceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_deviceList.Name = "toolStripComboBox_deviceList";
            this.toolStripComboBox_deviceList.Size = new System.Drawing.Size(200, 25);
            // 
            // toolStripButton_connect
            // 
            this.toolStripButton_connect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_connect.Image = global::Ymf825Server.Properties.Resources.connect;
            this.toolStripButton_connect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_connect.Name = "toolStripButton_connect";
            this.toolStripButton_connect.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_connect.Text = "接続";
            this.toolStripButton_connect.Click += new System.EventHandler(this.toolStripButton_connect_Click);
            // 
            // toolStripButton_disconnect
            // 
            this.toolStripButton_disconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_disconnect.Image = global::Ymf825Server.Properties.Resources.disconnect;
            this.toolStripButton_disconnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_disconnect.Name = "toolStripButton_disconnect";
            this.toolStripButton_disconnect.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_disconnect.Text = "切断";
            this.toolStripButton_disconnect.Click += new System.EventHandler(this.toolStripButton_disconnect_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_reset
            // 
            this.toolStripButton_reset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_reset.Image = global::Ymf825Server.Properties.Resources.lightning;
            this.toolStripButton_reset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_reset.Name = "toolStripButton_reset";
            this.toolStripButton_reset.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_reset.Text = "リセット送信";
            this.toolStripButton_reset.Click += new System.EventHandler(this.toolStripButton_reset_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(509, 320);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_readError);
            this.groupBox1.Controls.Add(this.label_burstWriteError);
            this.groupBox1.Controls.Add(this.label_writeError);
            this.groupBox1.Controls.Add(this.label_failedReadBytes);
            this.groupBox1.Controls.Add(this.label_failedBurstWriteBytes);
            this.groupBox1.Controls.Add(this.label_failedWriteBytes);
            this.groupBox1.Controls.Add(this.label_readCommands);
            this.groupBox1.Controls.Add(this.label_burstWriteCommands);
            this.groupBox1.Controls.Add(this.label_writeCommands);
            this.groupBox1.Controls.Add(this.label_readBytes);
            this.groupBox1.Controls.Add(this.label_burstWriteBytes);
            this.groupBox1.Controls.Add(this.label_writeBytes);
            this.groupBox1.Controls.Add(this.label_serialPortBaudRate);
            this.groupBox1.Controls.Add(this.label_serialPortName);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(228, 308);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "接続情報/統計";
            // 
            // label_readError
            // 
            this.label_readError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_readError.Location = new System.Drawing.Point(73, 286);
            this.label_readError.Name = "label_readError";
            this.label_readError.Size = new System.Drawing.Size(149, 13);
            this.label_readError.TabIndex = 29;
            this.label_readError.Text = "0";
            this.label_readError.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_burstWriteError
            // 
            this.label_burstWriteError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_burstWriteError.Location = new System.Drawing.Point(102, 268);
            this.label_burstWriteError.Name = "label_burstWriteError";
            this.label_burstWriteError.Size = new System.Drawing.Size(120, 13);
            this.label_burstWriteError.TabIndex = 28;
            this.label_burstWriteError.Text = "0";
            this.label_burstWriteError.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_writeError
            // 
            this.label_writeError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_writeError.Location = new System.Drawing.Point(75, 250);
            this.label_writeError.Name = "label_writeError";
            this.label_writeError.Size = new System.Drawing.Size(147, 13);
            this.label_writeError.TabIndex = 27;
            this.label_writeError.Text = "0";
            this.label_writeError.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_failedReadBytes
            // 
            this.label_failedReadBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_failedReadBytes.Location = new System.Drawing.Point(109, 228);
            this.label_failedReadBytes.Name = "label_failedReadBytes";
            this.label_failedReadBytes.Size = new System.Drawing.Size(113, 13);
            this.label_failedReadBytes.TabIndex = 26;
            this.label_failedReadBytes.Text = "0";
            this.label_failedReadBytes.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_failedBurstWriteBytes
            // 
            this.label_failedBurstWriteBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_failedBurstWriteBytes.Location = new System.Drawing.Point(138, 210);
            this.label_failedBurstWriteBytes.Name = "label_failedBurstWriteBytes";
            this.label_failedBurstWriteBytes.Size = new System.Drawing.Size(84, 13);
            this.label_failedBurstWriteBytes.TabIndex = 25;
            this.label_failedBurstWriteBytes.Text = "0";
            this.label_failedBurstWriteBytes.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_failedWriteBytes
            // 
            this.label_failedWriteBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_failedWriteBytes.Location = new System.Drawing.Point(111, 192);
            this.label_failedWriteBytes.Name = "label_failedWriteBytes";
            this.label_failedWriteBytes.Size = new System.Drawing.Size(111, 13);
            this.label_failedWriteBytes.TabIndex = 24;
            this.label_failedWriteBytes.Text = "0";
            this.label_failedWriteBytes.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_readCommands
            // 
            this.label_readCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_readCommands.Location = new System.Drawing.Point(103, 170);
            this.label_readCommands.Name = "label_readCommands";
            this.label_readCommands.Size = new System.Drawing.Size(119, 13);
            this.label_readCommands.TabIndex = 23;
            this.label_readCommands.Text = "0";
            this.label_readCommands.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_burstWriteCommands
            // 
            this.label_burstWriteCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_burstWriteCommands.Location = new System.Drawing.Point(132, 152);
            this.label_burstWriteCommands.Name = "label_burstWriteCommands";
            this.label_burstWriteCommands.Size = new System.Drawing.Size(90, 13);
            this.label_burstWriteCommands.TabIndex = 22;
            this.label_burstWriteCommands.Text = "0";
            this.label_burstWriteCommands.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_writeCommands
            // 
            this.label_writeCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_writeCommands.Location = new System.Drawing.Point(105, 134);
            this.label_writeCommands.Name = "label_writeCommands";
            this.label_writeCommands.Size = new System.Drawing.Size(117, 13);
            this.label_writeCommands.TabIndex = 21;
            this.label_writeCommands.Text = "0";
            this.label_writeCommands.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_readBytes
            // 
            this.label_readBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_readBytes.Location = new System.Drawing.Point(75, 112);
            this.label_readBytes.Name = "label_readBytes";
            this.label_readBytes.Size = new System.Drawing.Size(147, 13);
            this.label_readBytes.TabIndex = 20;
            this.label_readBytes.Text = "0";
            this.label_readBytes.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_burstWriteBytes
            // 
            this.label_burstWriteBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_burstWriteBytes.Location = new System.Drawing.Point(100, 94);
            this.label_burstWriteBytes.Name = "label_burstWriteBytes";
            this.label_burstWriteBytes.Size = new System.Drawing.Size(122, 13);
            this.label_burstWriteBytes.TabIndex = 19;
            this.label_burstWriteBytes.Text = "0";
            this.label_burstWriteBytes.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_writeBytes
            // 
            this.label_writeBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_writeBytes.Location = new System.Drawing.Point(75, 76);
            this.label_writeBytes.Name = "label_writeBytes";
            this.label_writeBytes.Size = new System.Drawing.Size(147, 13);
            this.label_writeBytes.TabIndex = 18;
            this.label_writeBytes.Text = "0";
            this.label_writeBytes.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_serialPortBaudRate
            // 
            this.label_serialPortBaudRate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_serialPortBaudRate.Location = new System.Drawing.Point(135, 36);
            this.label_serialPortBaudRate.Name = "label_serialPortBaudRate";
            this.label_serialPortBaudRate.Size = new System.Drawing.Size(87, 13);
            this.label_serialPortBaudRate.TabIndex = 16;
            this.label_serialPortBaudRate.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_serialPortName
            // 
            this.label_serialPortName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_serialPortName.Location = new System.Drawing.Point(97, 18);
            this.label_serialPortName.Name = "label_serialPortName";
            this.label_serialPortName.Size = new System.Drawing.Size(125, 13);
            this.label_serialPortName.TabIndex = 15;
            this.label_serialPortName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 268);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(90, 13);
            this.label10.TabIndex = 14;
            this.label10.Text = "BurstWrite error";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 210);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(126, 13);
            this.label11.TabIndex = 13;
            this.label11.Text = "Failed BurstWrite bytes";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 192);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(99, 13);
            this.label12.TabIndex = 9;
            this.label12.Text = "Failed Write bytes";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 286);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(61, 13);
            this.label13.TabIndex = 12;
            this.label13.Text = "Read error";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 228);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(97, 13);
            this.label14.TabIndex = 10;
            this.label14.Text = "Failed Read bytes";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 250);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(63, 13);
            this.label15.TabIndex = 11;
            this.label15.Text = "Write error";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 36);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(126, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Baud Rate (per second)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Serial Port";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 152);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "BurstWrite commands";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 94);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "BurstWrite bytes";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Write bytes";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Read commands";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 112);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Read bytes";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Write commands";
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.pictureBox1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(246, 6);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(257, 308);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "レジスタマップ";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 18);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(251, 287);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // timer_registerMap
            // 
            this.timer_registerMap.Enabled = true;
            this.timer_registerMap.Interval = 30;
            this.timer_registerMap.Tick += new System.EventHandler(this.timer_registerMap_Tick);
            // 
            // timer_stat
            // 
            this.timer_stat.Interval = 500;
            this.timer_stat.Tick += new System.EventHandler(this.timer_stat_Tick);
            // 
            // toolStripComboBox_baudRate
            // 
            this.toolStripComboBox_baudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_baudRate.Items.AddRange(new object[] {
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "28800",
            "38400",
            "57600",
            "115200",
            "256000",
            "460000",
            "512000",
            "1000000",
            "2000000",
            "3000000"});
            this.toolStripComboBox_baudRate.Name = "toolStripComboBox_baudRate";
            this.toolStripComboBox_baudRate.Size = new System.Drawing.Size(121, 25);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 345);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "YMF825Board Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Timer timer_registerMap;
        private System.Windows.Forms.ToolStripButton toolStripButton_refresh;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_deviceList;
        private System.Windows.Forms.ToolStripButton toolStripButton_connect;
        private System.Windows.Forms.ToolStripButton toolStripButton_disconnect;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_reset;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label_readError;
        private System.Windows.Forms.Label label_burstWriteError;
        private System.Windows.Forms.Label label_writeError;
        private System.Windows.Forms.Label label_failedReadBytes;
        private System.Windows.Forms.Label label_failedBurstWriteBytes;
        private System.Windows.Forms.Label label_failedWriteBytes;
        private System.Windows.Forms.Label label_readCommands;
        private System.Windows.Forms.Label label_burstWriteCommands;
        private System.Windows.Forms.Label label_writeCommands;
        private System.Windows.Forms.Label label_readBytes;
        private System.Windows.Forms.Label label_burstWriteBytes;
        private System.Windows.Forms.Label label_writeBytes;
        private System.Windows.Forms.Label label_serialPortBaudRate;
        private System.Windows.Forms.Label label_serialPortName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer_stat;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_baudRate;
    }
}

