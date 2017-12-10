using System.Threading;

namespace Ymf825.IO
{
    /// <summary>
    /// YMF825 と SPI による通信を行うための機能を提供します。
    /// </summary>
    /// <inheritdoc />
    public class Ymf825Spi : Spi
    {
        #region -- Constructors --

        /// <summary>
        /// パラメータを指定して新しい <see cref="Ymf825Spi"/> クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="deviceIndex">デバイスのインデクス。</param>
        /// <param name="csPin">FT シリーズにおける CS ピン位置を表す整数値。</param>
        /// <inheritdoc />
        public Ymf825Spi(int deviceIndex, byte csPin)
            : base(deviceIndex, false, csPin)
        {
            ResetHardware();
        }

        #endregion

        #region -- Public Methods --

        /// <summary>
        /// ハードウェアリセットを行います。
        /// このコマンドは即時に実行されます。
        /// </summary>
        public void ResetHardware()
        {
            QueueBuffer(0x82, 0xff, 0xff);
            QueueFlushCommand();
            SendBuffer();
            Thread.Sleep(2);

            QueueBuffer(0x82, 0x00, 0xff);
            QueueFlushCommand();
            SendBuffer();
            Thread.Sleep(2);

            QueueBuffer(0x82, 0xff, 0xff);
            QueueFlushCommand();
            SendBuffer();
        }

        #endregion
    }
}
