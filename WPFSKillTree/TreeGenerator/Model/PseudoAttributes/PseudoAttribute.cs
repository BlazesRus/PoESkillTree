using System;
using System.Collections.Generic;

namespace PoESkillTree.TreeGenerator.Model.PseudoAttributes
{
    /// <summary>
    /// Data class describing a PseudoAttribute as a collection of <see cref="Attribute"/>s.
    /// </summary>
    public class PseudoAttribute
    {
        /// <summary>
        /// Gets the name of the PseudoAttribute.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the list of Attributes this PseudoAttribute contains.
        /// </summary>
        public List<Attribute> Attributes { get; }

        /// <summary>
        /// Gets the name of the group this PseudoAttribute belongs to.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // Used in group and sort descriptions.
        public string Group { get;  private set; }

        /// <summary>
        /// Creates a new PseudoAttribute with the given name and group
        /// and an empty list of Attributes.
        /// </summary>
        /// <param name="name">Name (not null)</param>
        /// <param name="group">Group (not null)</param>
        internal PseudoAttribute(string name, string group)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Group = @group ?? throw new ArgumentNullException(nameof(@group));
            Attributes = new List<Attribute>();
        }

        public override string ToString() => Name;

        /// <summary>
        /// Calculates updated value
        /// </summary>
        /// <param name="attrlist">The attrlist.</param>
        public float CalculateValue(Dictionary<string, List<float>> attrlist)
        {
            float TotalStat = 0.0f;
            string AttributeName;
            float Multiplier;
            List<float> RetrievedVal;
            foreach (var Attribute in Attributes)
            {
                AttributeName = Attribute.Name;
                Multiplier = Attribute.ConversionMultiplier;
                attrlist.TryGetValue(AttributeName, out RetrievedVal);
                if (RetrievedVal != null && RetrievedVal.Count == 1)
                {
                    TotalStat += Multiplier * RetrievedVal[0];
                }
            }
            return TotalStat;
        }

        public PseudoAttribute(PseudoAttribute Target, string name)
        {
            Name = name;
            Group = Target.Group;
            Attributes = Target.Attributes;
        }
    }
}