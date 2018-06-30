using System;
using System.Threading;

namespace Ymf825.Driver
{
    /// <summary>
    /// YMF825 のレジスタ命令を抽象化し、各種操作を提供します。
    /// </summary>
    public partial class Ymf825Driver
    {
        #region -- Private Fields --

        private readonly object lockObject = new object();

        private Action<int> sleepAction = Thread.Sleep;
        
        private TargetChip previousTargetChip;

        private Thread sectionThread;

        #endregion

        #region -- Public Properties --

        /// <summary>
        /// レジスタの書き込み先となる YMF825 インタフェースオブジェクトを取得します。
        /// </summary>
        public IYmf825 SoundChip { get; }

        /// <summary>
        /// セクションモードであるかを表す真偽値を取得します。
        /// </summary>
        public bool SectionMode { get; private set; }

        /// <summary>
        /// スリープに使われるメソッドを取得または設定します。
        /// スリープ時間の単位はミリ秒 (ms) です。
        /// </summary>
        public Action<int> SleepAction
        {
            get => sleepAction;
            set => sleepAction = value ?? Thread.Sleep;
        }

        /// <summary>
        /// セクションに突入した回数を取得します。
        /// </summary>
        public long EnteredSectionCount { get; private set; }

        #endregion

        #region -- Constructors --

        /// <summary>
        /// YMF825 インタフェースを指定して新しい <see cref="Ymf825Driver"/> クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="soundChip">レジスタの読み書き先となる YMF825 インタフェースオブジェクト。</param>
        public Ymf825Driver(IYmf825 soundChip)
        {
            SoundChip = soundChip;
        }

        #endregion

        #region -- Public Methods --

        /// <summary>
        /// セクションモードを有効にします。
        /// このモードが有効であるとき、自動フラッシュは無効になり、<see cref="EndSection"/> メソッドが呼ばれるまでレジスタ書き込みが保留されます。
        /// </summary>
        public void EnableSectionMode()
        {
            if (SectionMode)
                return;

            SectionMode = true;
            SoundChip.AutoFlush = false;
        }

        /// <summary>
        /// セクションモードを無効にします。
        /// このモードが無効であるとき、自動フラッシュが有効になり、各レジスタ書き込み命令は即時に実行されます。
        /// </summary>
        public void DisableSectionMode()
        {
            if (!SectionMode)
                return;

            SectionMode = false;
            SoundChip.AutoFlush = true;
            sectionThread = null;

            if (Monitor.IsEntered(lockObject))
                Monitor.Exit(lockObject);
        }

        /// <summary>
        /// 指定されたメソッドをセクション内で実行し、セクションを終了します。
        /// </summary>
        /// <param name="action">セクション内で実行されるメソッド。</param>
        /// <param name="sleep">セクション終了後に待機する時間。単位はミリ秒 (ms) です。</param>
        public void Section(Action action, int sleep = 0)
        {
            BeginSection();
            action();
            EndSection(sleep);
        }

        /// <summary>
        /// 指定されたメソッドをセクション内で実行し、セクションを終了します。
        /// </summary>
        /// <param name="target">セクション内で実行されるレジスタ書き込みの対象となる、
        /// YMF825Board の組み合わせが格納された <see cref="TargetChip"/> 列挙体。</param>
        /// <param name="action">セクション内で実行されるメソッド。</param>
        /// <param name="sleep">セクション終了後に待機する時間。単位はミリ秒 (ms) です。</param>
        public void Section(TargetChip target, Action action, int sleep = 0)
        {
            BeginSection(target);
            action();
            EndSection(sleep);
        }

        /// <summary>
        /// セクションを開始します。
        /// セクションが開始されると、他のスレッドによるセクションの開始は待機状態となります。
        /// </summary>
        /// <param name="target">このセクションが対象とする YMF825Board の組み合わせを表す <see cref="TargetChip"/> 列挙体。</param>
        public void BeginSection(TargetChip target = TargetChip.None)
        {
            if (SectionMode)
            {
                Monitor.Enter(lockObject);
                EnteredSectionCount++;
                sectionThread = Thread.CurrentThread;
            }

            if (target == TargetChip.None)
                return;

            previousTargetChip = SoundChip.CurrentTargetChips;
            SoundChip.SetTarget(target);
        }

