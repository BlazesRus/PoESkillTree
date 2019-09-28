using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoESkillTree.TreeGenerator.Solver
{
    /// <summary>  TargetValue, Weight, and RequiredStat Storage class</summary>
    public class PseudoCalcCon
    {
        public float TargetValue;
        public double Weight;
        public bool Required;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstraintValues"/> class.
        /// </summary>
        /// <param name="targetValue">The target value.</param>
        /// <param name="weight">The weight.</param>
        public PseudoCalcCon(float targetValue, double weight, bool required)
        {
            TargetValue = targetValue;
            Weight = weight;
            Required = required;
        }
    }

    /// <summary>
    /// PseudoAttributeConstraint data object where the PseudoAttribute is converted
    /// into the applicable attributes and their conversion multiplier.
    /// </summary>
    public class ConvertedPseudoCalcCon
    {
        public List<Tuple<string, float>> Attributes { get; private set; }

        public Tuple<float, double, bool> TargetWeightTuple { get; private set; }

        //public bool Required { get { return TargetWeightTuple.Item3; } }

        public ConvertedPseudoCalcCon(List<Tuple<string, float>> attributes, Tuple<float, double, bool> tuple)
        {
            Attributes = attributes;
            TargetWeightTuple = tuple;
        }
    }
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
