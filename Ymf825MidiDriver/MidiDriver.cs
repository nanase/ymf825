using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using MidiUtils.IO;
using Ymf825;

namespace Ymf825MidiDriver
{
    internal class MidiDriver
    {
        #region -- Private Fields --

        private readonly int[] noteOnKeys = new int[16];
        private readonly ToneParameterCollection toneParameterList = new ToneParameterCollection();

        #endregion

        #region -- Public Properties --

        public IList<ToneItem> ToneItems { get; }

        public MidiIn MidiIn { get; }

        public IYmf825Client Ymf825Client { get; }

        public Ymf825Driver Driver { get; }

        #endregion

        #region -- Constructors --

        public MidiDriver(IList<ToneItem> toneItems, MidiIn midiIn, IYmf825Client ymf825Client)
        {
            ToneItems = toneItems;
            MidiIn = midiIn;
            Ymf825Client = ymf825Client;
            Driver = new Ymf825Driver(ymf825Client);

            for (var i = 0; i < 16; i++)
            {
                noteOnKeys[i] = -1;
            }
        }

        #endregion

        #region -- Public Methods --

        public void Start()
        {
            MidiIn.ReceivedMidiEvent += MidiInOnReceivedMidiEvent;
            MidiIn.ReceivedExclusiveMessage += MidiInOnReceivedExclusiveMessage;
            MidiIn.Start();
            Ymf825Client.ResetHardware();
            Ymf825Client.ResetSoftware();
            Ymf825Client.SetTarget(TargetDevice.Ymf825Board0 | TargetDevice.Ymf825Board1);

            Driver.SetSequencerSetting(SequencerSetting.AllKeyOff | SequencerSetting.AllMute | SequencerSetting.AllEgReset |
                                       SequencerSetting.R_FIFOR | SequencerSetting.R_SEQ | SequencerSetting.R_FIFO);
            Driver.SleepAction(1);
            Driver.SetSequencerSetting(SequencerSetting.Reset);
        }

        public void Stop()
        {
            MidiIn.Stop();
            MidiIn.ReceivedMidiEvent -= MidiInOnReceivedMidiEvent;
            MidiIn.ReceivedExclusiveMessage -= MidiInOnReceivedExclusiveMessage;
        }

        public void NotifyChangeTone(ToneItem toneItem)
        {
            if (!toneItem.ProgramNumberAssigned)
                return;

            var i = 0;
            for (; i < 16; i++)
            {
                if (toneParameterList[i] == toneItem.ToneParameter)
                    break;
            }

            if (i == 16)
                return;
            
            ProgramChange(i, toneItem.ProgramNumber);
        }

        #endregion

        #region -- Private Methods --

        private void MidiInOnReceivedMidiEvent(object sender, ReceivedMidiEventEventArgs e)
        {
            var midiEvent = e.Event;

            switch (midiEvent.Type)
            {
                case EventType.NoteOff:
                    NoteOff(midiEvent.Channel, midiEvent.Data1, midiEvent.Data2);
                    break;

                case EventType.NoteOn:
                    NoteOn(midiEvent.Channel, midiEvent.Data1, midiEvent.Data2);
                    break;

                case EventType.ControlChange:
                    break;

                case EventType.ProgramChange:
                    ProgramChange(midiEvent.Channel, midiEvent.Data1);
                    break;

                case EventType.ChannelPressure:
                    break;
                case EventType.Pitchbend:
                    break;
                case EventType.SystemExclusiveF0:
                    break;
                case EventType.SystemExclusiveF7:
                    break;
                case EventType.MetaEvent:
                    break;

                case EventType.PolyphonicKeyPressure:
                default:
                    return;
            }
        }

        private void MidiInOnReceivedExclusiveMessage(object sender, ReceivedExclusiveMessageEventArgs e)
        {

        }

        private void NoteOff(int channel, int key, int velocity)
        {
            if (noteOnKeys[channel] != key)
                return;

            Driver.SetToneFlag(channel, false, false, false);
            noteOnKeys[channel] = -1;
        }

        private void NoteOn(int channel, int key, int velocity)
        {
            if (noteOnKeys[channel] != -1)
                Driver.SetToneFlag(channel, false, false, false);

            Ymf825Driver.GetFnumAndBlock(key, out var fnum, out var block, out var correction);
            Ymf825Driver.ConvertForFrequencyMultiplier(correction, out var integer, out var fraction);

            var volume = velocity / 127.0;

            Driver.SetVoiceNumber(channel);
            Driver.SetChannelVolume((int)Math.Round(volume * 31.0), false);
            Driver.SetFrequencyMultiplier(integer, fraction);
            Driver.SetFnumAndBlock((int)Math.Round(fnum), block);
            Driver.SetToneFlag(channel, true, false, false);

            noteOnKeys[channel] = key;
        }

        private void ProgramChange(int channel, int program)
        {
            Console.WriteLine($"Ch: {channel}, Prog: {program}");
            var tone = ToneItems.FirstOrDefault(t => t.ProgramNumberAssigned && t.ProgramNumber == program) ?? new ToneItem();

            toneParameterList[channel] = tone.ToneParameter;

            var toneBuffer = new byte[512];
            var toneBufferSize = toneParameterList.Export(toneBuffer, 0, channel);
            
            var lchVolume = tone.Volume * Math.Cos(tone.Panpot < 0.0 ? 0.0 : tone.Panpot);
            var rchVolume = tone.Volume * Math.Cos(tone.Panpot < 0.0 ? -tone.Panpot : 0.0);

            Driver.SetVoiceNumber(channel);
            Driver.SetSequencerSetting(SequencerSetting.R_FIFOR | SequencerSetting.R_SEQ | SequencerSetting.R_FIFO);
            Driver.SleepAction(1);
            Driver.SetSequencerSetting(SequencerSetting.Reset);

            Ymf825Client.SetTarget(TargetDevice.Ymf825Board0);
            Driver.SetVoiceVolume((int)Math.Round(lchVolume * 31.0));

            Ymf825Client.SetTarget(TargetDevice.Ymf825Board1);
            Driver.SetVoiceVolume((int)Math.Round(rchVolume * 31.0));

            Ymf825Client.SetTarget(TargetDevice.Ymf825Board0 | TargetDevice.Ymf825Board1);

            Driver.WriteContentsData(toneBuffer, 0, toneBufferSize);
        }

        #endregion
    }
}
