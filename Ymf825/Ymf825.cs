using System;
using Ymf825.IO;

namespace Ymf825
{
    public abstract class Ymf825 : IDisposable
    {
        #region -- Private Fields --

        private readonly object lockObject = new object();

        #endregion

        #region -- Public Properties --

        public Ymf825Spi SpiInterface { get; }

        public bool IsDisposed { get; protected set; }

        public abstract TargetChip AvailableChip { get; }

        public TargetChip CurrentTargetChip { get; private set; }

        public bool AutoFlush { get; set; } = true;

        #endregion

        #region -- Constructors --

        protected Ymf825(int spiDeviceIndex, byte csPin)
        {
            SpiInterface = new Ymf825Spi(spiDeviceIndex, csPin);
            SpiInterface.SetCsTargetPin(csPin);
            CurrentTargetChip = AvailableChip;
        }

        #endregion

        #region -- Public Methods --

        public void Flush()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            lock (lockObject)
                SpiInterface.Flush();
        }

        public virtual void Write(byte address, byte data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if (address >= 0x80)
                throw new ArgumentOutOfRangeException(nameof(address));

            lock (lockObject)
            {
                SpiInterface.Write(address, data);

                if (AutoFlush)
                    SpiInterface.Flush();
            }
        }

        public virtual void BurstWrite(byte address, byte[] data, int offset, int count)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if (address >= 0x80)
                throw new ArgumentOutOfRangeException(nameof(address));

            lock (lockObject)
            {
                SpiInterface.BurstWrite(address, data, offset, count);

                if (AutoFlush)
                    SpiInterface.Flush();
            }
        }

        public virtual byte Read(byte address)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            if (address >= 0x80)
                throw new ArgumentOutOfRangeException(nameof(address));

            lock (lockObject)
                return SpiInterface.Read((byte)(address | 0x80));
        }

        public virtual void ResetHardware()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            lock (lockObject)
                SpiInterface.ResetHardware();
        }

        public void ChangeTargetDevice(TargetChip target)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString());

            var targetValue = (int) target;

            if (targetValue == 0 || targetValue > (int) AvailableChip)
                throw new ArgumentOutOfRangeException(nameof(target));

            lock (lockObject)
            {
                CurrentTargetChip = target;
                SpiInterface.SetCsTargetPin((byte) (targetValue << 3));
            }
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
