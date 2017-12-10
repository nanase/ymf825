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

    /// <summary>
    /// 通信可能なデバイスの情報を格納したクラスです。
    /// </summary>
    public class DeviceInfo
    {
        #region -- Private Fields --

        private readonly DeviceListInfoNode deviceInfoInternal;

        #endregion

        #region -- Public Properties --

        /// <summary>
        /// デバイスと通信を開始する際に使われるインデクスを取得します。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// フラグを表す整数値を取得します。
        /// </summary>
        public int Flags => (int)deviceInfoInternal.flags;

        /// <summary>
        /// デバイスの種類を表す整数値を取得します。
        /// </summary>
        public int Type => (int)deviceInfoInternal.type;

        /// <summary>
        /// デバイスを一意に識別するための ID を取得します。
        /// </summary>
        public int Id => (int)deviceInfoInternal.id;

        /// <summary>
        /// デバイスの接続先を識別するためのロケーションIDを取得します。
        /// </summary>
        public int LocationId => (int)deviceInfoInternal.locationId;

        /// <summary>
        /// デバイスに割り当てられているシリアルナンバーを取得します。
        /// </summary>
        public string SerialNumber => deviceInfoInternal.serialNumber;

        /// <summary>
        /// デバイスの説明を取得します。
        /// </summary>
        public string Description => deviceInfoInternal.description;

        /// <summary>
        /// デバイスと通信をする際に使われているハンドルを取得します。
        /// 通信が行われていないとき、ハンドルの値は 0 となります。
        /// </summary>
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Index}: {Description} ({SerialNumber})";
        }

        #endregion
    }
}
