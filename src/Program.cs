﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Composer.Util;

namespace Composer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var project = new Project.Project(256 * 4);
            project.tracks.Add(new Project.FretboardNotesTrack("Guitar 1", Tuning.Standard, "UGK_amped.sf2/0"));
            project.tracks.Add(new Project.FretboardNotesTrack("Guitar 2", Tuning.Standard, "Billy_bass.sf2/0"));
            project.tracks.Add(new Project.FretboardNotesTrack("Bass", Tuning.BassStandard, "Billy_bass.sf2/1"));

            project.InsertMeterChange(new Project.MeterChange(0, new Meter(4, 4)));
            project.InsertMeterChange(new Project.MeterChange(256 * 3, new Meter(3, 4)));

            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 0, Fret = 5, timeRange = new TimeRange(256 / 4 * 0, 256 / 4) });
            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 1, Fret = 5, timeRange = new TimeRange(256 / 4 * 1, 256 / 4) });
            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 3, Fret = 7, timeRange = new TimeRange(256 / 4 * 2, 256 / 4) });
            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 5, Fret = 12, timeRange = new TimeRange(256 / 4 * 3, 256 / 4) });
            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 4, Fret = 9, timeRange = new TimeRange(256 / 4 * 4, 256 / 4) });

            project.InsertPitchedNote(1, new Project.FretboardNote { StringNo = 0, Fret = 3, timeRange = new TimeRange(256 / 4 * 0, 256 / 4) });
            project.InsertPitchedNote(1, new Project.FretboardNote { StringNo = 0, Fret = 3, timeRange = new TimeRange(256 / 4 * 1, 256 / 4) });

            project.InsertPitchedNote(2, new Project.FretboardNote { StringNo = 0, Fret = 3, timeRange = new TimeRange(256 / 4 * 0, 2 * 256 / 4) });
            project.InsertPitchedNote(2, new Project.FretboardNote { StringNo = 0, Fret = 5, timeRange = new TimeRange(256 / 4 * 2, 2 * 256 / 4) });
            project.InsertPitchedNote(2, new Project.FretboardNote { StringNo = 0, Fret = 3, timeRange = new TimeRange(256 / 4 * 4, 2 * 256 / 4) });
            project.InsertPitchedNote(2, new Project.FretboardNote { StringNo = 0, Fret = 7, timeRange = new TimeRange(256 / 4 * 7, 4 * 256 / 4) });

            Application.Run(new FormMain(project));
        }
    }
}
