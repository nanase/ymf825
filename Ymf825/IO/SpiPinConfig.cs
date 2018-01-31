using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ymf825.IO
{
    public class SpiPinConfig
    {
        #region -- Public Properties --

        public bool IsHighByte { get; }

        public byte Value { get; }

        public byte Direction { get; }

        public bool EnableLevelHigh { get; } 

        #endregion

        #region -- Constructors --

        public SpiPinConfig(bool isHighByte, byte value, byte direction, bool enableLevelHigh)
        {
            IsHighByte = isHighByte;
            Value = value;
            Direction = direction;
            EnableLevelHigh = enableLevelHigh;
        }

        #endregion
    }
}
