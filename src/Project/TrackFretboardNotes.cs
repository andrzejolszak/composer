using Composer.Util;
using System.Collections.Generic;


namespace Composer.Project
{
    public class TrackFretboardNotes : Track
    {
        public Tuning Tuning { get; }

        public Util.TimeRangeSortedList<FretboardNote> notes;

        public string KitName { get; private set; }

        public TrackFretboardNotes(string name, Tuning tuning, string kitName)
        {
            this.name = name;
            this.Tuning = tuning;
            this.notes = new Util.TimeRangeSortedList<FretboardNote>(n => n.timeRange);
            this.KitName = kitName;
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
