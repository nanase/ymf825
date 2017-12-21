using System;
using static System.Math;

namespace Ymf825
{
    public static class FilterCoefficients
    {
        // フィルタ係数生成式
        // 参考: http://vstcpp.wpblog.jp/?page_id=523

        #region -- Private Fields --

        private const double SoundChipSamplingRate = 48000.0;

        private const double DefaultQ = 0.7071067811865476;

        #endregion

        #region -- Public Methods --

        public static double[] Lowpass(double cutoff, double q = DefaultQ)
        {
            if (cutoff < 0.0 || cutoff > SoundChipSamplingRate / 2.0)
                throw new ArgumentOutOfRangeException(nameof(cutoff));

            if (q < 0.0)
                throw new ArgumentOutOfRangeException(nameof(q));

            var omega = 2.0 * PI * cutoff / SoundChipSamplingRate;
            var alpha = Sin(omega) / (2.0 * q);

            return NormalizeFilter(
                 1.0 + alpha,               // a0
                -2.0 * Cos(omega),          // a1
                 1.0 - alpha,               // a2
                (1.0 - Cos(omega)) / 2.0,   // b0
                 1.0 - Cos(omega),          // b1
                (1.0 - Cos(omega)) / 2.0    // b2
            );
        }

        public static double[] Highpass(double cutoff, double q = DefaultQ)
        {
            if (cutoff < 0.0 || cutoff > SoundChipSamplingRate / 2.0)
                throw new ArgumentOutOfRangeException(nameof(cutoff));

            if (q < 0.0)
                throw new ArgumentOutOfRangeException(nameof(q));

            var omega = 2.0 * PI * cutoff / SoundChipSamplingRate;
            var alpha = Sin(omega) / (2.0 * q);

            return NormalizeFilter(
                  1.0 + alpha,               // a0
                 -2.0 * Cos(omega),          // a1
                  1.0 - alpha,               // a2
                 (1.0 + Cos(omega)) / 2.0,   // b0
                -(1.0 + Cos(omega)),         // b1
                 (1.0 + Cos(omega)) / 2.0    // b2
            );
        }

        public static double[] Bandpass(double cutoff, double bandwidth, double q = DefaultQ)
        {
            if (cutoff < 0.0 || cutoff > SoundChipSamplingRate / 2.0)
                throw new ArgumentOutOfRangeException(nameof(cutoff));

            if (bandwidth < 0.0)
                throw new ArgumentOutOfRangeException(nameof(bandwidth));

            if (q < 0.0)
                throw new ArgumentOutOfRangeException(nameof(q));

            var omega = 2.0 * PI * cutoff / SoundChipSamplingRate;
            var alpha = Sin(omega) * Sinh(Log(2.0) / 2.0 * bandwidth * omega / Sin(omega));

            return NormalizeFilter(
                 1.0 + alpha,       // a0
                -2.0 * Cos(omega),  // a1
                 1.0 - alpha,       // a2
                 alpha * q,         // b0
                 0.0,               // b1
                -alpha * q          // b2
            );
        }

        public static double[] Bandstop(double cutoff, double bandwidth)
        {
            if (cutoff < 0.0 || cutoff > SoundChipSamplingRate / 2.0)
                throw new ArgumentOutOfRangeException(nameof(cutoff));

            if (bandwidth < 0.0)
                throw new ArgumentOutOfRangeException(nameof(bandwidth));

            var omega = 2.0 * PI * cutoff / SoundChipSamplingRate;
            var alpha = Sin(omega) * Sinh(Log(2.0) / 2.0 * bandwidth * omega / Sin(omega));

            return NormalizeFilter(
                 1.0 + alpha,       // a0
                -2.0 * Cos(omega),  // a1
                 1.0 - alpha,       // a2
                 1.0,               // b0
                -2.0 * Cos(omega),  // b1
                 1.0                // b2
            );
        }

