using System;

namespace Ymf825
{
    [Flags]
    public enum TargetDevice : byte
    {
        None = 0x00,

        Ymf825Board0 = 0x01,
        Ymf825Board1 = 0x02
    }
}
