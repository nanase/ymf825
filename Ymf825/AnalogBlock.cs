using System;

namespace Ymf825
{
    /// <summary>
    /// YMF825 の物理ブロックを表す列挙体です。
    /// </summary>
    [Flags]
    public enum AnalogBlock : byte
    {
        /// <summary>
        /// 該当するブロックがないことを表します。
        /// </summary>
        None = 0x00,

        /// <summary>
        /// VREF および IREF ブロック。
        /// </summary>
        Ap0 = 0x01,

        /// <summary>
        /// SPAMP および SPOUT1 ブロック。
        /// </summary>
        Ap1 = 0x02,

        /// <summary>
        /// SPAMP および SPOUT2 ブロック。
        /// </summary>
        Ap2 = 0x04,

        /// <summary>
        /// DAC ブロック。
        /// </summary>
        Ap3 = 0x08,

        /// <summary>
        /// すべてのブロックを表します。
        /// </summary>
        All = 0x0f
    }
}
