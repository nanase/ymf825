using System;

namespace Ymf825
{
    [Flags]
    public enum TargetDevice
    {
        Board0 = 1,
        Board1 = 2,
        Board2 = 4,
        Board3 = 8,
        Board4 = 16,
        Board5 = 32,
        Board6 = 64,
        Board7 = 128
    }
}
