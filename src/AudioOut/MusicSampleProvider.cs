using System;
using System.Linq;
using MeltySynth;
using NAudio.Wave;

namespace Composer.AudioOut
{
    class MusicSampleProvider : ISampleProvider
    {
        private int delayBy;
        private int position;
        private readonly SampleSource sampleSource;
        private readonly Synthesizer _soundFontSynthesizer;

        public MusicSampleProvider(SampleSource sampleSource)
        {
            this.sampleSource = sampleSource;

            this._soundFontSynthesizer = new Synthesizer("AudioOut/UGK_amped.sf2", 44100);
            // _synthesizer.RenderInterleaved(buffer.AsSpan(offset, count));

        }

        /// <summary>
        /// Samples to delay before returning anything
        /// </summary>
        public int DelayBy
        {
            get => delayBy;
            set 
            { 
                if (value < 0)
                {
                    throw new ArgumentException("Cannot delay by negative number of samples");
                }
                delayBy = value; 
            }
        }

        /// <summary>
        /// 0 for sample's full duration.
        /// </summary>
        public int Duration { get; set; }

        public WaveFormat WaveFormat => sampleSource.SampleWaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesWritten = 0;
            if (position < delayBy)
            {
                int zeroFill = Math.Min(delayBy - position, count);
                Array.Clear(buffer, offset, zeroFill);
                position += zeroFill;
                samplesWritten += zeroFill;
            }
            
            if (samplesWritten < count)
            {
                int samplesNeeded = count - samplesWritten;
                int samplesAvailable = sampleSource.Length - (position - delayBy);
                int samplesToCopy = Math.Min(samplesNeeded, samplesAvailable);
                if (Duration > 0 && position > delayBy + Duration)
                {
                    if (this.IsPlaying)
                    {
                        this.IsPlaying = false;
                        this.PlayingStateChanged?.Invoke(false);
                    }

                    Array.Clear(buffer, samplesWritten, samplesToCopy);
                }
                else
                {
                    if (!this.IsPlaying)
                    {
                        this.IsPlaying = true;
                        this.PlayingStateChanged?.Invoke(true);
                    }

                    this._soundFontSynthesizer.NoteOn(0, 50, 100);
                    this._soundFontSynthesizer.RenderInterleaved(buffer.AsSpan(samplesWritten, samplesToCopy));
                    this._soundFontSynthesizer.NoteOff(0, 50);
                    
                    // Array.Copy(sampleSource.SampleData, PositionInSampleSource, buffer, samplesWritten, samplesToCopy);
                }

                position += samplesToCopy;
                samplesWritten += samplesToCopy;
            }
            return samplesWritten;
        }

        private int PositionInSampleSource => (position - delayBy) + sampleSource.StartIndex;

        public bool IsPlaying { get; set; }

        public event Action<bool> PlayingStateChanged;
    }
}
