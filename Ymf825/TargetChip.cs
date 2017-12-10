using System;

namespace Ymf825
{
    /// <summary>
    /// 命令を送信する YMF825Board の指定に使われる列挙体です。
    /// この列挙体はフラグとして扱います。
    /// </summary>
    [Flags]
    public enum TargetChip
    {
        /// <summary>
        /// 送信先は未指定です。
        /// </summary>
        None = 0,

        /// <summary>
        /// 1つ目の YMF825Board。
        /// </summary>
        Board0 = 1,

        /// <summary>
        /// 2つ目の YMF825Board。
        /// </summary>
        Board1 = 2,

        /// <summary>
        /// 3つ目の YMF825Board。
        /// </summary>
        Board2 = 4,

        /// <summary>
        /// 4つ目の YMF825Board。
        /// </summary>
        Board3 = 8,

        /// <summary>
        /// 5つ目の YMF825Board。
        /// </summary>
        Board4 = 16
    }
}
