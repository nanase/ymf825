using System;

namespace Ymf825
{
    public class DataTransferedEventArgs : EventArgs
    {
        #region -- Public Properties --

        public TargetDevice TargetDevice { get; }

        public byte Address { get; }

        public byte Data { get; }

        #endregion

        #region -- Constructors --

        public DataTransferedEventArgs(TargetDevice targetDevice, byte address, byte data)
        {
            TargetDevice = targetDevice;
            Address = address;
            Data = data;
        }

        #endregion
    }
}
