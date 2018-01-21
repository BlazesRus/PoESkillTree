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
        /// The default tracking directory
        /// </summary>
        public static string DefaultTrackingDir = Path.Combine(AppData.ProgramDirectory, "StatTracking" + Path.DirectorySeparatorChar);
        /// <summary>
        /// The stat tracking save path (Shared between TrackedStatsMenuModel and TrackedStatsMenu)
        /// </summary>
        public static string StatTrackingSavePath = DefaultTrackingDir;
    }
}