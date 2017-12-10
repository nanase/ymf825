using System;

namespace Ymf825
{
    /// <summary>
    /// 各ボイスのトーンパラメータを格納するクラスです。
    /// </summary>
    [Serializable]
    public class ToneParameterCollection
    {
        #region -- Private Fields --

        private readonly ToneParameter[] parameters;

        #endregion

        #region -- Public Indexer --

        /// <summary>
        /// 指定したインデクスのトーンパラメータオブジェクトを取得または設定します。
        /// </summary>
        /// <param name="index">ボイス番号。有効範囲は 0 から 15 です。</param>
        /// <returns><see cref="ToneParameter"/> クラスのインスタンス。</returns>
        public ToneParameter this[int index]
        {
            get
            {
                if (index < 0 || index > 15)
                    throw new IndexOutOfRangeException();

                return parameters[index];
            }
            set
            {
                if (index < 0 || index > 15)
                    throw new IndexOutOfRangeException();

                parameters[index] = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        #endregion

        #region -- Constructors --

        /// <summary>
        /// 新しい <see cref="ToneParameterCollection"/> クラスのインスタンスを初期化します。
        /// </summary>
        public ToneParameterCollection()
        {
            parameters = new ToneParameter[16];

            for (var i = 0; i < 16; i++)
                parameters[i] = new ToneParameter();
        }

        #endregion

        #region -- Public Methods --

        /// <summary>
        /// オブジェクトの内容を配列に書き出します。
        /// </summary>
        /// <param name="buffer">書き出し先の <see cref="byte"/> 型の配列。</param>
        /// <param name="offset">書き出しを開始するインデクス。</param>
        /// <param name="targetToneNumber">書き出し対象となるトーンパラメータのボイス番号。
        /// 指定された番号までのトーンパラメータが書き出されます。</param>
        /// <returns>書き出されたバイト数。</returns>
        public int Export(byte[] buffer, int offset, int targetToneNumber = 15)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            
            if (targetToneNumber < 0 || targetToneNumber > 15)
                throw new IndexOutOfRangeException();

            var maxToneNumber = targetToneNumber + 1;
            var requiredLength = 1 + 30 * maxToneNumber + 4;

            if (offset + requiredLength > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(buffer), $"バッファの長さが足りません。{requiredLength} バイト必要です。");

            // start header
            buffer[offset] = (byte)(0x80 + maxToneNumber);

            // sequence data
            for (var i = 0; i < maxToneNumber; i++)
                parameters[i].Export(buffer, offset + 1 + 30 * i);

            // end footer
            buffer[offset + 1 + 30 * maxToneNumber] = 0x80;
            buffer[offset + 1 + 30 * maxToneNumber + 1] = 0x03;
            buffer[offset + 1 + 30 * maxToneNumber + 2] = 0x81;
            buffer[offset + 1 + 30 * maxToneNumber + 3] = 0x80;

            return requiredLength;
        }

        #endregion
    }
}
