using System;

namespace Ymf825
{
    [Serializable]
    public class ToneParameterCollection
    {
        #region -- Private Fields --

        private readonly ToneParameter[] parameters;

        #endregion

        #region -- Public Indexer --

        public ToneParameter this[int index]
        {
            get
            {
                if (index < 0 || index > 15)
                    throw new IndexOutOfRangeException();

                return parameters[index];
            }
        }

        #endregion

        #region -- Constructors --

        public ToneParameterCollection()
        {
            parameters = new ToneParameter[16];

            for (var i = 0; i < 16; i++)
                parameters[i] = new ToneParameter();
        }

        #endregion

        #region -- Public Methods --

        public int Export(byte[] buffer, int offset, int targetToneNumber = 15)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (targetToneNumber < 0 || targetToneNumber > 15)
                throw new IndexOutOfRangeException();

            var maxToneNumber = targetToneNumber + 1;

            // start header
            buffer[offset] = (byte)(0x80 + maxToneNumber);

            // sequence data
            for (var i = 0; i < maxToneNumber; i++)
                parameters[i].Export(buffer, offset + 1 + 30 * i);

            // end footer
            buffer[offset + 1 + 30 * maxToneNumber] = 0x80;
            buffer[offset + 1 + 30 * maxToneNumber + 1] = 0x03;
            buffer[offset + 1 + 30 * maxToneNumber + 2] = 0x81;
            buffer[offset + 1 + 30 * maxToneNumber + 3] = 0x80;

            return 1 + 30 * maxToneNumber + 4;
        }

        #endregion
    }
}