        /// <summary>
        /// セクションを終了し、保留されていた書き込み命令を実行します。
        /// </summary>
        /// <param name="sleep">セクション終了後に待機する時間。単位はミリ秒 (ms) です。</param>
        public void EndSection(int sleep = 0)
        {
            SoundChip.Flush();

            if (sleep > 0)
                sleepAction(sleep);

            if (previousTargetChip != TargetChip.None)
            {
                SoundChip.SetTarget(previousTargetChip);
                previousTargetChip = TargetChip.None;
            }

            if (!SectionMode)
                return;

            sectionThread = null;
            Monitor.Exit(lockObject);
        }

        // レジスタの操作メソッドについてはスリープを入れない。
        // 操作メソッドをまとめて一連の処理を行うメソッドには適切なスリープを入れる。

        #region Hardware Reset

        /// <summary>
        /// ハードウェアリセット信号を /RST に送信します。
        /// </summary>
        public void ResetHardware()
        {
            SoundChip.InvokeHardwareReset();
        }

        #endregion

        #region Software Reset

        /// <summary>
        /// ソフトリセットを実行します。
        /// このメソッドはセクション内で実行できません。
        /// </summary>
        public void ResetSoftware()
        {
            if (Monitor.IsEntered(lockObject))
                throw new InvalidOperationException("このメソッドはセクション内で実行できません。");

            Section(() =>
            {
                SetPowerRailSelection(true);
                SetAnalogBlockPowerDown(AnalogBlock.All);
            }, 1);

            Section(() =>
            {
                SetClockEnable(true);
                SetAllRegisterReset(false);
                SetSoftReset(0xa3);
            }, 1);

            Section(() =>
            {
                SetSoftReset(0x00);
            }, 30);

            Section(() =>
            {
                SetAnalogBlockPowerDown(AnalogBlock.None);
                SetMasterVolume(0x30);
                SetVolumeInterpolationSetting(false, 0x03, 0x03, 0x03);
                SetInterporationInMuteState(true);
                SetGain(Gain.Level75);

                SetSequencerSetting(SequencerSetting.AllKeyOff | SequencerSetting.AllMute | SequencerSetting.AllEgReset |
                                    SequencerSetting.R_FIFOR | SequencerSetting.R_SEQ | SequencerSetting.R_FIFO);
            }, 21);

            Section(() =>
            {
                SetSequencerSetting(SequencerSetting.Reset);
                SetSequencerVolume(0x1f, false, 0);

                SetSequencerTimeUnitSetting(0x2000);
            });
        }

        #endregion

        #region #0 CLKE / Clock Enable (0x00)

        /// <summary>
        /// 内部マスタークロック (CLKE) を切り替えます。
        /// </summary>
        /// <param name="enable">true のとき、クロック有効。false のとき、クロック無効。</param>
        public void SetClockEnable(bool enable)
        {
            SoundChip.Write(0x00, (byte)(enable ? 0x01 : 0x00));
        }

        /// <summary>
        /// 内部マスタークロック (CLKE) の状態をデバイスから取得します。
        /// このメソッドはセクション内で実行できません。
        /// </summary>
        /// <param name="chip">読み込み対象の YMF825Board を表す <see cref="TargetChip"/> 列挙体。</param>
        /// <returns>true のとき、クロック有効。false のとき、クロック無効。</returns>
        public bool GetClockEnable(TargetChip chip)
        {
            return SectionForRead(chip, () => SoundChip.Read(0x00) == 0x01);
        }

        #endregion

        #region #1 ALRST / Reset (0x01)

        /// <summary>
        /// すべてのレジスタをリセット状態 (ALRST) に切り替え、特定のレジスタにアクセスできるようにします。
        /// </summary>
        /// <param name="state">true のとき、リセット状態。false のとき、非リセット状態。</param>
        public void SetAllRegisterReset(bool state)
        {
            SoundChip.Write(0x01, (byte)(state ? 0x80 : 0x00));
        }

