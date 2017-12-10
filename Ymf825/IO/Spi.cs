using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ymf825.IO
{
    /// <summary>
    /// USB を介して SPI を使用するための機能を提供します。
    /// </summary>
    /// <inheritdoc cref="IDisposable"/>
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

        static Spi()
        {
            DllDirectorySwitcher.Apply();
        }

        /// <summary>
        /// パラメータを指定して新しい <see cref="Spi"/> クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="deviceIndex">デバイスのインデクス。</param>
        /// <param name="csEnableLevelHigh">CS ピンを有効にする際の IO レベルを表す真偽値。
        /// true のとき、High レベル。false のとき、Low レベル。</param>
        /// <param name="csPin">FT シリーズにおける CS ピン位置を表す整数値。</param>
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

        /// <summary>
        /// CS ピンを有効にする際の対象となる、ピン位置を表す整数値を設定します。
        /// </summary>
        /// <param name="pin">ピン位置を表す整数値。有効範囲は 0x08 から 0xf8 です。</param>
        public void SetCsTargetPin(byte pin)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if ((csTargetPin & 0x07) != 0)
                throw new InvalidOperationException("使用できない CS ピンが指定されています。");

            csTargetPin = pin;
        }

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

        /// <summary>
        /// アドレスとデータを書き込むコマンドを送信キューに追加します。
        /// </summary>
        /// <param name="address">アドレスを表す 1 バイトの整数値。</param>
        /// <param name="data">データを表す 1 バイトの整数値。</param>
        public void Write(byte address, byte data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if (csTargetPin == 0)
                throw new InvalidOperationException("CS ピンが指定されていません。");

            QueueBufferCsEnable();
            QueueBuffer(0x11, 0x01, 0x00, address, data);
            QueueBufferCsDisable();
        }

        /// <summary>
        /// アドレスと可変長のデータを書き込むコマンドを送信キューに追加します。
        /// </summary>
        /// <param name="address">アドレスを表す 1 バイトの整数値。</param>
        /// <param name="data">データが格納されている <see cref="byte"/> 型の配列。</param>
        /// <param name="offset">配列を読み出しを開始するオフセット値。</param>
        /// <param name="count">配列から読み出すバイト数。</param>
        public void BurstWrite(byte address, byte[] data, int offset, int count)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (offset < 0 || offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count <= 0 || offset + count > data.Length || count > 65535)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (csTargetPin == 0)
                throw new InvalidOperationException("CS ピンが指定されていません。");

            QueueBufferCsEnable();
            QueueBuffer(0x11, (byte)((count) & 0x00ff), (byte)(count >> 8), address);
            QueueBuffer(data, offset, count);
            QueueBufferCsDisable();
        }

        /// <summary>
        /// アドレスを指定して SPI デバイスから 1 バイトを読み出します。
        /// このコマンドは即時に実行されます。
        /// </summary>
        /// <param name="address">アドレスを表す 1 バイトの整数値。</param>
        /// <returns>SPI デバイスから返却されたデータ。</returns>
        public byte Read(byte address)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if (csTargetPin == 0)
                throw new InvalidOperationException("CS ピンが指定されていません。");

            if (csTargetPin != 0 && (csTargetPin & (csTargetPin - 1)) != 0)
                throw new InvalidOperationException("複数の CS ピンを指定して Read 命令は実行できません。");
            
            SendBuffer();

            if (FT_GetQueueStatus(handle, out var rxBytes) == FtStatus.FT_OK && rxBytes > 0)
                CheckStatus(FT_Purge(handle, FtPurgeRx));

            Marshal.WriteInt16(ReadBuffer, 0);

            QueueBufferCsEnable();
            QueueBuffer(0x31, 0x01, 0x00, address, 0x00);
            QueueBufferCsDisable();
            QueueFlushCommand();
            SendBuffer();
            WaitQueue(2);

            return ReadRaw()[1];
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 利用可能なデバイスのリストを取得します。
        /// </summary>
        /// <returns>利用可能なデバイスが格納された、<see cref="DeviceInfo"/>クラスの配列。</returns>
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

        protected void SendBuffer()
        {
            if (WriteBufferIndex == 0)
                return;

            CheckStatus(FT_Write(handle, WriteBuffer, (uint)WriteBufferIndex - 1, out var _));
#if TRACE
            Trace(WriteBuffer, WriteBufferIndex);
#endif
            WriteBufferIndex = 0;
        }

        protected void QueueBuffer(byte[] data, int offset, int count)
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

        protected void QueueBuffer(params byte[] data) => QueueBuffer(data, 0, data.Length);

        protected void QueueFlushCommand() => QueueBuffer(0x87);

        protected void QueueBufferCsEnable() => QueueBuffer(
            0x80,
            (byte)(csEnableLevelHigh ? csPin & csTargetPin : csPin ^ csTargetPin),
            0xfb);

        protected void QueueBufferCsDisable() => QueueBuffer(
            0x80,
            (byte)(csEnableLevelHigh ? 0x00 : csPin),
            0xfb);

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

        private IReadOnlyList<byte> ReadRaw()
        {
            CheckStatus(FT_Read(handle, ReadBuffer, (uint)ReadBufferSize, out var bytesReturned));
            var readData = new byte[bytesReturned];
            Marshal.Copy(ReadBuffer, readData, 0, (int)bytesReturned);
            return readData;
        }

        private void WaitQueue(int requireBytes)
        {
            while (FT_GetQueueStatus(handle, out var rxBytes) == FtStatus.FT_OK && rxBytes < requireBytes)
            {
            }
        }

        private static void CheckStatus(FtStatus ftStatus)
        {
            if (ftStatus != FtStatus.FT_OK)
                throw new InvalidOperationException(ftStatus.GetErrorMessage());
        }

        #endregion
    }
}
