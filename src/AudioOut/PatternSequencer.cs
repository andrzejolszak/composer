using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using NAudio.Wave;
using Composer.Project;
using Composer.Util;
using MeltySynth;

namespace Composer.AudioOut
{
    class PatternSequencer
    {
        public readonly NotePattern Pattern;
        private readonly Synthesizer synth;
        private int tempo;
        private int samplesPerStep;

        public PatternSequencer(NotePattern drumPattern, Synthesizer synth, Tuning tuning)
        {
            this.synth = synth;
            this.Pattern = drumPattern;
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
                int samplesPerBeat = (NoteSampleProvider.ChannelCount * synth.SampleRate * 60) / tempo;
                samplesPerStep = samplesPerBeat / 4;
                //patternPosition = 0;
                newTempo = false;
            }

            while (samplePos < sampleCount)
            {
                double offsetFromCurrent = (currentStep - patternPosition);
                if (offsetFromCurrent < 0) offsetFromCurrent += Pattern.Steps;
                int delayForThisStep = (int)(samplesPerStep * offsetFromCurrent);
                if (delayForThisStep >= sampleCount)
                {
                    // don't queue up any samples beyond the requested time range
                    break;
                }

                if (Pattern.Pattern.TryGetValue(currentStep, out HashSet<FretboardNote> notes))
                {
                    foreach (FretboardNote note in notes)
                    {
                        (Note n, int octave) = note.ResolveNote(this.Tuning);
                        var sampleProvider = new NoteSampleProvider(n, octave, this.synth);
                        sampleProvider.DelayBy = (int)(samplesPerStep * note.timeRange.Start / (256 / 4));
                        sampleProvider.Duration = (int)(samplesPerStep * note.timeRange.Duration / (256 / 4));
                        sampleProvider.PlayingStateChanged += s => this.NotePlayingStateChanged?.Invoke(note, s);

                        Debug.WriteLine("beat at step {0}, patternPostion={1}, delayBy {2}", currentStep, patternPosition, delayForThisStep);
                        mixerInputs.Add(sampleProvider);
                    }
                }

                samplePos += samplesPerStep;
                currentStep++;
                if (!this.Loop && currentStep >= Pattern.Steps)
                {
                    break;
                }

                currentStep = currentStep % Pattern.Steps;

            }

            patternPosition += ((double)sampleCount / samplesPerStep);
            if (patternPosition > Pattern.Steps)
            {
                patternPosition -= Pattern.Steps;
            }

            return mixerInputs;
        }

        public event Action<FretboardNote, bool> NotePlayingStateChanged;
    }
}
