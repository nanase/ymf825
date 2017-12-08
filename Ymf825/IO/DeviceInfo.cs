using System;
using System.Runtime.InteropServices;

namespace Ymf825.IO
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DeviceListInfoNode
    {
        #region -- Public Fields --

        public readonly uint flags;
        public readonly uint type;
        public readonly uint id;
        public readonly uint locationId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public readonly string serialNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public readonly string description;

        public readonly IntPtr handle;

        #endregion
    }

    public class DeviceInfo
    {
        #region -- Private Fields --

        private readonly DeviceListInfoNode deviceInfoInternal;

        #endregion

        #region -- Public Properties --

        public int Index { get; }

        public int Flags => (int)deviceInfoInternal.flags;

        public int Type => (int)deviceInfoInternal.type;

        public int Id => (int)deviceInfoInternal.id;

        public int LocationId => (int)deviceInfoInternal.locationId;

        public string SerialNumber => deviceInfoInternal.serialNumber;

        public string Description => deviceInfoInternal.description;

        public IntPtr Handle => deviceInfoInternal.handle;

        #endregion

        #region -- Constructors --

        internal DeviceInfo(int index, DeviceListInfoNode deviceInfoInternal)
        {
            Index = index;
            this.deviceInfoInternal = deviceInfoInternal;
        }

        #endregion

        #region -- Public Methods --

        public override string ToString()
        {
            return $"{Index}: {Description} ({SerialNumber})";
        }

        #endregion
    }
}
