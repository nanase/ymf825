using System;

namespace Ymf825.IO
{
    /// <inheritdoc />
    /// <summary>
    /// YMF825 との接続を行うインタフェースが実装すべき機能です。
    /// </summary>
    public interface IYmf825Device : IDisposable
    {
        #region -- Methods --

        void Write(byte address, byte data);

        void BurstWrite(byte address, byte[] data, int offset, int count);

        byte Read(byte address);

        void Flush();

        void SetTarget(TargetChip chip);

        #endregion
    }
}
