using System;

namespace Ymf825MidiDriver
{
    [Serializable]
    public class Equalizer
    {
        #region -- Private Fields --

        private readonly double[] ceq = { 1.0, 0.0, 0.0, 0.0, 0.0 };

        #endregion

        #region -- Public Indexer --

        public double this[int index]
        {
            get => ceq[index];
            set
            {
                if (value <= -8.0 || value >= 8.0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                ceq[index] = value;
            }
        }

        #endregion

        #region -- Public Properties --
        
        public double B0
        {
            get => ceq[0];
            set => this[0] = value;
        }

        public double B1
        {
            get => ceq[1];
            set => this[1] = value;
        }

        public double B2
        {
            get => ceq[2];
            set => this[2] = value;
        }

        public double A1
        {
            get => ceq[3];
            set => this[3] = value;
        }

        public double A2
        {
            get => ceq[4];
            set => this[4] = value;
        }

        #endregion

        #region -- Public Methods --

        public double[] ToArray()
        {
            return new [] { ceq[0], ceq[1], ceq[2], ceq[3], ceq[4] };
        }

        #endregion
    }
}
