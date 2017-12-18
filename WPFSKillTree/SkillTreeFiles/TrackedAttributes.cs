using System;
using System.Collections.Generic;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.SkillTreeFiles
{
    public class TrackedAttribute
    {
        /// <summary>
        /// The attribute data
        /// </summary>
        public PseudoAttribute AttributeData;
        /// <summary>
        /// Names this instance.
        /// </summary>
        /// <returns></returns>
        public string Name()
        {
            return AttributeData.Name;
        }
        public float TotalStat=0.0f;
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedAttribute"/> class.
        /// </summary>
        /// <param name="Attribute">The attribute.</param>
        public TrackedAttribute(PseudoAttribute Attribute)
        {
            AttributeData = Attribute;
        }

//        /// <summary>
//        /// Updates the value.
//        /// </summary>
//        /// <param name="attrlist">The attrlist.</param>
//        public void UpdateValue(List<POESKillTree.ViewModels.Attribute> attrlist)
//        {
//            TotalStat = 0.0f;
//            string AttributeName;
//            float Multiplier;
//            foreach (var Attribute in AttributeData.Attributes)
//            {
//                AttributeName = Attribute.Name;
//                Multiplier = Attribute.ConversionMultiplier;
//                for (int Index = 0; Index < attrlist.Count && Multiplier != 0.0f; ++Index)
//                {
//                    if (attrlist[Index].Text == AttributeName)
//                    {
//                        TotalStat += Multiplier * attrlist[Index].Deltas[0];
//                        Multiplier = 0.0f;
//                    }
//                }
//            }
//#if(DEBUG)
//            Console.WriteLine(Name() + " tested from List<POESKillTree.ViewModels.Attribute> to have total stat of " + TotalStat);
//#endif
//        }

        /// <summary>
        /// Updates the value.
        /// </summary>
        /// <param name="attrlist">The attrlist.</param>
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
#if(DEBUG)
            Console.WriteLine(Name() + " tested from attrDictionary to have total stat of " + TotalStat);
#endif
        }

        //        /// <summary>
        //        /// Updates the value.(Fails to correctly calculate from ControllViewModelSolveAsync)
        //        /// </summary>
        //        /// <param name="tree">The tree.</param>
        //        public void UpdateValue(SkillTree tree)
        //        {
        //            TotalStat = 0.0f;
        //            string AttributeName;
        //            float Multiplier;
        //            List<float> RetrievedVal;
        //            foreach (var Attribute in AttributeData.Attributes)
        //            {
        //                AttributeName = Attribute.Name;
        //                Multiplier = Attribute.ConversionMultiplier;
        //                tree.SelectedAttributes.TryGetValue(AttributeName, out RetrievedVal);
        //                if (RetrievedVal!=null&&RetrievedVal.Count == 1)
        //                {
        //                    TotalStat += Multiplier * RetrievedVal[0];
        //                }
        //            }
        //#if(DEBUG)
        //            Console.WriteLine(Name() + " tested from SkillTree to have total stat of " + TotalStat);
        //#endif
        //        }
    }

    public class TrackedAttributes : System.Collections.Generic.List<TrackedAttribute>
    {
        /// <summary>
        /// Gets the index of attribute.
        /// </summary>
        /// <param name="Name">The name of Attribute</param>
        /// <returns></returns>
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
        /// <summary>
        /// Adds the specified attribute.
        /// </summary>
        /// <param name="Attribute">The attribute.</param>
        public void Add(PseudoAttribute Attribute)
        {
            Add(new TrackedAttribute(Attribute));
        }
        /// <summary>
        /// Starts the tracking.
        /// </summary>
        /// <param name="pseudoAttributeConstraints">The pseudo attribute constraints.</param>
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
        /// <summary>
        /// Gets the name of attribute.
        /// </summary>
        /// <param name="Index">The index.</param>
        /// <returns></returns>
        public string GetNameOfAttribute(int Index)
        {
            TrackedAttribute CurrentAttribute = this[Index];
            return CurrentAttribute.Name();

        }

        ///// <summary>
        ///// Updates the stats.(Fails to correctly calculate from ControllViewModelSolveAsync)
        ///// </summary>
        ///// <param name="tree">The tree.</param>
        //public void UpdateStats(SkillTree tree)
        //{
        //    if(Count==0){return;}
        //    for (int Index=0;Index<Count;++Index)
        //    {
        //        this[Index].UpdateValue(tree);
        //    }
        //}

        /// <summary>
        /// Updates the value.
        /// </summary>
        /// <param name="selectedAttributes">The selected attributes.</param>
        public void UpdateValue(Dictionary<string, List<float>> selectedAttributes)
        {
            if (Count == 0) { return; }
            for (int Index = 0; Index < Count; ++Index)
            {
                this[Index].UpdateValue(selectedAttributes);
            }
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

        ///// <summary>
        ///// Places the Tracked Attributes into attribute dictionary
        ///// </summary>
        ///// <param name="AttributeDic">The attribute dictionary</param>
        ///// <param name="Target">The target.</param>
        //public void PlaceIntoAttributeDic(Dictionary<string, List<float>> AttributeDic, TrackedAttribute Target)
        //{
        //    if (AttributeDic.ContainsKey(Target.Name()))
        //    {
        //        List<float> TargetValue = new List<float>(1);
        //        TargetValue.Add(Target.TotalStat);
        //        AttributeDic[Target.Name()] = TargetValue;
        //    }
        //    else
        //    {
        //        List<float> TargetValue = new List<float>(1);
        //        TargetValue.Add(Target.TotalStat);
        //        AttributeDic.Add(Target.Name(), TargetValue);
        //    }
        //}

        /// <summary>
        /// Places the Tracked Attributes into attribute dictionary
        /// </summary>
        /// <param name="AttributeDic">The attribute dic.</param>
        /// <returns></returns>
        public Dictionary<string, List<float>> PlaceIntoAttributeDic(Dictionary<string, List<float>> AttributeDic)
        {
            if (Count == 0) { return AttributeDic; }
            for (int Index = 0; Index < Count; ++Index)
            {
                if (AttributeDic.ContainsKey(this[Index].Name()))
                {
                    List<float> TargetValue = new List<float>(1);
                    TargetValue.Add(this[Index].TotalStat);
                    AttributeDic[this[Index].Name()] = TargetValue;
                }
                else
                {
                    List<float> TargetValue = new List<float>(1);
                    TargetValue.Add(this[Index].TotalStat);
                    AttributeDic.Add(this[Index].Name(), TargetValue);
                }
            }
            return AttributeDic;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            for (int Index = 0; Index < Count; ++Index)
            {
                this[Index].TotalStat = 0.0f;
            }
        }
    }
}