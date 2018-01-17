using POESKillTree.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.TreeGenerator.Settings;
using System.Threading.Tasks;
using System.IO;

namespace POESKillTree.Model
{
    /// <summary>
    /// Data class for settings for TrackedStatsMenuModel
    /// </summary>
    /// <seealso cref="POESKillTree.Utils.Notifier" />
    public class TrackedStatOptions : Notifier
    {
        private static string DefaultTrackingDir = AppData.ProgramDirectory+ "\\StatTracking\\";
        private string _StatTrackingSavePath = DefaultTrackingDir;

        /// <summary>
        /// Gets or sets the stat tracking save path.
        /// </summary>
        /// <value>
        /// The stat tracking save path.
        /// </value>
        public string StatTrackingSavePath
        {
            get { return _StatTrackingSavePath; }
            set
            {
                SetProperty(ref _StatTrackingSavePath, value, async () =>
                {
                    if (value == null||value=="")
                    {
                        SetProperty(ref _StatTrackingSavePath, DefaultTrackingDir);
                    }
                    await LoadTrackedStatFileNames();
                });
            }
        }
        /// <summary>
        /// Loads the tracked stat file names.
        /// </summary>
        /// <returns></returns>
        public async Task LoadTrackedStatFileNames()
        {
            //string[] FilesInPath = System.IO.Directory.GetFiles(Options.StatTrackingSavePath);
             //SLightly async version of Getting files from http://writeasync.net/?p=2621
             // Avoid blocking the caller for the initial enumerate call.
                await Task.Yield();
                List<string> FileList = new List<string>();

                foreach (string file in Directory.EnumerateFiles(StatTrackingSavePath))
                {
                    FileList.Add(file);
                }
                GlobalSettings.TrackedStatFileNames = FileList.ToArray();
        }
    }
}
