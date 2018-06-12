using System;
using Ymf825.IO;

namespace Ymf825
{
    /// <summary>
    /// YMF825 と通信するための機能を提供します。
    /// </summary>
    /// <inheritdoc />
    public abstract class Ymf825 : IDisposable
    {
        #region -- Private Fields --

        private readonly object lockObject = new object();

        #endregion

        #region -- Public Properties --

        /// <summary>
        /// YMF825 との通信に使われる SPI インタフェースのオブジェクトを取得します。
        /// </summary>
        public Ymf825Spi SpiInterface { get; }

        /// <summary>
        /// このオブジェクトが破棄されたかを表す真偽値を取得します。
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// サポートされている YMF825 の組み合わせを表す <see cref="TargetChip"/> 列挙体の組み合わせを取得します。
        /// </summary>
        public abstract TargetChip AvailableChip { get; }

        /// <summary>
        /// 書き込み命令で対象となる、現在の YMF825 の組み合わせを表す <see cref="TargetChip"/> 列挙体の組み合わせを取得します。
        /// </summary>
        public TargetChip CurrentTargetChip { get; private set; }

        /// <summary>
        /// 書き込み命令を即時に送信し、YMF825 に実行させるかを表す真偽値を取得します。
        /// </summary>
        public bool AutoFlush { get; set; } = true;

        /// <summary>
        /// レジスタに書き込んだバイト数を取得します。
        /// </summary>
        public long WriteBytes { get; private set; }

        /// <summary>
        /// レジスタに BurstWrite コマンドで書き込んだバイト数を取得します。
        /// </summary>
        public long BurstWriteBytes { get; private set; }

        /// <summary>
        /// レジスタを読み込んだバイト数を取得します。
        /// </summary>
        public long ReadBytes { get; private set; }

        /// <summary>
        /// レジスタに書き込んだ回数を取得します。
        /// </summary>
        public long WriteCommandCount { get; private set; }

        /// <summary>
        /// レジスタに BurstWrite コマンドで書き込んだ回数を取得します。
        /// </summary>
        public long BurstWriteCommandCount { get; private set; }

        /// <summary>
        /// レジスタを読み込んだ回数を取得します。
        /// </summary>
        public long ReadCommandCount { get; private set; }

        #endregion

        #region -- Public Events --

        public event EventHandler<DataTransferedEventArgs> DataWrote;

        public event EventHandler<DataBurstWriteEventArgs> DataBurstWrote;

        public event EventHandler<DataTransferedEventArgs> DataRead;

        #endregion

        #region -- Constructors --

        protected Ymf825(int spiDeviceIndex, TargetChip availableChips, SpiPinConfig csPinConfig, SpiPinConfig icPinConfig)
        {
            SpiInterface = new Ymf825Spi(spiDeviceIndex, csPinConfig, icPinConfig);
            SpiInterface.SetTarget(availableChips);
            CurrentTargetChip = AvailableChip;
        }

        #endregion

        #region -- Public Methods --

        /// <summary>
        /// 書き込み命令の送信キューをフラッシュし、YMF825 に即時に実行させます。
        /// </summary>
        public void Flush()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            lock (lockObject)
                SpiInterface.Flush();
        }

        /// <summary>
        /// アドレスとデータの書き込み命令を送信キューに追加します。
        /// </summary>
        /// <param name="address">YMF825 のレジスタ番号。範囲は 0x00 から 0x7f です。</param>
        /// <param name="data">レジスタに書き込まれるデータ。</param>
        public virtual void Write(byte address, byte data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if (address >= 0x80)
                throw new ArgumentOutOfRangeException(nameof(address));

            lock (lockObject)
            {
                SpiInterface.Write(address, data);
                WriteBytes += 2;
                WriteCommandCount++;
                DataWrote?.Invoke(this, new DataTransferedEventArgs(CurrentTargetChip, address, data));

                if (AutoFlush)
                    SpiInterface.Flush();
            }
        }

        /// <summary>
        /// アドレスと可変長データの書き込み命令を送信キューに追加します。
        /// </summary>
        /// <param name="address">YMF825 のレジスタ番号。範囲は 0x00 から 0x7f です。</param>
        /// <param name="data">可変長データが格納された <see cref="byte"/> 型の配列。</param>
        /// <param name="offset">配列の読み出しを開始するインデクス。</param>
        /// <param name="count">配列を読み出すバイト数。</param>
        public virtual void BurstWrite(byte address, byte[] data, int offset, int count)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if (address >= 0x80)
                throw new ArgumentOutOfRangeException(nameof(address));

            lock (lockObject)
            {
                SpiInterface.BurstWrite(address, data, offset, count);

                WriteBytes++;
                BurstWriteBytes += count;
                BurstWriteCommandCount++;
                
                DataBurstWrote?.Invoke(this, new DataBurstWriteEventArgs(CurrentTargetChip, address, data, offset, count));

                if (AutoFlush)
                    SpiInterface.Flush();
            }
        }

        /// <summary>
        /// 指定したアドレスを読み出す命令を実行します。
        /// この命令は即時に実行されます。
        /// </summary>
        /// <param name="address">YMF825 のレジスタ番号。範囲は 0x00 から 0x7f です。</param>
        /// <returns>YMF825 から返されたデータ。</returns>
        public virtual byte Read(byte address)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if (address >= 0x80)
                throw new ArgumentOutOfRangeException(nameof(address));

            lock (lockObject)
            {
                var result = SpiInterface.Read((byte)(address | 0x80));

                WriteBytes++;
                ReadBytes++;
                ReadCommandCount++;

                DataRead?.Invoke(this, new DataTransferedEventArgs(CurrentTargetChip, address, result));

                return result;
            }
        }

        /// <summary>
        /// YMF825 をハードウェアリセットします。
        /// この命令は即時に実行されます。
        /// </summary>
        public virtual void ResetHardware()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            lock (lockObject)
                SpiInterface.ResetHardware();
        }

        /// <summary>
        /// 書き込み命令の対象となる YMF825 の組み合わせを設定します。
        /// </summary>
        /// <param name="target">YMF825 の組み合わせが格納された <see cref="TargetChip"/> 列挙体の値。</param>
        public void ChangeTargetDevice(TargetChip target)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            var targetValue = (int)target;

            if (targetValue == 0 || targetValue > (int)AvailableChip)
                throw new ArgumentOutOfRangeException(nameof(target));

            lock (lockObject)
            {
                CurrentTargetChip = target;
                SpiInterface.SetTarget(target);
            }
        }

        /// <inheritdoc />
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

            if (disposing)
            {
                SpiInterface.Dispose();
            }

            IsDisposed = true;
        }

        #endregion
    }
}
