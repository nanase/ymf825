using System;
using System.Collections.Generic;

namespace Ymf825
{
    /// <inheritdoc />
    /// <summary>
    /// デバイスに BurstWrite 命令によってデータが送信されたときに発生するイベントの引数を格納します。
    /// </summary>
    public class DataBurstWriteEventArgs : EventArgs
    {
        #region -- Public Properties --

        /// <summary>
        /// 送信先のデバイスを表す <see cref="Ymf825.TargetDevice"/> 列挙体から構成されるフラグ値を取得します。
        /// </summary>
        public TargetDevice TargetDevice { get; }

        /// <summary>
        /// データアドレスを取得します。
        /// </summary>
        public byte Address { get; }

        /// <summary>
        /// 送信されたデータのコレクションを取得します。
        /// </summary>
        public IReadOnlyList<byte> Data { get; }

        /// <summary>
        /// 送信されたデータのコレクション <see cref="Data"/> で、実際に送信されたデータ開始位置を取得します。
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// 送信されたデータのコレクション <see cref="Data"/> で、実際に送信されたデータ数を取得します。
        /// </summary>
        public int Count { get; }

        #endregion

        #region -- Constructors --

        /// <inheritdoc />
        /// <summary>
        /// 引数を指定して新しい <see cref="T:Ymf825.DataBurstWriteEventArgs" /> のインスタンスを初期化します。
        /// </summary>
        /// <param name="targetDevice">送信先のデバイスを表す <see cref="T:Ymf825.TargetDevice" /> 列挙体から構成されるフラグ値。</param>
        /// <param name="address">データアドレス。</param>
        /// <param name="data">送信されたデータのコレクション。</param>
        /// <param name="offset">送信されたデータのコレクション <see cref="P:Ymf825.DataBurstWriteEventArgs.Data" /> で、実際に送信されたデータ開始位置。</param>
        /// <param name="count">送信されたデータのコレクション <see cref="P:Ymf825.DataBurstWriteEventArgs.Data" /> で、実際に送信されたデータ数。</param>
        public DataBurstWriteEventArgs(TargetDevice targetDevice, byte address, IReadOnlyList<byte> data, int offset, int count)
        {
            TargetDevice = targetDevice;
            Address = address;
            Data = data;
            Offset = offset;
            Count = count;
        }

        #endregion
    }
}
