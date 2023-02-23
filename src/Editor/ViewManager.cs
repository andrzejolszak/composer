using Composer.AudioOut;
using Composer.Project;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MeltySynth;
using System.IO;
using System.Linq;

namespace Composer.Editor
{
    class ViewManager
    {
        Dictionary<string, Synthesizer> _kits = new Dictionary<string, Synthesizer>();

        ControlEditor control;
        public Project.Project project;
        public List<Row> rows;
        public List<Element> elements;

        float width, height;
        float scrollX, scrollY;
        Util.Rect layoutRect;

        enum MouseAction
        {
            Selection, Cursor, Scrolling
        }

        bool mouseIsDown;
        MouseAction mouseAction;
        float mouseCurrentX, mouseCurrentY;
        float mouseDragOriginX, mouseDragOriginY;
        float timeDragOrigin;
        int trackDragOrigin;
        int pitchDragOrigin;

        public bool cursorVisible;
        private ElementCaret _elementCaret;
        public float cursorTime1, cursorTime2;
        public int cursorTrack1, cursorTrack2;
        public bool noteInsertionMode;
        public Element currentHoverElement;
        public InteractableRegion currentHoverRegion;
        public InteractableRegion currentDraggingIsolatedRegion;
        internal WaveOut _audioOut;
        private FretboardNote _lastNote;

        public ViewManager(ControlEditor control, Project.Project project)
        {
            this.control = control;
            this.project = project;
            this.rows = new List<Row>();
            this.elements = new List<Element>();

            foreach (string s in Directory.GetFiles("AudioOut").Where(x => x.EndsWith("sf2")))
            {
                string name = s.Split(Path.DirectorySeparatorChar).Last();
                for (int i = 0; i < 16; i++)
                {
                    this._kits[name + "/" + i] = new Synthesizer(new SoundFont(s), new SynthesizerSettings(44100) { MaximumPolyphony = 128 }, defaultChannel: i);
                }
            }

            cursorVisible = true;

            this._elementCaret = new ElementCaret(this);
        }


        public void SetSize(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public void SetCursorTimeRange(float time1, float time2)
        {
            this.cursorTime1 =
                System.Math.Max(0,
                System.Math.Min(this.project.Length, time1));

            this.cursorTime2 =
                System.Math.Max(0,
                System.Math.Min(this.project.Length, time2));
        }


        public void SetCursorTrackIndices(int track1, int track2)
        {
            this.cursorTrack1 = track1;
            this.cursorTrack2 = track2;
        }


        public bool GetInsertionPosition(out int trackIndex, out float time)
        {
            trackIndex = -1;
            time = 0;

            if (!this.cursorVisible)
                return false;

            if (this.cursorTrack1 != this.cursorTrack2)
                return false;

            var trackSegment = this.rows[0].trackSegments[this.cursorTrack1];
            var trackSegmentPitchedNotes = trackSegment as TrackSegmentFretboardNotes;
            if (trackSegmentPitchedNotes != null)
            {
                trackIndex = this.project.GetTrackIndex(trackSegmentPitchedNotes.projectTracks[0]);
                time = this.cursorTime1;
                return true;
            }

            return false;
        }


        public void SetNoteInsertionMode(bool enabled)
        {
            this.noteInsertionMode = enabled;
        }

        public void Rebuild()
        {
            this.rows.Clear();
            this.elements.Clear();

            this.elements.Add(this._elementCaret);

            var currentTime = 0f;
            var currentSegment = 0;
            while (currentSegment <= this.project.sectionBreaks.Count)
            {
                var endTime = this.project.Length;
                var isLastRow = true;
                if (currentSegment < this.project.sectionBreaks.Count)
                {
                    endTime = this.project.sectionBreaks[currentSegment].time;
                    isLastRow = false;
                }

                foreach (var meterChange in this.project.meterChanges)
                    this.elements.Add(new ElementMeterChange(this, meterChange));

                foreach (TrackFretboardNotes track in this.project.tracks)
                {
                    var row = new Row(this, new Util.TimeRange(currentTime, endTime - currentTime), isLastRow, track.Tuning);

                    if (!track.visible)
                        continue;

                    var seg = new TrackSegmentFretboardNotes(
                        this, row,
                        new List<TrackFretboardNotes> { track });
                    row.trackSegments.Add(seg);

                    this.rows.Add(row);

                    foreach (var note in track.notes)
                    {
                        var element = new ElementFretboardNote(this, track, seg, note);
                        this.elements.Add(element);
                    }
                }

                currentTime = endTime;
                currentSegment++;
            }

            this.SetCursorTimeRange(this.cursorTime1, this.cursorTime2);
            this.SetCursorTrackIndices(this.cursorTrack1, this.cursorTrack2);

            this.Refresh();
            this.ScrollTo(this.scrollX, this.scrollY);
        }


        public void Refresh()
        {
            this.layoutRect = new Util.Rect(0, 0, 0, 0);

            var y = TopMargin;
            foreach (var row in this.rows)
            {
                row.RefreshLayout(LeftMargin, y);
                y = row.layoutRect.yMax;

                this.layoutRect = this.layoutRect.Include(row.layoutRect);
            }

            foreach (var element in this.elements)
                element.RefreshLayout();

            this.control.Invalidate();
        }


        public void ScrollTo(float x, float y)
        {
            this.scrollX =
                System.Math.Max(0,
                System.Math.Min(this.layoutRect.xMax - 100, x));

            this.scrollY =
                System.Math.Max(0,
                System.Math.Min(this.layoutRect.yMax - this.height + 30, y));
        }

        public bool OnKey(Keys keyData, bool isKeyDown)
        {
            this.ModifySelected((elem) => elem.OnPressKeyPreview(keyData));

            if (!isKeyDown)
            {
                return false;
            }

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
                if (this.GetInsertionPosition(out trackIndex, out time))
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

                    this.InsertPitchedNote(trackIndex, time, 256 / 4, ((int)relativePitch) % 6, 5);
                }

                return true;
            }
            else if (keyData == Keys.Up)
            {
                this.OnPressUp(ctrlKey, shiftKey);
                return true;
            }
            else if (keyData == Keys.Down)
            {
                this.OnPressDown(ctrlKey, shiftKey);
                return true;
            }
            else if (keyData == Keys.Right)
            {
                this.OnPressRight(ctrlKey, shiftKey);
                return true;
            }
            else if (keyData == Keys.Left)
            {
                this.OnPressLeft(ctrlKey, shiftKey);
                return true;
            }
            else if (keyData == Keys.F5)
            {
                this.Rebuild();
                return true;
            }
            else if (keyData == Keys.Space)
            {
                this.ExecuteAudioJob();
            }

