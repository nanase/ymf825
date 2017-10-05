using System;
using System.Threading;

namespace Ymf825
{
    public class Ymf825Driver
    {
        #region -- Private Fields --

        private readonly byte[] register = new byte[256];
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

        #region Hardware Reset

        /// <summary>
        /// ハードウェアリセット信号を /RST に送信します。
        /// </summary>
        /// <param name="resetEnableValue">リセットを開始するときに GPIO ピンから出力される信号。デフォルトは <code>0x00</code> です。</param>
        /// <param name="resetDisableValue">リセットを解除するときに GPIO ピンから出力される信号。デフォルトは <code>0xff</code> です。</param>
        /// <param name="direction">GPIO ピンの入出力方向。デフォルトは <code>0xff</code> (すべて出力) です。</param>
        public void ResetHardware(byte resetEnableValue = 0x00, byte resetDisableValue = 0xff, byte direction = 0xff)
        {
            Client.WriteGpio(direction, resetDisableValue);
            Client.WriteGpio(direction, resetEnableValue);
            sleepAction(1);

            Client.WriteGpio(direction, resetDisableValue);
            sleepAction(30);
        }

        #endregion

        #region #0 CLKE

        /// <summary>
        /// 内部マスタークロック (CLKE) を切り替えます。
        /// </summary>
        /// <param name="enable">true のとき、クロック有効。false のとき、クロック無効。</param>
        public void SetClockEnable(bool enable)
        {
            Client.Write(0x00, enable ? (byte)0x01 : (byte)0x00);
        }

        /// <summary>
        /// 内部マスタークロック (CLKE) の状態をデバイスから取得します。
        /// </summary>
        /// <returns>true のとき、クロック有効。false のとき、クロック無効。</returns>
        public bool GetClockEnable()
        {
            return Client.ReadByte(0x00) == 0x01;
        }

        #endregion

        #region #1 ALRST

        /// <summary>
        /// すべてのレジスタをリセット状態 (ALRST) に切り替え、特定のレジスタにアクセスできるようにします。
        /// </summary>
        /// <param name="state">true のとき、リセット状態。false のとき、非リセット状態。</param>
        public void SetAllRegisterReset(bool state)
        {
            Client.Write(0x01, state ? (byte)0x80 : (byte)0x00);
        }

        /// <summary>
        /// レジスタのリセット状態 (ALRST) をデバイスから取得します。
        /// </summary>
        /// <returns>true のとき、リセット状態。false のとき、非リセット状態。</returns>
        public bool GetAllRegisterReset()
        {
            return Client.ReadByte(0x01) == 0x80;
        }

        #endregion

        #endregion
    }
}
