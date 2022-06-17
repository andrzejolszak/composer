using System;
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

            var project = new Project.Project(960 * 4);
            project.tracks.Add(new Project.TrackFretboardNotes("Track 1"));

            project.InsertMeterChange(new Project.MeterChange(0, new Util.Meter(4, 4)));
            project.InsertMeterChange(new Project.MeterChange(960 * 2, new Util.Meter(3, 4)));

            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 0, Fret = 5, timeRange = new Util.TimeRange(960 / 4 * 0, 960 / 4 * 1) });
            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 1, Fret = 5, timeRange = new Util.TimeRange(960 / 4 * 1, 960 / 4 * 2) });
            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 3, Fret = 7, timeRange = new Util.TimeRange(960 / 4 * 2, 960 / 4 * 3) });
            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 5, Fret = 12, timeRange = new Util.TimeRange(960 / 4 * 3, 960 / 4 * 4) });
            project.InsertPitchedNote(0, new Project.FretboardNote { StringNo = 4, Fret = 9, timeRange = new Util.TimeRange(960 / 4 * 4, 960 / 4 * 5) });

            Application.Run(new FormMain(project));
        }
    }
}
