using POESKillTree.TrackedStatViews;
using POESKillTree.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POESKillTree
{
    public static class GlobalSettings
    {
        /// <summary>
        /// The tracked stats
        /// </summary>
        public static SkillTreeFiles.TrackedAttributes TrackedStats = new SkillTreeFiles.TrackedAttributes();

        /// <summary>
        /// The tracking list (Shared between TrackedStatsMenu and TrackedStatsMenuModel etc)
        /// </summary>
        public static ObservableCollection<StringData> TrackingList = new ObservableCollection<StringData>();

        public static string FallbackValue = "CurrentTrackedAttributes.txt";

        private static string _CurrentTrackedFile = FallbackValue;

        public static string CurrentTrackedFile
        {
            get { return _CurrentTrackedFile; }
            set
            {
                if (value != "" && value != null && value != CurrentTrackedFile)
                {
                    _CurrentTrackedFile = value;
                }
            }
        }

        /// <summary>
        /// The default tracking directory
        /// </summary>
        public static string DefaultTrackingDir = Path.Combine(AppData.ProgramDirectory, "StatTracking" + Path.DirectorySeparatorChar);
        /// <summary>
        /// The stat tracking save path
        /// </summary>
        public static string StatTrackingSavePath = DefaultTrackingDir;
    }
}