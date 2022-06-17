namespace Composer.Util
{
    public enum Note
    {
        C, Cs, D, Ds, E, F, Fs, G, Gs, A, As, B
    }


    public static class RelativePitchData
    {
        public static Note AddSemitones(this Note note, int semitones) => (Note)(((int)note + semitones + 1200) % 12);

        public static string GetSimpleName(this Note note)
        {
            switch (note)
            {
                case Note.C:  return "C";
                case Note.Cs: return "C#";
                case Note.D:  return "D";
                case Note.Ds: return "D#";
                case Note.E:  return "E";
                case Note.F:  return "F";
                case Note.Fs: return "F#";
                case Note.G:  return "G";
                case Note.Gs: return "G#";
                case Note.A:  return "A";
                case Note.As: return "A#";
                case Note.B:  return "B";
                default: return "?";
            }
        }
    }
}
