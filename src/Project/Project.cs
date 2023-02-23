using System.Collections.Generic;


namespace Composer.Project
{
    public class Project
    {
        float length;
        public Util.TimeSortedList<MeterChange> meterChanges;
        public List<FretboardNotesTrack> tracks;

        public Project(float startingLength)
        {
            this.length = startingLength;
            this.meterChanges = new Util.TimeSortedList<MeterChange>(mc => mc.time);
            this.tracks = new List<FretboardNotesTrack>();
        }


        public float BarDuration
        {
            get { return 256; }
        }


        public float Length
        {
            get { return this.length; }
        }


        public int GetTrackIndex(FretboardNotesTrack track)
        {
            return this.tracks.FindIndex(tr => tr == track);
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

            var track = (FretboardNotesTrack)this.tracks[trackIndex];
            track.InsertPitchedNote(pitchedNote);
        }


        public void RemovePitchedNote(int trackIndex, FretboardNote pitchedNote)
        {
            var track = (FretboardNotesTrack)this.tracks[trackIndex];
            track.RemovePitchedNote(pitchedNote);
        }


        public void RemoveMeterChange(MeterChange meterChange)
        {
            this.meterChanges.Remove(meterChange);
        }
    }
}
