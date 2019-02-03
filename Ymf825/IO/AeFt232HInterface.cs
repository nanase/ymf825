namespace Ymf825.IO
{
    /// <summary>
    /// YMF825Boardステレオ化基板 (CBW-YMF825-BB) と通信するための機能を提供します。
    /// </summary>
    /// <inheritdoc />
    public class AeFt232HInterface : D2XxInterface
    {
        #region -- Private Fields --

        private const int CsPin = 0x18;
        private const int CsDirection = 0xfb;

        private const int IcPin = 0xff;
        private const int IcDirection = 0xff;

        private static readonly D2XxSpiPinConfig CsConfig = new D2XxSpiPinConfig(false, CsPin, CsDirection, false);
        private static readonly D2XxSpiPinConfig IcConfig = new D2XxSpiPinConfig(true, IcPin, IcDirection, false);

        #endregion

        #region -- Public Properties --

        public override TargetChip AvailableChips => TargetChip.Board0 | TargetChip.Board1;

        public override bool SupportReadOperation { get; } = true;

        public override bool SupportHardwareReset { get; } = true;

        #endregion

        #region -- Constructors --

        public AeFt232HInterface(int deviceIndex)
            : this(deviceIndex, CsConfig, IcConfig)
        {
        }

        public AeFt232HInterface(int deviceIndex, D2XxSpiPinConfig csPinConfig, D2XxSpiPinConfig icPinConfig) 
            : base(deviceIndex, csPinConfig, icPinConfig)
        {
        }

        #endregion
    }
}
