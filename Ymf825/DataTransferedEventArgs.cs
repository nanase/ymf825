using System;

namespace Ymf825
{
    /// <inheritdoc />
    /// <summary>
    /// デバイスにデータが送信または受信されたときに発生するイベントの引数を格納します。
    /// </summary>
    public class DataTransferedEventArgs : EventArgs
    {
        #region -- Public Properties --

        /// <summary>
        /// 送信先または受信元のデバイスを表す <see cref="TargetChip"/> 列挙体から構成されるフラグ値を取得します。
        /// </summary>
        public TargetChip Target { get; }

        /// <summary>
        /// データアドレスを取得します。
        /// </summary>
        public byte Address { get; }

        /// <summary>
        /// 送信または受信されたデータを取得します。
        /// </summary>
        public byte Data { get; }

        #endregion

        #region -- Constructors --

        /// <inheritdoc />
        /// <summary>
        /// 引数を指定して新しい <see cref="DataTransferedEventArgs" /> クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="target">送信先または受信元のデバイスを表す <see cref="TargetChip"/> 列挙体から構成されるフラグ値。</param>
        /// <param name="address">データアドレス。</param>
        /// <param name="data">送信または受信されたデータ。</param>
        public DataTransferedEventArgs(TargetChip target, byte address, byte data)
        {
            Target = target;
            Address = address;
            Data = data;
        }

        #endregion
    }
}