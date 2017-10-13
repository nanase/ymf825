using System;

namespace Ymf825
{
    [Flags]
    public enum SequencerSetting : byte
    {
        Reset = 0x00,

        AllKeyOff = 0x80,
        AllMute = 0x40,
        AllEgReset = 0x20,

        R_FIFOR = 0x10,
        REP_SQ = 0x08,
        R_SEQ = 0x04,
        R_FIFO = 0x02,
        START = 0x01
    }
}
