using System;
using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class ElementFretboardNote : Element
    {
        public Project.FretboardNote Note;
        public Project.TrackFretboardNotes projectTrackPitchedNode;
        List<Segment> segments;

        Util.TimeRange _timeRange;
        int _stringNo;
        int _fret;
        private (Util.Note, int) _resolvedNote;
        public TrackSegmentFretboardNotes trackPitchedNote;

        public ElementFretboardNote(
            ViewManager manager,
            Project.TrackFretboardNotes projectTrackPitchedNode,
            TrackSegmentFretboardNotes trackPitchedNote,
            Project.FretboardNote pitchedNote)
            : base(manager)
        {
            this.projectTrackPitchedNode = projectTrackPitchedNode;
            this.Note = pitchedNote;
            this.segments = new List<Segment>();

            this._timeRange = this.Note.timeRange;
            this._stringNo = this.Note.StringNo;
            this._fret = this.Note.Fret;
            this._resolvedNote = this.Note.ResolveNote(this.projectTrackPitchedNode.Tuning);

            this.trackPitchedNote = trackPitchedNote;
        }


        class Segment
        {
            public Util.Rect noteRect;
        }


        public override void RefreshLayout()
        {
            this.segments.Clear();

            var tMult = this.manager.TimeToPixelsMultiplier;
            var pMult = this.manager.PitchedNoteHeight;

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
        }

        public override void Draw(Graphics g, bool hovering)
        {
            foreach (var segment in this.segments)
            {
                g.FillRectangle(
                    (hovering ? Brushes.LightGray : Brushes.White),
                    segment.noteRect.xMin + 6,
                    segment.noteRect.yMin + 1,
                    segment.noteRect.xSize - 13,
                    segment.noteRect.ySize - 1);
                
                if (Highlighted)
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
