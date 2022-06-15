using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using NAudio.Wave;

namespace Composer.AudioOut
{
    class PatternSequencer
    {
        private readonly NotePattern drumPattern;
        private readonly SampleKit drumKit;
        private int tempo;
        private int samplesPerStep;

        public PatternSequencer(NotePattern drumPattern, SampleKit kit)
        {
            drumKit = kit;
            this.drumPattern = drumPattern;
            Tempo = 120;
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

                for (int note = 0; note < drumPattern.Notes; note++)
                {
                    if (drumPattern[note, currentStep] != 0)
                    {
                        var sampleProvider = drumKit.GetSampleProvider(note);
                        sampleProvider.DelayBy = delayForThisStep;
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
    }
}
