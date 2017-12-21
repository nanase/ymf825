using System;
using System.Collections.Generic;
using System.Linq;
using MidiUtils.IO;
using Ymf825;

namespace Ymf825MidiDriver
{
    internal class MidiDriver
    {
        #region -- Private Fields --

        private readonly int[] noteOnKeys = new int[16];
        private readonly int[] programNumbers = new int[16];
        private readonly int[] rpnMsb = new int[16];
        private readonly int[] rpnLsb = new int[16];
        private readonly int[] dataEntryMsb = new int[16];
        private readonly int[] dataEntryLsb = new int[16];
        private readonly double[] pitchBendWidth = new double[16];
        private readonly double[] corrections = new double[16];
        private readonly double[] pitchBends = new double[16];
        private readonly double[] fineTune = new double[16];
        private readonly double[] volumes = new double[16];
        private readonly double[] toneVolumes = new double[16];
        private readonly double[] lchVolumes = new double[16];
        private readonly double[] rchVolumes = new double[16];
        private readonly double[] expressions = new double[16];
        private readonly bool[] percussionMode = { false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false };
        private readonly ToneParameterCollection toneParameterList = new ToneParameterCollection();

        #endregion

        #region -- Public Properties --

        public IList<ToneItem> ToneItems { get; }

        public IList<EqualizerItem> EqualizerItems { get; }

        public MidiIn MidiIn { get; }

        public Ymf825Driver Driver { get; }

        #endregion

        #region -- Constructors --

        public MidiDriver(IList<ToneItem> toneItems, IList<EqualizerItem> equalizerItems, MidiIn midiIn, Ymf825Driver driver)
        {
            ToneItems = toneItems;
            EqualizerItems = equalizerItems;
            MidiIn = midiIn;
            Driver = driver;
            Driver.EnableSectionMode();

            for (var i = 0; i < 16; i++)
            {
                noteOnKeys[i] = -1;
                pitchBends[i] = 1.0;
                corrections[i] = 1.0;

                volumes[i] = 100.0 / 127.0;
                toneVolumes[i] = 1.0;
                lchVolumes[i] = 1.0;
                rchVolumes[i] = 1.0;
                expressions[i] = 1.0;

                rpnMsb[i] = 127;
                rpnLsb[i] = 127;
                fineTune[i] = 1.0;
                pitchBendWidth[i] = 2.0;
            }
        }

        #endregion

        #region -- Public Methods --

        public void Start()
        {
            MidiIn.ReceivedMidiEvent += MidiInOnReceivedMidiEvent;
            MidiIn.ReceivedExclusiveMessage += MidiInOnReceivedExclusiveMessage;
            MidiIn.Start();
            Driver.ResetHardware();
            Driver.ResetSoftware();

            Driver.Section(() => Driver.SetSequencerSetting(SequencerSetting.AllKeyOff | SequencerSetting.AllMute | SequencerSetting.AllEgReset |
                                                            SequencerSetting.R_FIFOR | SequencerSetting.R_SEQ | SequencerSetting.R_FIFO), 1);

            Driver.Section(() => Driver.SetSequencerSetting(SequencerSetting.Reset));
        }

        public void Stop()
        {
            MidiIn.Stop();
            MidiIn.ReceivedMidiEvent -= MidiInOnReceivedMidiEvent;
            MidiIn.ReceivedExclusiveMessage -= MidiInOnReceivedExclusiveMessage;
        }

        public void NotifyChangeTone(ToneItem toneItem)
        {
            if (toneItem == null || !toneItem.ProgramNumberAssigned)
                return;

            var i = 0;
            for (; i < 16; i++)
            {
                if (toneParameterList[i] == toneItem.ToneParameter)
                    break;
            }

            if (i == 16)
                return;

            ProgramChange(i, toneItem.ProgramNumber, true);
        }

        #endregion

        #region -- Private Methods --

        private void MidiInOnReceivedMidiEvent(object sender, ReceivedMidiEventEventArgs e)
        {
            var midiEvent = e.Event;

            switch (midiEvent.Type)
            {
                case EventType.NoteOff:
                    NoteOff(midiEvent.Channel, midiEvent.Data1);
                    break;

                case EventType.NoteOn:
                    NoteOn(midiEvent.Channel, midiEvent.Data1, midiEvent.Data2);
                    break;

                case EventType.ControlChange:
                    ControlChange(midiEvent.Channel, midiEvent.Data1, midiEvent.Data2);
                    break;

                case EventType.ProgramChange:
                    ProgramChange(midiEvent.Channel, midiEvent.Data1, true);
                    break;

                case EventType.ChannelPressure:
                    break;
                case EventType.Pitchbend:
                    PitchBend(midiEvent.Channel, midiEvent.Data1, midiEvent.Data2);
                    break;

                case EventType.SystemExclusiveF0:
                    break;
                case EventType.SystemExclusiveF7:
                    break;
                case EventType.MetaEvent:
                    break;

                default:
                    return;
            }
        }

