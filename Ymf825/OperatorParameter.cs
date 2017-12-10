using System;

namespace Ymf825
{
    /// <summary>
    /// FM音源のオペレータのパラメータを格納したクラスです。
    /// </summary>
    [Serializable]
    public class OperatorParameter
    {
        #region -- Private Fields --

        private int sustainRate;

        private int releaseRate;
        private int decayRate;

        private int attackRate;
        private int sustainLevel;

        private int totalLevel;
        private int keyScalingLevel;

        private int amplitudeModurationDepth;
        private int vibratoDepth;

        private int magnificationOfFrequency;
        private int detune;

        private int waveShape;
        private int feedbackLevel;

        #endregion

        #region -- Public Properties --

        /// <summary>
        /// サスティンレート (SR) を取得または設定します。
        /// 有効範囲は 0 から 15 です。0 のとき、音量は保持状態となります。
        /// </summary>
        public int SustainRate
        {
            get => sustainRate;
            set
            {
                if (value < 0 || value > 15)
                    throw new ArgumentOutOfRangeException(nameof(value));

                sustainRate = value;
            }
        }

        /// <summary>
        /// キーオフを無視 (XOF) するかどうかを表す真偽値を取得または設定します。
        /// true のとき、キーオフ時に状態を変化させません。
        /// </summary>
        public bool IgnoreKeyOff { get; set; }

        /// <summary>
        /// キースケールセンシティビティ (KSR) を有効化するかどうかを表す真偽値を取得または設定します。
        /// true のとき、Fnum の値に応じてエンベロープジェネレータの値が変化します。
        /// </summary>
        public bool EnableKeyScaleSensitivity { get; set; }

        /// <summary>
        /// リリースレート (RR) を取得または設定します。
        /// 有効範囲は 0 から 15 です。
        /// </summary>
        public int ReleaseRate
        {
            get => releaseRate;
            set
            {
                if (value < 0 || value > 15)
                    throw new ArgumentOutOfRangeException(nameof(value));

                releaseRate = value;
            }
        }

        /// <summary>
        /// ディケイレート (DR) を取得または設定します。
        /// 有効範囲は 0 から 15 です。
        /// </summary>
        public int DecayRate
        {
            get => decayRate;
            set
            {
                if (value < 0 || value > 15)
                    throw new ArgumentOutOfRangeException(nameof(value));

                decayRate = value;
            }
        }

        /// <summary>
        /// アタックレート (AR) を取得または設定します。
        /// 有効範囲は 0 から 15 です。
        /// </summary>
        public int AttackRate
        {
            get => attackRate;
            set
            {
                if (value < 0 || value > 15)
                    throw new ArgumentOutOfRangeException(nameof(value));

                attackRate = value;
            }
        }

        /// <summary>
        /// サスティンレベル (SL) を取得または設定します。
        /// 有効範囲は 0 から 15 です。
        /// </summary>
        public int SustainLevel
        {
            get => sustainLevel;
            set
            {
                if (value < 0 || value > 15)
                    throw new ArgumentOutOfRangeException(nameof(value));

                sustainLevel = value;
            }
        }

        /// <summary>
        /// トータルレベル (TL) を取得または設定します。
        /// 有効範囲は 0 から 63 です。
        /// </summary>
        public int TotalLevel
        {
            get => totalLevel;
            set
            {
                if (value < 0 || value > 63)
                    throw new ArgumentOutOfRangeException(nameof(value));

                totalLevel = value;
            }
        }

        /// <summary>
        /// キースケールレベル (KSL) を取得または設定します。
        /// 有効範囲は 0 から 3 です。
        /// </summary>
        public int KeyScalingLevel
        {
            get => keyScalingLevel;
            set
            {
                if (value < 0 || value > 3)
                    throw new ArgumentOutOfRangeException(nameof(value));

                keyScalingLevel = value;
            }
        }

        /// <summary>
        /// 振幅変調を有効化 (EAM) するかどうかを表す真偽値を取得または設定します。
        /// </summary>
        public bool EnableAmplitudeModuration { get; set; }

