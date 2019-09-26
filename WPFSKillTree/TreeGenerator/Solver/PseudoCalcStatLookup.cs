using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoESkillTree.TreeGenerator.Solver
{
    public class PseudoCalcStatLookup : Dictionary<string, PseudoCalcCon>
    {
        public List<string> MainKeys;
        public List<string> OtherKeys;
        public Dictionary<string, float> StatTotals;

        /*public void RecalculateMainKeys()
        {
            MainKeys.Clear();
            OtherKeys.Clear();

        }
        */
    }
}