            return false;
        }

        private void InsertPitchedNote(int trackIndex, float time, float duration, int stringNo, int fret)
        {
            var note = new Project.FretboardNote
            {
                StringNo = stringNo,
                Fret = fret,
                timeRange = new Util.TimeRange(time, duration)
            };

            this.project.InsertPitchedNote(trackIndex, note);

            this.Rebuild();
            // this.SetPitchedNoteSelection(trackIndex, note, true);
            this.SetNoteInsertionMode(true);
            this.SetCursorTimeRange(time + duration, time + duration);
        }

        public void ExecuteAudioJob()
        {
            lock (this)
            {
                if (this._audioOut is not null)
                {
                    if (this._audioOut.PlaybackState == PlaybackState.Playing)
                    {
                        this._audioOut.Pause();
                    }
                    else
                    {
                        this._audioOut.Resume();
                    }

                    return;
                }

                this._audioOut = new WaveOut();
                this._audioOut.Volume = 1;

                List<NotePatternSampleProvider> trackSequencers = new List<NotePatternSampleProvider>();

                foreach (TrackFretboardNotes track in this.project.tracks)
                {
                    NotePattern pattern = new NotePattern();
                    foreach (FretboardNote note in track.notes)
                    {
                        pattern.Add((int)(note.timeRange.Start / (256 / 4)), note);

                        if (this._lastNote is null || this._lastNote.timeRange.End < note.timeRange.End)
                        {
                            this._lastNote = note;
                        }
                    }

                    NotePatternSampleProvider patternSequencer = new NotePatternSampleProvider(this._kits[track.KitName], pattern, false, track.Tuning);
                    patternSequencer.Tempo = 90;

                    patternSequencer.Sequencer.NotePlayingStateChanged += Sequencer_NotePlayingStateChanged;

                    trackSequencers.Add(patternSequencer);
                }

                this._lastNote = new FretboardNote() { Fret = 0, StringNo = 0, timeRange = new Util.TimeRange(this._lastNote.timeRange.End, 0) };
                trackSequencers.Last().Sequencer.Pattern.Add((int)(this._lastNote.timeRange.Start / (256 / 4)), this._lastNote);

                this._audioOut.Init(new MixingSampleProvider(trackSequencers));
                this._audioOut.PlaybackStopped += (s, e) =>
                {
                    lock (this)
                    {
                        this._audioOut.Volume = 0;
                        this._audioOut.Dispose();
                        this._audioOut = null;
                        this.control.Invalidate();
                    }
                };

                this._audioOut.Play();

                this.control.Invalidate();
            }
        }

