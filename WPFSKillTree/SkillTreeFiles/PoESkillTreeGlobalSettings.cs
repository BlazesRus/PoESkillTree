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
        /// The current tracked attributes
        /// </summary>
        public static POESKillTree.SkillTreeFiles.TrackedAttributes CurrentTrackedAttributes = new SkillTreeFiles.TrackedAttributes();
        /// <summary>
        /// The current tracked total stats(SkillTree+Item Stats)
        /// </summary>
        public static POESKillTree.SkillTreeFiles.TrackedAttributes CurrentTrackedTotalStats = new SkillTreeFiles.TrackedAttributes();
        //public static int NodesLastTrackingUpdate = 0;
        //public static IndexDictionary TrackedAttributeIndexes;
    }
}
