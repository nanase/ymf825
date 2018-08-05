namespace Ymf825.IO
{
    /// <summary>
    /// Adafruit FT232H Breakout を介して YMF825Board と通信するための機能を提供します。
    /// </summary>
    /// <inheritdoc />
    public class AdafruitFt232HInterface : D2XxInterface
    {
        #region -- Private Fields --

        private const int CsPin = 0x80;
        private const int CsDirection = 0xfb;

        private const int IcPin = 0x01;
        private const int IcDirection = 0xff;

        private static readonly D2XxSpiPinConfig CsConfig = new D2XxSpiPinConfig(false, CsPin, CsDirection, false);
        private static readonly D2XxSpiPinConfig IcConfig = new D2XxSpiPinConfig(true, IcPin, IcDirection, false);

        #endregion

        #region -- Public Properties --

        public override TargetChip AvailableChips => TargetChip.Board0;

        #endregion

        #region -- Constructors --
        
        public AdafruitFt232HInterface(int deviceIndex)
            : this(deviceIndex, CsConfig, IcConfig)
        {
        }

        public AdafruitFt232HInterface(int deviceIndex, D2XxSpiPinConfig csPinConfig, D2XxSpiPinConfig icPinConfig) 
            : base(deviceIndex, csPinConfig, icPinConfig)
        {
        }

        #endregion
    }
}