        /// <summary>
        /// レジスタのリセット状態 (ALRST) をデバイスから取得します。
        /// このメソッドはセクション内で実行できません。
        /// </summary>
        /// <param name="chip">読み込み対象の YMF825Board を表す <see cref="TargetChip"/> 列挙体。</param>
        /// <returns>true のとき、リセット状態。false のとき、非リセット状態。</returns>
        public bool GetAllRegisterReset(TargetChip chip)
        {
            return SectionForRead(chip, () => SoundChip.Read(0x01) == 0x80);
        }

        #endregion

        #region #2 AP0, AP1, AP2, AP3 / Analog Block Power-down control (0x02)

        /// <summary>
        /// アナログブロックの Power-down 状態を設定します。
        /// </summary>
        /// <param name="block">Power-down 状態に設定されるブロックを表す <see cref="AnalogBlock"/> 構造体の値。</param>
        public void SetAnalogBlockPowerDown(AnalogBlock block)
        {
            SoundChip.Write(0x02, (byte)((int)block & 0x0f));
        }

        /// <summary>
        /// アナログブロックの Power-down 状態を取得します。
        /// このメソッドはセクション内で実行できません。
        /// </summary>
        /// <param name="chip">読み込み対象の YMF825Board を表す <see cref="TargetChip"/> 列挙体。</param>
        /// <returns>Power-down 状態に設定されたブロックを表す <see cref="AnalogBlock"/> 列挙体。</returns>
        public AnalogBlock GetAnalogBlockPowerDown(TargetChip chip)
        {
            return SectionForRead(chip, () => (AnalogBlock)(SoundChip.Read(0x02) & 0x0f));
        }

        #endregion

        #region #3 GAIN / Speaker Amplifier Gain Setting (0x03)

        /// <summary>
        /// 出力のゲインレベルを設定します。
        /// </summary>
        /// <param name="gainLevel">ゲインレベルを表す <see cref="Gain"/> 列挙体。</param>
        public void SetGain(Gain gainLevel)
        {
            SoundChip.Write(0x03, (byte)((int)gainLevel & 0x03));
        }

        /// <summary>
        /// 出力のゲインレベルを取得します。
        /// このメソッドはセクション内で実行できません。
        /// </summary>
        /// <param name="chip">読み込み対象の YMF825Board を表す <see cref="TargetChip"/> 列挙体。</param>
        /// <returns>ゲインレベルを表す <see cref="Gain"/> 構造体の値。</returns>
        public Gain GetGein(TargetChip chip)
        {
            return SectionForRead(chip, () => (Gain)(SoundChip.Read(0x03) & 0x03));
        }

        #endregion

        #region #4 HW_ID (0x04)

        /// <summary>
        /// ハードウェアの ID を取得します。
        /// このメソッドはセクション内で実行できません。
        /// </summary>
        /// <param name="chip">読み込み対象の YMF825Board を表す <see cref="TargetChip"/> 列挙体。</param>
        /// <returns>デバイスに割り当てられているハードウェア ID を表す整数値。</returns>
        public int GetHardwareId(TargetChip chip)
        {
            return SectionForRead(chip, () => SoundChip.Read(0x04));
        }

        #endregion

        #region #7 CONTENTS_DATA_REG (0x07)

