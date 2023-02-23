using Composer.Util;
using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class Row
    {
        ViewManager manager;
        public Util.TimeRange timeRange;
        public Util.Rect layoutRect;
        public Util.Rect contentRect;

        public TrackSegmentMeterChanges trackSegmentMeterChanges;
        public List<TrackSegment> trackSegments;

        public bool isLastRow;
        public Tuning tuning;
        public float resizeEndTime;

        const int SECTION_HANDLE_HEIGHT = 16;
        const int ADD_SECTION_BUTTON_SIZE = 20;
        const int ADD_SECTION_BUTTON_MARGIN = 2;


        public Row(ViewManager manager, Util.TimeRange timeRange, bool isLastRow, Tuning tuning)
        {
            this.manager = manager;
            this.timeRange = timeRange;
            this.resizeEndTime = timeRange.End;
            this.trackSegments = new List<TrackSegment>();
            this.isLastRow = isLastRow;
            this.tuning = tuning;

            this.trackSegmentMeterChanges = new TrackSegmentMeterChanges(manager, this);
            this.trackSegments.Add(this.trackSegmentMeterChanges);
        }


        public void RefreshLayout(float x, float y)
        {
            this.resizeEndTime = this.timeRange.End;

            this.layoutRect = new Util.Rect(
                x, y,
                x, y + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN + SECTION_HANDLE_HEIGHT);

            this.contentRect = new Util.Rect(
                x, this.layoutRect.yMax,
                x, this.layoutRect.yMax);

            foreach (var track in this.trackSegments)
            {
                track.RefreshLayout(x, this.layoutRect.yMax);
                this.layoutRect = this.layoutRect.Include(track.layoutRect);
                this.contentRect = this.contentRect.Include(track.contentRect);
            }

            this.layoutRect.yMax += ADD_SECTION_BUTTON_MARGIN;
        }


        public float GetTimeAtPosition(float x)
        {
            return this.timeRange.Start + (x - this.layoutRect.xMin) / this.manager.TimeToPixelsMultiplier;
        }

        public void Draw(Graphics g)
        {
            foreach (var track in this.trackSegments)
                track.Draw(g);
        }


        public void DrawOverlay(Graphics g)
        {
            if (this.resizeEndTime != this.timeRange.End)
            {
                var overlayStartTime = (this.resizeEndTime < this.timeRange.End) ?
                    this.resizeEndTime : this.timeRange.End;

                var overlayEndTime = (this.resizeEndTime > this.timeRange.End) ?
                    this.resizeEndTime : this.timeRange.End;

                var overlayStartX =
                    (int)(this.layoutRect.xMin +
                    (overlayStartTime - this.timeRange.Start) * this.manager.TimeToPixelsMultiplier);

                var overlayEndX =
                    (int)(this.layoutRect.xMin +
                    (overlayEndTime - this.timeRange.Start) * this.manager.TimeToPixelsMultiplier);

                using (var brush = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    g.FillRectangle(brush,
                        overlayStartX + 1,
                        (int)this.layoutRect.yMin + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN * 2,
                        overlayEndX - overlayStartX,
                        (int)this.layoutRect.ySize - ADD_SECTION_BUTTON_SIZE - ADD_SECTION_BUTTON_MARGIN * 2);
                }
            }

            this.DrawCursor(g);
        }


        private void DrawCursor(Graphics g)
        {
            var cursorTimeRange = this.manager.CursorTimeRange;
            var cursorFirstTrack = this.manager.CursorFirstTrackIndex;
            var cursorLastTrack = this.manager.CursorLastTrackIndex;

            if (this.manager.cursorVisible &&
                this.timeRange.OverlapsRangeInclusive(cursorTimeRange))
            {
                var drawStart = this.timeRange.OverlapsInclusive(cursorTimeRange.Start);
                var drawEnd = this.timeRange.OverlapsInclusive(cursorTimeRange.End);

                var cursorStartX = this.layoutRect.xMin +
                    (cursorTimeRange.Start - this.timeRange.Start) * this.manager.TimeToPixelsMultiplier;

                var cursorEndX = this.layoutRect.xMin +
                    (cursorTimeRange.End - this.timeRange.Start) * this.manager.TimeToPixelsMultiplier;

                var cursorStartY = this.trackSegments[cursorFirstTrack].layoutRect.yMin;
                var cursorEndY = this.trackSegments[cursorLastTrack].layoutRect.yMax;

                Brush brush = this.manager._audioOut is null ? Brushes.Blue : Brushes.IndianRed;
                using (var pen = new Pen(this.manager._audioOut is null ? Color.Blue : Color.IndianRed, 3))
                {
                    if (drawStart)
                    {
                        g.DrawLine(pen,
                            cursorStartX, cursorStartY,
                            cursorStartX, cursorEndY);

                        g.FillPolygon(brush, new PointF[]
                        {
                            new PointF(cursorStartX, cursorStartY + 7),
                            new PointF(cursorStartX, cursorStartY),
                            new PointF(cursorStartX + 7, cursorStartY)
                        });

                        g.FillPolygon(brush, new PointF[]
                        {
                            new PointF(cursorStartX, cursorEndY - 7),
                            new PointF(cursorStartX, cursorEndY),
                            new PointF(cursorStartX + 7, cursorEndY)
                        });
                    }

                    if (drawEnd)
                    {
                        g.DrawLine(pen,
                            cursorEndX, cursorStartY,
                            cursorEndX, cursorEndY);

                        g.FillPolygon(brush, new PointF[]
                        {
                            new PointF(cursorEndX, cursorStartY + 7),
                            new PointF(cursorEndX, cursorStartY),
                            new PointF(cursorEndX - 7, cursorStartY)
                        });

                        g.FillPolygon(brush, new PointF[]
                        {
                            new PointF(cursorEndX, cursorEndY - 7),
                            new PointF(cursorEndX, cursorEndY),
                            new PointF(cursorEndX - 7, cursorEndY)
                        });
                    }
                }
            }
        }


        private void DrawResizeHandle(Graphics g, bool selected, bool hovering)
        {
            var endX = (int)(this.layoutRect.xMin +
                (this.resizeEndTime - this.timeRange.Start) * this.manager.TimeToPixelsMultiplier);

            using (var pen = new Pen(
                selected ? Color.Gray :
                hovering ? Color.DarkGray : Color.Black,
                3))
            {
                g.DrawLine(pen,
                    endX, (int)this.layoutRect.yMin + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN * 2,
                    endX, (int)this.contentRect.yMax);

                g.FillRectangle(
                    selected ? Brushes.Gray :
                    hovering ? Brushes.DarkGray : Brushes.Black,
                    endX - 5, (int)this.layoutRect.yMin + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN * 2,
                    11, 11);
            }
        }


        private void DrawAddSectionButton(Graphics g, float x, float y, bool selected, bool hovering)
        {
            g.FillRectangle(
                selected ? Brushes.DarkCyan :
                hovering ? Brushes.LightCyan : Brushes.Transparent,
                x, y,
                ADD_SECTION_BUTTON_SIZE, ADD_SECTION_BUTTON_SIZE);

            g.DrawRectangle(
                Pens.Black,
                x, y,
                ADD_SECTION_BUTTON_SIZE, ADD_SECTION_BUTTON_SIZE);

            using (var pen = new Pen(Color.Green, 3))
            {
                g.DrawLine(pen,
                    x + 4, y + ADD_SECTION_BUTTON_SIZE / 2,
                    x + ADD_SECTION_BUTTON_SIZE - 3, y + ADD_SECTION_BUTTON_SIZE / 2);
                g.DrawLine(pen,
                    x + ADD_SECTION_BUTTON_SIZE / 2, y + 4,
                    x + ADD_SECTION_BUTTON_SIZE / 2, y + ADD_SECTION_BUTTON_SIZE - 3);
            }
        }


        public float DrawnYMax
        {
            get
            {
                return this.trackSegments[this.trackSegments.Count - 1].layoutRect.yMax;
            }
        }
    }
}
