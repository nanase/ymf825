using System;

namespace Ymf825.Driver
{
    public partial class Ymf825Driver
    {
        #region -- Private Fields --

        private static readonly double[] FnumTable = new double[12];

        private static readonly double[] AttackRateTimeTable = {
            double.PositiveInfinity,
            double.PositiveInfinity,
            double.PositiveInfinity,
            double.PositiveInfinity,
            2942.78e-3,
            2354.23e-3,
            1961.86e-3,
            1681.59e-3,
            1471.39e-3,
            1177.11e-3,

            980.92e-3,  // 10
            840.80e-3,
            735.69e-3,
            588.55e-3,
            490.47e-3,
            420.40e-3,
            367.85e-3,
            294.28e-3,
            245.23e-3,
            210.20e-3,

            183.92e-3,  // 20
            147.14e-3,
            122.61e-3,
            105.09e-3,
            91.97e-3,
            73.57e-3,
            61.31e-3,
            52.55e-3,
            45.98e-3,
            36.79e-3,

            30.66e-3,   // 30
            26.28e-3,
            22.99e-3,
            18.39e-3,
            15.32e-3,
            13.14e-3,
            11.49e-3,
            9.19e-3,
            7.66e-3,
            6.56e-3,

            5.75e-3,    // 40
            4.60e-3,
            3.83e-3,
            3.28e-3,
            2.88e-3,
            2.23e-3,
            1.91e-3,
            1.65e-3,
            1.44e-3,
            1.15e-3,

            0.96e-3,    // 50
            0.82e-3,
            0.71e-3,
            0.62e-3,
            0.53e-3,
            0.43e-3,
            0.38e-3,
            0.34e-3,
            0.30e-3,
            0.26e-3,

            0.0,    // 60
            0.0,
            0.0,
            0.0,
            0.0
        };

        private static readonly double[] EnvelopeRateTimeTable = {
            double.PositiveInfinity,
            double.PositiveInfinity,
            double.PositiveInfinity,
            double.PositiveInfinity,
            43008.00e-3,
            34406.40e-3,
            28672.00e-3,
            24576.00e-3,
            21504.00e-3,
            17203.20e-3,

            14336.00e-3,    // 10
            12288.00e-3,
            10752.01e-3,
            8601.60e-3,
            7168.00e-3,
            6144.00e-3,
            5376.00e-3,
            4300.80e-3,
            3584.00e-3,
            3072.00e-3,

            2688.00e-3,     // 20
            2150.41e-3,
            1792.00e-3,
            1536.00e-3,
            1344.00e-3,
            1075.20e-3,
            896.00e-3,
            768.00e-3,
            672.00e-3,
            537.60e-3,

            448.00e-3,  // 30
            384.00e-3,
            336.00e-3,
            268.80e-3,
            224.00e-3,
            192.00e-3,
            168.00e-3,
            134.00e-3,
            112.00e-3,
            96.00e-3,

            84.00e-3,   // 40
            67.20e-3,
            56.00e-3,
            48.00e-3,
            42.00e-3,
            33.60e-3,
            28.00e-3,
            24.00e-3,
            21.00e-3,
            16.80e-3,

            14.00e-3,   // 50
            12.00e-3,
            10.50e-3,
            8.40e-3,
            7.00e-3,
            6.01e-3,
            5.25e-3,
            4.20e-3,
            3.50e-3,
            3.00e-3,

            2.63e-3,    // 60
            2.63e-3,
            2.63e-3,
            2.63e-3,
            2.63e-3
        };

        #endregion

        #region -- Constructors --

        static Ymf825Driver()
        {
            for (var i = 0; i < 12; i++)
                FnumTable[i] = CalcFnum(440.0 * Math.Pow(2.0, (48.0 + i - 69.0) / 12.0), 3);
        }

        #endregion

        #region -- Static Methods --

        /// <summary>
        /// 周波数と BLOCK 値から FNUM 値を求めます。
        /// </summary>
        /// <param name="frequency">発音される周波数 (Hz)。</param>
        /// <param name="block">オクターブを表す BLOCK 値。</param>
        /// <returns>FNUM 値。</returns>
        public static double CalcFnum(double frequency, int block)
        {
            // Original Formula
            // const fnum = (freq * Math.pow(2, 19)) / (Math.pow(2, block - 1) * 48000);
            return Math.Pow(2.0, 13 - block) * frequency / 375.0;
        }

