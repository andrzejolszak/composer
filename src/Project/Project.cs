using System.Collections.Generic;


namespace Composer.Project
{
    public class Project
    {
        float length;
        public Util.TimeSortedList<SectionBreak> sectionBreaks;
        public Util.TimeSortedList<MeterChange> meterChanges;
        public List<Track> tracks;

        public Project(float startingLength)
        {
            this.length = startingLength;
            this.sectionBreaks = new Util.TimeSortedList<SectionBreak>(sb => sb.time);
            this.meterChanges = new Util.TimeSortedList<MeterChange>(mc => mc.time);
            this.tracks = new List<Track>();
        }


        public float BarDuration
        {
            get { return 256; }
        }


        public float Length
        {
            get { return this.length; }
        }


        public int GetTrackIndex(Track track)
        {
            return this.tracks.FindIndex(tr => tr == track);
        }


        public void InsertSectionBreak(SectionBreak newSectionBreak)
        {
            if (newSectionBreak.time <= 0 || newSectionBreak.time >= this.length)
                return;

            this.sectionBreaks.RemoveAll(sb => sb.time == newSectionBreak.time);
            this.sectionBreaks.Add(newSectionBreak);
        }


        public void InsertMeterChange(MeterChange newMeterChange)
        {
            if (newMeterChange.time < 0 || newMeterChange.time >= this.length)
                return;

            this.meterChanges.RemoveAll(mc => mc.time == newMeterChange.time);
            this.meterChanges.Add(newMeterChange);
        }


        public void InsertPitchedNote(int trackIndex, FretboardNote pitchedNote)
        {
            float start = System.Math.Max(
                0,
                pitchedNote.timeRange.Start);
            pitchedNote.timeRange = new Util.TimeRange (
                start, 
                System.Math.Min(
                this.length - start,
                pitchedNote.timeRange.Duration));

            if (pitchedNote.timeRange.Duration <= 0)
                return;

            var track = (TrackFretboardNotes)this.tracks[trackIndex];
            track.InsertPitchedNote(pitchedNote);
        }


        public void RemovePitchedNote(int trackIndex, FretboardNote pitchedNote)
        {
            var track = (TrackFretboardNotes)this.tracks[trackIndex];
            track.RemovePitchedNote(pitchedNote);
        }


        public void RemoveSectionBreak(SectionBreak sectionBreak)
        {
            this.sectionBreaks.Remove(sectionBreak);
        }


        public void RemoveMeterChange(MeterChange meterChange)
        {
            this.meterChanges.Remove(meterChange);
        }

        public void CutRange(Util.TimeRange timeRange)
        {
            foreach (var track in this.tracks)
                track.CutRange(timeRange);

            this.sectionBreaks.RemoveAll(sb => timeRange.Overlaps(sb.time));
            foreach (var sectionBreak in this.sectionBreaks.Clone())
            {
                if (sectionBreak.time >= timeRange.Start)
                {
                    this.RemoveSectionBreak(sectionBreak);
                    sectionBreak.time -= timeRange.Duration;
                    this.InsertSectionBreak(sectionBreak);
                }
            }

            this.meterChanges.RemoveAll(mc => timeRange.Overlaps(mc.time));
            foreach (var meterChange in this.meterChanges.Clone())
            {
                if (meterChange.time >= timeRange.Start)
                {
                    this.RemoveMeterChange(meterChange);
                    meterChange.time -= timeRange.Duration;
                    this.InsertMeterChange(meterChange);
                }
            }
        }


        public void InsertSection(float atTime, float duration)
        {
            if (atTime < 0 || duration <= 0)
                return;

            this.InsertSectionBreak(new SectionBreak(atTime));
            this.InsertSectionBreak(new SectionBreak(atTime + duration));
        }
    }
}
