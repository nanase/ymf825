using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ymf825.IO
{
    /// <summary>
    /// YMF825 と SPI による通信を行うための機能を提供します。
    /// </summary>
    /// <inheritdoc cref="Spi" />
    public class Ymf825Spi : Spi, IYmf825Device
    {
        #region -- Private Fields --

        private byte csTargetPin;

        private readonly Dictionary<TargetChip, byte> csPinMap;

        #endregion

        #region -- Public Properties --

        public SpiPinConfig CsPinConfig { get; }

        public SpiPinConfig IcPinConfig { get; }

        #endregion

        #region -- Constructors --

        /// <summary>
        /// パラメータを指定して新しい <see cref="Ymf825Spi"/> クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="deviceIndex">デバイスのインデクス。</param>
        /// <inheritdoc />
        public Ymf825Spi(int deviceIndex, SpiPinConfig csPinConfig, SpiPinConfig icPinConfig)
            : base(deviceIndex)
        {
            CsPinConfig = csPinConfig;
            IcPinConfig = icPinConfig;
            csTargetPin = csPinConfig.Value;
            csPinMap = CreateDeviceMap(csPinConfig.Value);

            ResetHardware();
        }

        #endregion

        #region -- Public Methods --

        public void SetTarget(TargetChip chip)
        {
            if (csPinMap.ContainsKey(chip))
                csTargetPin = csPinMap[chip];
            else
                throw new ArgumentOutOfRangeException(nameof(chip));
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
            PurgeRxBuffer();

            Marshal.WriteInt16(ReadBuffer, 0);

            QueueBufferCsEnable();
            QueueBuffer(0x31, 0x01, 0x00, address, 0x00);
            QueueBufferCsDisable();
            QueueFlushCommand();
            SendBuffer();
            WaitQueue(2);

            return ReadRaw()[1];
        }

        /// <summary>
        /// ハードウェアリセットを行います。
        /// このコマンドは即時に実行されます。
        /// </summary>
        public void ResetHardware()
        {
            QueueGpio(IcPinConfig.IsHighByte, (byte)(IcPinConfig.EnableLevelHigh ? 0x00 : IcPinConfig.Value), IcPinConfig.Direction);
            QueueFlushCommand();
            SendBuffer();
            Thread.Sleep(2);

            QueueGpio(IcPinConfig.IsHighByte, (byte)(IcPinConfig.EnableLevelHigh ? IcPinConfig.Value : 0x00), IcPinConfig.Direction);
            QueueFlushCommand();
            SendBuffer();
            Thread.Sleep(2);

            QueueGpio(IcPinConfig.IsHighByte, (byte)(IcPinConfig.EnableLevelHigh ? 0x00 : IcPinConfig.Value), IcPinConfig.Direction);
            QueueFlushCommand();
            SendBuffer();
        }

        #endregion

        #region -- Private Methods --

        private static Dictionary<TargetChip, byte> CreateDeviceMap(byte pinValue)
        {
            var map = new Dictionary<TargetChip, byte> { { TargetChip.None, 0x00 } };
            var availableChip = new Dictionary<TargetChip, int>();
            var chipIndex = 0x01;
            var bit = 0x01;

            for (var i = 0; i < 8; i++)
            {
                if ((bit & pinValue) != 0)
                {
                    availableChip.Add((TargetChip)chipIndex, bit);
                    chipIndex <<= 1;
                }

                bit <<= 1;
            }

            foreach (var x in availableChip)
            {
                foreach (var y in availableChip)
                {
                    var z = new { chip = x.Key | y.Key, value = x.Value | y.Value };

                    if (!map.ContainsKey(z.chip))
                        map.Add(z.chip, (byte)z.value);
                }
            }

            return map;
        }

        private void QueueBufferCsEnable() => QueueGpio(
            CsPinConfig.IsHighByte,
            (byte)(CsPinConfig.EnableLevelHigh ? CsPinConfig.Value & csTargetPin : CsPinConfig.Value ^ csTargetPin),
            CsPinConfig.Direction);

        private void QueueBufferCsDisable() => QueueGpio(
            CsPinConfig.IsHighByte,
            (byte)(CsPinConfig.EnableLevelHigh ? 0x00 : CsPinConfig.Value),
            CsPinConfig.Direction);

        #endregion
    }
}
