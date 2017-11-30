using System;
using Ymf825;

namespace Ymf825MidiDriver
{
    [Serializable]
    public class ToneItem
    {
        #region -- Private Fields --

        private int programNumber;
        private int percussionNumber;
        private int percussionNoteNumber;
        private double panpot;
        private double volume = 1.0;

        #endregion

        #region -- Public Properties --

        public ToneParameter ToneParameter { get; } = new ToneParameter();

        public string Name { get; set; }

        public int ProgramNumber
        {
            get => programNumber;
            set
            {
                if (value < 0 || value > 127)
                    throw new ArgumentOutOfRangeException(nameof(value));

                programNumber = value;
            }
        }

        public bool ProgramNumberAssigned { get; set; }

        public int PercussionNumber
        {
            get => percussionNumber;
            set
            {
                if (value < 0 || value > 127)
                    throw new ArgumentOutOfRangeException(nameof(value));

                percussionNumber = value;
            }
        }

        public int PercussionNoteNumber
        {
            get => percussionNoteNumber;
            set
            {
                if (value < 0 || value > 127)
                    throw new ArgumentOutOfRangeException(nameof(value));

                percussionNoteNumber = value;
            }
        }

        public bool PercussionNumberAssigned { get; set; }

        public double Panpot
        {
            get => panpot;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value) || value < -1.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                panpot = value;
            }
        }

        public double Volume
        {
            get => volume;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value) || value < 0.0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                volume = value;
            }
        }

        #endregion
    }
}
