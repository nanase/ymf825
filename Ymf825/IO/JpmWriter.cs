using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ymf825.IO
{
    /// <inheritdoc />
    /// <summary>
    /// JPM 形式を書き出すための機能を提供します。
    /// </summary>
    public class JpmWriter : IYmf825BinaryWriter
    {
        #region -- Private Fields --

        private readonly BinaryWriter binaryWriter;
        private readonly Queue<(byte address, byte data)> writeBuffer = new Queue<(byte, byte)>();
        private int waitTickQueue;

        private SelxValue selx = SelxValue.Both;

        private const byte Version = 0x00;

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
        public TargetChip CurrentTargetChips { get; private set; } = TargetChip.Board0 | TargetChip.Board1;

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
        public double TickResolution { get; } = 100;

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
        public JpmWriter(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanWrite)
                throw new InvalidOperationException();

            BaseStream = stream;

            binaryWriter = new BinaryWriter(stream, Encoding.ASCII, true);
            WriteHeader();
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
                binaryWriter.Write((byte)(0b1100_0000 | (byte)selx));  // WOPx = 11xx
                binaryWriter.Write(address);
                binaryWriter.Write(data, offset, count);
            }
            else
            {
                // tone parameter
                var length = (count - 5) / 30;

                binaryWriter.Write((byte)(0b1000_0000 | (byte)selx | (length - 1)));  // WOPx = 10xx
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
                var k = (byte)(0x0000_0000 | (byte)selx | (count - 1));   // WOPx = 00xx
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

            waitTickQueue += tick;
        }

        public void RealtimeWait(int tick)
        {
            Flush();

            // thru
        }

        public void SetTarget(TargetChip chips)
        {
            FlushWait();
            Flush();

            CurrentTargetChips = chips;

            if (chips == (TargetChip.Board0 | TargetChip.Board1))
                selx = SelxValue.Both;
            else if (chips == TargetChip.Board0)
                selx = SelxValue.Lch;
            else
                selx = SelxValue.Rch;
        }

        public void WriteCharsToLcd(byte[] chars, int index, int count)
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            if (count < 0 || count > 16 || chars.Length > count)
                throw new ArgumentOutOfRangeException(nameof(chars));

            if (index < 0 || index + count > chars.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            FlushWait();

            if (count == 0)
                return;

            binaryWriter.Write((byte)(0b0100_0000 | (count - 1)));
            binaryWriter.Write(chars, index, count);
        }

        public void SetLcdPosition(int line, int row)
        {
            if (line < 0 || line > 1)
                throw new ArgumentOutOfRangeException(nameof(line));

            if (row < 0 || row > 15)
                throw new ArgumentOutOfRangeException(nameof(row));

            FlushWait();
            binaryWriter.Write((byte)(0b0110_0000 | line << 4 | row));
        }

        public void ClearLcdDisplay()
        {
            FlushWait();
            binaryWriter.Write((byte)0b0101_0000);
        }

        public void SetLcdBacklightColor(byte r, byte g, byte b)
        {
            FlushWait();
            binaryWriter.Write((byte)0b0101_0001);
            binaryWriter.Write(r);
            binaryWriter.Write(g);
            binaryWriter.Write(b);
        }

        public void ScrollLcdToLeft()
        {
            FlushWait();
            binaryWriter.Write((byte)0b0101_0010);
        }

        public void ScrollLcdToRight()
        {
            FlushWait();
            binaryWriter.Write((byte)0b0101_0011);
        }

        public void SetLcdCursorVisibility(bool visibility)
        {
            FlushWait();
            binaryWriter.Write(visibility ? (byte)0b0101_0101 : (byte)0b0101_0100);
        }

        public void SetLcdBlinking(bool blinking)
        {
            FlushWait();
            binaryWriter.Write(blinking ? (byte)0b0101_0110 : (byte)0b0101_0111);
        }

        public void CreateLcdChar(int number, byte[] data, int index, int count)
        {
            if (number < 0 || number > 7)
                throw new ArgumentOutOfRangeException(nameof(number));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (count != 7)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (index < 0 || index + count > data.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            FlushWait();
            binaryWriter.Write((byte)(0b0101_0000 | number));
            binaryWriter.Write(data, index, count);
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

        private void WriteHeader()
        {
            binaryWriter.Write('J');
            binaryWriter.Write('P');
            binaryWriter.Write('M');
            binaryWriter.Write(Version);
        }

        private void FlushWait()
        {
            while (waitTickQueue > 0)
            {
                var waitTick = Math.Min(waitTickQueue, 16);
                binaryWriter.Write((byte)(waitTick - 1));   // WOPx = 0, SELx = 0
                waitTickQueue -= waitTick;
            }
        }

        #endregion

        [Flags]
        private enum SelxValue : byte
        {
            Lch = 0b0001_0000,
            Rch = 0b0010_0000,
            Both = 0b0011_0000,
        }
    }
}
