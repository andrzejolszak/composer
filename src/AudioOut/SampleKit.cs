using System.Collections.Generic;
using NAudio.Wave;

namespace Composer.AudioOut
{
    class SampleKit
    {
        private readonly List<SampleSource> sampleSources;

        public SampleKit(NotePattern pattern)
        {
            sampleSources = new List<SampleSource>();
            foreach (string name in pattern.NoteNames)
            {
                sampleSources.Add(SampleSource.CreateFromWaveFile("AudioOut\\Guitar1\\" + name + ".wav"));
            }

            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleSources[0].SampleWaveFormat.SampleRate, sampleSources[0].SampleWaveFormat.Channels);
        }

        public virtual WaveFormat WaveFormat { get; }

        public MusicSampleProvider GetSampleProvider(int note)
        {
            return new MusicSampleProvider(sampleSources[note]);
        }
    }
}