        private void Sequencer_NotePlayingStateChanged(FretboardNote note, bool isPlaying)
        {
            lock (this)
            {
                if (note == this._lastNote)
                {
                    this._audioOut?.Stop();

                    this.SetCursorTimeRange(0, 0);

                    this.Refresh();
                    return;
                }

                if (!isPlaying)
                {
                    return;
                }

                this.SetCursorTimeRange(note.timeRange.Start + note.timeRange.Duration / 2, note.timeRange.Start + note.timeRange.Duration / 2);

                this.Refresh();
            }
        }

        public void OnPressUp(bool ctrlKey, bool shiftKey)
        {
            if (this.ModifySelected((elem) => elem.OnPressUp(ctrlKey, shiftKey)))
                this.Rebuild();
        }


        public void OnPressDown(bool ctrlKey, bool shiftKey)
        {
            if (this.ModifySelected((elem) => elem.OnPressDown(ctrlKey, shiftKey)))
                this.Rebuild();
        }


        public void OnPressRight(bool ctrlKey, bool shiftKey)
        {
            if (this.ModifySelected((elem) => elem.OnPressRight(ctrlKey, shiftKey)))
                this.Rebuild();
        }


        public void OnPressLeft(bool ctrlKey, bool shiftKey)
        {
            if (this.ModifySelected((elem) => elem.OnPressLeft(ctrlKey, shiftKey)))
                this.Rebuild();
        }


        public bool ModifySelected(System.Action<Element> func)
        {
            this._elementCaret.BeginModify();
            func(this._elementCaret);
            bool wasModified = this._elementCaret.EndModify();
            return wasModified;
        }


