using Composer.Util;

namespace Composer.Project
{
    public record FretboardNote {
        
        public int StringNo { get; set; }
        
        public int Fret { get; set; }

        public Util.TimeRange timeRange { get; set; }

        public (Note, int) ResolveNote(Tuning tuning) => (
            RelativePitchData.AddSemitones(tuning.TuningStrings[StringNo].Item1, Fret),
            (12 * tuning.TuningStrings[StringNo].Item2 + (int)tuning.TuningStrings[StringNo].Item1) / 12);
    }
}
