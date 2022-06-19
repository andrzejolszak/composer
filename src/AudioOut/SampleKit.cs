using System.Collections.Generic;
using System.IO;
using System.Linq;
using Composer.Util;
using NAudio.Wave;

namespace Composer.AudioOut
{
    class SampleKit
    {
        private readonly Dictionary<string, SampleSource> sampleSources;

        public SampleKit()
        {
            // TODO: preaload this
            sampleSources = new();
            
            foreach (string name in Directory.GetFiles("AudioOut\\Nylon1\\").Where(x => x.EndsWith(".wav")))
            {
                string noteName = name.Split(new[] { "\\", ".wav" }, System.StringSplitOptions.RemoveEmptyEntries).Last().ToUpperInvariant();
                sampleSources.Add(noteName, SampleSource.CreateFromWaveFile(name));
            }

            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleSources.First().Value.SampleWaveFormat.SampleRate, sampleSources.First().Value.SampleWaveFormat.Channels);
        }

        public virtual WaveFormat WaveFormat { get; }

        public MusicSampleProvider GetSampleProvider(Note note, int octave)
        {
            if (!sampleSources.TryGetValue(note.ToString().ToUpperInvariant() + octave, out SampleSource sampleSource))
            {
                sampleSource = sampleSources["ERROR"];
            }

            return new MusicSampleProvider(sampleSource);
        }
    }
}
