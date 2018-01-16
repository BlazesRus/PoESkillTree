using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using System;
//using System;
using System.Collections.Generic;

namespace POESKillTree.SkillTreeFiles
{
    public class TrackedAttribute
    {
        /// <summary>
        /// Gets the name of the PseudoAttribute.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the list of Attributes this PseudoAttribute contains.
        /// </summary>
        public List<POESKillTree.TreeGenerator.Model.PseudoAttributes.Attribute> Attributes { get; private set; }

        /// <summary>
        /// Gets the name of the group this PseudoAttribute belongs to.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // Used in group and sort descriptions.
        public string Group { get; private set; }

        /// <summary>
        /// Creates a new PseudoAttribute with the given name and group
        /// and an empty list of Attributes.
        /// </summary>
        /// <param name="name">Name (not null)</param>
        /// <param name="group">Group (not null)</param>
        internal TrackedAttribute(string name, string group)
        {
            if (name == null) throw new System.ArgumentNullException("name");
            if (group == null) throw new System.ArgumentNullException("group");
            Name = name;
            Group = group;
            Attributes = new List<POESKillTree.TreeGenerator.Model.PseudoAttributes.Attribute>();
        }

        public TrackedAttribute(PseudoAttribute attribute)
        {
            this.Name = attribute.Name;
            this.Group = attribute.Group;
            this.Attributes = attribute.Attributes; 
        }

        public override string ToString()
        {
            return Name;
        }

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

        public static implicit operator TrackedAttribute(PseudoAttribute attribute)
        {
            TrackedAttribute NewSelf = new TrackedAttribute(attribute);
            return NewSelf;
        }

        public static explicit operator PseudoAttribute(TrackedAttribute self)
        {
            PseudoAttribute NewSelf = new PseudoAttribute(self.Name, self.Group);
            NewSelf.Attributes = self.Attributes;
            return NewSelf;
        }
    }

    public class TrackedAttributes : System.Collections.Generic.List<TrackedAttribute>
    {
        /// <summary>
        /// Adds the specified attribute.
        /// </summary>
        /// <param name="Attribute">The attribute.</param>
        public void Add(PseudoAttribute Attribute)
        {
            Add(new TrackedAttribute(Attribute));
        }

        public TrackedAttributes CloneSelf()
        {
            TrackedAttributes NewSelf = this;
            return NewSelf;
        }

