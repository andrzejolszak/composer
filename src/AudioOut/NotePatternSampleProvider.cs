using Composer.Util;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Composer.AudioOut
{
    class NotePatternSampleProvider : ISampleProvider
    {
        private readonly MixingSampleProvider mixer;
        private readonly WaveFormat waveFormat;
        private readonly PatternSequencer sequencer;

        public NotePatternSampleProvider(NotePattern pattern, bool loop, Tuning tuning)
        {
            var kit = new SampleKit();
            sequencer = new PatternSequencer(pattern, kit, tuning);
            sequencer.Loop = loop;
            waveFormat = kit.WaveFormat;
            mixer = new MixingSampleProvider(waveFormat);
        }

        public int Tempo
        {
            get => sequencer.Tempo;
            set => sequencer.Tempo = value;
        }

        public WaveFormat WaveFormat => waveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            foreach (var mixerInput in sequencer.GetNextMixerInputs(count))
            {
                mixer.AddMixerInput(mixerInput);
            }

            // now we just need to read from the mixer
            var samplesRead = mixer.Read(buffer, offset, count);
            while (samplesRead < count)
            {
                buffer[samplesRead++] = 0;
            }
            return samplesRead;
        }
    }
}
