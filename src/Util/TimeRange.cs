﻿namespace Composer.Util
{
    public struct TimeRange
    {
        float start;
        float end;


        public TimeRange(float start, float duration)
        {
            this.start = start;
            this.end = start + duration;
        }


        public static TimeRange StartEnd(float start, float end)
        {
            return new TimeRange { start = start, end = end };
        }


        public float Start
        {
            get { return this.start; }
            set { this.start = value; }
        }


        public float End
        {
            get { return this.end; }
            set { this.end = value; }
        }


        public float Duration
        {
            get { return this.end - this.start; }
            set { this.end = this.start + value; }
        }


        public TimeRange OffsetBy(float amount)
        {
            return new TimeRange(this.start + amount, this.Duration);
        }


        public bool Overlaps(float time)
        {
            return time >= this.start && time < this.end;
        }


        public bool OverlapsInclusive(float time)
        {
            return time >= this.start && time <= this.end;
        }


        public bool OverlapsRange(Util.TimeRange other)
        {
            return this.start < other.end && this.end > other.start;
        }


        public bool OverlapsRangeInclusive(Util.TimeRange other)
        {
            return this.start <= other.end && this.end >= other.start;
        }
    }
}
