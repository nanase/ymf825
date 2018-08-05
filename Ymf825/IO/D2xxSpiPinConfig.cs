namespace Ymf825.IO
{
    public class D2XxSpiPinConfig
    {
        #region -- Public Properties --

        /// <summary>
        /// 割り当て割り当て可能なピン 16 ビットのうち、この設定のピンが上位 8 ビットを表すかの真偽値を取得します。
        /// </summary>
        public bool IsHighByte { get; }

        /// <summary>
        /// ピンの出力の値を取得します。
        /// </summary>
        public byte Value { get; }

        /// <summary>
        /// ピンの入出力方向を取得します。
        /// </summary>
        public byte Direction { get; }

        /// <summary>
        /// ピンの出力が H レベルのとき、有効を表すかの真偽値を取得します。
        /// </summary>
        public bool HighLevelToEnable { get; } 

        #endregion

        #region -- Constructors --

        public D2XxSpiPinConfig(bool isHighByte, byte value, byte direction, bool highLevelToEnable)
        {
            IsHighByte = isHighByte;
            Value = value;
            Direction = direction;
            HighLevelToEnable = highLevelToEnable;
        }

        #endregion
    }
}
