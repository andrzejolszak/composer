using System.Collections.Generic;


namespace Composer.Project
{
    public class TrackFretboardNotes : Track
    {
        public Util.TimeRangeSortedList<FretboardNote> notes;


        public TrackFretboardNotes(string name)
        {
            this.name = name;
            this.notes = new Util.TimeRangeSortedList<FretboardNote>(n => n.timeRange);
        }


        public void InsertPitchedNote(FretboardNote pitchedNote)
        {
            this.EraseRange(pitchedNote.timeRange, pitchedNote.StringNo);
            this.notes.Add(pitchedNote);
        }


        public void RemovePitchedNote(FretboardNote pitchedNote)
        {
            this.notes.Remove(pitchedNote);
        }

        public override void CutRange(Util.TimeRange timeRange)
        {
            this.notes.RemoveOverlappingRange(timeRange);
            foreach (var note in this.notes.EnumerateEntirelyAfter(timeRange.End))
                note.timeRange = note.timeRange.OffsetBy(-timeRange.Duration);
        }


        public void EraseRange(Util.TimeRange timeRange, int? onlyAtString = null)
        {
            this.notes.RemoveOverlappingRange(timeRange,
                (note) => !onlyAtString.HasValue || note.StringNo == onlyAtString);
        }
    }
}
