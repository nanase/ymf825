using System;

namespace Ymf825
{
    public class CbYmf825Bb : Ymf825
    {
        public CbYmf825Bb(int spiDeviceIndex)
            : base(spiDeviceIndex)
        {
            SpiInterface.SetCsTargetPin(0x18);
        }
        private const TargetDevice AllAvailableChip = TargetDevice.Board0 | TargetDevice.Board1;

        public override TargetDevice AvailableChip => AllAvailableChip;


        }
    }
}
