using static Composer.Util.Note;


namespace Composer.Util
{
    public enum Scale
    {
        Major,
        Dorian,
        Phrygian,
        Lydian,
        Mixolydian,
        NaturalMinor,
        Locrian,
        MelodicMinor
    }


    public static class ScaleData
    {
        private static readonly string[] names = new string[]
        {
            "Major",
            "Dorian",
            "Phrygian",
            "Lydian",
            "Mixolydian",
            "Natural Minor",
            "Locrian",
            "Melodic Minor"
        };


        private static readonly Note[][] relativePitches = new Note[][]
        {
            new Note[] { C,  D,  E,  F,  G,  A,  B  }, /* Major */ 
            new Note[] { C,  D,  Ds, F,  G,  A,  As }, /* Dorian */ 
            new Note[] { C,  Cs, Ds, F,  G,  Gs, As }, /* Phrygian */ 
            new Note[] { C,  D,  E,  Fs, G,  A,  B  }, /* Lydian */ 
            new Note[] { C,  D,  E,  F,  G,  A,  As }, /* Mixolydian */ 
            new Note[] { C,  D,  Ds, F,  G,  Gs, As }, /* NaturalMinor */ 
            new Note[] { C,  Cs, Ds, F,  G,  Gs, As }, /* Locrian */ 
            new Note[] { C,  D,  Ds, F,  G,  Gs, B  }, /* MelodicMinor */ 
        };


        private static int[][] relativePitchesToDegree;


        static ScaleData()
        {
            relativePitchesToDegree = new int[relativePitches.Length][];
            for (var i = 0; i < relativePitchesToDegree.Length; i++)
            {
                relativePitchesToDegree[i] = new int[12]
                    { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

                for (var j = 0; j < relativePitches[i].Length; j++)
                    relativePitchesToDegree[i][(int)relativePitches[i][j]] = j;
            }
        }


        public static Note[] GetRelativePitches(this Scale scale)
        {
            return relativePitches[(int)scale];
        }


        public static string GetName(this Scale scale)
        {
            return names[(int)scale];
        }


        public static bool HasRelativePitch(this Scale scale, Note relativePitch)
        {
            return relativePitchesToDegree[(int)scale][(int)relativePitch] >= 0;
        }
    }
}
