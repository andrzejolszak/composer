using Composer.Project;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Composer.AudioOut
{
    public class NotePattern
    {       
        public NotePattern()
        {
            this.Pattern = new Dictionary<int, HashSet<FretboardNote>>();
        }

        public int Steps { get; private set; }
        
        public Dictionary<int, HashSet<FretboardNote>> Pattern { get; }

        public void Add(int step, FretboardNote note)
        {
            if (!this.Pattern.TryGetValue(step, out HashSet<FretboardNote> notes))
            {
                notes = new HashSet<FretboardNote>();
                this.Pattern.Add(step, notes);
            }

            if (notes.Add(note))
            {
                this.Steps = this.Pattern.Keys.Max() + 1;
                PatternChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Remove(int step, FretboardNote note)
        {
            if (!this.Pattern.TryGetValue(step, out HashSet<FretboardNote> notes))
            {
                return;
            }

            if (notes.Remove(note))
            {
                this.Steps = this.Pattern.Keys.Max() + 1;
                PatternChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler PatternChanged;
    }
}
