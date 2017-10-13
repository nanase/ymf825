using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ymf825
{
    internal static class DllDirectorySwitcher
    {
        public static void Initialize()
        {
            var executionDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? ".";
            SetDllDirectory(null);

            SetDllDirectory(Environment.Is64BitProcess
                ? Path.Combine(executionDirectory, "lib", "x64")
                : Path.Combine(executionDirectory, "lib", "x86"));
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }

    public class Spi : IDisposable
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private enum Status : uint
        {
            FT_OK,
            FT_INVALID_HANDLE,
            FT_DEVICE_NOT_FOUND,
            FT_DEVICE_NOT_OPENED,
            FT_IO_ERROR,
            FT_INSUFFICIENT_RESOURCES,
            FT_INVALID_PARAMETER,
            FT_INVALID_BAUD_RATE,

            FT_DEVICE_NOT_OPENED_FOR_ERASE,
            FT_DEVICE_NOT_OPENED_FOR_WRITE,
            FT_FAILED_TO_WRITE_DEVICE,
            FT_EEPROM_READ_FAILED,
            FT_EEPROM_WRITE_FAILED,
            FT_EEPROM_ERASE_FAILED,
            FT_EEPROM_NOT_PRESENT,
            FT_EEPROM_NOT_PROGRAMMED,
            FT_INVALID_ARGS,
            FT_NOT_SUPPORTED,
            FT_OTHER_ERROR,
            FT_DEVICE_LIST_NOT_READY,
        }

        [Flags]
        public enum ChannelConfigOption : uint
        {
            Default = 0x00000000,

            Mode0 = 0b00,
            Mode1 = 0b01,
            Mode2 = 0b10,
            Mode3 = 0b11,

            CsDbus3 = 0b00000,
            CsDbus4 = 0b00100,
            CsDbus5 = 0b01000,
            CsDbus6 = 0b01100,
            CsDbus7 = 0b10000,

            CsActiveHigh = 0b000000,
            CsActiveLow = 0b100000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ChannelConfig
        {
            #region -- Private Fields --

            private readonly uint clockRate;
            private readonly byte latencyTimer;
            private readonly ChannelConfigOption configOption;
            private readonly uint pin;              /* FinalVal-FinalDir-InitVal-InitDir (for dir 0=in, 1=out) */

            // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
            private readonly ushort reserved;

            #endregion

            #region -- Public Propeties --

            public int ClockRate => (int)clockRate;

            // ReSharper disable once ConvertToAutoProperty
            public byte LatencyTimer => latencyTimer;
            // ReSharper disable once ConvertToAutoProperty
            public ChannelConfigOption ConfigOption => configOption;
            public int Pin => (int)pin;

            #endregion

            #region -- Constructors --

            public ChannelConfig(int clockRateHz, byte latencyTimer = 255, ChannelConfigOption configOption = ChannelConfigOption.Default, int pin = 0x00000000)
            {
                clockRate = (uint)clockRateHz;
                this.latencyTimer = latencyTimer;
                this.configOption = configOption;
                this.pin = (uint)pin;

                reserved = 0;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DeviceInfoInternal
        {
            #region -- Private Fields --

            public readonly uint flags;
            public readonly uint type;
            public readonly uint id;
            public readonly uint locationId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public readonly string serialNumber;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public readonly string description;

            public IntPtr handle;

            #endregion
        }

        public class DeviceInfo
        {
            #region -- Private Fields --

            private readonly DeviceInfoInternal deviceInfoInternal;

            #endregion

            #region -- Public Properties --

            public int Index { get; }

            public int Flags => (int)deviceInfoInternal.flags;

            public int Type => (int)deviceInfoInternal.type;

            public int Id => (int)deviceInfoInternal.id;

            public int LocationId => (int)deviceInfoInternal.locationId;

            public string SerialNumber => deviceInfoInternal.serialNumber;

            public string Description => deviceInfoInternal.description;

            public IntPtr Handle => deviceInfoInternal.handle;

            public bool IsBusy => Handle != IntPtr.Zero;

            #endregion

            #region -- Constructors --

            internal DeviceInfo(int index, DeviceInfoInternal deviceInfoInternal)
            {
                Index = index;
                this.deviceInfoInternal = deviceInfoInternal;
            }

            #endregion

            #region -- Public Methods --

            public override string ToString()
            {
                return $"{Index}: {Description} ({SerialNumber})";
            }

            #endregion
        }

        #region -- Extern Methods --

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SPI_GetNumChannels(out uint numChannels);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SPI_GetChannelInfo(uint index, [Out] out DeviceInfoInternal chanInfoInternal);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SPI_OpenChannel(uint index, out IntPtr handle);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SPI_InitChannel(IntPtr handle, [In] ref ChannelConfig config);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SPI_CloseChannel(IntPtr handle);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SPI_Read(IntPtr handle, IntPtr buffer, uint sizeToTransfer, out uint sizeTransfered, uint options);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SPI_Write(IntPtr handle, IntPtr buffer, uint sizeToTransfer, out uint sizeTransfered, uint options);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SPI_ReadWrite(IntPtr handle, IntPtr inBuffer, IntPtr outBuffer, uint sizeToTransfer, out uint sizeTransfered, uint transferOptions);

        //[DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        //private static extern Status SPI_IsBusy(IntPtr handle, out bool state);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Init_libMPSSE();

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Cleanup_libMPSSE();

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SPI_ChangeCS(IntPtr handle, ChannelConfigOption configOptions);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status FT_WriteGPIO(IntPtr handle, byte dir, byte value);

        [DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status FT_ReadGPIO(IntPtr handle, out byte value);

        //[DllImport("libmpsse", CallingConvention = CallingConvention.Cdecl)]
        //private static extern Status SPI_ToggleCS(IntPtr handle, bool value);

        //FTDI_API FT_STATUS SPI_GetNumChannels(uint32* numChannels);
        //FTDI_API FT_STATUS SPI_GetChannelInfo(uint32 index, FT_DEVICE_LIST_INFO_NODE* chanInfoInternal);
        //FTDI_API FT_STATUS SPI_OpenChannel(uint32 index, FT_HANDLE* handle);
        //FTDI_API FT_STATUS SPI_InitChannel(FT_HANDLE handle, ChannelConfig* config);
        //FTDI_API FT_STATUS SPI_CloseChannel(FT_HANDLE handle);
        //FTDI_API FT_STATUS SPI_Read(FT_HANDLE handle, uint8* buffer, uint32 sizeToTransfer, uint32* sizeTransfered, uint32 options);
        //FTDI_API FT_STATUS SPI_Write(FT_HANDLE handle, uint8* buffer, uint32 sizeToTransfer, uint32* sizeTransfered, uint32 options);
        //FTDI_API FT_STATUS SPI_ReadWrite(FT_HANDLE handle, uint8* inBuffer, uint8* outBuffer, uint32 sizeToTransfer, uint32* sizeTransferred, uint32 transferOptions);
        //FTDI_API FT_STATUS SPI_IsBusy(FT_HANDLE handle, bool* state);
        //FTDI_API void Init_libMPSSE(void);
        //FTDI_API void Cleanup_libMPSSE(void);
        //FTDI_API FT_STATUS SPI_ChangeCS(FT_HANDLE handle, uint32 configOption);
        //FTDI_API FT_STATUS FT_WriteGPIO(FT_HANDLE handle, uint8 dir, uint8 value);
        //FTDI_API FT_STATUS FT_ReadGPIO(FT_HANDLE handle, uint8* value);
        //FTDI_API FT_STATUS SPI_ToggleCS(FT_HANDLE handle, bool state);

        #endregion

        #region -- Private Fields --

        private static readonly object LockObject = new object();
        private readonly IntPtr handle;
        private readonly int deviceIndex;
        private DeviceInfo deviceInfo;
        private readonly ChannelConfigOption[] configOption;
        
        private const int SpiTransferOptionsSizeInBytes = 0x00000000;
        private const int SpiTransferOptionsChipselectEnable = 0x00000002;
        private const int SpiTransferOptionsChipselectDisable = 0x00000004;

        #endregion

        #region -- Public Properties --

        public static bool IsInitialized { get; private set; }

        public static int DeviceCount => SPI_GetNumChannels(out var channels) != Status.FT_OK ? 0 : (int)channels;
        
        public bool IsDisposed { get; private set; }

        public DeviceInfo Info
        {
            get
            {
                if (deviceInfo == null)
                {
                    CheckStatus(SPI_GetChannelInfo((uint)deviceIndex, out var channelInfo));
                    deviceInfo = new DeviceInfo(deviceIndex, channelInfo);
                }

                return deviceInfo;
            }
        }

        #endregion

        #region -- Constructors --

        public Spi(int deviceIndex, ChannelConfig channelConfig, params ChannelConfigOption[] configOption)
        {
            var config = new ChannelConfig(
                channelConfig.ClockRate,
                channelConfig.LatencyTimer,
                channelConfig.ConfigOption | configOption[0],
                channelConfig.Pin);

            CheckStatus(SPI_OpenChannel((uint)deviceIndex, out handle));
            CheckStatus(SPI_InitChannel(handle, ref config));

            for (var i = 0; i < configOption.Length; i++)
                configOption[i] |= channelConfig.ConfigOption;

            this.configOption = configOption;
            this.deviceIndex = deviceIndex;
        }

        static Spi()
        {
            DllDirectorySwitcher.Initialize();
        }

        #endregion

        #region -- Public Methods --

        public void WriteGpio(byte direction, byte value)
        {
            CheckStatus(FT_WriteGPIO(handle, direction, value));
        }

        public byte ReadGpio()
        {
            CheckStatus(FT_ReadGPIO(handle, out var value));
            return value;
        }

        public int Write(byte[] buffer, int offset, int count)
        {
            var heapBuffer = Marshal.AllocHGlobal(sizeof(byte) * count);
            Marshal.Copy(buffer, offset, heapBuffer, count);
            CheckStatus(SPI_Write(handle, heapBuffer, (uint)count, out var sizeTransfered,
                SpiTransferOptionsSizeInBytes |
                SpiTransferOptionsChipselectEnable |
                SpiTransferOptionsChipselectDisable));
            Marshal.FreeHGlobal(heapBuffer);

            return (int)sizeTransfered;
        }

        public int WriteByte(byte data)
        {
            var heapBuffer = Marshal.AllocHGlobal(sizeof(byte));
            Marshal.WriteByte(heapBuffer, data);
            CheckStatus(SPI_Write(handle, heapBuffer, 1, out var sizeTransfered,
                SpiTransferOptionsSizeInBytes |
                SpiTransferOptionsChipselectEnable |
                SpiTransferOptionsChipselectDisable));
            Marshal.FreeHGlobal(heapBuffer);

            return (int)sizeTransfered;
        }

        public int WriteBytes(params byte[] buffer)
        {
            return Write(buffer, 0, buffer.Length);
        }

        public int BurstWriteBytes(byte address, params byte[] data)
        {
            var heapBuffer = Marshal.AllocHGlobal(sizeof(byte) * (data.Length + 1));
            Marshal.WriteByte(heapBuffer, address);
            Marshal.Copy(data, 0, heapBuffer + 1, data.Length);
            CheckStatus(SPI_Write(handle, heapBuffer, (uint)(data.Length + 1), out var sizeTransfered,
                SpiTransferOptionsSizeInBytes |
                SpiTransferOptionsChipselectEnable |
                SpiTransferOptionsChipselectDisable));
            Marshal.FreeHGlobal(heapBuffer);

            return (int)sizeTransfered;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var heapBuffer = Marshal.AllocHGlobal(sizeof(byte) * count);
            CheckStatus(SPI_Read(handle, heapBuffer, (uint)count, out var sizeTransfered,
                SpiTransferOptionsSizeInBytes |
                SpiTransferOptionsChipselectEnable |
                SpiTransferOptionsChipselectDisable));
            Marshal.Copy(heapBuffer, buffer, offset, (int)sizeTransfered);
            Marshal.FreeHGlobal(heapBuffer);

            return (int)sizeTransfered;
        }

        public int ReadByte()
        {
            var heapBuffer = Marshal.AllocHGlobal(sizeof(byte));
            CheckStatus(SPI_Read(handle, heapBuffer, 1, out var sizeTransfered,
                SpiTransferOptionsSizeInBytes |
                SpiTransferOptionsChipselectEnable |
                SpiTransferOptionsChipselectDisable));
            var readByte = Marshal.ReadByte(heapBuffer);
            Marshal.FreeHGlobal(heapBuffer);

            return sizeTransfered > 0 ? readByte : -1;
        }

        public int WriteAndReadByte(byte writeData)
        {
            var writeBuffer = Marshal.AllocHGlobal(sizeof(byte) * 2);
            var readBuffer = Marshal.AllocHGlobal(sizeof(byte) * 2);
            Marshal.WriteByte(writeBuffer, writeData);
            Marshal.WriteByte(writeBuffer + 1, 0);
            Marshal.WriteInt16(readBuffer, 0);
            CheckStatus(SPI_ReadWrite(handle, readBuffer, writeBuffer, 2, out var sizeTransfered, SpiTransferOptionsSizeInBytes |
                                                                                                  SpiTransferOptionsChipselectEnable |
                                                                                                  SpiTransferOptionsChipselectDisable));
            var readByte = Marshal.ReadByte(readBuffer + 1);
            Marshal.FreeHGlobal(writeBuffer);
            Marshal.FreeHGlobal(readBuffer);

            return sizeTransfered > 0 ? readByte : -1;

            //var heapBuffer = Marshal.AllocHGlobal(sizeof(byte));
            //Marshal.WriteByte(heapBuffer, writeData);
            //CheckStatus(SPI_Write(handle, heapBuffer, 1, out var sizeTransferedWrite, SpiTransferOptionsSizeInBytes | SpiTransferOptionsChipselectEnable));
            //CheckStatus(SPI_Read(handle, heapBuffer, 1, out var sizeTransferedRead, SpiTransferOptionsSizeInBytes | SpiTransferOptionsChipselectDisable));
            //var readByte = Marshal.ReadByte(heapBuffer);
            //Console.WriteLine("{0}/{1} - {2:x2}", sizeTransferedWrite, sizeTransferedRead, readByte);
            //Marshal.FreeHGlobal(heapBuffer);

            //return sizeTransferedWrite > 0 && sizeTransferedRead > 0 ? readByte : -1;
        }

        public void ChangeConfig(int index)
        {
            CheckStatus(SPI_ChangeCS(handle, configOption[index]));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public static void CleanUpLibrary()
        {
            lock (LockObject)
            {
                if (!IsInitialized)
                    return;

                Cleanup_libMPSSE();
                IsInitialized = false;
            }
        }

        public static void InitializeLibrary()
        {
            lock (LockObject)
            {
                if (IsInitialized)
                    return;

                Init_libMPSSE();
                IsInitialized = true;
            }
        }

        public static IEnumerable<DeviceInfo> GetDeviceInfoList() => GetDeviceInfoLazy(DeviceCount);
        
        #endregion

        #region -- Protected Methods --

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
            }

            if (handle != IntPtr.Zero)
                CheckStatus(SPI_CloseChannel(handle));

            IsDisposed = true;
        }

        ~Spi()
        {
            Dispose(false);
        }

        #endregion

        #region -- Private Methods --

        private static IEnumerable<DeviceInfo> GetDeviceInfoLazy(int channels)
        {
            for (var i = 0; i < channels; i++)
                if (SPI_GetChannelInfo((uint)i, out var channelInfo) == Status.FT_OK)
                    yield return new DeviceInfo(i, channelInfo);
        }

        private static void CheckStatus(Status status)
        {
            switch (status)
            {
                case Status.FT_OK:
                    break;

                case Status.FT_INVALID_HANDLE:
                    throw new InvalidOperationException("無効なハンドルが指定されました。");

                case Status.FT_DEVICE_NOT_FOUND:
                    throw new InvalidOperationException("指定されたデバイスが見つかりませんでした。");

                case Status.FT_DEVICE_NOT_OPENED:
                    throw new InvalidOperationException("指定されたデバイスを開けませんでした。");

                case Status.FT_IO_ERROR:
                    throw new InvalidOperationException("IOエラーが発生しました。");

                case Status.FT_INSUFFICIENT_RESOURCES:
                    throw new InvalidOperationException("リソースが不足しています。");

                case Status.FT_INVALID_ARGS:
                case Status.FT_INVALID_PARAMETER:
                    throw new InvalidOperationException("無効なパラメータが指定されました。");

                case Status.FT_INVALID_BAUD_RATE:
                    throw new InvalidOperationException("無効なボーレートが指定されました。");

                case Status.FT_EEPROM_READ_FAILED:
                case Status.FT_EEPROM_ERASE_FAILED:
                case Status.FT_DEVICE_NOT_OPENED_FOR_ERASE:
                    throw new InvalidOperationException("読み込まれようとしたデバイスは開かれていませんでした。");

                case Status.FT_EEPROM_WRITE_FAILED:
                case Status.FT_DEVICE_NOT_OPENED_FOR_WRITE:
                    throw new InvalidOperationException("書き込まれようとしたデバイスは開かれていませんでした。");

                case Status.FT_FAILED_TO_WRITE_DEVICE:
                    throw new InvalidOperationException("デバイスへの書き込みに失敗しました。");

                case Status.FT_EEPROM_NOT_PRESENT:
                case Status.FT_EEPROM_NOT_PROGRAMMED:
                    throw new InvalidOperationException("EEPROM がデバイスに存在しません。");

                case Status.FT_NOT_SUPPORTED:
                    throw new InvalidOperationException("実行された命令はサポートされていません。");

                case Status.FT_OTHER_ERROR:
                    throw new InvalidOperationException("不明なエラーです。");

                case Status.FT_DEVICE_LIST_NOT_READY:
                    throw new InvalidOperationException("デバイスリストを取得中です。");

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        #endregion
    }
}
