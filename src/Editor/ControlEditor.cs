using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace Composer.Editor
{
    class ControlEditor : Control
    {
        FormMain ownerFormMain;
        public EditorViewManager viewManager;


        public ControlEditor(FormMain owner, Project.Project project)
        {
            this.ownerFormMain = owner;
            this.viewManager = new EditorViewManager(this, project);

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.viewManager.SetSize(this.Width, this.Height);
            this.viewManager.Rebuild();
        }


        public void Rebuild()
        {
            this.viewManager.Rebuild();
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillRectangle(System.Drawing.Brushes.White, 0, 0, Width, Height);
            this.viewManager.Draw(e.Graphics);
            base.OnPaint(e);
        }
    }
}
