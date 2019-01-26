using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

#if DEBUG
using System.Linq;
#endif

namespace Ymf825.IO
{
    /// <summary>
    /// USB を介して SPI を使用するための機能を提供します。
    /// </summary>
    /// <inheritdoc cref="IDisposable"/>
    public class D2XxSpi : IDisposable
    {
        #region -- Extern Methods --

        // from: ftd2xx.h

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_CreateDeviceInfoList(out uint numDevs);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_GetDeviceInfoList(IntPtr dest, ref uint numDevs);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_Open(uint deviceNumber, out IntPtr handle);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_Close(IntPtr handle);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_GetQueueStatus(IntPtr handle, out uint rxBytes);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_SetBitMode(IntPtr handle, byte mask, byte enable);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_SetTimeouts(IntPtr handle, uint readTimeout, uint writeTimeout);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_SetLatencyTimer(IntPtr handle, byte latency);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_Read(IntPtr handle, IntPtr buffer, uint bytesToRead, out uint bytesReturned);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_Write(IntPtr handle, IntPtr buffer, uint bytesToWrite, out uint bytesWritten);

        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_Purge(IntPtr handle, uint mask);

#if DEBUG
        [DllImport(D2XxLibrary, CallingConvention = CallingConvention.StdCall)]
        protected static extern FtStatus FT_GetStatus(IntPtr handle, out uint rxBytes, out uint txBytes, out uint eventDWord);
#endif
        #endregion

        #region -- Private Fields --

        private const string D2XxLibrary = "ftd2xx.dll";

        private const int FtPurgeRx = 1;
        private const int FtPurgeTx = 2;
        private const int FtBitmodeReset = 0x00;
        private const int FtBitmodeMpsse = 0x02;

        private readonly IntPtr handle;

        #endregion

        #region -- Protected Fields --

        protected IntPtr ReadBuffer;
        protected IntPtr WriteBuffer;
        protected int WriteBufferIndex;
        protected int ReadBufferSize = 16;
        protected int WriteBufferSize = 4096;

        #endregion

        #region -- Public Properties --

        /// <summary>
        /// 利用可能なデバイスの数を取得します。
        /// </summary>
        public static int DeviceCount => FT_CreateDeviceInfoList(out var numDevs) != FtStatus.FT_OK ? 0 : (int)numDevs;

        /// <summary>
        /// このインスタンスが破棄されたかを表す真偽値を取得します。
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion

        #region -- Constructors --

        static D2XxSpi()
        {
            DllDirectorySwitcher.Apply();
        }

        /// <summary>
        /// パラメータを指定して新しい <see cref="D2XxSpi"/> クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="deviceIndex">デバイスのインデクス。</param>
        public D2XxSpi(int deviceIndex)
        {
            CheckStatus(FT_Open((uint)deviceIndex, out handle));
            ReadBuffer = Marshal.AllocHGlobal(ReadBufferSize);
            WriteBuffer = Marshal.AllocHGlobal(WriteBufferSize);

            Initialize();
        }

        #endregion

        #region -- Public Methods --

        /// <summary>
        /// デバイスの送信キューをフラッシュし、コマンドを即時に実行します。
        /// </summary>
        public void Flush()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            QueueFlushCommand();
            SendBuffer();
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 利用可能なデバイスのリストを取得します。
        /// </summary>
        /// <returns>利用可能なデバイスが格納された、<see cref="FtDeviceInfo"/>クラスの配列。</returns>
        public static FtDeviceInfo[] GetDeviceInfoList()
        {
            var devNums = (uint)DeviceCount;
            var deviceInfoList = new FtDeviceInfo[devNums];

            if (devNums < 1)
                return deviceInfoList;

            var structSize = Marshal.SizeOf<FtDeviceListInfoNode>();
            var listPointer = Marshal.AllocHGlobal(structSize * (int)devNums);

            if (FT_GetDeviceInfoList(listPointer, ref devNums) == FtStatus.FT_OK)
            {
                for (var i = 0; i < devNums; i++)
                {
                    var node = Marshal.PtrToStructure<FtDeviceListInfoNode>(listPointer + structSize * i);
                    deviceInfoList[i] = new FtDeviceInfo(i, node);
                }
            }
            else
            {
                deviceInfoList = new FtDeviceInfo[0];
            }

            Marshal.FreeHGlobal(listPointer);

            return deviceInfoList;
        }

        public void QueueBuffer(byte[] data, int offset, int count)
        {
            if (WriteBufferIndex + count > WriteBufferSize)
            {
                var tempBuffer = new byte[WriteBufferSize];
                var newWriteBuffer = Marshal.AllocHGlobal(WriteBufferIndex + count);
                Marshal.Copy(WriteBuffer, tempBuffer, 0, WriteBufferSize);
                Marshal.Copy(tempBuffer, 0, newWriteBuffer, WriteBufferSize);
                Marshal.FreeHGlobal(WriteBuffer);
                WriteBuffer = newWriteBuffer;
                WriteBufferSize = WriteBufferIndex + count;
            }

            Marshal.Copy(data, offset, WriteBuffer + WriteBufferIndex, count);
            WriteBufferIndex += count;
        }

        public void QueueBuffer(params byte[] data) => QueueBuffer(data, 0, data.Length);

        public void QueueFlushCommand() => QueueBuffer(0x87);

        public void SendBuffer()
        {
            if (WriteBufferIndex == 0)
                return;

            CheckStatus(FT_Write(handle, WriteBuffer, (uint)WriteBufferIndex - 1, out var _));
#if DEBUG
            Trace(WriteBuffer, WriteBufferIndex);
#endif
            WriteBufferIndex = 0;
        }

        public IReadOnlyList<byte> ReadRaw()
        {
            CheckStatus(FT_Read(handle, ReadBuffer, (uint)ReadBufferSize, out var bytesReturned));
            var readData = new byte[bytesReturned];
            Marshal.Copy(ReadBuffer, readData, 0, (int)bytesReturned);
            return readData;
        }

        public void WaitQueue(int requireBytes)
        {
            while (FT_GetQueueStatus(handle, out var rxBytes) == FtStatus.FT_OK && rxBytes < requireBytes)
            {
            }
        }

        public void PurgeRxBuffer()
        {
            if (FT_GetQueueStatus(handle, out var rxBytes) == FtStatus.FT_OK && rxBytes > 0)
                CheckStatus(FT_Purge(handle, FtPurgeRx));
        }

        public void PurgeTxBuffer()
        {
            if (FT_GetQueueStatus(handle, out var rxBytes) == FtStatus.FT_OK && rxBytes > 0)
                CheckStatus(FT_Purge(handle, FtPurgeTx));
        }

        public void QueueGpio(bool highByte, byte value, byte direction)
        {
            QueueBuffer((byte)(highByte ? 0x82 : 0x80), value, direction);
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

        ~D2XxSpi()
        {
            Dispose(false);
        }

        protected static void CheckStatus(FtStatus ftStatus)
        {
            if (ftStatus != FtStatus.FT_OK)
                throw new InvalidOperationException(ftStatus.GetErrorMessage());
        }

        #endregion

        #region -- Private Methods --

#if DEBUG
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

            QueueBuffer(
                0x86, 0x02, 0x00,   // SCK is 10MHz
                0x80, 0xf8, 0xfb,   // ADBUS: v - 1111 1000, d - 1111 1011 (0: in, 1: out)
                0x82, 0xff, 0xff,   // ACBUS: v - 1111 1111, d - 1111 1111 (0: in, 1: out)
                0x8a,
                0x85,
                0x8d
            );
            SendBuffer();

            Thread.Sleep(100);
        }

        #endregion
    }
}
