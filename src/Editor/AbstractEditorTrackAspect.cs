﻿using System.Drawing;


namespace Composer.Editor
{
    abstract class AbstractEditorTrackAspect
    {
        public EditorViewManager manager;
        public EditorTrack row;
        public Util.Rect layoutRect;
        public Util.Rect contentRect;


        public AbstractEditorTrackAspect(EditorViewManager manager, EditorTrack row)
        {
            this.manager = manager;
            this.row = row;
        }


        public abstract void RefreshLayout(float x, float y);

        public abstract float GetTimeAtPosition(float x);

        public abstract int GetStringAtPosition(float y);

        public abstract void Draw(Graphics g);
    }
}
