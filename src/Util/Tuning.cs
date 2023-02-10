using System.Collections.Generic;

namespace Composer.Util
{
    public class Tuning
    {
        public string Name { get; }
        public List<(Note, int)> TuningStrings { get; }

        public Note tonicPitch;
        public Scale scale;


        public Tuning(string name, List<(Note, int)> tuning)
        {
            this.Name = name;
            this.TuningStrings = tuning;
        }

        public static Tuning Standard => new Tuning(
            "Standard",
            new List<(Note, int)>
            {
                (Note.E, 2),
                (Note.A, 2),
                (Note.D, 3),
                (Note.G, 3),
                (Note.B, 3),
                (Note.E, 4),
            });

        public static Tuning BassStandard => new Tuning(
            "BassStandard",
            new List<(Note, int)>
            {
                (Note.E, 1),
                (Note.A, 1),
                (Note.D, 2),
                (Note.G, 2)
            });
    }
}
