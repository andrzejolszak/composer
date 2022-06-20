using Composer.Util;
using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class TrackSegmentFretboardNotes : TrackSegment
    {
        public List<Project.TrackFretboardNotes> projectTracks;

        const float LAYOUT_MARGIN = 4;


        public TrackSegmentFretboardNotes(
            ViewManager manager,
            Row row,
            List<Project.TrackFretboardNotes> projectTracks)
            : base(manager, row)
        {
            this.projectTracks = projectTracks;
        }


        public override void RefreshLayout(float x, float y)
        {
            var pitchRange = 6;

            this.layoutRect = new Util.Rect(
                x,
                y,
                x + this.row.timeRange.Duration * this.manager.TimeToPixelsMultiplier,
                y + LAYOUT_MARGIN * 2 + pitchRange * this.manager.PitchedNoteHeight);

            this.contentRect = new Util.Rect(
                x,
                y + LAYOUT_MARGIN,
                x + this.row.timeRange.Duration * this.manager.TimeToPixelsMultiplier,
                y + LAYOUT_MARGIN + pitchRange * this.manager.PitchedNoteHeight);
        }


        public override float GetTimeAtPosition(float x)
        {
            return this.row.timeRange.Start + (x - this.contentRect.xMin) / this.manager.TimeToPixelsMultiplier;
        }


        public override int GetStringAtPosition(float y)
        {
            return 6 - (int)((this.contentRect.yMax - y) / this.manager.PitchedNoteHeight);
        }


        public override void Draw(Graphics g)
        {
            var rowEndTime = System.Math.Max(this.row.timeRange.End, this.row.resizeEndTime);
            var rowDuration = rowEndTime - this.row.timeRange.Start;
            var rowEndX = (int)(this.layoutRect.xMin + rowDuration * this.manager.TimeToPixelsMultiplier);

            // Draw beat separators.
            for (var i = 0; i < this.row.trackSegmentMeterChanges.affectingMeterChanges.Count; i++)
            {
                var meterChange = this.row.trackSegmentMeterChanges.affectingMeterChanges[i];
                if (meterChange == null)
                    continue;

                var meterEndTime = rowEndTime;
                if (i + 1 < this.row.trackSegmentMeterChanges.affectingMeterChanges.Count)
                    meterEndTime = this.row.trackSegmentMeterChanges.affectingMeterChanges[i + 1].time;

                var beatCount = 0;
                var beatDuration = this.manager.project.BarDuration / meterChange.meter.denominator;

                for (var n = meterChange.time; n < meterEndTime; n += beatDuration)
                {
                    if (n > this.row.timeRange.Start)
                    {
                        var nMinusRowStart = n - this.row.timeRange.Start;
                        var x = (int)this.contentRect.xMin + nMinusRowStart * this.manager.TimeToPixelsMultiplier;

                        g.DrawLine(beatCount == 0 ? Pens.Gray : Pens.LightGray,
                            x,
                            (int)(this.contentRect.yMin),
                            x,
                            (int)(this.contentRect.yMax));
                    }

                    beatCount = (beatCount + 1) % meterChange.meter.numerator;
                }
            }

            using (var font = new Font("Verdana", this.manager.PitchedNoteHeight / 1.5f))
            {
                var keyStartTime = this.row.timeRange.Start;

                var keyEndTime = rowEndTime;

                var keyStartX = (int)
                    (this.contentRect.xMin + (keyStartTime - this.row.timeRange.Start) *
                    this.manager.TimeToPixelsMultiplier);

                var keyEndX = (int)
                    (this.contentRect.xMin + (keyEndTime - this.row.timeRange.Start) *
                    this.manager.TimeToPixelsMultiplier);

                for (var p = 0; p < this.row.tuning.TuningStrings.Count; p++)
                {
                    var y = (int) (this.contentRect.yMax - (p - 0) * this.manager.PitchedNoteHeight);

                    using Pen stringPen = new Pen(Brushes.Gray, 0.1f + 2f * ((this.row.tuning.TuningStrings.Count - p) / 6f));
                    g.DrawLine(stringPen, keyStartX, y - this.manager.PitchedNoteHeight / 2f, keyEndX, y - this.manager.PitchedNoteHeight / 2f);

                    (Note, int) e = this.row.tuning.TuningStrings[p];
                    g.DrawString(
                        Util.RelativePitchData.GetSimpleName(e.Item1) + e.Item2,
                        font,
                        Brushes.MediumVioletRed,
                        keyStartX - 15, y - this.manager.PitchedNoteHeight);
                }
            }
        }
    }
}
