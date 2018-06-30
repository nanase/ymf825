namespace Ymf825.IO
{
    /// <summary>
    /// YMF825Boardステレオ化基板 (CBW-YMF825-BB) と通信するための機能を提供します。
    /// </summary>
    /// <inheritdoc />
    public class AdafruitFt232hInterface : D2xxInterface
    {
        #region -- Private Fields --

        private const int CsPin = 0x80;
        private const int CsDirection = 0xfb;

        private const int IcPin = 0x01;
        private const int IcDirection = 0xff;

        private static readonly D2xxSpiPinConfig csPinConfig = new D2xxSpiPinConfig(false, CsPin, CsDirection, false);
        private static readonly D2xxSpiPinConfig icPinConfig = new D2xxSpiPinConfig(true, IcPin, IcDirection, false);

        #endregion

        #region -- Public Properties --

        public override TargetChip AvailableChips => TargetChip.Board0;

        #endregion

        #region -- Constructors --
        
        public AdafruitFt232hInterface(int deviceIndex)
            : this(deviceIndex, csPinConfig, icPinConfig)
        {
        }

        public AdafruitFt232hInterface(int deviceIndex, D2xxSpiPinConfig csPinConfig, D2xxSpiPinConfig icPinConfig) 
            : base(deviceIndex, csPinConfig, icPinConfig)
        {
        }

        #endregion
    }
}
