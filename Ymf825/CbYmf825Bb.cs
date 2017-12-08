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

        public override void ChangeTargetDevice(TargetDevice target)
        {
            var targetValue = (int)target;

            if (targetValue == 0 || targetValue > 0x03)
                throw new ArgumentOutOfRangeException(nameof(target));

            SpiInterface.SetCsTargetPin((byte)(targetValue << 3));
        }
    }
}
