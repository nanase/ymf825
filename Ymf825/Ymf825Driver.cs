using System;
using System.Threading;

namespace Ymf825
{
    public class Ymf825Driver
    {
        #region -- Private Fields --

        private Action<int> sleepAction = Thread.Sleep;

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

        public void WriteContentsData(byte header, params byte[] data)
        {
            var newArray = new byte[data.Length + 1];
            data[0] = header;
            Array.Copy(data, 1, newArray, 0, data.Length);
            Client.BurstWriteBytes(0x07, newArray);
        }

        public void WriteContentsData(byte[] data)
        {
            Client.BurstWriteBytes(0x07, data);
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

        public static double CalcFnum(double frequency, int block)
        {
            // Original Formula
            // const fnum = (freq * Math.pow(2, 19)) / (Math.pow(2, block - 1) * 48000);
            return Math.Pow(2.0, 13 - block) * frequency / 375.0;
        }

        public static double CalcFrequency(double fnum, double block)
        {
            // Original Formula
            // const freq = (48000 * Math.pow(2, block - 1) * fnum) / Math.pow(2, 19);
            return 375.0 * Math.Pow(2.0, block - 13) * fnum;
        }

        #endregion
    }
}
