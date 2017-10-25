using System;
using System.Threading;

namespace Ymf825
{
    public class Ymf825Driver
    {
        #region -- Private Fields --

        private Action<int> sleepAction = Thread.Sleep;

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

        #region -- Public Properties --

        public IYmf825Client Client { get; }

        public Action<int> SleepAction
        {
            get => sleepAction;
            set => sleepAction = value ?? (i => { });
        }

        #endregion

        #region -- Constructors --

        public Ymf825Driver(IYmf825Client client)
        {
            Client = client;
        }

        static Ymf825Driver()
        {
            for (var i = 0; i < 12; i++)
                FnumTable[i] = CalcFnum(440.0 * Math.Pow(2.0, (48.0 + i - 69.0) / 12.0), 3);
        }

        #endregion

        #region -- Public Methods --

        // レジスタの操作メソッドについてはスリープを入れない。
        // 操作メソッドをまとめて一連の処理を行うメソッドには適切なスリープを入れる。

        #region Hardware Reset

        /// <summary>
        /// ハードウェアリセット信号を /RST に送信します。
        /// </summary>
        public void ResetHardware()
        {
            Client.ResetHardware();
        }

        #endregion

        #region Software Reset

        public void ResetSoftware()
        {
            SetPowerRailSelection(false);
            SetAnalogBlockPowerDown(AnalogBlock.Ap0 | AnalogBlock.Ap1 | AnalogBlock.Ap2);
            sleepAction(1);

            SetClockEnable(true);
            SetAllRegisterReset(false);
            SetSoftReset(0xa3);
            sleepAction(1);

            SetSoftReset(0x00);
            sleepAction(30);

            SetAnalogBlockPowerDown(AnalogBlock.Ap2);
            sleepAction(1);

            SetAnalogBlockPowerDown(AnalogBlock.None);
            SetMasterVolume(0xf0);
            SetVolumeInterpolationSetting(false, 0x03, 0x03, 0x03);
            SetInterporationInMuteState(true);
            SetGain(Gain.Level65);

            SetSequencerSetting(SequencerSetting.AllKeyOff | SequencerSetting.AllMute | SequencerSetting.AllEgReset |
                                SequencerSetting.R_FIFOR | SequencerSetting.R_SEQ | SequencerSetting.R_FIFO);
            sleepAction(21);

            SetSequencerSetting(SequencerSetting.Reset);
            SetSequencerVolume(0x1f, false, 0);

            SetSequencerTimeUnitSetting(0x2000);
        }

        #endregion

        #region #0 CLKE / Clock Enable (0x00)

        /// <summary>
        /// 内部マスタークロック (CLKE) を切り替えます。
        /// </summary>
        /// <param name="enable">true のとき、クロック有効。false のとき、クロック無効。</param>
        public void SetClockEnable(bool enable)
        {
            Client.Write(0x00, (byte)(enable ? 0x01 : 0x00));
        }

        /// <summary>
        /// 内部マスタークロック (CLKE) の状態をデバイスから取得します。
        /// </summary>
        /// <returns>true のとき、クロック有効。false のとき、クロック無効。</returns>
        public bool GetClockEnable(TargetDevice device)
        {
            return Client.Read(device, 0x00) == 0x01;
        }

        #endregion

        #region #1 ALRST / Reset (0x01)

        /// <summary>
        /// すべてのレジスタをリセット状態 (ALRST) に切り替え、特定のレジスタにアクセスできるようにします。
        /// </summary>
        /// <param name="state">true のとき、リセット状態。false のとき、非リセット状態。</param>
        public void SetAllRegisterReset(bool state)
        {
            Client.Write(0x01, (byte)(state ? 0x80 : 0x00));
        }

        /// <summary>
        /// レジスタのリセット状態 (ALRST) をデバイスから取得します。
        /// </summary>
        /// <returns>true のとき、リセット状態。false のとき、非リセット状態。</returns>
        public bool GetAllRegisterReset(TargetDevice device)
        {
            return Client.Read(device, 0x01) == 0x80;
        }

        #endregion

        #region #2 AP0, AP1, AP2, AP3 / Analog Block Power-down control (0x02)

        /// <summary>
        /// アナログブロックの Power-down 状態を設定します。
        /// </summary>
        /// <param name="block">Power-down 状態に設定されるブロックを表す <see cref="AnalogBlock"/> 構造体の値。</param>
        public void SetAnalogBlockPowerDown(AnalogBlock block)
        {
            Client.Write(0x02, (byte)((int)block & 0x0f));
        }

