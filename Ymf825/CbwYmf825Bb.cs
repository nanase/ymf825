namespace Ymf825
{
    /// <summary>
    /// YMF825Boardステレオ化基板 (CBW-YMF825-BB) と通信するための機能を提供します。
    /// </summary>
    /// <inheritdoc />
    public class CbwYmf825Bb : Ymf825
    {
        #region -- Private Fields --

        private const int CsPin = 0x18;
        private const TargetChip AllAvailableChip = TargetChip.Board0 | TargetChip.Board1;

        #endregion

        #region -- Public Properties --
        
        /// <inheritdoc />
        public override TargetChip AvailableChip => AllAvailableChip;

        #endregion

        #region -- Constructors --

        /// <summary>
        /// パラメータを指定して新しい <see cref="CbwYmf825Bb"/> クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="spiDeviceIndex">接続する SPI デバイスの番号。</param>
        public CbwYmf825Bb(int spiDeviceIndex)
            : base(spiDeviceIndex, AllAvailableChip, new IO.SpiPinConfig(true, CsPin, 0x00, false), new IO.SpiPinConfig(true, 0x00, 0x00, false))
        {
        }

        #endregion
    }
}
