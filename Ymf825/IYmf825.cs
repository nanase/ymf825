namespace Ymf825
{
    /// <inheritdoc />
    /// <summary>
    /// YMF825 との接続を行うインタフェースが実装すべき機能です。
    /// </summary>
    public interface IYmf825
    {
        #region -- Properties --

        /// <summary>
        /// インタフェースに接続されている有効な YMF825 の組み合わせを取得します。
        /// </summary>
        TargetChip AvailableChips { get; }

        /// <summary>
        /// 命令が実行される対象の YMF825 を取得します。
        /// </summary>
        TargetChip CurrentTargetChips { get; }

        /// <summary>
        /// インタフェースが自動的に命令をフラッシュするかの真偽値を取得または設定します。
        /// </summary>
        bool AutoFlush { get; set; }

        #endregion

        #region -- Methods --

        /// <summary>
        /// アドレスとデータを指定して Write 命令を 1 バイト書き込みます。
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        void Write(byte address, byte data);

        /// <summary>
        /// アドレスとデータを指定して BurstWrite 命令を複数バイト書き込みます。
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void BurstWrite(byte address, byte[] data, int offset, int count);

        /// <summary>
        /// アドレスを指定して Read 命令から 1 バイト読み取ります。
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        byte Read(byte address);

        /// <summary>
        /// ハードウェアリセットを行います。
        /// </summary>
        void InvokeHardwareReset();

        /// <summary>
        /// インタフェースが即時に命令を実行するようバッファをフラッシュします。
        /// </summary>
        void Flush();

        /// <summary>
        /// 命令が実行される対象の YMF825 を指定します。
        /// </summary>
        /// <param name="chip"></param>
        void SetTarget(TargetChip chips);

        #endregion
    }
}
