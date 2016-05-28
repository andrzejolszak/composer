﻿using System.Collections.Generic;
using System.Linq;


namespace Composer.Project
{
    public class Project
    {
        float length;
        public Util.TimeSortedList<SectionBreak> sectionBreaks;
        public Util.TimeSortedList<KeyChange> keyChanges;
        public Util.TimeSortedList<MeterChange> meterChanges;
        public List<Track> tracks;


        public Project()
        {
            this.length = this.WholeNoteDuration * 4;
            this.sectionBreaks = new Util.TimeSortedList<SectionBreak>(sb => sb.time);
            this.keyChanges = new Util.TimeSortedList<KeyChange>(kc => kc.time);
            this.meterChanges = new Util.TimeSortedList<MeterChange>(mc => mc.time);
            this.tracks = new List<Track>();

            this.InsertMeterChange(new MeterChange(0, new Util.Meter(4, 4)));
        }


        public float WholeNoteDuration
        {
            get { return 960; }
        }


        public float Length
        {
            get { return this.length; }
        }


        public void InsertSectionBreak(SectionBreak newSectionBreak)
        {
            if (newSectionBreak.time <= 0 || newSectionBreak.time >= this.length)
                return;

            this.sectionBreaks.RemoveAll(sb => sb.time == newSectionBreak.time);
            this.sectionBreaks.Add(newSectionBreak);
        }


        public void InsertKeyChange(KeyChange newKeyChange)
        {
            if (newKeyChange.time < 0 || newKeyChange.time >= this.length)
                return;

            this.keyChanges.RemoveAll(kc => kc.time == newKeyChange.time);
            this.keyChanges.Add(newKeyChange);
        }


        public void InsertMeterChange(MeterChange newMeterChange)
        {
            if (newMeterChange.time < 0 || newMeterChange.time >= this.length)
                return;

            this.meterChanges.RemoveAll(mc => mc.time == newMeterChange.time);
            this.meterChanges.Add(newMeterChange);
        }


        public void MoveSectionBreak(SectionBreak sectionBreak, float newTime)
        {
            this.sectionBreaks.Remove(sectionBreak);
            sectionBreak.time = newTime;
            this.InsertSectionBreak(sectionBreak);
        }


        public void MoveKeyChange(KeyChange keyChange, float newTime)
        {
            this.keyChanges.Remove(keyChange);
            keyChange.time = newTime;
            this.InsertKeyChange(keyChange);
        }


        public void MoveMeterChange(MeterChange meterChange, float newTime)
        {
            this.meterChanges.Remove(meterChange);
            meterChange.time = newTime;
            this.InsertMeterChange(meterChange);
        }


        public void InsertEmptySpace(float startTime, float duration)
        {
            if (startTime < 0 || duration <= 0)
                return;

            this.length += duration;

            foreach (var sectionBreak in this.sectionBreaks.Clone())
            {
                if (sectionBreak.time >= startTime)
                    this.MoveSectionBreak(sectionBreak, sectionBreak.time + duration);
            }

            foreach (var keyChange in this.keyChanges.Clone())
            {
                if (keyChange.time >= startTime)
                    this.MoveKeyChange(keyChange, keyChange.time + duration);
            }

            foreach (var meterChange in this.meterChanges.Clone())
            {
                if (meterChange.time >= startTime)
                    this.MoveMeterChange(meterChange, meterChange.time + duration);
            }

            foreach (var track in this.tracks)
                track.InsertEmptySpace(startTime, duration);
        }


        public void CutRange(Util.TimeRange timeRange)
        {
            foreach (var track in this.tracks)
                track.CutRange(timeRange);

            this.sectionBreaks.RemoveAll(sb => timeRange.Overlaps(sb.time));
            foreach (var sectionBreak in this.sectionBreaks.Clone())
            {
                if (sectionBreak.time >= timeRange.Start)
                    this.MoveSectionBreak(sectionBreak, sectionBreak.time - timeRange.Duration);
            }

            this.keyChanges.RemoveAll(kc => timeRange.Overlaps(kc.time));
            foreach (var keyChange in this.keyChanges.Clone())
            {
                if (keyChange.time >= timeRange.Start)
                    this.MoveKeyChange(keyChange, keyChange.time - timeRange.Duration);
            }

            this.meterChanges.RemoveAll(mc => timeRange.Overlaps(mc.time));
            foreach (var meterChange in this.meterChanges.Clone())
            {
                if (meterChange.time >= timeRange.Start)
                    this.MoveMeterChange(meterChange, meterChange.time - timeRange.Duration);
            }

            this.length -= timeRange.Duration;
        }


        public void InsertSection(float atTime, float duration)
        {
            if (atTime < 0 || duration <= 0)
                return;

            this.InsertEmptySpace(atTime, duration);
            this.InsertSectionBreak(new SectionBreak(atTime));
            this.InsertSectionBreak(new SectionBreak(atTime + duration));
        }
    }
}
