using System;

namespace Ymf825
{
    [Flags]
    public enum TargetChip
    {
        Board0 = 1,
        Board1 = 2,
        Board2 = 4,
        Board3 = 8,
        Board4 = 16
    }
}