        /// <summary>
        /// アナログブロックの Power-down 状態を取得します。
        /// </summary>
        /// <returns>Power-down 状態に設定されたブロックを表す <see cref="AnalogBlock"/> 構造体の値。</returns>
        public AnalogBlock GetAnalogBlockPowerDown(TargetDevice device)
        {
            return (AnalogBlock)(Client.Read(device, 0x02) & 0x0f);
        }

        #endregion

        #region #3 GAIN / Speaker Amplifier Gain Setting (0x03)

        /// <summary>
        /// 出力のゲインレベルを設定します。
        /// </summary>
        /// <param name="gainLevel">ゲインレベルを表す <see cref="Gain"/> 構造体の値。</param>
        public void SetGain(Gain gainLevel)
        {
            Client.Write(0x03, (byte)((int)gainLevel & 0x03));
        }

        /// <summary>
        /// 出力のゲインレベルを取得します。
        /// </summary>
        /// <returns>ゲインレベルを表す <see cref="Gain"/> 構造体の値。</returns>
        public Gain GetGein(TargetDevice device)
        {
            return (Gain)(Client.Read(device, 0x03) & 0x03);
        }

        #endregion

        #region #4 HW_ID (0x04)

        /// <summary>
        /// ハードウェアの ID を取得します。
        /// </summary>
        /// <returns>デバイスに割り当てられているハードウェア ID を表す整数値。</returns>
        public int GetHardwareId(TargetDevice device)
        {
            return Client.Read(device, 0x04);
        }

        #endregion

        #region #7 CONTENTS_DATA_REG (0x07)

        public void WriteContentsData(byte[] data, int offset, int count)
        {
            Client.BurstWriteBytes(0x07, data, offset, count);
        }

        #endregion

        #region #8 Sequencer Setting (0x08)

        public void SetSequencerSetting(SequencerSetting setting)
        {
            Client.Write(0x08, (byte)((int)setting & 0xff));
        }

        public SequencerSetting GetSequencerSetting(TargetDevice device)
        {
            return (SequencerSetting)(Client.Read(device, 0x08) & 0xff);
        }

        #endregion

        #region #9,10 Sequencer Volume (0x09, 0x0a)

        public void SetSequencerVolume(int volume, bool applyInterpolation, int size)
        {
            volume &= 0x1f;
            size &= 0x01ff;
            Client.Write(0x09, (byte)(volume << 3 | (applyInterpolation ? 0x00 : 0x04) | size >> 8));
            Client.Write(0x0a, (byte)(size & 0xff));
        }

        public (int volume, bool applyInterpolation, int size) GetSequencerVolume(TargetDevice device)
        {
            var msb = Client.Read(device, 0x09);
            var lsb = Client.Read(device, 0x0a);
            return (msb >> 3, (msb & 0x04) == 0, (msb & 0x01) << 8 | lsb);
        }

        #endregion

        #region #11 CRGD_VNO (0x0b)

        public void SetVoiceNumber(int number)
        {
            Client.Write(0x0b, (byte)(number & 0x0f));
        }

        public int GetVoiceNumber(TargetDevice device)
        {
            return Client.Read(device, 0x0b);
        }

        #endregion

        #region #12 VoVol (0x0c)

        public void SetVoiceVolume(int volume)
        {
            Client.Write(0x0c, (byte)((volume & 0x1f) << 2));
        }

        #endregion

        #region #13,14 FNUM, BLOCK (0x0d, 0x0e)

        public void SetFnumAndBlock(int fnum, int block)
        {
            fnum &= 0x03ff;
            Client.Write(0x0d, (byte)(((fnum & 0x0380) >> 4) | (block & 0x07)));
            Client.Write(0x0e, (byte)(fnum & 0x7f));
        }

        #endregion

        #region #15 ToneNum, KeyOn, Mute, EG_RST (0x0f)

        public void SetToneFlag(int toneNumber, bool keyOn, bool mute, bool resetEnvelopeGenerator)
        {
            Client.Write(0x0f, (byte)(
                (keyOn ? 0x40 : 0x00) |
                (mute ? 0x20 : 0x00) |
                (resetEnvelopeGenerator ? 0x10 : 0x00) |
                toneNumber & 0x0f));
        }

        #endregion

        #region #16 ChVol (0x10)