        /// <summary>
        /// <see cref="ToneParameterCollection"/> オブジェクトを指定して、データ配列を書き込みます。
        /// </summary>
        /// <param name="toneParameterCollection">書き込まれる <see cref="ToneParameterCollection"/> オブジェクト。</param>
        /// <param name="targetToneNumber">書き込み対象のトーン番号。</param>
        public void WriteContentsData(ToneParameterCollection toneParameterCollection, int targetToneNumber = 15)
        {
            var buffer = new byte[1 + 30 * (targetToneNumber + 1) + 4];
            toneParameterCollection.Export(buffer, 0, targetToneNumber);
            WriteContentsData(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// トーンパラメータのデータ配列を書き込みます。
        /// </summary>
        /// <param name="data">トーンパラメータが格納された <see cref="byte"/> 型の配列。</param>
        /// <param name="offset">読み取りを開始するインデクス。</param>
        /// <param name="count">読み取るバイト数。</param>
        public void WriteContentsData(byte[] data, int offset, int count)
        {
            SoundChip.BurstWrite(0x07, data, offset, count);
        }

        #endregion

        #region #8 Sequencer Setting (0x08)

        /// <summary>
        /// シーケンサ設定を設定します。
        /// </summary>
        /// <param name="setting">シーケンサ設定の組み合わせを表す <see cref="SequencerSetting"/> 列挙体。</param>
        public void SetSequencerSetting(SequencerSetting setting)
        {
            SoundChip.Write(0x08, (byte)((int)setting & 0xff));
        }

        /// <summary>
        /// シーケンサ設定を取得します。
        /// このメソッドはセクション内で実行できません。
        /// </summary>
        /// <param name="chip">読み込み対象の YMF825Board を表す <see cref="TargetChip"/> 列挙体。</param>
        /// <returns>シーケンサ設定の組み合わせを表す <see cref="SequencerSetting"/> 列挙体。</returns>
        public SequencerSetting GetSequencerSetting(TargetChip chip)
        {
            return SectionForRead(chip, () => (SequencerSetting)(SoundChip.Read(0x08) & 0xff));
        }

        #endregion

        #region #9,10 Sequencer Volume (0x09, 0x0a)

        public void SetSequencerVolume(int volume, bool applyInterpolation, int size)
        {
            volume &= 0x1f;
            size &= 0x01ff;
            SoundChip.Write(0x09, (byte)(volume << 3 | (applyInterpolation ? 0x00 : 0x04) | size >> 8));
            SoundChip.Write(0x0a, (byte)(size & 0xff));
        }

        public (int volume, bool applyInterpolation, int size) GetSequencerVolume(TargetChip chip)
        {
            return SectionForRead(chip, () =>
            {
                var msb = SoundChip.Read(0x09);
                var lsb = SoundChip.Read(0x0a);
                return (msb >> 3, (msb & 0x04) == 0, (msb & 0x01) << 8 | lsb);
            });
        }

        #endregion

        #region #11 CRGD_VNO (0x0b)

        public void SetVoiceNumber(int number)
        {
            SoundChip.Write(0x0b, (byte)(number & 0x0f));
        }

        public int GetVoiceNumber(TargetChip chip)
        {
            return SectionForRead(chip, () => SoundChip.Read(0x0b));
        }

        #endregion

        #region #12 VoVol (0x0c)

        public void SetVoiceVolume(int volume)
        {
            SoundChip.Write(0x0c, (byte)((volume & 0x1f) << 2));
        }

        #endregion

        #region #13,14 FNUM, BLOCK (0x0d, 0x0e)

        public void SetFnumAndBlock(int fnum, int block)
        {
            block &= 0x07;
            fnum &= 0x03ff;
            SoundChip.Write(0x0d, (byte)(((fnum & 0x0380) >> 4) | (block & 0x07)));
            SoundChip.Write(0x0e, (byte)(fnum & 0x7f));
        }

        #endregion

        #region #15 ToneNum, KeyOn, Mute, EG_RST (0x0f)

        public void SetToneFlag(int toneNumber, bool keyOn, bool mute, bool resetEnvelopeGenerator)
        {
            SoundChip.Write(0x0f, (byte)(
                (keyOn ? 0x40 : 0x00) |
                (mute ? 0x20 : 0x00) |
                (resetEnvelopeGenerator ? 0x10 : 0x00) |
                toneNumber & 0x0f));
        }

        #endregion

        #region #16 ChVol (0x10)

        public void SetChannelVolume(int volume, bool applyInterpolation)
        {
            SoundChip.Write(0x10, (byte)(
                (volume & 0x1f) << 2 |
                (applyInterpolation ? 0x00 : 0x01)));
        }

        #endregion

        #region #17 XVB (0x11)

        public void SetVibratoModuration(int depth)
        {
            SoundChip.Write(0x11, (byte)(depth & 0x07));
        }

        #endregion

        #region #18,19 INT, FRAC (0x12, 0x13)

        public void SetFrequencyMultiplier(int integer, int fraction)
        {
            fraction &= 0x01ff;
            SoundChip.Write(0x12, (byte)((integer & 0x03) << 3 | fraction >> 6));
            SoundChip.Write(0x13, (byte)((fraction & 0x3f) << 1));
        }

        #endregion

        #region #20 DIR_MT (0x14)

        public void SetInterporationInMuteState(bool enable)
        {
            SoundChip.Write(0x14, (byte)(enable ? 0x00 : 0x01));
        }

        #endregion

        #region #23,24 MS_S / Sequencer Time unit Setting (0x17, 0x18)

        public void SetSequencerTimeUnitSetting(int timeUnit)
        {
            timeUnit &= 0x3fff;
            SoundChip.Write(0x17, (byte)(timeUnit >> 7));
            SoundChip.Write(0x18, (byte)(timeUnit & 0x7f));
        }

        #endregion

        #region #25 MASTER_VOL / Master Volume (0x19)

        public void SetMasterVolume(int volume)
        {
            SoundChip.Write(0x19, (byte)((volume & 0x3f) << 2));
        }

        public int GetMasterVolume(TargetChip chip)
        {
            return SectionForRead(chip, () => SoundChip.Read(0x19) >> 2);
        }

        #endregion

        #region #26 SFTRST / Soft Reset (0x1a)

        public void SetSoftReset(byte value)
        {
            // TODO: value の詳細を調査
            SoundChip.Write(0x1a, value);
        }

        public byte GetSoftReset(TargetChip chip)
        {
            return SectionForRead(chip, () => SoundChip.Read(0x1a));
        }

        #endregion

        #region #27 DADJT, MUTE_ITIME, CHVOL_ITIME, MVOL_ITIME / Sequencer Delay, Recovery Function Setting, Volume Interpolation Setting (0x1b)

        public void SetVolumeInterpolationSetting(bool dadjt, byte muteItime, byte chvolItime, byte mvolItime)
        {
            // TODO: パラメータは列挙体を使って書き換える
            SoundChip.Write(0x1b, (byte)(
                (dadjt ? 0x40 : 0x00) |
                ((muteItime & 0x03) << 4) |
                ((chvolItime & 0x03) << 2) |
                (mvolItime & 0x03)));
        }

        public (bool dadjt, byte muteItime, byte chvolItime, byte mvolItime) GetVolumeInterpolationSetting(TargetChip chip)
        {
            return SectionForRead(chip, () =>
            {
                var readByte = SoundChip.Read(0x1b);
                return (
                    (readByte & 0x40) != 0,
                    (byte)((readByte >> 4) & 0x03),
                    (byte)((readByte >> 2) & 0x03),
                    (byte)(readByte & 0x03));
            });
        }

        #endregion

        #region #28 LFO_RST / LFO Reset (0x1c)

        public void SetLfoReset(bool reset)
        {
            SoundChip.Write(0x1c, (byte)(reset ? 0x01 : 0x00));
        }

        public bool GetLfoReset(TargetChip chip)
        {
            return SectionForRead(chip, () => SoundChip.Read(0x1c) == 0x01);
        }

        #endregion

        #region #29 DRV_SEL / Power Rail Selection (0x1d) 

        public void SetPowerRailSelection(bool select)
        {
            SoundChip.Write(0x1d, (byte)(select ? 0x01 : 0x00));
        }

        public bool GetPowerRailSelection(TargetChip chip)
        {
            return SectionForRead(chip, () => SoundChip.Read(0x1d) == 0x01);
        }

        #endregion

        // #30, 31 (0x1e, 0x1f) is reserved

        #region #32,33,34 W_CEQ0/1/2 (0x20,0x21,0x22)

        /// <summary>
        /// イコライザの係数を設定します。
        /// </summary>
        /// <param name="band">イコライザのバンド番号を表す整数値。範囲は 0 から 2 です。</param>
        /// <param name="coefficients">イコライザの係数を表す数値。範囲は -8.0 から 8.0 です。</param>
        public void SetEqualizer(int band, double[] coefficients)
        {
            if (band < 0 || band > 2)
                throw new ArgumentOutOfRangeException(nameof(band));

            if (coefficients == null)
                throw new ArgumentNullException(nameof(coefficients));

            if (coefficients.Length != 5)
                throw new ArgumentNullException(nameof(coefficients));

            var data = new byte[15];

            for (var i = 0; i < 5; i++)
            {
                var registerFormat = coefficients[i].ToRegisterFormat();

                data[i * 3 + 0] = (byte)(registerFormat >> 16);
                data[i * 3 + 1] = (byte)((registerFormat >> 8) & 0xff);
                data[i * 3 + 2] = (byte)(registerFormat & 0xff);
            }

            SoundChip.BurstWrite((byte)(0x20 + band), data, 0, 15);
        }

        #endregion

        #region #35-79 CEQ (0x23-0x4f)

        /// <summary>
        /// イコライザの係数の値をレジスタ形式で取得します。
        /// このメソッドはセクション内で実行できません。
        /// </summary>
        /// <param name="chip">読み込み対象の YMF825Board を表す <see cref="TargetChip"/> 列挙体。</param>
        /// <param name="band">イコライザのバンド番号。有効範囲は 0 から 2 です。</param>
        /// <param name="coefficientNumber">イコライザの係数番号。有効範囲は 0 から 4 です。</param>
        /// <returns>イコライザの係数の値を表す、レジスタ形式で表された整数値。有効なビットは下位 24 ビットです。</returns>
        public int GetEqualizerCoefficient(TargetChip chip, int band, int coefficientNumber)
        {
            return SectionForRead(chip, () =>
            {
                var baseAddress = 35 + band * 15 + coefficientNumber * 3;
                var byte0 = SoundChip.Read((byte)(baseAddress + 0));
                var byte1 = SoundChip.Read((byte)(baseAddress + 1));
                var byte2 = SoundChip.Read((byte)(baseAddress + 2));
                return (byte0 << 16) + (byte1 << 8) + byte2;
            });
        }

        /// <summary>
        /// イコライザの係数の各値を取得します。
        /// このメソッドはセクション内で実行できません。
        /// </summary>
        /// <param name="chip">読み込み対象の YMF825Board を表す <see cref="TargetChip"/> 列挙体。</param>
        /// <param name="band">イコライザのバンド番号。有効範囲は 0 から 2 です。</param>
        /// <returns>イコライザの係数の値を表す実数の配列。</returns>
        public double[] GetEqualizer(TargetChip chip, int band)
        {
            var coefficients = new double[5];

            for (var i = 0; i < coefficients.Length; i++)
                coefficients[i] = FilterCoefficients.ToDouble(GetEqualizerCoefficient(chip, band, i));

            return coefficients;
        }

        #endregion

        #region #80 Software test communication (0x50)

        /// <summary>
        /// 通信テストのためのレジスタを設定します。
        /// </summary>
        /// <param name="value">書き込まれる値。</param>
        public void SetSoftwareTestCommunication(byte value)
        {
            SoundChip.Write(0x50, value);
        }

        /// <summary>
        /// 通信テストのためのレジスタから値を取得します。
        /// </summary>
        /// <param name="chip">読み込み対象の YMF825Board を表す <see cref="TargetChip"/> 列挙体。</param>
        /// <returns>レジスタの値。</returns>
        public byte GetSoftwareTestCommunication(TargetChip chip)
        {
            return SectionForRead(chip, () => SoundChip.Read(0x50));
        }

        #endregion

        #endregion

        #region -- Private Methods --

        private T SectionForRead<T>(TargetChip target, Func<T> func)
        {
            if (sectionThread != null && sectionThread == Thread.CurrentThread)
                throw new InvalidOperationException("読み取り命令は同じスレッドで複数のセクションに突入させることはできません。");

            BeginSection(target);
            var result = func();
            EndSection();

            return result;
        }

        #endregion
    }
}
