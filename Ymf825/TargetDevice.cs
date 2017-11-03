using System;

namespace Ymf825
{
    /// <summary>
    /// データを送信または受信するデバイスを指定するための列挙体です。
    /// 送信時は複数のデバイスを指定し、受信時は 1 つのデバイスのみ指定できます。
    /// </summary>
    [Flags]
    public enum TargetDevice : byte
    {
        /// <summary>
        /// デバイスなし。
        /// </summary>
        None = 0x00,

        /// <summary>
        /// 1つ目のデバイス。
        /// </summary>
        Ymf825Board0 = 0x01,

        /// <summary>
        /// 2つ目のデバイス。
        /// </summary>
        Ymf825Board1 = 0x02
    }
}
