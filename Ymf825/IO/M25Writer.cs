using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ymf825.IO
{
    /// <inheritdoc />
    /// <summary>
    /// M25 形式を書き出すための機能を提供します。
    /// </summary>
    public class M25Writer : IYmf825BinaryWriter
    {
        #region -- Private Fields --

        private readonly BinaryWriter binaryWriter;
        private readonly Queue<(byte address, byte data)> writeBuffer = new Queue<(byte, byte)>();
        private readonly byte[] burstwriteBuffer = new byte[487];
        private int waitTickQueue;
        private int burstwriteSize;

        private SelxValue selx = SelxValue.Both;

        #endregion

        #region -- Properties --

        /// <inheritdoc />
        /// <summary>
        /// インタフェースに接続されている有効な YMF825 の組み合わせを取得します。
        /// </summary>
        public TargetChip AvailableChips { get; } = TargetChip.Board0 | TargetChip.Board1;

        /// <inheritdoc />
        /// <summary>
        /// 命令が実行される対象の YMF825 を取得します。
        /// </summary>
        public TargetChip CurrentTargetChips { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// このプロパティは常に <code>true</code> を返します。
        /// </summary>
        public bool AutoFlush
        {
            get => true;
            set { }
        }

        /// <inheritdoc />
        /// <summary>
        /// 書き出し先のストリームを取得します。
        /// </summary>
        public Stream BaseStream { get; }

        /// <inheritdoc />
        /// <summary>
        /// インスタンスのリソースが破棄されたかを表す真偽値を取得します。
        /// </summary>
        public bool Disposed { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// Tick値の基準となる時間分解能を取得します。単位は 1/秒 です。
        /// </summary>
        public double TickResolution { get; } = 0.01;

        /// <inheritdoc />
        /// <summary>
        /// YMF825 のレジスタ読み取り命令に対応しているかを表す真偽値を取得します。
        /// このプロパティは常に <code>false</code> を返します。
        /// </summary>
        public bool SupportReadOperation { get; } = false;

        /// <inheritdoc />
        /// <summary>
        /// ハードウェアリセット命令に対応しているかを表す真偽値を取得します。
        /// このプロパティは常に <code>false</code> を返します。
        /// </summary>
        public bool SupportHardwareReset { get; } = false;

        #endregion

        #region -- Constructors --

        /// <summary>
        /// 書き込み先の <see cref="Stream"/> を指定して、新しい <see cref="M25Writer"/> クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="stream">書込み可能なストリーム。</param>
        public M25Writer(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanWrite)
                throw new InvalidOperationException();

            BaseStream = stream;

            binaryWriter = new BinaryWriter(stream, Encoding.ASCII, true);
        }

        #endregion

        #region -- Public Methods --

        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        public void Write(byte address, byte data)
        {
            FlushWait();
            writeBuffer.Enqueue((address, data));
        }

        public void BurstWrite(byte address, byte[] data, int offset, int count)
        {
            FlushWait();
            Flush();

            if (count == 15)
            {
                // equalizer
                binaryWriter.Write((byte)((byte)selx | 0x30));
                binaryWriter.Write(address);
                binaryWriter.Write(data, offset, count);
            }
            else
            {
                // tone parameter
                var length = (count - 5) / 30;

                binaryWriter.Write((byte)((byte)selx | 0x20 | (length - 1)));
                binaryWriter.Write(address);
                binaryWriter.Write(data, offset, count);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// このクラスは読み取り命令をサポートしていません。
        /// 常に <see cref="NotSupportedException"/> 例外を送出します。
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte Read(byte address)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        /// <summary>
        /// このクラスはハードウェアリセットをサポートしていません。
        /// 常に <see cref="NotSupportedException" /> 例外を送出します。
        /// </summary>
        public void InvokeHardwareReset()
        {
            throw new NotSupportedException();
        }

        public void Flush()
        {
            while (writeBuffer.Count > 0)
            {
                var count = Math.Min(writeBuffer.Count, 16);
                var k = (byte)((byte)selx | (count - 1));
                binaryWriter.Write(k);

                for (var i = 0; i < count; i++)
                {
                    var (address, data) = writeBuffer.Dequeue();
                    binaryWriter.Write(address);
                    binaryWriter.Write(data);
                }
            }
        }

        public void Wait(int tick)
        {
            Flush();
            FlushBurstWrite();

            waitTickQueue += tick;
        }

        public void RealtimeWait(int tick)
        {
            Flush();
            FlushBurstWrite();

            // thru
        }

        public void SetTarget(TargetChip chips)
        {
            CurrentTargetChips = chips;

            if (chips == (TargetChip.Board0 | TargetChip.Board1))
                selx = SelxValue.Both;
            else if (chips == TargetChip.Board0)
                selx = SelxValue.Lch;
            else
                selx = SelxValue.Rch;
        }

        #endregion

        #region -- Protected Methods --

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                Flush();
                binaryWriter.Close();
            }

            // unmanaged objects

            Disposed = true;
        }

        #endregion

        #region -- Private Methods --

        private void FlushWait()
        {
            while (waitTickQueue > 0)
            {
                var waitTick = Math.Min(waitTickQueue, 16);
                binaryWriter.Write((byte)(waitTick - 1));
                waitTickQueue -= waitTick;
            }
        }

        private void FlushBurstWrite()
        {
            if (burstwriteSize == 0)
                return;

            binaryWriter.Write(burstwriteBuffer, 0, burstwriteSize);
            burstwriteSize = 0;
        }

        #endregion

        [Flags]
        private enum SelxValue : byte
        {
            Lch = 0b0100_0000,
            Rch = 0b1000_0000,
            Both = 0b1100_0000,
        }
    }
}