        /// <summary>
        /// FNUM 値と BLOCK 値から発音される周波数を求めます。
        /// </summary>
        /// <param name="fnum">FNUM 値。</param>
        /// <param name="block">オクターブを表す BLOCK 値。</param>
        /// <returns>発音される周波数 (Hz)。</returns>
        public static double CalcFrequency(double fnum, double block)
        {
            // Original Formula
            // const freq = (48000 * Math.pow(2, block - 1) * fnum) / Math.pow(2, 19);
            return 375.0 * Math.Pow(2.0, block - 13) * fnum;
        }

        /// <summary>
        /// MIDI ノートナンバーから FNUM 値と BLOCK 値を求めます。
        /// </summary>
        /// <param name="key">元になる MIDI ノートナンバー。</param>
        /// <param name="fnum">FNUM 値。</param>
        /// <param name="block">オクターブを表す BLOCK 値。</param>
        /// <param name="correction">理想周波数と FNUM および BLOCK 値によって発音される周波数との誤差補正値。</param>
        public static void GetFnumAndBlock(int key, out double fnum, out int block, out double correction)
        {
            if (key < 0 || key > 127)
                throw new ArgumentOutOfRangeException(nameof(key));

            var blockMod = 1.0;
            block = key / 12 - 2;

            if (block < 0)
            {
                blockMod = Math.Pow(2.0, block);
                block = 0;
            }
            else if (block > 6)
            {
                blockMod = Math.Pow(2.0, block - 6);
                block = 6;
            }

            fnum = FnumTable[key % 12] * blockMod;
            var idealFreq = CalcFrequency(fnum, block);

            if (fnum > 1023.0)
                fnum = 1023.0;

            correction = idealFreq / CalcFrequency(Math.Round(fnum), block);
        }

        /// <summary>
        /// 周波数倍率をレジスタに書き込める値に変換します。
        /// </summary>
        /// <param name="multiplier">周波数倍率。有効範囲は 0.0 から 4.0 未満です。</param>
        /// <param name="integer">周波数倍率の整数値部分 (INT)。</param>
        /// <param name="fraction">周波数倍率の小数値部分 (FRAC)。</param>
        public static void ConvertForFrequencyMultiplier(double multiplier, out int integer, out int fraction)
        {
            if (multiplier >= 4.0 || multiplier < 0.0)
                throw new ArgumentOutOfRangeException(nameof(multiplier));

            multiplier = Math.Round(multiplier * 512.0);

            if (multiplier >= 4.0 * 512.0)
            {
                integer = 3;
                fraction = 511;
            }
            else
            {
                integer = (int)(multiplier / 512.0);
                fraction = (int)(multiplier - integer * 512);
            }
        }

        /// <summary>
        /// エンベロープジェネレータの各値を求めるための Rof 値を計算します。
        /// </summary>
        /// <param name="ksr">キースケールセンシティビティ (KSR)。</param>
        /// <param name="block">BLOCK 値。</param>
        /// <param name="basicOctave">基本オクターブの値。</param>
        /// <param name="fnum">FNUM 値。</param>
        /// <returns>時間値のオフセットを表す Rof の値。</returns>
        public static int CalcRof(bool ksr, int block, int basicOctave, int fnum)
        {
            if (ksr)
                return (block + basicOctave) * 2 + (fnum >= 512 ? 1 : 0);

            return (block + basicOctave) / 2;
        }

        /// <summary>
        /// アタック時間を計算します。
        /// </summary>
        /// <param name="attackRate">アタック値。</param>
        /// <param name="rof"><see cref="CalcRof"/> メソッドで算出した Rof 値。</param>
        /// <returns>アタック時間。単位は秒 (s) です。</returns>
        public static double CalcAttackRateTime(int attackRate, int rof)
        {
            if (attackRate < 0 || attackRate > 15)
                throw new ArgumentOutOfRangeException(nameof(attackRate));

            if (rof < 0 || rof > 15)
                throw new ArgumentOutOfRangeException(nameof(rof));

            var index = attackRate * 4 + rof;

            if (index > 64)
                index = 64;

            return AttackRateTimeTable[index];
        }

        /// <summary>
        /// ディケイ、サスティンまたはリリース時間を計算します。
        /// </summary>
        /// <param name="rate">ディケイ、サスティンまたはリリース値。</param>
        /// <param name="rof"><see cref="CalcRof"/> メソッドで算出した Rof 値。</param>
        /// <returns>アタック時間。単位は秒 (s) です。</returns>
        public static double CalcEnvelopeRateTime(int rate, int rof)
        {
            if (rate < 0 || rate > 15)
                throw new ArgumentOutOfRangeException(nameof(rate));

            if (rof < 0 || rof > 15)
                throw new ArgumentOutOfRangeException(nameof(rof));

            var index = rate * 4 + rof;

            if (index > 64)
                index = 64;

            return EnvelopeRateTimeTable[index];
        }

        #endregion
    }
}
