using System;
using Ymf825.IO;

namespace Ymf825
{
    public abstract class Ymf825 : IDisposable
    {
        #region -- Public Properties --
        
        public Ymf825Spi SpiInterface { get; }
        
        public bool IsDisposed { get; protected set; }

        public abstract TargetChip AvailableChip { get; }

        #endregion

        #region -- Constructors --

        protected Ymf825(int spiDeviceIndex, byte csPin)
        {
            SpiInterface = new Ymf825Spi(spiDeviceIndex, csPin);
        }

        #endregion

        #region -- Public Methods --
        
        public virtual void Write(byte address, byte data)
        {
            if (address >= 0x80)
                throw new ArgumentOutOfRangeException(nameof(address));

            SpiInterface.Write(address, data);
        }

        public virtual void BurstWrite(byte address, byte[] data, int offset, int count)
        {
            if (address >= 0x80)
                throw new ArgumentOutOfRangeException(nameof(address));

            SpiInterface.BurstWrite(address, data, offset, count);
        }

        public virtual byte Read(byte address)
        {
            if (address >= 0x80)
                throw new ArgumentOutOfRangeException(nameof(address));

            return SpiInterface.Read((byte)(address | 0x80));
        }

        public virtual void ResetHardware()
        {
            SpiInterface.ResetHardware();
        }

        public virtual void ChangeTargetDevice(TargetChip target)
        {
            var targetValue = (int)target;

            if (targetValue == 0 || targetValue > (int)AvailableChip)
                throw new ArgumentOutOfRangeException(nameof(target));

            SpiInterface.SetCsTargetPin((byte)(targetValue << 3));
        }

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
