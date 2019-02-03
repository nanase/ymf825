using System.IO;

namespace Ymf825.IO
{
    /// <inheritdoc />
    /// <summary>
    /// YMF825 との通信内容を書き出すクラスが実装すべき機能です。
    /// </summary>
    public interface IYmf825BinaryWriter : IYmf825
    {
        #region -- Properties --

        /// <summary>
        /// 書き出し先のストリームを取得します。
        /// </summary>
        Stream BaseStream { get; }
 
        /// <summary>
        /// インスタンスのリソースが破棄されたかを表す真偽値を取得します。
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// Tick値の基準となる時間分解能を取得します。単位は 1/秒 です。
        /// </summary>
        double TickResolution { get; }

        #endregion

        #region -- Methods --

        /// <summary>
        /// 次の命令までの時間間隔を Tick で指定します。
        /// </summary>
        /// <param name="tick">時間間隔を表す Tick 値。</param>
        void Wait(int tick);

        /// <summary>
        /// 指定された Tick 値だけ、YMF825 への通信を停止させます。
        /// </summary>
        /// <param name="tick">時間間隔を表す Tick 値。</param>
        void RealtimeWait(int tick);

        #endregion
    }
}