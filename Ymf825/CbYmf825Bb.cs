namespace Ymf825
{
    public class CbYmf825Bb : Ymf825
    {
        #region -- Private Fields --
        
        private const int CsPin = 0x18;
        private const TargetChip AllAvailableChip = TargetChip.Board0 | TargetChip.Board1;

        #endregion

        #region -- Public Properties --
        
        public override TargetChip AvailableChip => AllAvailableChip;

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