        public static double[] LowShelf(double cutoff, double gain, double q = DefaultQ)
        {
            if (cutoff < 0.0 || cutoff > SoundChipSamplingRate / 2.0)
                throw new ArgumentOutOfRangeException(nameof(cutoff));

            if (q < 0.0)
                throw new ArgumentOutOfRangeException(nameof(q));

            var omega = 2.0 * PI * cutoff / SoundChipSamplingRate;
            var a = Pow(10.0, gain / 40.0);
            var beta = Sqrt(a) / q;

            return NormalizeFilter(
                            a + 1.0 + (a - 1.0) * Cos(omega) + beta * Sin(omega),
                -2.0 * (a - 1.0 + (a + 1.0) * Cos(omega)),
                            a + 1.0 + (a - 1.0) * Cos(omega) - beta * Sin(omega),
                       a * (a + 1.0 - (a - 1.0) * Cos(omega) + beta * Sin(omega)),
                 2.0 * a * (a - 1.0 - (a + 1.0) * Cos(omega)),
                       a * (a + 1.0 - (a - 1.0) * Cos(omega) - beta * Sin(omega))
            );
        }

        public static double[] HighShelf(double cutoff, double gain, double q = DefaultQ)
        {
            if (cutoff < 0.0 || cutoff > SoundChipSamplingRate / 2.0)
                throw new ArgumentOutOfRangeException(nameof(cutoff));

            if (q < 0.0)
                throw new ArgumentOutOfRangeException(nameof(q));

            var omega = 2.0 * PI * cutoff / SoundChipSamplingRate;
            var a = Pow(10.0, gain / 40.0);
            var beta = Sqrt(a) / q;

            return NormalizeFilter(
                            a + 1.0 - (a - 1.0) * Cos(omega) + beta * Sin(omega),
                 2.0 * (a - 1.0 - (a + 1.0) * Cos(omega)),
                            a + 1.0 - (a - 1.0) * Cos(omega) - beta * Sin(omega),
                       a * (a + 1.0 + (a - 1.0) * Cos(omega) + beta * Sin(omega)),
                -2.0 * a * (a - 1.0 + (a + 1.0) * Cos(omega)),
                       a * (a + 1.0 + (a - 1.0) * Cos(omega) - beta * Sin(omega))
            );
        }

        public static double[] Peaking(double cutoff, double bandwidth, double gain)
        {
            if (cutoff < 0.0 || cutoff > SoundChipSamplingRate / 2.0)
                throw new ArgumentOutOfRangeException(nameof(cutoff));

            if (bandwidth < 0.0)
                throw new ArgumentOutOfRangeException(nameof(bandwidth));

            var omega = 2.0 * PI * cutoff / SoundChipSamplingRate;
            var alpha = Sin(omega) * Sinh(Log(2.0) / 2.0 * bandwidth * omega / Sin(omega));
            var a = Pow(10.0, gain / 40.0);

            return NormalizeFilter(
                 1.0 + alpha / a,   // a0
                -2.0 * Cos(omega),  // a1
                 1.0 - alpha / a,   // a2
                 1.0 + alpha * a,   // b0
                -2.0 * Cos(omega),  // b1
                 1.0 - alpha * a    // b2
            );
        }

        public static double[] Allpass(double cutoff, double q = DefaultQ)
        {
            if (cutoff < 0.0 || cutoff > SoundChipSamplingRate / 2.0)
                throw new ArgumentOutOfRangeException(nameof(cutoff));

            if (q < 0.0)
                throw new ArgumentOutOfRangeException(nameof(q));

            var omega = 2.0 * PI * cutoff / SoundChipSamplingRate;
            var alpha = Sin(omega) / (2.0 * q);

            return NormalizeFilter(
                 1.0 + alpha,       // a0
                -2.0 * Cos(omega),  // a1
                 1.0 - alpha,       // a2
                 1.0 - alpha,       // b0
                -2.0 * Cos(omega),  // b1
                 1.0 + alpha        // b2
            );
        }

        #endregion

        #region -- Private Methods --

        private static double[] NormalizeFilter(double a0, double a1, double a2, double b0, double b1, double b2)
        {
            return new[]
            {
                b0 / a0,
                b1 / a0,
                b2 / a0,
                -a1 / a0,
                -a2 / a0
            };
        }

        #endregion
    }
}
