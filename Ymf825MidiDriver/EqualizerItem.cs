using System;

namespace Ymf825MidiDriver
{
    [Serializable]
    public class EqualizerItem
    {
        #region -- Private Fields --

        private int programNumber;

        #endregion

        #region -- Public Properties --

        public Equalizer Equalizer0 { get; set; } = new Equalizer();

        public Equalizer Equalizer1 { get; set; } = new Equalizer();

        public Equalizer Equalizer2 { get; set; } = new Equalizer();

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

        #endregion

    }
}
