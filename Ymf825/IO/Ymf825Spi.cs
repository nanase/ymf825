using System.Threading;

namespace Ymf825.IO
{
    public class Ymf825Spi : Spi
    {
        #region -- Constructors --

        public Ymf825Spi(int deviceIndex, byte csPin)
            : base(deviceIndex, false, csPin)
        {
            ResetHardware();
        }

        #endregion

        #region -- Public Methods --

        public void ResetHardware()
        {
            if (WriteBufferSize < 3)
                ExtendBuffer(ref WriteBuffer, ref WriteBufferSize, 3);

            WriteRaw(0x82, 0xff, 0xff);
            Thread.Sleep(2);

            WriteRaw(0x82, 0x00, 0xff);
            Thread.Sleep(2);

            WriteRaw(0x82, 0xff, 0xff);
        }

        #endregion
    }
}
