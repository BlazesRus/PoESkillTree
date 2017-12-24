using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using System;
using System.Collections.Generic;

namespace POESKillTree.SkillTreeFiles
{
    public class TrackedAttribute
    {
        /// <summary>
        /// The attribute data
        /// </summary>
        public PseudoAttribute AttributeData;

        public float TotalStat = 0.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedAttribute"/> class.
        /// </summary>
        /// <param name="Attribute">The attribute.</param>
        public TrackedAttribute(PseudoAttribute Attribute)
        {
            AttributeData = Attribute;
        }

        /// <summary>
        /// Names this instance.
        /// </summary>
        /// <returns></returns>
        public string Name()
        {
            return AttributeData.Name;
        }

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
            //#if(DEBUG)
            //            Console.WriteLine(Name() + " tested from attrDictionary to have total stat of " + TotalStat);
            //#endif
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
                if (this[Index].Name() == Name)
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
                if (this[Index].Name() == Attribute.Name)
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
            return CurrentAttribute.Name();
        }

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

        /// <summary>
        /// Starts the tracking.
        /// </summary>
        /// <param name="pseudoAttributeConstraints">The pseudo attribute constraints.</param>
        public void StartTracking(Dictionary<PseudoAttribute, Tuple<float, double>> pseudoAttributeConstraints)
        {
            int Index;
            foreach (var Attribute in pseudoAttributeConstraints.Keys)//Don't need target value and weight
            {
                if (this.Count != 0)
                {
                    this.Reset();
                }
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
                this[Index].UpdateValue(selectedAttributes);
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
                    if (this[Index].Name() == IndexKey)
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
                    if (this[Index].Name() == IndexKey)
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
                    if (this[Index].Name() == IndexKey)
                    {
                        indexFound = Index;
                    }
                }
                if (indexFound != -1)
                {
                    return this[indexFound].AttributeData;
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
                    if (this[Index].Name() == IndexKey)
                    {
                        indexFound = Index;
                    }
                }
                if (indexFound != -1)
                {
                    this[indexFound].AttributeData = value;
                }
                else
                {
                    this.Add(value);
                }
            }
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
        ///// <summary>
        ///// Places the Tracked Attributes into attribute dictionary
        ///// </summary>
        ///// <param name="AttributeDic">The attribute dic.</param>
        ///// <returns></returns>
        //public List<POESKillTree.ViewModels.Attribute> PlaceIntoAttributeDic(List<POESKillTree.ViewModels.Attribute> AttributeDic)
        //{
        //    if (Count == 0) { return AttributeDic; }
        //    for (int Index = 0; Index < Count; ++Index)
        //    {
        //        //if (AttributeDic.ContainsKey(this[Index].Name()))
        //        //{
        //        //    List<float> TargetValue = new List<float>(1);
        //        //    TargetValue.Add(this[Index].TotalStat);
        //        //    AttributeDic[this[Index].Name()] = TargetValue;
        //        //}
        //        //else
        //        //{
        //        //    List<float> TargetValue = new List<float>(1);
        //        //    TargetValue.Add(this[Index].TotalStat);
        //        //    AttributeDic.Add(this[Index].Name(), TargetValue);
        //        //}
        //    }
        //    return AttributeDic;
        //}

        //private const string AttributeKey = "Attribute";
        //private const string TargetValueKey = "TargetValue";
        //private const string WeightKey = "Weight";

        //private readonly TreeGenerator.ViewModels.AdvancedTabViewModel _vm;

        //private var attr = attrToken.ToObject<string>();

        //

        //private var newConstraints = new List<AttributeConstraint>();
        //private var obj = element as JObject;
        ////IReadOnlyList<ISetting> SubSettings;
        ////JObject LoadedData = _persistentData.CurrentBuild.AdditionalData;
        ////JToken token;
        //////if (!LoadedData.TryGetValue(Key, out token) || !(token is JObject))
        //////{
        //////	Reset();
        //////	return;
        //////}
        ////SubSettings.ForEach(s => s.LoadFrom((JObject)token));
        //private Newtonsoft.Json.Linq.JToken token;
        //private new AttributeConstraint(attr)
        //{
        //    TargetValue = targetToken.ToObject<float>(),
        //    Weight = weightToken.ToObject<int>()
        //}
        //private const string AttributeKey = "Attribute";
        //private const string TargetValueKey = "TargetValue";
        //private const string WeightKey = "Weight";
        //public void StartTrackingFromSave(Newtonsoft.Json.Linq.JObject LoadedData)//_persistentData.CurrentBuild.AdditionalData;
        //{
        //    //var obj = element as Newtonsoft.Json.Linq.JObject;
        //    Newtonsoft.Json.Linq.JToken attrToken, targetToken, weightToken;
        //    TreeGenerator.ViewModels.AdvancedTabViewModel _vm;
        //    var newConstraints = new List<AttributeConstraint>();
        //    //AttributeConstraint
        //    //_vm.ClearAttributeConstraints();
        //    if (LoadedData.TryGetValue(nameof(AttributeConstraints), out token) && token.Any())
        //    {
        //        foreach (var element in token)
        //        {
        //            if (obj == null)
        //                continue;
        //            if (!obj.TryGetValue(AttributeKey, out attrToken)
        //                || !obj.TryGetValue(TargetValueKey, out targetToken)
        //                || !obj.TryGetValue(WeightKey, out weightToken))
        //                continue;
        //            newConstraints.Add();

        //            _vm._addedAttributes.Add(attr);
        //        }

        //        _vm.AttributesView.Refresh();
        //        _vm.AttributesView.MoveCurrentToFirst();
        //        _vm.NewAttributeConstraint.Data = _vm.AttributesView.CurrentItem as string;
        //        _vm.AttributeConstraints.AddRange(newConstraints);
        //    }

        //    _vm.ClearPseudoAttributeConstraints();
        //    if (LoadedData.TryGetValue(nameof(PseudoAttributeConstraints), out token) && token.Any())
        //    {
        //        var pseudoDict = _vm._pseudoAttributes.ToDictionary(p => p.Name);

        //        var newConstraints = new List<PseudoAttributeConstraint>();
        //        foreach (var element in token)
        //        {
        //            var obj = element as JObject;
        //            if (obj == null)
        //                continue;
        //            JToken attrToken, targetToken, weightToken;
        //            if (!obj.TryGetValue(AttributeKey, out attrToken)
        //                || !obj.TryGetValue(TargetValueKey, out targetToken)
        //                || !obj.TryGetValue(WeightKey, out weightToken))
        //                continue;

        //            PseudoAttribute attr;
        //            if (!pseudoDict.TryGetValue(attrToken.ToObject<string>(), out attr))
        //                continue;
        //            newConstraints.Add(new PseudoAttributeConstraint(attr)
        //            {
        //                TargetValue = targetToken.ToObject<float>(),
        //                Weight = weightToken.ToObject<int>()
        //            });
        //            _vm._addedPseudoAttributes.Add(attr);
        //        }

        //        _vm.PseudoAttributesView.Refresh();
        //        _vm.PseudoAttributesView.MoveCurrentToFirst();
        //        _vm.NewPseudoAttributeConstraint.Data = _vm.PseudoAttributesView.CurrentItem as PseudoAttribute;
        //        _vm.PseudoAttributeConstraints.AddRange(newConstraints);
        //    }
        //}
    }
}