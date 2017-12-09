using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ymf825.IO
{
    public class Spi : IDisposable
    {
        #region -- Extern Methods --

        // from: ftd2xx.h

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_CreateDeviceInfoList(out uint numDevs);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_GetDeviceInfoList(IntPtr dest, ref uint numDevs);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_Open(uint deviceNumber, out IntPtr handle);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_Close(IntPtr handle);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_GetQueueStatus(IntPtr handle, out uint rxBytes);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_SetBitMode(IntPtr handle, byte mask, byte enable);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_SetTimeouts(IntPtr handle, uint readTimeout, uint writeTimeout);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_SetLatencyTimer(IntPtr handle, byte latency);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_Read(IntPtr handle, IntPtr buffer, uint bytesToRead, out uint bytesReturned);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_Write(IntPtr handle, IntPtr buffer, uint bytesToWrite, out uint bytesWritten);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_Purge(IntPtr handle, uint mask);

#if TRACE
        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        private static extern FtStatus FT_GetStatus(IntPtr handle, out uint rxBytes, out uint txBytes, out uint eventDWord);
#endif
        #endregion

        #region -- Private Fields --

        private const string D2XxLibrary = "ftd2xx.dll";

        private const int FtPurgeRx = 1;
        private const int FtPurgeTx = 2;
        private const int FtBitmodeReset = 0x00;
        private const int FtBitmodeMpsse = 0x02;

        private readonly IntPtr handle;
        private readonly bool csEnableLevelHigh;
        
        private readonly byte csPin;
        private byte csTargetPin;

        #endregion

        #region -- Protected Fields --

        protected IntPtr ReadBuffer;
        protected IntPtr WriteBuffer;
        protected int ReadBufferSize = 16;
        protected int WriteBufferSize = 16;

        #endregion

        #region -- Public Properties --

        public static int DeviceCount => FT_CreateDeviceInfoList(out var numDevs) != FtStatus.FT_OK ? 0 : (int)numDevs;

        public bool IsDisposed { get; private set; }

        #endregion

        #region -- Constructors --

        static Spi()
        {
            DllDirectorySwitcher.Apply();
        }

        public Spi(int deviceIndex, bool csEnableLevelHigh, byte csPin)
        {
            CheckStatus(FT_Open((uint)deviceIndex, out handle));
            ReadBuffer = Marshal.AllocHGlobal(ReadBufferSize);
            WriteBuffer = Marshal.AllocHGlobal(WriteBufferSize);
            this.csEnableLevelHigh = csEnableLevelHigh;
            this.csPin = csPin;
            Initialize();
        }

        #endregion

        #region -- Public Methods --

        public static DeviceInfo[] GetDeviceInfoList()
        {
            var devNums = (uint)DeviceCount;
            var deviceInfoList = new DeviceInfo[devNums];

            if (devNums < 1)
                return deviceInfoList;

            var structSize = Marshal.SizeOf<DeviceListInfoNode>();
            var listPointer = Marshal.AllocHGlobal(structSize * (int)devNums);

            if (FT_GetDeviceInfoList(listPointer, ref devNums) == FtStatus.FT_OK)
            {
                for (var i = 0; i < devNums; i++)
                {
                    var node = Marshal.PtrToStructure<DeviceListInfoNode>(listPointer + structSize * i);
                    deviceInfoList[i] = new DeviceInfo(i, node);
                }
            }
            else
            {
                deviceInfoList = new DeviceInfo[0];
            }

            Marshal.FreeHGlobal(listPointer);

            return deviceInfoList;
        }

        public void SetCsTargetPin(byte pin)
        {
            if ((csTargetPin & 0x07) != 0)
                throw new InvalidOperationException("使用できない CS ピンが指定されています。");

            csTargetPin = pin;
        }

        public void Write(byte address, byte data)
        {
            if (csTargetPin == 0)
                throw new InvalidOperationException("CS ピンが指定されていません。");

            if (WriteBufferSize < 11)
                ExtendBuffer(ref WriteBuffer, ref WriteBufferSize, 11);

            BufferWriteCsEnable(0);
            Marshal.Copy(new byte[] { 0x11, 0x01, 0x00, address, data }, 0, WriteBuffer + 3, 5);
            BufferWriteCsDisable(8);
            FlushBuffer(11);
        }

        public void BurstWrite(byte address, byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (offset < 0 || offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count <= 0 || offset + count > data.Length || count > 65535)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (csTargetPin == 0)
                throw new InvalidOperationException("CS ピンが指定されていません。");

            if (WriteBufferSize < 10 + count)
                ExtendBuffer(ref WriteBuffer, ref WriteBufferSize, 10 + count);

            BufferWriteCsEnable(0);
            Marshal.Copy(new byte[] { 0x11, (byte)((count) & 0x00ff), (byte)((count) >> 8), address }, 0, WriteBuffer + 3, 4);
            Marshal.Copy(data, offset, WriteBuffer + 7, count);
            BufferWriteCsDisable(7 + count);
            FlushBuffer(10 + count);
        }

        public byte Read(byte address)
        {
            if (ReadBufferSize < 2)
                ExtendBuffer(ref ReadBuffer, ref ReadBufferSize, 2);

            if (WriteBufferSize < 11)
                ExtendBuffer(ref WriteBuffer, ref WriteBufferSize, 11);

            if (csTargetPin == 0)
                throw new InvalidOperationException("CS ピンが指定されていません。");

            if (csTargetPin != 0 && (csTargetPin & (csTargetPin - 1)) != 0)
                throw new InvalidOperationException("複数の CS ピンを指定して Read 命令は実行できません。");

            Marshal.WriteInt16(ReadBuffer, 0);
            BufferWriteCsEnable(0);
            Marshal.Copy(new byte[] { 0x31, 0x01, 0x00, address, 0x00 }, 0, WriteBuffer + 3, 5);
            BufferWriteCsDisable(8);
            FlushBuffer(11);
            WaitQueue(2);

            return ReadRaw().ToArray()[1];
        }

        

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region -- Protected Methods --

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            //if (disposing)
            //{
            // mamaged
            //}

            if (handle != IntPtr.Zero)
                CheckStatus(FT_Close(handle));

            Marshal.FreeHGlobal(WriteBuffer);
            Marshal.FreeHGlobal(ReadBuffer);

            IsDisposed = true;
        }

        ~Spi()
        {
            Dispose(false);
        }

        protected void WaitQueue(int requireBytes)
        {
            while (FT_GetQueueStatus(handle, out var rxBytes) == FtStatus.FT_OK && rxBytes < requireBytes)
            {
            }
        }

        protected void FlushBuffer(int length)
        {
            CheckStatus(FT_Write(handle, WriteBuffer, (uint)length - 1, out var _));
#if TRACE
            Trace(writeBuffer, length);
#endif
            Marshal.WriteByte(WriteBuffer, 0x87);
            CheckStatus(FT_Write(handle, WriteBuffer, 1, out var _));
        }

        protected void WriteRaw(params byte[] data)
        {
            if (WriteBufferSize < data.Length)
                ExtendBuffer(ref WriteBuffer, ref WriteBufferSize, data.Length);

            Marshal.Copy(data, 0, WriteBuffer, data.Length);
            CheckStatus(FT_Write(handle, WriteBuffer, (uint)data.Length - 1, out var _));
#if TRACE
            Trace(writeBuffer, length);
#endif
            Marshal.WriteByte(WriteBuffer, 0x87);
            CheckStatus(FT_Write(handle, WriteBuffer, 1, out var _));
        }

        protected IEnumerable<byte> ReadRaw()
        {
            CheckStatus(FT_Read(handle, ReadBuffer, (uint)ReadBufferSize, out var bytesReturned));
            var readData = new byte[bytesReturned];
            Marshal.Copy(ReadBuffer, readData, 0, (int)bytesReturned);
            return readData;
        }
        
        protected static void ExtendBuffer(ref IntPtr buffer, ref int bufferSize, int extendedSize)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            bufferSize = extendedSize;
            Marshal.FreeHGlobal(buffer);
            buffer = Marshal.AllocHGlobal(bufferSize);
        }

        #endregion

        #region -- Private Methods --

