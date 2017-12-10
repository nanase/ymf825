using System;

namespace Ymf825
{
    /// <summary>
    /// トーンパラメータを格納したクラスです。
    /// </summary>
    [Serializable]
    public class ToneParameter
    {
        #region -- Private Fields --

        private int basicOctave;
        private int lfoFrequency;
        private int algorithm;

        #endregion

        #region -- Public Properties --

        /// <summary>
        /// 基本オクターブ (BO) を取得または設定します。
        /// 有効範囲は 0 から 3 です。
        /// </summary>
        public int BasicOctave
        {
            get => basicOctave;
            set
            {
                if (value < 0 || value > 3)
                    throw new ArgumentOutOfRangeException(nameof(value));

                basicOctave = value;
            }
        }

        /// <summary>
        /// LFO周波数 (LFO) を取得または設定します。
        /// 有効範囲は 0 から 3 です。
        /// </summary>
        public int LfoFrequency
        {
            get => lfoFrequency;
            set
            {
                if (value < 0 || value > 3)
                    throw new ArgumentOutOfRangeException(nameof(value));

                lfoFrequency = value;
            }
        }

        /// <summary>
        /// アルゴリズム (ALG) を取得または設定します。
        /// 有効範囲は 0 から 7 です。
        /// </summary>
        public int Algorithm
        {
            get => algorithm;
            set
            {
                if (value < 0 || value > 7)
                    throw new ArgumentOutOfRangeException(nameof(value));

                algorithm = value;
            }
        }

        /// <summary>
        /// オペレータ 1 のパラメータオブジェクトを取得します。
        /// </summary>
        public OperatorParameter Operator1 { get; } = new OperatorParameter();

        /// <summary>
        /// オペレータ 2 のパラメータオブジェクトを取得します。
        /// </summary>
        public OperatorParameter Operator2 { get; } = new OperatorParameter();

        /// <summary>
        /// オペレータ 3 のパラメータオブジェクトを取得します。
        /// </summary>
        public OperatorParameter Operator3 { get; } = new OperatorParameter();

        /// <summary>
        /// オペレータ 4 のパラメータオブジェクトを取得します。
        /// </summary>
        public OperatorParameter Operator4 { get; } = new OperatorParameter();

        #endregion

        #region -- Public Methods --

        /// <summary>
        /// パラメータをバッファに書き出します。バッファには書き出しのために 30 バイト必要です。
        /// </summary>
        /// <param name="buffer">書き込まれるバッファを表す <see cref="byte"/> 型の配列。</param>
        /// <param name="offset">書き込まれるバッファの開始インデクスを表すオフセット。</param>
        public void Export(byte[] buffer, int offset)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            buffer[offset] = (byte)(basicOctave & 0x03);
            buffer[offset + 1] = (byte)((lfoFrequency & 0x03) << 6 | (algorithm & 0x07));

            Operator1.Export(buffer, offset + 2);
            Operator2.Export(buffer, offset + 9);
            Operator3.Export(buffer, offset + 16);
            Operator4.Export(buffer, offset + 23);
        }

        /// <summary>
        /// 正弦波を出力するトーンパラメータを取得します。
        /// </summary>
        /// <returns>正弦波を出力するトーンパラメータオブジェクト。</returns>
        public static ToneParameter GetSine()
        {
            var tone = new ToneParameter
            {
                BasicOctave = 1,
                Algorithm = 1
            };
            
            tone.Operator2.TotalLevel = 0;
            tone.Operator2.AttackRate = 8;
            tone.Operator2.DecayRate = 15;
            tone.Operator2.SustainRate = 0;
            tone.Operator2.ReleaseRate = 8;
            tone.Operator2.MagnificationOfFrequency = 1;

            return tone;
        }

        /// <summary>
        /// 矩形波を出力するトーンパラメータを取得します。
        /// </summary>
        /// <returns>矩形波を出力するトーンパラメータオブジェクト。</returns>
        public static ToneParameter GetSquare()
        {
            var tone = new ToneParameter
            {
                BasicOctave = 1,
                Algorithm = 1
            };

            tone.Operator2.TotalLevel = 0;
            tone.Operator2.AttackRate = 8;
            tone.Operator2.DecayRate = 15;
            tone.Operator2.SustainRate = 0;
            tone.Operator2.ReleaseRate = 8;
            tone.Operator2.MagnificationOfFrequency = 1;
            tone.Operator2.WaveShape = 6;

            return tone;
        }

        /// <summary>
        /// 鋸歯波を出力するトーンパラメータを取得します。
        /// </summary>
        /// <returns>鋸歯波を出力するトーンパラメータオブジェクト。</returns>
        public static ToneParameter GetSawtooth()
        {
            var tone = new ToneParameter
            {
                BasicOctave = 1,
                Algorithm = 1
            };

            tone.Operator2.TotalLevel = 0;
            tone.Operator2.AttackRate = 8;
            tone.Operator2.DecayRate = 15;
            tone.Operator2.SustainRate = 0;
            tone.Operator2.ReleaseRate = 8;
            tone.Operator2.MagnificationOfFrequency = 1;
            tone.Operator2.WaveShape = 24;

            return tone;
        }

        /// <summary>
        /// 三角波を出力するトーンパラメータを取得します。
        /// </summary>
        /// <returns>三角波を出力するトーンパラメータオブジェクト。</returns>
        public static ToneParameter GetTriangle()
        {
            var tone = new ToneParameter
            {
                BasicOctave = 1,
                Algorithm = 1
            };

            tone.Operator2.TotalLevel = 0;
            tone.Operator2.AttackRate = 8;
            tone.Operator2.DecayRate = 15;
            tone.Operator2.SustainRate = 0;
            tone.Operator2.ReleaseRate = 8;
            tone.Operator2.MagnificationOfFrequency = 1;
            tone.Operator2.WaveShape = 16;

            return tone;
        }

        #endregion
    }
}
