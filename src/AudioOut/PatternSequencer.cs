using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using NAudio.Wave;
using Composer.Project;
using Composer.Util;

namespace Composer.AudioOut
{
    class PatternSequencer
    {
        private readonly NotePattern drumPattern;
        private readonly SampleKit drumKit;
        private int tempo;
        private int samplesPerStep;

        public PatternSequencer(NotePattern drumPattern, SampleKit kit, Tuning tuning)
        {
            drumKit = kit;
            this.drumPattern = drumPattern;
            Tempo = 120;
            Tuning = tuning;
        }

        public bool Loop { get; set; }

        public int Tempo
        {
            get => tempo;
            set
            {
                if (tempo != value)
                { 
                    tempo = value;
                    newTempo = true;
                }
            }
        }

        public Tuning Tuning { get; }

        private bool newTempo;
        private int currentStep;
        private double patternPosition;

        public IList<ISampleProvider> GetNextMixerInputs(int sampleCount)
        {
            List<ISampleProvider> mixerInputs = new List<ISampleProvider>();
            int samplePos = 0;
            if (newTempo)
            {
                int samplesPerBeat = (drumKit.WaveFormat.Channels * drumKit.WaveFormat.SampleRate * 60) / tempo;
                samplesPerStep = samplesPerBeat / 4;
                //patternPosition = 0;
                newTempo = false;
            }

            while (samplePos < sampleCount)
            {
                double offsetFromCurrent = (currentStep - patternPosition);
                if (offsetFromCurrent < 0) offsetFromCurrent += drumPattern.Steps;
                int delayForThisStep = (int)(samplesPerStep * offsetFromCurrent);
                if (delayForThisStep >= sampleCount)
                {
                    // don't queue up any samples beyond the requested time range
                    break;
                }

                if (drumPattern.Pattern.TryGetValue(currentStep, out HashSet<FretboardNote> notes))
                {
                    foreach (FretboardNote note in notes)
                    {
                        (Note n, int octave) = note.ResolveNote(this.Tuning);
                        var sampleProvider = drumKit.GetSampleProvider(n, octave);
                        sampleProvider.DelayBy = (int)(samplesPerStep * note.timeRange.Start / (256 / 4));
                        sampleProvider.Duration = (int)(samplesPerStep * note.timeRange.Duration / (256 / 4));
                        sampleProvider.PlayingStateChanged += s => this.NotePlayingStateChanged?.Invoke(note, s);

                        Debug.WriteLine("beat at step {0}, patternPostion={1}, delayBy {2}", currentStep, patternPosition, delayForThisStep);
                        mixerInputs.Add(sampleProvider);
                    }
                }

                samplePos += samplesPerStep;
                currentStep++;
                if (!this.Loop && currentStep >= drumPattern.Steps)
                {
                    break;
                }

                currentStep = currentStep % drumPattern.Steps;

            }

            patternPosition += ((double)sampleCount / samplesPerStep);
            if (patternPosition > drumPattern.Steps)
            {
                patternPosition -= drumPattern.Steps;
            }

            return mixerInputs;
        }

        public event Action<FretboardNote, bool> NotePlayingStateChanged;
    }
}
