using System;
using System.Collections.Generic;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.SkillTreeFiles
{
	public class TrackedAttribute
	{
        public PseudoAttribute AttributeData;
        public string Name()
        {
            return AttributeData.Name;
        }
		public float TotalStat=0.0f;
        public TrackedAttribute(PseudoAttribute Attribute)
        {
            AttributeData = Attribute;
        }
        public void UpdateValue(List<POESKillTree.ViewModels.Attribute> attrlist)
        {
            TotalStat = 0.0f;
            string AttributeName;
            float Multiplier;
            foreach (var Attribute in AttributeData.Attributes)
            {
                AttributeName = Attribute.Name;
                Multiplier = Attribute.ConversionMultiplier;
                for (int Index = 0; Index < attrlist.Count && Multiplier != 0.0f; ++Index)
                {
                    if (attrlist[Index].Text == AttributeName)
                    {
                        TotalStat += Multiplier * attrlist[Index].Deltas[0];
                        Multiplier = 0.0f;
                    }
                }
            }
            Console.WriteLine(Name() + " tested to have total stat of " + TotalStat);
        }

        public void UpdateValue(Dictionary<string, List<float>> attrlist)
        {
            TotalStat = 0.0f;
            string AttributeName;
            float Multiplier;
            List<float> RetrievedVal;
            foreach (var Attribute in AttributeData.Attributes)
            {
                AttributeName = Attribute.Name;
                Multiplier = Attribute.ConversionMultiplier;
                attrlist.TryGetValue(AttributeName, out RetrievedVal);
                if (RetrievedVal != null && RetrievedVal.Count == 1)
                {
                    TotalStat += Multiplier * RetrievedVal[0];
                }
            }
            Console.WriteLine(Name() + " tested to have total stat of " + TotalStat);
        }

        public void UpdateValue(SkillTree tree)
        {
            TotalStat = 0.0f;
            string AttributeName;
            float Multiplier;
			List<float> RetrievedVal;
			foreach (var Attribute in AttributeData.Attributes)
			{
				AttributeName = Attribute.Name;
				Multiplier = Attribute.ConversionMultiplier;
				tree.SelectedAttributes.TryGetValue(AttributeName, out RetrievedVal);
				if (RetrievedVal!=null&&RetrievedVal.Count == 1)
				{
					TotalStat += Multiplier * RetrievedVal[0];
				}
			}
            Console.WriteLine(Name() + " tested to have total stat of " + TotalStat);
        }
    }

    public class TrackedAttributes : System.Collections.Generic.List<TrackedAttribute>
    {
        public int GetIndexOfAttribute(string Name)
        {
            for(int Index=0;Index<this.Count;Index++)
            { 
                if(this[Index].Name()==Name)
                {
                    return Index;
                }
            }
            return -1;
        }
        public void Add(PseudoAttribute Attribute)
        {
            Add(new TrackedAttribute(Attribute));
        }
        public void StartTracking(Dictionary<PseudoAttribute, Tuple<float, double>> pseudoAttributeConstraints)
        {
            int Index;
            foreach(var Attribute in pseudoAttributeConstraints.Keys)//Don't need target value and weight
            {
                if(this.Count!=0)
                {
                    this.Reset();
                }
                Index = GetIndexOfAttribute(Attribute);
                if(Index==-1)
                {
                    this.Add(Attribute);
                }
                else
                {
                    this[Index] = new TrackedAttribute(Attribute);
                }
            }

        }
        public string GetNameOfAttribute(int Index)
        {
            TrackedAttribute CurrentAttribute = this[Index];
            return CurrentAttribute.Name();

        }
        public void UpdateStats(SkillTree tree)
        {
            if(Count==0){return;}
            for (int Index=0;Index<Count;++Index)
            {
                this[Index].UpdateValue(tree);
            }

        }

        public void UpdateValue(Dictionary<string, List<float>> selectedAttributes)
        {
            if (Count == 0) { return; }
            for (int Index = 0; Index < Count; ++Index)
            {
                this[Index].UpdateValue(selectedAttributes);
            }
        }

        public int GetIndexOfAttribute(PseudoAttribute Attribute)
        {
            for (int Index = 0; Index < Count; ++Index)
            {
                if (this[Index].Name() == Attribute.Name)
                {
                    return Index;
                }
            }
            return -1;
        }

        //public PseudoAttribute this[TrackedAttribute IndexKey]
        //{
        //    get
        //    {
        //        return GetIndexOfAttribute(IndexKey);
        //    }
        //    set
        //    {
        //        int index = FindMatchingName(IndexKey);
        //        if (index == -1)
        //        {
        //            Add(IndexKey, value);
        //        }
        //    }
        //}

        public void Reset()
        {
            for (int Index = 0; Index < Count; ++Index)
            {
                this[Index].TotalStat = 0.0f;
            }
        }
    }
}