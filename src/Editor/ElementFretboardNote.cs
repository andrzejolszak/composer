using System;
using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class ElementFretboardNote : Element
    {
        int assignedTrack = -1;
        public Project.FretboardNote Note;
        public Project.TrackFretboardNotes projectTrackPitchedNode;
        List<Segment> segments;

        Util.TimeRange _timeRange;
        int _stringNo;
        int _fret;
        private (Util.Note, int) _resolvedNote;

        public ElementFretboardNote(
            ViewManager manager,
            Project.TrackFretboardNotes projectTrackPitchedNode,
            Project.FretboardNote pitchedNote)
            : base(manager)
        {
            this.projectTrackPitchedNode = projectTrackPitchedNode;
            this.Note = pitchedNote;
            this.interactableRegions = new List<InteractableRegion>();
            this.segments = new List<Segment>();

            this._timeRange = this.Note.timeRange;
            this._stringNo = this.Note.StringNo;
            this._fret = this.Note.Fret;
            this._resolvedNote = this.Note.ResolveNote(this.projectTrackPitchedNode.Tuning);

            this.assignedTrack = -1;
            for (var i = 0; i < this.manager.rows[0].trackSegments.Count; i++)
            {
                var trackPitchedNote = (this.manager.rows[0].trackSegments[i] as TrackSegmentFretboardNotes);
                if (trackPitchedNote != null &&
                    trackPitchedNote.projectTracks.Contains(this.projectTrackPitchedNode))
                {
                    this.assignedTrack = i;
                    break;
                }
            }
        }


        class Segment
        {
            public Util.Rect noteRect;
        }


        public override void RefreshLayout()
        {
            this.segments.Clear();
            this.interactableRegions.Clear();

            var tMult = this.manager.TimeToPixelsMultiplier;
            var pMult = this.manager.PitchedNoteHeight;

            foreach (var row in this.manager.EnumerateRowsInTimeRange(this._timeRange))
            {
                var trackPitchedNote = (TrackSegmentFretboardNotes)row.trackSegments[this.assignedTrack];

                var startTimeMinusTrackStart = System.Math.Max(
                    0,
                    this._timeRange.Start - trackPitchedNote.row.timeRange.Start);

                var endTimeMinusTrackStart = System.Math.Min(
                    trackPitchedNote.row.timeRange.End,
                    this._timeRange.End) - trackPitchedNote.row.timeRange.Start;

                var noteRect = new Util.Rect(
                    trackPitchedNote.contentRect.xMin + tMult * startTimeMinusTrackStart,
                    trackPitchedNote.contentRect.yMax - pMult * (_stringNo + 1),
                    trackPitchedNote.contentRect.xMin + tMult * endTimeMinusTrackStart,
                    trackPitchedNote.contentRect.yMax - pMult * _stringNo);

                this.segments.Add(new Segment { noteRect = noteRect });

                this.interactableRegions.Add(
                    new InteractableRegion(InteractableRegion.CursorKind.MoveAll, noteRect));
            }
        }


        public override void BeginModify()
        {
            this.manager.project.RemovePitchedNote(
                this.manager.project.GetTrackIndex(this.projectTrackPitchedNode),
                this.Note);
        }


        public override void EndModify()
        {
            this.Note.timeRange = this._timeRange;
            this.Note.StringNo = this._stringNo;
            this.Note.Fret = this._fret;

            this.manager.project.InsertPitchedNote(
                this.manager.project.GetTrackIndex(this.projectTrackPitchedNode),
                this.Note);
        }


        public override void Drag()
        {
            this._timeRange =
                this.Note.timeRange.OffsetBy(this.manager.DragTimeOffsetClampedToRow);

            this._stringNo = (int)Math.Round(
                this.Note.StringNo + this.manager.DragMidiPitchOffset);
        }


        public override void OnPressUp(bool ctrlKey, bool shiftKey)
        {
            this._stringNo++;
        }


        public override void OnPressDown(bool ctrlKey, bool shiftKey)
        {
            this._stringNo--;
        }


        public override void OnPressRight(bool ctrlKey, bool shiftKey)
        {
            if (shiftKey)
            {
                this._timeRange.Duration =
                    System.Math.Max(
                        this.manager.TimeSnap,
                        this._timeRange.Duration + this.manager.TimeSnap);
            }
            else
                this._timeRange =
                    this._timeRange.OffsetBy(this.manager.TimeSnap);
        }


        public override void OnPressLeft(bool ctrlKey, bool shiftKey)
        {
            if (shiftKey)
            {
                this._timeRange.Duration =
                    System.Math.Max(
                        this.manager.TimeSnap,
                        this._timeRange.Duration - this.manager.TimeSnap);
            }
            else
                this._timeRange =
                    this._timeRange.OffsetBy(-this.manager.TimeSnap);
        }


        public override void Draw(Graphics g, bool hovering, bool selected)
        {
            foreach (var segment in this.segments)
            {
                g.FillRectangle(
                    (hovering ? Brushes.LightGray : Brushes.White),
                    segment.noteRect.xMin + 6,
                    segment.noteRect.yMin + 1,
                    segment.noteRect.xSize - 13,
                    segment.noteRect.ySize - 1);
                
                if (selected)
                {
                    g.DrawRectangle(
                        Pens.Salmon,
                        segment.noteRect.xMin + 5,
                        segment.noteRect.yMin,
                        segment.noteRect.xSize - 12,
                        segment.noteRect.ySize);
                }

                using (var font = new Font("Verdana", this.manager.PitchedNoteHeight / 1.5f))
                {
                    g.DrawString(
                        this._fret.ToString(),
                        font,
                        Brushes.Black,
                        segment.noteRect.xMin + segment.noteRect.xSize / 2 - 5,
                        segment.noteRect.yMin);

                    if (hovering)
                    {
                        g.DrawString(
                            Util.RelativePitchData.GetSimpleName(_resolvedNote.Item1) + + _resolvedNote.Item2,
                            font,
                            Brushes.Green,
                            segment.noteRect.xMin + segment.noteRect.xSize / 2 + 10,
                            segment.noteRect.yMin - segment.noteRect.ySize / 2);
                    }
                }
            }
        }
    }
}
