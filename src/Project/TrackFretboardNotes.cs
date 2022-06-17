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


        public override void InsertEmptySpace(float startTime, float duration)
        {
            this.SplitNotesAt(startTime);
            foreach (var note in this.notes.EnumerateEntirelyAfter(startTime))
                note.timeRange = note.timeRange.OffsetBy(duration);
        }


        public override void CutRange(Util.TimeRange timeRange)
        {
            this.SplitNotesAt(timeRange.Start);
            this.SplitNotesAt(timeRange.End);
            this.notes.RemoveOverlappingRange(timeRange);
            foreach (var note in this.notes.EnumerateEntirelyAfter(timeRange.End))
                note.timeRange = note.timeRange.OffsetBy(-timeRange.Duration);
        }


        public void EraseRange(Util.TimeRange timeRange, int? onlyAtString = null)
        {
            this.SplitNotesAt(timeRange.Start, onlyAtString);
            this.SplitNotesAt(timeRange.End, onlyAtString);

            this.notes.RemoveOverlappingRange(timeRange,
                (note) => !onlyAtString.HasValue || note.StringNo == onlyAtString);
        }


        public void SplitNotesAt(float splitTime, int? onlyAtString = null)
        {
            var newNotes = new List<FretboardNote>();

            foreach (var note in this.notes.EnumerateOverlapping(splitTime))
            {
                if (onlyAtString.HasValue && note.StringNo != onlyAtString)
                    continue;

                newNotes.Add(new FretboardNote
                {
                    StringNo = note.StringNo,
                    Fret = note.Fret,
                    timeRange = Util.TimeRange.StartEnd(splitTime, note.timeRange.End)
                });
                note.timeRange = new Util.TimeRange(note.timeRange.Start, splitTime);
            }

            this.notes.Sort();
            this.notes.AddRange(newNotes);
        }
    }
}
