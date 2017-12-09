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
            QueueBuffer(0x82, 0xff, 0xff);
            QueueFlushCommand();
            SendBuffer();
            Thread.Sleep(2);

            QueueBuffer(0x82, 0x00, 0xff);
            QueueFlushCommand();
            SendBuffer();
            Thread.Sleep(2);

            QueueBuffer(0x82, 0xff, 0xff);
            QueueFlushCommand();
            SendBuffer();
        }

        #endregion
    }
}