#if TRACE
        private void Trace(IntPtr buffer, int length)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            var debug = new byte[length];
            Marshal.Copy(buffer, debug, 0, length);
            Console.WriteLine(string.Join(" ", debug.Select(i => i.ToString("x2"))));
            Console.ForegroundColor = ConsoleColor.White;
            FT_GetStatus(handle, out var rxBytes, out var txBytes, out var _);
            Console.WriteLine("RX: {0}, TX: {1}", rxBytes, txBytes);
        }
#endif

        private void Initialize()
        {
            CheckStatus(FT_Purge(handle, FtPurgeRx | FtPurgeTx));
            Thread.Sleep(10);

            CheckStatus(FT_SetTimeouts(handle, 1, 1000));
            CheckStatus(FT_SetLatencyTimer(handle, 1));                 // 1ms

            CheckStatus(FT_SetBitMode(handle, 0x00, FtBitmodeReset));   // Reset
            Thread.Sleep(10);

            CheckStatus(FT_SetBitMode(handle, 0x00, FtBitmodeMpsse));   // Enable MPSSE Mode
            CheckStatus(FT_Purge(handle, FtPurgeRx));
            Thread.Sleep(20);

            WriteRaw(
                0x86, 0x02, 0x00,   // SCK is 10MHz
                0x80, 0xf8, 0xfb,   // ADBUS: v - 1111 1000, d - 1111 1011 (0: in, 1: out)
                0x82, 0xff, 0xff,   // ACBUS: v - 1111 1111, d - 1111 1111 (0: in, 1: out)
                0x8a,
                0x85,
                0x8d
            );

            Thread.Sleep(100);
        }
        
        private void BufferWriteCsEnable(int offset)
        {
            if (WriteBufferSize < offset + 3)
                ExtendBuffer(ref WriteBuffer, ref WriteBufferSize, offset + 3);

            Marshal.WriteByte(WriteBuffer, offset, 0x80);

            if (csEnableLevelHigh)
                Marshal.WriteByte(WriteBuffer, offset + 1, (byte)(csPin & csTargetPin));
            else
                Marshal.WriteByte(WriteBuffer, offset + 1, (byte)(csPin ^ csTargetPin));

            Marshal.WriteByte(WriteBuffer, offset + 2, 0xfb);
        }

        private void BufferWriteCsDisable(int offset)
        {
            if (WriteBufferSize < offset + 3)
                ExtendBuffer(ref WriteBuffer, ref WriteBufferSize, offset + 3);

            Marshal.WriteByte(WriteBuffer, offset, 0x80);
            Marshal.WriteByte(WriteBuffer, offset + 1, (byte)(csEnableLevelHigh ? 0x00 : csPin));
            Marshal.WriteByte(WriteBuffer, offset + 2, 0xfb);
        }
        
        private static void CheckStatus(FtStatus ftStatus)
        {
            if (ftStatus != FtStatus.FT_OK)
                throw new InvalidOperationException(ftStatus.GetErrorMessage());
        }

        #endregion
    }
}