        public void OnMouseMove(float x, float y)
        {
            this.mouseCurrentX = x + scrollX;
            this.mouseCurrentY = y + scrollY;

            if (this.mouseIsDown)
            {
                if (this.mouseAction == MouseAction.Selection)
                {
                    if (this.currentHoverRegion != null && this.currentHoverRegion.isolated)
                        this.currentHoverRegion.dragFunc?.Invoke(this.currentHoverRegion);
                    else
                    {
                        foreach (var element in this.elements)
                        {
                            if (element.Highlighted)
                            {
                                element.Drag();
                                element.RefreshLayout();
                            }
                        }
                    }
                }
                else if (this.mouseAction == MouseAction.Scrolling)
                {
                    var deltaX = this.mouseDragOriginX - this.mouseCurrentX;
                    var deltaY = this.mouseDragOriginY - this.mouseCurrentY;

                    this.ScrollTo(this.scrollX + deltaX, this.scrollY + deltaY);

                    this.mouseCurrentX = this.mouseDragOriginX = x + scrollX;
                    this.mouseCurrentY = this.mouseDragOriginY = y + scrollY;
                }
                else if (this.mouseAction == MouseAction.Cursor)
                {
                    var track = this.GetTrackIndexAtPosition(this.mouseCurrentX, this.mouseCurrentY);

                    var time = this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, true);
                    var timeSnapped = (float)(System.Math.Round(time / TimeSnap) * TimeSnap);

                    this.SetCursorTimeRange(this.cursorTime1, timeSnapped);
                    this.SetCursorTrackIndices(this.cursorTrack1, track);
                }
            }
            else
            {
                var lastHoverRegion = this.currentHoverRegion;

                this.currentHoverElement = null;
                this.currentHoverRegion = null;

                foreach (var element in this.elements)
                {
                    foreach (var region in element.interactableRegions)
                    {
                        if (region.rect.Contains(x + scrollX, y + scrollY))
                        {
                            this.currentHoverElement = element;
                            this.currentHoverRegion = region;
                        }
                    }
                }

                foreach (var row in this.rows)
                {
                    foreach (var region in row.interactableRegions)
                    {
                        if (region.rect.Contains(x + scrollX, y + scrollY))
                        {
                            this.currentHoverElement = null;
                            this.currentHoverRegion = region;
                        }
                    }
                }

                if (this.currentHoverRegion != null)
                {
                    switch (this.currentHoverRegion.cursorKind)
                    {
                        case InteractableRegion.CursorKind.Select:
                            this.control.Cursor = System.Windows.Forms.Cursors.Hand; break;
                        case InteractableRegion.CursorKind.MoveAll:
                            this.control.Cursor = System.Windows.Forms.Cursors.UpArrow; break;
                        case InteractableRegion.CursorKind.MoveHorizontal:
                            this.control.Cursor = System.Windows.Forms.Cursors.SizeWE; break;
                        default:
                            this.control.Cursor = System.Windows.Forms.Cursors.Default; break;
                    }
                }
                else
                    this.control.Cursor = System.Windows.Forms.Cursors.Default;
            }
        }


        public void OnMouseDown(bool scroll, float x, float y, bool ctrlKey, bool shiftKey)
        {
            this.mouseIsDown = true;
            this.mouseDragOriginX = x + scrollX;
            this.mouseDragOriginY = y + scrollY;
            this.noteInsertionMode = false;

            if (scroll)
                this.mouseAction = MouseAction.Scrolling;
            else
            {
                this.mouseAction = MouseAction.Selection;
                this.cursorVisible = false;

                if (this.currentHoverElement != null)
                    this.currentHoverElement.SetHighlighted(true);

                this.timeDragOrigin =
                    this.GetTimeAtPosition(this.mouseDragOriginX, this.mouseDragOriginY, true);

                this.trackDragOrigin =
                    this.GetTrackIndexAtPosition(this.mouseDragOriginX, this.mouseDragOriginY);

                var timeSnapped = (float)(System.Math.Round(this.timeDragOrigin / TimeSnap) * TimeSnap);

                this.pitchDragOrigin =
                    this.GetTrackSegmentAtPosition(this.mouseDragOriginX, this.mouseDragOriginY).
                    GetStringAtPosition(this.mouseDragOriginY);
                
                if (this.currentHoverRegion != null && this.currentHoverRegion.isolated)
                {
                    this.currentDraggingIsolatedRegion = this.currentHoverRegion;
                    this.currentDraggingIsolatedRegion.dragStartFunc?.Invoke(this.currentDraggingIsolatedRegion);
                }
                else if (!ctrlKey && !shiftKey && this.currentHoverRegion == null)
                {
                    this.mouseAction = MouseAction.Cursor;
                    this.SetCursorTimeRange(timeSnapped, timeSnapped);
                    this.SetCursorTrackIndices(this.trackDragOrigin, this.trackDragOrigin);
                    this.cursorVisible = true;
                }
            }
        }


        public void OnMouseUp(float x, float y)
        {
            this.mouseIsDown = false;

            if (this.mouseAction == MouseAction.Selection)
            {
                if (this.currentDraggingIsolatedRegion != null)
                {
                    this.currentDraggingIsolatedRegion.dragEndFunc?.Invoke(this.currentDraggingIsolatedRegion);

                    if (this.currentHoverRegion == this.currentDraggingIsolatedRegion)
                        this.currentDraggingIsolatedRegion.clickFunc?.Invoke(this.currentDraggingIsolatedRegion);

                    this.currentDraggingIsolatedRegion = null;
                }
                else
                {
                    this.ModifySelected((elem) => { });
                }

                this.Rebuild();
            }
        }


        public void Draw(Graphics g)
        {
            g.TranslateTransform(-scrollX, -scrollY);

            foreach (var row in this.rows)
                row.Draw(g);

            foreach (var element in this.elements)
                element.Draw(g, currentHoverElement == element);

            foreach (var row in this.rows)
                row.DrawOverlay(g);
        }


        public Row GetRowOverlapping(float time)
        {
            foreach (var row in this.rows)
            {
                if (row.timeRange.Overlaps(time))
                    return row;
            }

            return null;
        }


        public IEnumerable<Row> EnumerateRowsInTimeRange(Util.TimeRange timeRange)
        {
            foreach (var row in this.rows)
            {
                if (row.timeRange.OverlapsRange(timeRange))
                    yield return row;
            }
        }


        public TrackSegment GetTrackSegmentAtPosition(float x, float y)
        {
            foreach (var row in this.rows)
            {
                foreach (var track in row.trackSegments)
                {
                    if (track.layoutRect.ContainsY(y))
                        return track;
                }
            }

            if (y <= this.TopMargin)
                return this.rows[0].trackSegments[0];

            var lastRow = this.rows[this.rows.Count - 1];
            return lastRow.trackSegments[lastRow.trackSegments.Count - 1];
        }


        public int GetTrackIndexAtPosition(float x, float y)
        {
            foreach (var row in this.rows)
            {
                for (var i = 0; i < row.trackSegments.Count; i++)
                {
                    if (row.trackSegments[i].layoutRect.ContainsY(y))
                        return i;
                }
            }

            if (y <= this.TopMargin)
                return 0;

            var lastRow = this.rows[this.rows.Count - 1];
            return lastRow.trackSegments.Count - 1;
        }


        public float GetTimeAtPosition(float x, float y, bool clampToRowRange)
        {
            var time = 0f;
            var timeClampMin = 0f;
            var timeClampMax = 0f;

            if (this.rows.Count > 0)
            {
                if (y <= this.rows[0].layoutRect.yMin)
                {
                    time = this.rows[0].GetTimeAtPosition(x);
                    timeClampMin = this.rows[0].timeRange.Start;
                    timeClampMax = this.rows[0].timeRange.End;
                }
                else
                {
                    var lastRow = this.rows[this.rows.Count - 1];
                    time = lastRow.GetTimeAtPosition(x);
                    timeClampMin = lastRow.timeRange.Start;
                    timeClampMax = lastRow.timeRange.End;
                }
            }

            foreach (var row in this.rows)
            {
                if (row.layoutRect.ContainsY(y))
                {
                    time = row.GetTimeAtPosition(x);
                    timeClampMin = row.timeRange.Start;
                    timeClampMax = row.timeRange.End;
                    break;
                }
            }

            var timeClamped = System.Math.Max(timeClampMin, System.Math.Min(timeClampMax, time));
            return (clampToRowRange ? timeClamped : time);
        }


        public float LeftMargin
        {
            get { return 20; }
        }


        public float RightMargin
        {
            get { return 20; }
        }


        public float TopMargin
        {
            get { return 20; }
        }


        public float PitchedNoteHeight
        {
            get { return 8; }
        }


        public float TimeToPixelsMultiplier
        {
            get { return 100f / this.project.BarDuration; }
        }


        public float TimeSnap
        {
            get { return this.project.BarDuration / 4; }
        }


        public Util.TimeRange CursorTimeRange
        {
            get
            {
                return new Util.TimeRange(
                    System.Math.Min(this.cursorTime1, this.cursorTime2),
                    System.Math.Max(this.cursorTime1, this.cursorTime2) - System.Math.Min(this.cursorTime1, this.cursorTime2));
            }
        }


        public int CursorFirstTrackIndex
        {
            get
            {
                return System.Math.Min(this.cursorTrack1, this.cursorTrack2);
            }
        }


        public int CursorLastTrackIndex
        {
            get
            {
                return System.Math.Max(this.cursorTrack1, this.cursorTrack2);
            }
        }


        public float MouseTime
        {
            get
            {
                var time = this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, false);
                return (float)(System.Math.Round(time / TimeSnap) * TimeSnap);
            }
        }


        public float MouseTimeClampedToRow
        {
            get
            {
                var time = this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, true);
                return (float)(System.Math.Round(time / TimeSnap) * TimeSnap);
            }
        }


        public float DragTimeOffsetIrrespectiveOfRow
        {
            get
            {
                var offset =
                    (this.mouseCurrentX - this.mouseDragOriginX) / this.TimeToPixelsMultiplier;

                return (float)(System.Math.Round(offset / TimeSnap) * TimeSnap);
            }
        }


        public float DragTimeOffset
        {
            get
            {
                var offset =
                    this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, false) -
                    this.timeDragOrigin;

                return (float)(System.Math.Round(offset / TimeSnap) * TimeSnap);
            }
        }


        public float DragTimeOffsetClampedToRow
        {
            get
            {
                var offset =
                    this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, true) -
                    this.timeDragOrigin;

                return (float)(System.Math.Round(offset / TimeSnap) * TimeSnap);
            }
        }


        public float DragMidiPitchOffset
        {
            get
            {
                var pitchAtMouse =
                    this.GetTrackSegmentAtPosition(this.mouseCurrentX, this.mouseCurrentY)
                    .GetStringAtPosition(this.mouseCurrentY);

                var offset = - pitchAtMouse + this.pitchDragOrigin;
                return (float)offset;
            }
        }
    }
}
