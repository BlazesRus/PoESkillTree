using System;
using System.Collections.Generic;
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
        /// The tracked stat file names
        /// </summary>
        public static string[] TrackedStatFileNames = { "" };
        public static string CurrentTrackedFileName = "";
    }
}
