using System;
using System.Collections.Generic;

namespace Ymf825
{
    public class DataBurstWriteEventArgs : EventArgs
    {
        #region -- Public Properties --

        public TargetDevice TargetDevice { get; }

        public byte Address { get; }

        public IReadOnlyList<byte> Data { get; }

        public int Offset { get; }

        public int Count { get; }

        #endregion

        #region -- Constructors --

        public DataBurstWriteEventArgs(TargetDevice targetDevice, byte address, IReadOnlyList<byte> data, int offset, int count)
        {
            TargetDevice = targetDevice;
            Address = address;
            Data = data;
            Offset = offset;
            Count = count;
        }

        #endregion
    }
}