        private void MidiInOnReceivedExclusiveMessage(object sender, ReceivedExclusiveMessageEventArgs e)
        {
            var message = e.Message.ToArray();

            // Set Percussion Channel
            if (message.Length == 9 && message[4] == 0x40 && message[6] == 0x15)
            {
                int channel;
                switch (message[5])
                {
                    case 16: channel = 9; break;
                    case 17: channel = 0; break;
                    case 18: channel = 1; break;
                    case 19: channel = 2; break;
                    case 20: channel = 3; break;
                    case 21: channel = 4; break;
                    case 22: channel = 5; break;
                    case 23: channel = 6; break;
                    case 24: channel = 7; break;
                    case 25: channel = 8; break;
                    case 26: channel = 10; break;
                    case 27: channel = 11; break;
                    case 28: channel = 12; break;
                    case 29: channel = 13; break;
                    case 30: channel = 14; break;
                    case 31: channel = 15; break;
                    default: channel = 0; break;
                }

                var isPercussion = message[7] != 0;
                percussionMode[channel] = isPercussion;
                Console.WriteLine($"Change Chanel Mode: Ch {channel} - {isPercussion}");
            }
        }

        private void NoteOff(int channel, int key)
        {
            if (noteOnKeys[channel] != key)
                return;

            SendNoteOff(channel);
        }

        private void NoteOn(int channel, int key, int velocity)
        {
            if (noteOnKeys[channel] != -1)
                NoteOff(channel, key);

            if (percussionMode[channel])
            {
                SendNoteOff(channel);
                PercussionNoteOn(channel, key, velocity);
                return;
            }

            SendNoteOn(channel, key, velocity);
            noteOnKeys[channel] = key;
        }

        private void PercussionNoteOn(int channel, int key, int velocity)
        {
            var tone = ToneItems.FirstOrDefault(t =>
                t.ProgramNumberAssigned && t.ProgramNumber == programNumbers[channel] &&
                t.PercussionNumberAssigned && t.PercussionNumber == key) ?? new ToneItem();
            SetProgram(channel, tone.ProgramNumber, tone);
            SendNoteOn(channel, tone.PercussionNoteNumber, velocity);
            noteOnKeys[channel] = key;
        }

        private void ProgramChange(int channel, int program, bool forceChange = false)
        {
            var tone = ToneItems.FirstOrDefault(t => t.ProgramNumberAssigned && t.ProgramNumber == program) ?? new ToneItem();
            SetProgram(channel, program, tone, forceChange);
        }

        private void ControlChange(int channel, int data1, int data2)
        {
            switch (data1)
            {
                case 6: // data entry MSB
                    dataEntryMsb[channel] = data2;
                    Rpn(channel);
                    break;

                case 7: // volume
                    volumes[channel] = data2 / 127.0;
                    SetVoiceVolume(channel, true);
                    break;

                case 10: // panpot
                    SetPanpot(channel, data2 / 64.0 - 1.0);
                    SetVoiceVolume(channel, true);
                    break;

                case 11: // expression
                    expressions[channel] = data2 / 127.0;
                    SetVoiceVolume(channel, true);
                    break;

                case 36: // data entry LSB
                    dataEntryMsb[channel] = data2;
                    Rpn(channel);
                    break;

                case 100: // RPN LSB
                    rpnLsb[channel] = data2;
                    break;
                    
                case 101: // RPN MSB
                    rpnMsb[channel] = data2;
                    break;

                case 112: // x-equalizer
                    SetEqualizer(data2);
                    break;
            }
        }

        private void PitchBend(int channel, int data1, int data2)
        {
            var bendData = ((data2 << 7) | data1) - 8192;
            pitchBends[channel] = Math.Pow(Math.Pow(2.0, pitchBendWidth[channel] / 12.0), bendData / 8192.0);
            var correction = corrections[channel] * pitchBends[channel] * fineTune[channel];

            Ymf825Driver.ConvertForFrequencyMultiplier(correction, out var integer, out var fraction);

            Driver.Section(() =>
            {
                Driver.SetVoiceNumber(channel);
                Driver.SetFrequencyMultiplier(integer, fraction);
            });
        }

