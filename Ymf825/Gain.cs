namespace Ymf825
{
    public enum Gain : byte
    {
        /// <summary>
        /// ゲインレベル 5.0 dB。
        /// </summary>
        Level50 = 0x00,

        /// <summary>
        /// ゲインレベル 6.5 dB。
        /// </summary>
        Level65 = 0x01,

        /// <summary>
        /// ゲインレベル 7.0 dB。
        /// </summary>
        Level70 = 0x02,

        /// <summary>
        /// ゲインレベル 7.5 dB。
        /// </summary>
        Level75 = 0x03,

        /// <summary>
        /// リセット時に設定されているゲインレベル。
        /// </summary>
        Reset = Level65
    }
}
