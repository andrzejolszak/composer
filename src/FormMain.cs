using System.Windows.Forms;
using System.Collections.Generic;
using System;
using NAudio.Wave;
using System.Threading;
using System.Linq;
using Composer.Project;
using Composer.AudioOut;
using Microsoft.VisualBasic;
using NAudio.Wave.SampleProviders;
using System.Diagnostics;
using System.IO;
using MeltySynth;

namespace Composer
{
    class FormMain : Form
    {
        public Project.Project currentProject;
        public Editor.ControlEditor editorControl;
        public Editor.EditorViewManager editor;

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

            this.editorControl = new Editor.ControlEditor(this, project);
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

            this.KeyDown += FormMain_KeyDown;
            this.KeyUp += FormMain_KeyUp;
            this.KeyPreview = true;
        }

        private void FormMain_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.editor.OnKey(e.KeyData, false))
            {
                this.Invalidate();
                e.Handled = true;
            }
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.editor.OnKey(e.KeyData, true))
            {
                this.Invalidate();
                e.Handled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down ||
                keyData == Keys.Left || keyData == Keys.Right)
            {
                object sender = Control.FromHandle(msg.HWnd);
                KeyEventArgs e = new KeyEventArgs(keyData);
                FormMain_KeyDown(sender, e);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