        private void Rpn(int channel)
        {
            if (rpnMsb[channel] == 0 && rpnLsb[channel] == 1)
            {
                var bendData = ((dataEntryMsb[channel] << 7) | dataEntryLsb[channel]) - 8192;
                // precalc: Math.Pow(2.0, 2.0 / 12.0) = 1.122462048309373
                fineTune[channel] = Math.Pow(Math.Pow(2.0, 2.0 / 12.0), bendData / 8192.0);
                var correction = corrections[channel] * pitchBends[channel] * fineTune[channel];

                Ymf825Driver.ConvertForFrequencyMultiplier(correction, out var integer, out var fraction);

                Driver.Section(() =>
                {
                    Driver.SetVoiceNumber(channel);
                    Driver.SetFrequencyMultiplier(integer, fraction);
                });
            }
            if (rpnMsb[channel] == 0 && rpnLsb[channel] == 0)
            {
                pitchBendWidth[channel] = dataEntryMsb[channel];
            }
        }

        // ------

        private void SetProgram(int channel, int programNumber, ToneItem tone, bool forceChange = false)
        {
            if (!forceChange && toneParameterList[channel] == tone.ToneParameter)
            {
                toneVolumes[channel] = tone.Volume;
                SetPanpot(channel, tone.Panpot);
                return;
            }

            toneParameterList[channel] = tone.ToneParameter;
            programNumbers[channel] = programNumber;
            toneVolumes[channel] = tone.Volume;
            SetPanpot(channel, tone.Panpot);
            SendProgramChange(channel);
            //Console.WriteLine($"Perc: {tone.Name} - {tone.PercussionNumber}");
        }

        private void SetVoiceVolume(int channel, bool setVoiceNumber = false)
        {
            var lch = (int)Math.Round(expressions[channel] * volumes[channel] * lchVolumes[channel] * toneVolumes[channel] * 31.0);
            var rch = (int)Math.Round(expressions[channel] * volumes[channel] * rchVolumes[channel] * toneVolumes[channel] * 31.0);

            Driver.Section(TargetChip.Board0, () =>
            {
                if (setVoiceNumber)
                    Driver.SetVoiceNumber(channel);

                Driver.SetVoiceVolume(lch);
            });

            Driver.Section(TargetChip.Board1, () =>
            {
                if (setVoiceNumber)
                    Driver.SetVoiceNumber(channel);

                Driver.SetVoiceVolume(rch);
            });

            //Console.WriteLine($"Volume: #{channel} - L:{lch}, R:{rch}");
        }

        private void SendNoteOff(int channel)
        {
            Driver.Section(() =>
            {
                Driver.SetVoiceNumber(channel);
                Driver.SetToneFlag(channel, false, false, false);
            });

            noteOnKeys[channel] = -1;
        }

        private void SendNoteOn(int channel, int key, int velocity)
        {
            var volume = velocity / 127.0;

            Ymf825Driver.GetFnumAndBlock(key, out var fnum, out var block, out var correction);
            corrections[channel] = correction;
            correction *= pitchBends[channel] * fineTune[channel];
            Ymf825Driver.ConvertForFrequencyMultiplier(correction, out var integer, out var fraction);

            Driver.Section(() =>
            {
                Driver.SetVoiceNumber(channel);
                Driver.SetChannelVolume((int)Math.Round(volume * 31.0), false);
                Driver.SetFrequencyMultiplier(integer, fraction);
                Driver.SetFnumAndBlock((int)Math.Round(fnum), block);
                Driver.SetToneFlag(channel, true, false, false);
            });
        }

        private void SendProgramChange(int channel)
        {
            Driver.Section(() =>
            {
                Driver.SetVoiceNumber(channel);
                Driver.SetSequencerSetting(SequencerSetting.R_FIFOR | SequencerSetting.R_SEQ | SequencerSetting.R_FIFO);
                Driver.SetSequencerSetting(SequencerSetting.Reset);
                Driver.SetVibratoModuration(1);
                Driver.WriteContentsData(toneParameterList, channel);
            });

            SetVoiceVolume(channel, true);
        }

        private void SetPanpot(int channel, double panValue)
        {
            lchVolumes[channel] = panValue > 0.0 ? Math.Sin((panValue + 1.0) * Math.PI / 2.0) : 1.0;
            rchVolumes[channel] = panValue < 0.0 ? Math.Sin((-panValue + 1.0) * Math.PI / 2.0) : 1.0;
            //Console.WriteLine($"Panpot: #{channel} - L:{lchVolumes[channel]:f2}, R:{rchVolumes[channel]:f2}");
        }

        private void SetEqualizer(int number)
        {
            var equalizerItem =
                EqualizerItems.FirstOrDefault(e => e.ProgramNumberAssigned && e.ProgramNumber == number) ??
                new EqualizerItem();

            Driver.Section(() =>
            {
                Driver.SetEqualizer(0, equalizerItem.Equalizer0.ToArray());
                Driver.SetEqualizer(1, equalizerItem.Equalizer1.ToArray());
                Driver.SetEqualizer(2, equalizerItem.Equalizer2.ToArray());
            });
        }

        #endregion
    }
}
