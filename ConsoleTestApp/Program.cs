using System;
using System.Threading;
using Ymf825;
using Ymf825.IO;

namespace TestConsoleApp
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Image type: {0}bit", Environment.Is64BitProcess ? "64" : "32");

            if (Spi.DeviceCount < 1)
                return;

            using (var ymf825 = new CbwYmf825Bb(0))
            {
                var driver = new Ymf825Driver(ymf825);
                driver.EnableSectionMode();

                Console.WriteLine("Software Reset");
                driver.ResetSoftware();
                
                {
                    var tones = new ToneParameterCollection();
                    {
                        var tone = tones[0];
                        tone.BasicOctave = 0;
                        tone.LfoFrequency = 2;
                        tone.Algorithm = 3;

                        tone.Operator1.ReleaseRate = 6;
                        tone.Operator1.DecayRate = 15;
                        tone.Operator1.AttackRate = 15;
                        tone.Operator1.SustainLevel = 4;
                        tone.Operator1.TotalLevel = 0;
                        tone.Operator1.KeyScalingLevel = 3;
                        tone.Operator1.MagnificationOfFrequency = 0;
                        tone.Operator1.WaveShape = 1;
                        tone.Operator1.FeedbackLevel = 4;

                        tone.Operator2.ReleaseRate = 6;
                        tone.Operator2.DecayRate = 15;
                        tone.Operator2.AttackRate = 10;
                        tone.Operator2.TotalLevel = 3;
                        tone.Operator2.KeyScalingLevel = 2;
                        tone.Operator2.VibratoDepth = 1;
                        tone.Operator2.EnableVibrato = true;
                        tone.Operator2.MagnificationOfFrequency = 1;
                        tone.Operator2.WaveShape = 1;

                        tone.Operator3.ReleaseRate = 2;
                        tone.Operator3.DecayRate = 15;
                        tone.Operator3.AttackRate = 15;
                        tone.Operator3.SustainLevel = 3;
                        tone.Operator3.TotalLevel = 38;
                        tone.Operator3.KeyScalingLevel = 3;
                        tone.Operator3.MagnificationOfFrequency = 4;
                        tone.Operator3.WaveShape = 0;
                        tone.Operator3.FeedbackLevel = 4;

                        tone.Operator4.ReleaseRate = 10;
                        tone.Operator4.DecayRate = 15;
                        tone.Operator4.AttackRate = 10;
                        tone.Operator4.SustainLevel = 0;
                        tone.Operator4.TotalLevel = 3;
                        tone.Operator4.KeyScalingLevel = 2;
                        tone.Operator4.EnableVibrato = true;
                        tone.Operator4.MagnificationOfFrequency = 2;
                        tone.Operator4.WaveShape = 0;
                    }

                    Console.WriteLine("Tone Init");

                    driver.Section(() =>
                    {
                        driver.WriteContentsData(tones, 0);
                        driver.SetSequencerSetting(SequencerSetting.AllKeyOff | SequencerSetting.AllMute | SequencerSetting.AllEgReset |
                                                   SequencerSetting.R_FIFOR | SequencerSetting.R_SEQ | SequencerSetting.R_FIFO);
                    }, 1);

                    driver.Section(() =>
                    {
                        driver.SetSequencerSetting(SequencerSetting.Reset);

                        driver.SetToneFlag(0, false, true, true);
                        driver.SetChannelVolume(28, true);
                        driver.SetVibratoModuration(0);
                        driver.SetFrequencyMultiplier(1, 0);
                    });
                }

                var noteon = new Action<int>(key =>
                {
                    Ymf825Driver.GetFnumAndBlock(key, out var fnum, out var block, out var correction);
                    Ymf825Driver.ConvertForFrequencyMultiplier(correction, out var integer, out var fraction);
                    var freq = Ymf825Driver.CalcFrequency(fnum, block);
                    Console.WriteLine("key: {0}, freq: {4:f1} Hz, fnum: {5:f0}, block: {6}, correction: {1:f3}, integer: {2}, fraction: {3}", key, correction, integer, fraction, freq, fnum, block);

                    driver.Section(() =>
                    {
                        driver.SetVoiceNumber(0);
                        driver.SetVoiceVolume(15);
                        driver.SetFrequencyMultiplier(integer, fraction);
                        driver.SetFnumAndBlock((int)Math.Round(fnum), block);
                        driver.SetToneFlag(0, true, false, false);
                    });
                });

                var noteoff = new Action(() =>
                {
                    driver.Section(() => driver.SetToneFlag(0, false, false, false));
                });

                var index = 0;
                var score = new[]
                {
                    60, 62, 64, 65, 67, 69, 71, 72,
                    72, 74, 76, 77, 79, 81, 83, 84,
                    84, 83, 81, 79, 77, 76, 74, 72,
                    72, 71, 69, 67, 65, 64, 62, 60
                };
                while (true)
                {
                    const int noteOnTime = 250;
                    const int sleepTime = 0;

                    noteon(score[index]);
                    Thread.Sleep(noteOnTime);
                    noteoff();

                    Thread.Sleep(sleepTime);

                    if (Console.KeyAvailable)
                        break;

                    index++;
                    if (index >= score.Length)
                        index = 0;
                }
            }
        }
    }
}
