using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POESKillTree.SkillTreeFiles
{
    /// <summary>
    /// 
    /// </summary>
    public class ConvertedJewelData
    {
        /// <summary>
        /// Calculates the total attributes in jewel area.(Based on https://github.com/PoESkillTree/PoESkillTree/issues/163)
        /// </summary>
        /// <returns></returns>
        static public List<float> CalculateTotalAttributesInJewelArea()
        {
            List<float> AttributeTotal = new List<float>(4);
            //Point p = ((MouseEventArgs)e.OriginalSource).GetPosition(zbSkillTreeBackground.Child);
            //var v = new Vector2D(p.X, p.Y);

            //v = v * _multransform + _addtransform;

            //IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
            //    SkillTree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
            return AttributeTotal;
        }
    }
}