        public void SetChannelVolume(int volume, bool applyInterpolation)
        {
            Client.Write(0x10, (byte)(
                (volume & 0x1f) << 2 |
                (applyInterpolation ? 0x00 : 0x01)));
        }

        #endregion

        #region #17 XVB (0x11)

        public void SetVibratoModuration(int depth)
        {
            Client.Write(0x11, (byte)(depth & 0x07));
        }

        #endregion

        #region #18,19 INT, FRAC (0x12, 0x13)

        public void SetFrequencyMultiplier(int integer, int fraction)
        {
            fraction &= 0x01ff;
            Client.Write(0x12, (byte)((integer & 0x03) << 3 | fraction >> 6));
            Client.Write(0x13, (byte)((fraction & 0x3f) << 1));
        }

        #endregion

        #region #20 DIR_MT (0x14)

        public void SetInterporationInMuteState(bool enable)
        {
            Client.Write(0x14, (byte)(enable ? 0x00 : 0x01));
        }

        #endregion

        #region #23,24 MS_S / Sequencer Time unit Setting (0x17, 0x18)

        public void SetSequencerTimeUnitSetting(int timeUnit)
        {
            timeUnit &= 0x3fff;
            Client.Write(0x17, (byte)(timeUnit >> 7));
            Client.Write(0x18, (byte)(timeUnit & 0x7f));
        }

        #endregion

        #region #25 MASTER_VOL / Master Volume (0x19)

        public void SetMasterVolume(int volume)
        {
            Client.Write(0x19, (byte)((volume & 0x3f) << 2));
        }

        public int GetMasterVolume(TargetDevice device)
        {
            return Client.Read(device, 0x19) >> 2;
        }

        #endregion

        #region #26 SFTRST / Soft Reset (0x1a)

        public void SetSoftReset(byte value)
        {
            // TODO: value の詳細を調査
            Client.Write(0x1a, value);
        }

        public byte GetSoftReset(TargetDevice device)
        {
            return Client.Read(device, 0x1a);
        }

        #endregion

        #region #27 DADJT, MUTE_ITIME, CHVOL_ITIME, MVOL_ITIME / Sequencer Delay, Recovery Function Setting, Volume Interpolation Setting (0x1b)

        public void SetVolumeInterpolationSetting(bool dadjt, byte muteItime, byte chvolItime, byte mvolItime)
        {
            // TODO: パラメータは列挙体を使って書き換える
            Client.Write(0x1b, (byte)(
                (dadjt ? 0x40 : 0x00) |
                ((muteItime & 0x03) << 4) |
                ((chvolItime & 0x03) << 2) |
                (mvolItime & 0x03)));
        }

        public (bool dadjt, byte muteItime, byte chvolItime, byte mvolItime) GetVolumeInterpolationSetting(TargetDevice device)
        {
            var readByte = Client.Read(device, 0x1b);
            return (
                (readByte & 0x40) != 0,
                (byte)((readByte >> 4) & 0x03),
                (byte)((readByte >> 2) & 0x03),
                (byte)(readByte & 0x03));
        }

        #endregion

        #region #28 LFO_RST / LFO Reset (0x1c)

        public void SetLfoReset(bool reset)
        {
            Client.Write(0x1c, (byte)(reset ? 0x01 : 0x00));
        }

        public bool GetLfoReset(TargetDevice device)
        {
            return Client.Read(device, 0x1c) == 0x01;
        }

        #endregion

        #region #29 DRV_SEL / Power Rail Selection (0x1d) 

        public void SetPowerRailSelection(bool select)
        {
            Client.Write(0x1d, (byte)(select ? 0x01 : 0x00));
        }

        public bool GetPowerRailSelection(TargetDevice device)
        {
            return Client.Read(device, 0x1d) == 0x01;
        }

        #endregion

        // #30, 31 (0x1e, 0x1f) is reserved

        #region #32,33,34 W_CEQ0/1/2 (0x20,0x21,0x22)

        #endregion

        #region #35-79 CEQ (0x23-0x4f)

        #endregion

        #region #80 Software test communication (0x50)

        public void SetSoftwareTestCommunication(byte value)
        {
            Client.Write(0x50, value);
        }

        public byte GetSoftwareTestCommunication(TargetDevice device)
        {
            return Client.Read(device, 0x50);
        }

        #endregion

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
            else if (block > 7)
            {
                blockMod = Math.Pow(2.0, block - 7);
                block = 7;
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
