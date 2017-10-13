using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace Ymf825
{
    [ServiceContract]
    public interface IYmf825Client
    {
        #region -- Methods --

        [OperationContract]
        void WriteGpio(byte direction, byte value);

        [OperationContract]
        byte ReadGpio();

        [OperationContract]
        int Write(byte address, byte data);

        [OperationContract]
        int WriteBuffer(byte[] buffer, int offset, int count);

        [OperationContract]
        int BurstWriteBytes(byte address, params byte[] data);

        [OperationContract]
        int ReadByte(byte address);

        [OperationContract]
        void SendReset();

        [OperationContract]
        void SetTarget(params int[] targetIndices);

        [OperationContract]
        void ChangeTarget(int newTargetIndex);

        #endregion
    }

    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Single,
        UseSynchronizationContext = false)]
    public class Ymf825Client : IYmf825Client
    {
        #region -- Public Fields --

        public const string ServiceUri = "net.pipe://localhost/nanase.cc";
        public const string ServiceName = "YMF825Board_SPI_Server";
        public const string ServiceAddress = ServiceUri + "/" + ServiceName;

        #endregion

        #region -- Private Fields --

        private readonly Spi spiDevice;
        private int[] targetIndices;
        private int currentTargetIndex;

        #endregion

        #region -- Public Properties --

        public long WriteBytesTotal { get; private set; }

        public long BurstWriteBytesTotal { get; private set; }

        public long ReadBytesTotal { get; private set; }

        public long WriteCommandsTotal { get; private set; }

        public long BurstWriteCommandsTotal { get; private set; }

        public long ReadCommandsTotal { get; private set; }

        public long FailedWriteBytesTotal { get; private set; }

        public long FailedBurstWriteBytesTotal { get; private set; }

        public long FailedReadBytesTotal { get; private set; }

        public long WriteErrorTotal { get; private set; }

        public long BurstWriteErrorTotal { get; private set; }

        public long ReadErrorTotal { get; private set; }

        #endregion

        #region -- Public Events --

        public event EventHandler<SpiServiceTransferedEventArgs> DataWrote;

        public event EventHandler<SpiServiceBurstWriteEventArgs> DataBurstWrote;

        public event EventHandler<SpiServiceTransferedEventArgs> DataRead;

        #endregion

        #region -- Constructors --

        public Ymf825Client(Spi spiDevice)
        {
            this.spiDevice = spiDevice;
        }

        #endregion

        #region -- Public Methods --

        public void WriteGpio(byte direction, byte value)
        {
            spiDevice.WriteGpio(direction, value);
        }

        public byte ReadGpio()
        {
            return spiDevice.ReadGpio();
        }

        public int Write(byte address, byte data)
        {
            var totalTransfered = 0;
            address &= 0b01111111;

            for (var i = 0; i < targetIndices.Length; i++)
            {
                var transfered = 0;

                try
                {
                    ChangeTarget(i);
                    transfered = spiDevice.WriteBytes(address, data);
                    WriteBytesTotal += transfered;
                    WriteCommandsTotal++;
                    FailedWriteBytesTotal += 2 - transfered;

                    DataWrote?.Invoke(this, new SpiServiceTransferedEventArgs(address, data));

                    totalTransfered += transfered;
                }
                catch (InvalidOperationException)
                {
                    FailedWriteBytesTotal += 2 - transfered;
                    WriteErrorTotal++;
                }
            }

            return totalTransfered;
        }

        public int WriteBuffer(byte[] buffer, int offset, int count)
        {
            if (count < 1)
                return 0;

            if (count == 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 2)
                return Write(buffer[offset], buffer[offset + 1]);

            var newBuffer = new byte[count - 1];
            Array.Copy(buffer, offset, newBuffer, 0, count - 1);
            return BurstWriteBytes(buffer[offset], newBuffer);
        }

        public int BurstWriteBytes(byte address, params byte[] data)
        {
            var totalTransfered = 0;
            address &= 0b01111111;

            for (var i = 0; i < targetIndices.Length; i++)
            {
                var transfered = 0;

                try
                {
                    ChangeTarget(i);
                    transfered = spiDevice.BurstWriteBytes(address, data);
                    BurstWriteBytesTotal += transfered;
                    BurstWriteCommandsTotal++;
                    FailedBurstWriteBytesTotal += (1 + data.Length) - transfered;

                    DataBurstWrote?.Invoke(this, new SpiServiceBurstWriteEventArgs(address, data));

                    totalTransfered += transfered;
                }
                catch (InvalidOperationException)
                {
                    FailedBurstWriteBytesTotal += (1 + data.Length) - transfered;
                    BurstWriteErrorTotal++;
                }
            }

            return totalTransfered;
        }

        public int ReadByte(byte address)
        {
            try
            {
                address |= 0b10000000;
                var readData = spiDevice.WriteAndReadByte(address);
                ReadBytesTotal += readData > -1 ? 1 : 0;
                WriteBytesTotal += readData > -1 ? 1 : 0;
                ReadCommandsTotal++;
                WriteCommandsTotal++;
                FailedWriteBytesTotal += readData > -1 ? 0 : 1;
                FailedReadBytesTotal += readData > -1 ? 0 : 1;
                WriteErrorTotal += readData > -1 ? 0 : 1;
                ReadErrorTotal += readData > -1 ? 0 : 1;

                if (readData >= 0)
                    DataRead?.Invoke(this, new SpiServiceTransferedEventArgs(address, (byte)readData));

                return readData;
            }
            catch (InvalidOperationException)
            {
                FailedWriteBytesTotal++;
                FailedReadBytesTotal++;
                ReadErrorTotal++;
                WriteErrorTotal++;

                return 0;
            }
        }

        public void SendReset()
        {
            Write(0x1D, 0);
            Write(0x02, 0x0E);
            Thread.Sleep(1);

            Write(0x00, 0x01); //CLKEN
            Write(0x01, 0x00); //AKRST
            Write(0x1A, 0xA3);
            Thread.Sleep(1);

            Write(0x1A, 0x00);
            Thread.Sleep(30);

            Write(0x02, 0x04); //AP1,AP3
            Thread.Sleep(1);

            Write(0x02, 0x00);
            //add
            Write(0x19, 0xF0); //MASTER VOL
            Write(0x1B, 0x3F); //interpolation
            Write(0x14, 0x00); //interpolation
            Write(0x03, 0x01); //Analog Gain

            Write(0x08, 0xF6);
            Thread.Sleep(21);

            Write(0x08, 0x00);
            Write(0x09, 0xF8);
            Write(0x0A, 0x00);

            Write(0x17, 0x40); //MS_S
            Write(0x18, 0x00);
        public void SetTarget(params int[] target)
        {
            targetIndices = target;
        }

        public static bool IsAvailable()
        {
            var spiServiceChannel = new ChannelFactory<IYmf825Client>(
                new NetNamedPipeBinding(),
                new EndpointAddress(ServiceAddress));
            var spiService = spiServiceChannel.CreateChannel();

            try
            {
                spiService.WriteBuffer(new byte[0], 0, 0);
                spiServiceChannel.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static IYmf825Client GetClient()
        {
            var spiServiceChannel = new ChannelFactory<IYmf825Client>(
                new NetNamedPipeBinding(),
                new EndpointAddress(ServiceAddress));
            var spiService = spiServiceChannel.CreateChannel();

            try
            {
                // spiService.SendReset();
                spiService.WriteBuffer(new byte[0], 0, 0);
            }
            catch (EndpointNotFoundException e)
            {
                throw new InvalidOperationException("Ymf825Server が起動していません。クライアントを実行する前に、サーバを起動してください。", e);
            }

            return spiService;
        }

        public void ChangeTarget(int newTargetIndex)
        {
            if (currentTargetIndex == newTargetIndex)
                return;

            spiDevice.ChangeConfig(newTargetIndex);
            currentTargetIndex = newTargetIndex;
        }

        #endregion
    }

    public class SpiServiceTransferedEventArgs : EventArgs
    {
        #region -- Public Properties --

        public byte Address { get; }

        public byte Data { get; }

        #endregion

        #region -- Constructors --

        public SpiServiceTransferedEventArgs(byte address, byte data)
        {
            Address = address;
            Data = data;
        }

        #endregion
    }

    public class SpiServiceBurstWriteEventArgs : EventArgs
    {
        #region -- Public Properties --

        public byte Address { get; }

        public IReadOnlyList<byte> Data { get; }

        #endregion

        #region -- Constructors --

        public SpiServiceBurstWriteEventArgs(byte address, IReadOnlyList<byte> data)
        {
            Address = address;
            Data = data;
        }

        #endregion
    }
}