        /// <summary>
        /// Gets the index of attribute.
        /// </summary>
        /// <param name="Name">The name of Attribute</param>
        /// <returns></returns>
        public int GetIndexOfAttribute(string Name)
        {
            for (int Index = 0; Index < this.Count; Index++)
            {
                if (this[Index].Name == Name)
                {
                    return Index;
                }
            }
            return -1;
        }
        /// <summary>
        /// Gets the index of attribute.
        /// </summary>
        /// <param name="Attribute">The attribute.</param>
        /// <returns></returns>
        public int GetIndexOfAttribute(PseudoAttribute Attribute)
        {
            for (int Index = 0; Index < Count; ++Index)
            {
                if (this[Index].Name == Attribute.Name)
                {
                    return Index;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the name of attribute.
        /// </summary>
        /// <param name="Index">The index.</param>
        /// <returns></returns>
        public string GetNameOfAttribute(int Index)
        {
            TrackedAttribute CurrentAttribute = this[Index];
            return CurrentAttribute.Name;
        }

        /// <summary>
        /// Creates the attribute dictionary.
        /// </summary>
        /// <param name="AttributeDic">The attribute dic.</param>
        /// <returns></returns>
        public Dictionary<string, float> CreateAttributeDictionary(Dictionary<string, List<float>> AttributeDic)
        {
            Dictionary<string, float> AttributeTotals = new Dictionary<string, float>(this.Count);
            for (int Index = 0; Index < Count; ++Index)
            {
                AttributeTotals.Add(this[Index].Name, this[Index].CalculateValue(AttributeDic));
            }
            return AttributeTotals;
        }

        /// <summary>
        /// Places the Tracked Attributes into attribute dictionary
        /// </summary>
        /// <param name="AttributeDic">The attribute dic.</param>
        /// <returns></returns>
        public Dictionary<string, List<float>> PlaceIntoAttributeDic(Dictionary<string, List<float>> AttributeDic)
        {
            if (Count == 0) { return AttributeDic; }
            Dictionary<string, float> AttributeTotals = CreateAttributeDictionary(AttributeDic);
            foreach(var Element in AttributeTotals.Keys)
            {
                if (AttributeDic.ContainsKey(Element))
                {
                    List<float> TargetValue = new List<float>(1);
                    TargetValue.Add(AttributeTotals[Element]);
                    AttributeDic[Element] = TargetValue;
                }
                else
                {
                    List<float> TargetValue = new List<float>(1);
                    TargetValue.Add(AttributeTotals[Element]);
                    AttributeDic.Add(Element, TargetValue);
                }
            }
            return AttributeDic;
        }

        /// <summary>
        /// Starts the tracking.
        /// </summary>
        /// <param name="pseudoAttributeConstraints">The pseudo attribute constraints.</param>
        public void StartTracking(Dictionary<PseudoAttribute, System.Tuple<float, double>> pseudoAttributeConstraints)
        {
            int Index;
            foreach (var Attribute in pseudoAttributeConstraints.Keys)//Don't need target value and weight
            {
                Index = GetIndexOfAttribute(Attribute);
                if (Index == -1)
                {
                    this.Add(Attribute);
                }
                else
                {
                    this[Index] = new TrackedAttribute(Attribute);
                }
            }
        }

        /// <summary>
        /// Updates the value.
        /// </summary>
        /// <param name="selectedAttributes">The selected attributes.</param>
        public void UpdateValue(Dictionary<string, List<float>> selectedAttributes)
        {
            if (Count == 0) { return; }
            for (int Index = 0; Index < Count; ++Index)
            {
                this[Index].CalculateValue(selectedAttributes);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TrackedAttribute"/> with the specified index key using data field to create new tracked attribute if indexKey not found 
        /// </summary>
        /// <value>
        /// The <see cref="TrackedAttribute"/>.
        /// </value>
        /// <param name="IndexKey">The index key.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">IndexKey has found no matches in indexes - IndexKey</exception>
        public TrackedAttribute this[string IndexKey, PseudoAttribute data]
        {
            get
            {
                int indexFound = -1;
                for (int Index = 0; Index < Count && indexFound == -1; ++Index)
                {
                    if (this[Index].Name == IndexKey)
                    {
                        indexFound = Index;
                    }
                }
                if (indexFound != -1)
                {
                    return this[indexFound];
                }
                else
                {
                    if(data==null)
                    {
                        throw new System.ArgumentException("IndexKey has found no matches in indexes", "IndexKey");
                    }
                    else
                    {
                        indexFound = this.Count;
                        this.Add(new TrackedAttribute(data));
                        return this[indexFound];
                    }
                }
            }
            set
            {
                int indexFound = -1;
                for (int Index = 0; Index < Count && indexFound == -1; ++Index)
                {
                    if (this[Index].Name == IndexKey)
                    {
                        indexFound = Index;
                    }
                }
                if (indexFound != -1)
                {
                    this[indexFound] = value;
                }
                else
                {
                    this.Add(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PseudoAttribute"/> with the specified index key.
        /// </summary>
        /// <value>
        /// The <see cref="PseudoAttribute"/>.
        /// </value>
        /// <param name="IndexKey">The index key.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">IndexKey has found no matches in indexes - IndexKey</exception>
        public PseudoAttribute this[string IndexKey]
        {
            get
            {
                int indexFound = -1;
                for (int Index = 0; Index < Count && indexFound == -1; ++Index)
                {
                    if (this[Index].Name == IndexKey)
                    {
                        indexFound = Index;
                    }
                }
                if (indexFound != -1)
                {
                    return (PseudoAttribute)this[indexFound];
                }
                else
                {
                    throw new System.ArgumentException("IndexKey has found no matches in indexes", "IndexKey");
                }
            }
            set
            {
                int indexFound = -1;
                for (int Index = 0; Index < Count && indexFound == -1; ++Index)
                {
                    if (this[Index].Name == IndexKey)
                    {
                        indexFound = Index;
                    }
                }
                if (indexFound != -1)
                {
                    this[indexFound] = value;
                }
                else
                {
                    this.Add(value);
                }
            }
        }
    }
}