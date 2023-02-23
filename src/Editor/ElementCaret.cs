using Composer.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Composer.Editor
{
    class ElementCaret : Element
    {
        int _stringIndex;
        private int _trackIndex;
        private TimeRange _timeRange;
        private Rect _rect;
        private ElementFretboardNote _targetNote;

        public ElementCaret(
            ViewManager manager)
            : base(manager)
        {
            this.interactableRegions = new List<InteractableRegion>();

            this._stringIndex = 1;
            this._trackIndex = 0;
            this._timeRange = new TimeRange(manager.project.BarDuration / 4, manager.project.BarDuration / 4);

            this.Selected = true;
        }

        public override void SetSelected(bool isSelected)
        {
            this.Selected = true;
        }

        public override void RefreshLayout()
        {
            this.interactableRegions.Clear();

            // TODO: segments
            TrackSegmentFretboardNotes trackPitchedNotes = this.manager.rows[this._trackIndex].trackSegments.SingleOrDefault(x => x is TrackSegmentFretboardNotes) as TrackSegmentFretboardNotes;

            if (trackPitchedNotes is null)
            {
                return;
            }

            var tMult = this.manager.TimeToPixelsMultiplier;
            var pMult = this.manager.PitchedNoteHeight;

            var startTimeMinusTrackStart = Math.Max(
                0,
                this._timeRange.Start - trackPitchedNotes.row.timeRange.Start);

            var endTimeMinusTrackStart = Math.Min(
                trackPitchedNotes.row.timeRange.End,
                this._timeRange.End) - trackPitchedNotes.row.timeRange.Start;

            this._rect = new Rect(
                trackPitchedNotes.contentRect.xMin + tMult * startTimeMinusTrackStart,
                trackPitchedNotes.contentRect.yMax - pMult * (_stringIndex + 1),
                trackPitchedNotes.contentRect.xMin + tMult * endTimeMinusTrackStart,
                trackPitchedNotes.contentRect.yMax - pMult * _stringIndex);

            this.interactableRegions.Add(new InteractableRegion(InteractableRegion.CursorKind.MoveAll, this._rect));
        }


        public override void BeginModify()
        {
            this.UpdateCurrentPositionSelection();
        }


        public override void EndModify()
        {
            this._targetNote.SetSelected(false);
            this.UpdateCurrentPositionSelection();
            this._targetNote.SetSelected(true);
        }


        public override void OnPressUp(bool ctrlKey, bool shiftKey)
        {
            this._stringIndex++;
            if (this._stringIndex > 5)
            {
                this._trackIndex--;
                this._stringIndex = 0;
            }
        }


        public override void OnPressDown(bool ctrlKey, bool shiftKey)
        {
            this._stringIndex--;
            if (this._stringIndex < 0)
            {
                this._trackIndex++;
                this._stringIndex = 5;
            }
        }


        public override void OnPressRight(bool ctrlKey, bool shiftKey)
        {
            this._timeRange = this._timeRange.OffsetBy(this.manager.TimeSnap);
        }


        public override void OnPressLeft(bool ctrlKey, bool shiftKey)
        {
            this._timeRange = this._timeRange.OffsetBy(-this.manager.TimeSnap);
        }


        public override void Draw(Graphics g, bool hovering)
        {
            if (this._rect is null)
            {
                return;
            }

            g.DrawRectangle(
                Pens.Magenta,
                this._rect.xMin,
                this._rect.yMin,
                this._rect.xSize,
                this._rect.ySize);
        }

        private void UpdateCurrentPositionSelection()
        {
            TrackSegmentFretboardNotes caretSegment = this.manager.rows[this._trackIndex].trackSegments.SingleOrDefault(x => x is TrackSegmentFretboardNotes) as TrackSegmentFretboardNotes;
            foreach (var element in this.manager.elements)
            {
                var note = element as ElementFretboardNote;
                if (note != null && note.Note.StringNo == this._stringIndex && note.Note.timeRange.Start == this._timeRange.Start && note.trackPitchedNote == caretSegment)
                {
                    this._targetNote = note;
                }
            }
        }
    }
}
