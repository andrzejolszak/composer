using System.Windows.Forms;
using System.Collections.Generic;
using System;
using NAudio.Wave;
using System.Threading;
using System.Linq;
using Composer.Project;
using Composer.AudioOut;

namespace Composer
{
    class FormMain : Form
    {
        public Project.Project currentProject;
        public Editor.ControlEditor editorControl;
        public Editor.ViewManager editor;

        public FormMain(Project.Project project)
        {
            this.currentProject = project;

            SuspendLayout();

            var menuStrip = new MenuStrip();
            menuStrip.Items.Add("File");
            menuStrip.Items.Add("Edit");
            menuStrip.Items.Add("View");

            var toolStrip = new ToolStrip();
            toolStrip.Items.Add("Insert Key Change");
            toolStrip.Items.Add("Insert Meter Change");

            var split = new SplitContainer();
            split.Dock = DockStyle.Fill;

            var trackManager = new ToolWindows.TrackManagerWindow(this);
            trackManager.Dock = DockStyle.Fill;
            split.Panel1.Controls.Add(trackManager);

            this.editorControl = new Editor.ControlEditor(this);
            this.editorControl.Dock = DockStyle.Fill;
            split.Panel2.Controls.Add(this.editorControl);
            this.editor = this.editorControl.viewManager;

            trackManager.RefreshTracks();

            this.Controls.Add(split);
            this.Controls.Add(toolStrip);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            this.Width = 900;
            this.Height = 600;

            ResumeLayout(false);
            PerformLayout();

            split.SplitterDistance = 200;
            editor.Rebuild();
            Refresh();

        }


        protected override void OnClosed(EventArgs e)
        {
        }


        public void ExecuteAudioJob()
        {
            WaveOut audioOut = new WaveOut();
            NotePattern pattern = new NotePattern();
            int c = 0;
            foreach (FretboardNote note in (this.currentProject.tracks.First() as TrackFretboardNotes).notes)
            {
                pattern.Add(c, note);
                c++;
            }

            NotePatternSampleProvider patternSequencer = new NotePatternSampleProvider(pattern, false, this.currentProject.Tuning);
            patternSequencer.Tempo = 90;

            audioOut.Init(patternSequencer);
            audioOut.PlaybackStopped += (s, e) => audioOut.Dispose();
            audioOut.Play();
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var ctrlKey = (keyData & Keys.Control) != 0;
            var shiftKey = (keyData & Keys.Shift) != 0;

            keyData = (keyData & ~(Keys.Control | Keys.Shift));

            if (keyData == Keys.Z ||
                keyData == Keys.X || keyData == Keys.C ||
                keyData == Keys.V || keyData == Keys.B || keyData == Keys.N ||
                keyData == Keys.M ||
                keyData == Keys.S || keyData == Keys.D ||
                keyData == Keys.G || keyData == Keys.H || keyData == Keys.J)
            {
                int trackIndex;
                float time;
                if (this.editor.GetInsertionPosition(out trackIndex, out time))
                {
                    var relativePitch = Util.Note.C;
                    if (keyData == Keys.Z) relativePitch = Util.Note.C;
                    if (keyData == Keys.S) relativePitch = Util.Note.Cs;
                    if (keyData == Keys.X) relativePitch = Util.Note.D;
                    if (keyData == Keys.D) relativePitch = Util.Note.Ds;
                    if (keyData == Keys.C) relativePitch = Util.Note.E;
                    if (keyData == Keys.V) relativePitch = Util.Note.F;
                    if (keyData == Keys.G) relativePitch = Util.Note.Fs;
                    if (keyData == Keys.B) relativePitch = Util.Note.G;
                    if (keyData == Keys.H) relativePitch = Util.Note.Gs;
                    if (keyData == Keys.N) relativePitch = Util.Note.A;
                    if (keyData == Keys.J) relativePitch = Util.Note.As;
                    if (keyData == Keys.M) relativePitch = Util.Note.B;

                    this.InsertPitchedNote(trackIndex, time, 960 / 4, ((int)relativePitch) % 6, 5);
                }

                return true;
            }

            else if (keyData == Keys.Up)
            {
                this.editor.OnPressUp(ctrlKey, shiftKey);
                this.editorControl.Refresh();
                return true;
            }
            else if (keyData == Keys.Down)
            {
                this.editor.OnPressDown(ctrlKey, shiftKey);
                this.editorControl.Refresh();
                return true;
            }
            else if (keyData == Keys.Right)
            {
                this.editor.OnPressRight(ctrlKey, shiftKey);
                var currentNote = this.editor.GetNoteInsertionModeNote();
                if (currentNote != null)
                    this.editor.SetCursorTimeRange(currentNote.timeRange.End, currentNote.timeRange.End);

                this.editorControl.Refresh();
                return true;
            }
            else if (keyData == Keys.Left)
            {
                this.editor.OnPressLeft(ctrlKey, shiftKey);
                var currentNote = this.editor.GetNoteInsertionModeNote();
                if (currentNote != null)
                    this.editor.SetCursorTimeRange(currentNote.timeRange.End, currentNote.timeRange.End);
                
                this.editorControl.Refresh();
                return true;
            }
            else if (keyData == Keys.Return)
            {
                this.editor.UnselectAll();
                this.editorControl.Refresh();
                return true;
            }
            else if (keyData == Keys.F5)
            {
                this.editor.Rebuild();
                this.editorControl.Refresh();
                return true;
            }
            else if (keyData == Keys.Space)
            {
                this.ExecuteAudioJob();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void InsertPitchedNote(int trackIndex, float time, float duration, int stringNo, int fret)
        {
            this.editor.UnselectAll();

            var note = new Project.FretboardNote
            {
                StringNo = stringNo,
                Fret = fret,
                timeRange = new Util.TimeRange(time, time + duration)
            };

            this.currentProject.InsertPitchedNote(trackIndex, note);

            this.editor.Rebuild();
            this.editor.SetPitchedNoteSelection(trackIndex, note, true);
            this.editor.SetNoteInsertionMode(true);
            this.editor.SetCursorTimeRange(time + duration, time + duration);
            this.editor.SetCursorVisible(true);

            this.editorControl.Refresh();
        }
    }
}
