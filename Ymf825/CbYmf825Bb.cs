namespace Ymf825
{
    public class CbYmf825Bb : Ymf825
    {
        #region -- Private Fields --
        
        private const int CsPin = 0x18;
        private const TargetDevice AllAvailableChip = TargetDevice.Board0 | TargetDevice.Board1;

        #endregion

        #region -- Public Properties --
        
        public override TargetDevice AvailableChip => AllAvailableChip;

        #endregion

        #region -- Constructors --

        public CbYmf825Bb(int spiDeviceIndex)
            : base(spiDeviceIndex, CsPin)
        {
            SpiInterface.SetCsTargetPin(CsPin);
        }

        #endregion
    }
}