        /// <summary>
        /// 振幅変調の深さ (DAM) を取得または設定します。
        /// 有効範囲は 0 から 3 です。
        /// </summary>
        public int AmplitudeModurationDepth
        {
            get => amplitudeModurationDepth;
            set
            {
                if (value < 0 || value > 3)
                    throw new ArgumentOutOfRangeException(nameof(value));

                amplitudeModurationDepth = value;
            }
        }

        /// <summary>
        /// ビブラートを有効化 (EVB) するかどうかを表す真偽値を取得または設定します。。
        /// </summary>
        public bool EnableVibrato { get; set; }

        /// <summary>
        /// ビブラートの深さ (DVB) を取得または設定します。
        /// 有効範囲は 0 から 3 です。
        /// </summary>
        public int VibratoDepth
        {
            get => vibratoDepth;
            set
            {
                if (value < 0 || value > 3)
                    throw new ArgumentOutOfRangeException(nameof(value));

                vibratoDepth = value;
            }
        }

        /// <summary>
        /// 周波数の倍率 (MULTI) を取得または設定します。
        /// 有効範囲は 0 から 15 です。
        /// </summary>
        public int MagnificationOfFrequency
        {
            get => magnificationOfFrequency;
            set
            {
                if (value < 0 || value > 15)
                    throw new ArgumentOutOfRangeException(nameof(value));

                magnificationOfFrequency = value;
            }
        }

        /// <summary>
        /// デチューン (DT) を取得または設定します。
        /// 有効範囲は 0 から 7 です。
        /// </summary>
        public int Detune
        {
            get => detune;
            set
            {
                if (value < 0 || value > 7)
                    throw new ArgumentOutOfRangeException(nameof(value));

                detune = value;
            }
        }

        /// <summary>
        /// ジェネレータの波形 (WS) を取得または設定します。
        /// 有効範囲は 0 から 31 です。
        /// </summary>
        public int WaveShape
        {
            get => waveShape;
            set
            {
                if (value < 0 || value > 31)
                    throw new ArgumentOutOfRangeException(nameof(value));

                waveShape = value;
            }
        }

        /// <summary>
        /// フィードバックレベル (FB) を取得または設定します。
        /// 有効範囲は 0 から 7 です。
        /// </summary>
        public int FeedbackLevel
        {
            get => feedbackLevel;
            set
            {
                if (value < 0 || value > 7)
                    throw new ArgumentOutOfRangeException(nameof(value));

                feedbackLevel = value;
            }
        }

        #endregion

        #region -- Public Methods --

        /// <summary>
        /// パラメータをバッファに書き出します。バッファには書き込みのために 7 バイト必要です。
        /// </summary>
        /// <param name="buffer">書き込まれるバッファを表す <see cref="byte"/> 型の配列。</param>
        /// <param name="offset">書き込まれるバッファの開始インデクスを表すオフセット。</param>
        public void Export(byte[] buffer, int offset)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            buffer[offset] = (byte)((sustainRate & 0x0f) << 4 |
                                    (IgnoreKeyOff ? 0x08 : 0x00) |
                                    (EnableKeyScaleSensitivity ? 0x01 : 0x00));
            buffer[offset + 1] = (byte)((releaseRate & 0x0f) << 4 | (decayRate & 0x0f));
            buffer[offset + 2] = (byte)((attackRate & 0x0f) << 4 | (sustainLevel & 0x0f));
            buffer[offset + 3] = (byte)((totalLevel & 0x3f) << 2 | (keyScalingLevel & 0x03));
            buffer[offset + 4] = (byte)((amplitudeModurationDepth & 0x03) << 5 |
                                        (EnableAmplitudeModuration ? 0x10 : 0x00) |
                                        (vibratoDepth & 0x03) << 1 |
                                        (EnableVibrato ? 0x01 : 0x00));
            buffer[offset + 5] = (byte)((magnificationOfFrequency & 0x0f) << 4 | (detune & 0x07));
            buffer[offset + 6] = (byte)((waveShape & 0x1f) << 3 | (feedbackLevel & 0x07));
        }

        #endregion
    }
}
