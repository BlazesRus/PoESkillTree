// ***********************************************************************
// Code Created by James Michael Armstrong (https://github.com/BlazesRus)
// Latest GlobalCode Release at https://github.com/BlazesRus/MultiPlatformGlobalCode
// ***********************************************************************
using POESKillTree.TrackedStatViews;
using POESKillTree.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.Model.Items;
using POESKillTree.ViewModels.Equipment;
using POESKillTree.SkillTreeFiles;

using System.Runtime.CompilerServices;//Needed for Notifier parts of JewelData
using POESKillTree.ViewModels;
using System.Text.RegularExpressions;
using System.Globalization;

namespace POESKillTree
{
    /// <summary>
    /// Class named JewelNodeData.
    /// Implements the <see cref="POESKillTree.Utils.Notifier" />
    /// </summary>
    /// <seealso cref="POESKillTree.Utils.Notifier" />
    public class JewelNodeData : Notifier
    {
        /// <summary>
        /// The item model
        /// </summary>
        public JewelItemViewModel ItemModel;
        /// <summary>
        /// Gets the jewel data.
        /// </summary>
        /// <value>The jewel data.</value>
        public JewelItem JewelData { get { return ItemModel.Item; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="JewelNodeData"/> class.
        /// </summary>
        public JewelNodeData()
        {
            ItemModel = null;
        }
        public JewelNodeData(JewelData TargetJewelData, ushort slot)
        {
            ItemModel = TargetJewelData.CreateSlotVm(slot);
        }
    }

    /// <summary>
    /// Dictionary  holding NodeIDs for Jewel Slots  as keys and JewelItems as data
    /// Implements the <see cref="System.Collections.Generic.Dictionary{System.UInt16, POESKillTree.JewelNodeData}" />
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.UInt16, POESKillTree.JewelNodeData}" />
    [Serializable]
    public class JewelData : Dictionary<ushort, JewelNodeData>
    {//Notifier combined directly into class since can't have 2 base classes
        #region NotifierCode
        /// <summary>
        /// Sets <paramref name="backingStore"/> to <paramref name="value"/> and
        /// raises <see cref="PropertyChanging"/> before and <see cref="PropertyChanged"/>
        /// after setting the value.
        /// </summary>
        /// <param name="backingStore">Target variable</param>
        /// <param name="value">Source variable</param>
        /// <param name="onChanged">Called after changing the value but before raising <see cref="PropertyChanged"/>.</param>
        /// <param name="onChanging">Called before changing the value and before raising <see cref="PropertyChanging"/> with <paramref name="value"/> as parameter.</param>
        /// <param name="propertyName">Name of the changed property</param>
        protected void SetProperty<T>(
            ref T backingStore, T value,
            Action onChanged = null,
            Action<T> onChanging = null,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;

            onChanging?.Invoke(value);
            OnPropertyChanging(propertyName);

            backingStore = value;

            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// INotifyPropertyChanged event that is called right before a property is changed.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        private void OnPropertyChanging(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// INotifyPropertyChanged event that is called right after a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Equivalent to <c>o.MemberwiseClone()</c> except that events are set to null.
        /// Override if your subclass has events or if you need to re-register handlers.
        /// </summary>
        protected virtual JewelData SafeMemberwiseClone()
        {
            var t = (JewelData)MemberwiseClone();
            t.PropertyChanged = null;
            t.PropertyChanging = null;
            return t;
        }
        #endregion

        public bool NotSetup;

        private JewelItemAttributes _itemAttributes;
        public JewelItemAttributes JewelAttributes
        {
            get { return _itemAttributes; }
            set
            {
                if (value == _itemAttributes)
                    return;
                _itemAttributes = value;
            }
        }

        private JewelData(JewelItemAttributes itemAttributes):base()
        {
            _itemAttributes = itemAttributes;
            StrJewelSlots = new System.Collections.Generic.List<ushort>();
            IntJewelSlots = new System.Collections.Generic.List<ushort>();
            DexJewelSlots = new System.Collections.Generic.List<ushort>();
            StrIntJewelSlots = new System.Collections.Generic.List<ushort>();
            StrDexJewelSlots = new System.Collections.Generic.List<ushort>();
            IntDexJewelSlots = new System.Collections.Generic.List<ushort>();
            NeutralJewelSlots = new System.Collections.Generic.List<ushort>();
        }

        public JewelItemViewModel CreateSlotVm(ushort slot)
        {
            var imageName = "Jewel";
            return new JewelItemViewModel(GlobalSettings.SharedDialogCoordinator, JewelAttributes, slot)
            {
                EmptyBackgroundImagePath = $"/POESKillTree;component/Images/EquipmentUI/ItemDefaults/{imageName}.png"
            };
        }

        /// <summary>
        /// Keys for Strength Threshold Jewel Slots
        /// </summary>
        public System.Collections.Generic.List<ushort> StrJewelSlots;
        /// <summary>
        /// Keys for Intelligence Threshold Jewel Slots
        /// </summary>
        public System.Collections.Generic.List<ushort> IntJewelSlots;
        /// <summary>
        /// Keys for Dexterity Threshold Jewel Slots
        /// </summary>
        public System.Collections.Generic.List<ushort> DexJewelSlots;
        /// <summary>
        /// Keys for Hybrid Strength+Intelligence Threshold Jewel Slots
        /// </summary>
        public System.Collections.Generic.List<ushort> StrIntJewelSlots;
        /// <summary>
        /// Keys for Hybrid Strength+Dexterity Threshold Jewel Slots
        /// </summary>
        public System.Collections.Generic.List<ushort> StrDexJewelSlots;
        /// <summary>
        /// Keys for Hybrid Intelligence+Dexterity Threshold Jewel Slots
        /// </summary>
        public System.Collections.Generic.List<ushort> IntDexJewelSlots;
        /// <summary>
        /// Keys for Non-Threshold Jewel Slots
        /// </summary>
        public System.Collections.Generic.List<ushort> NeutralJewelSlots;
        /// <summary>
        /// Initializes a new instance of the <see cref="JewelDictionary"/> class.
        /// </summary>
        public JewelData() : base(21)
        {
            JewelAttributes = new JewelItemAttributes();
            StrJewelSlots = new System.Collections.Generic.List<ushort>();
            IntJewelSlots = new System.Collections.Generic.List<ushort>();
            DexJewelSlots = new System.Collections.Generic.List<ushort>();
            StrIntJewelSlots = new System.Collections.Generic.List<ushort>();
            StrDexJewelSlots = new System.Collections.Generic.List<ushort>();
            IntDexJewelSlots = new System.Collections.Generic.List<ushort>();
            NeutralJewelSlots = new System.Collections.Generic.List<ushort>();
            NotSetup = true;
        }
        /// <summary>
        /// Adds the jewel slot.
        /// </summary>
        /// <param name="nodeID">The node identifier.</param>
        public void AddJewelSlot(ushort nodeID)
        {
            Add(nodeID, new JewelNodeData(this, nodeID));
        }
        /// <summary>
        /// Generate JewelDictionary Categories from  Data from SkillTree and add extra fake attributes to label threshold type and Node id for identifying node in inventory view
        /// </summary>
        public void CategorizeJewelSlots()
        {
            bool IsStrThreshold;
            bool IsIntThreshold;
            bool IsDexThreshold;

            SkillNode CurrentNode = null;
            ushort NodeID;
            string IDLabel;
            string AttributeName = "";
            float AttributeTotal;
            Vector2D nodePosition;
            IEnumerable<KeyValuePair<ushort, SkillNode>> affectedNodes;
            foreach (KeyValuePair<ushort, JewelNodeData> JewelElement in GlobalSettings.JewelInfo)
            {
                NodeID = JewelElement.Key;
                IDLabel = "Jewel Socket ID: " + NodeID;
                IsStrThreshold = false;
                IsIntThreshold = false;
                IsDexThreshold = false;
                var regexAttrib = new Regex("[0-9]*\\.?[0-9]+");
                for (int AttrIndex = 0; AttrIndex < 3; ++AttrIndex)
                {
                    AttributeTotal = 0.0f;
                    switch (AttrIndex)
                    {
                        case 1:
                            AttributeName = "+# to Intelligence";
                            break;
                        case 2:
                            AttributeName = "+# to Dexterity";
                            break;
                        default:
                            AttributeName = "+# to Strength";
                            break;
                    }
                    nodePosition = SkillTree.Skillnodes[NodeID].Position;
                    affectedNodes = SkillTree.Skillnodes.Where(n => ((n.Value.Position - nodePosition).Length < 1200.0)).ToList();
                    foreach (KeyValuePair<ushort, SkillNode> NodePair in affectedNodes)
                    {
                        CurrentNode = NodePair.Value;
                        if (CurrentNode.Attributes!=null&&CurrentNode.Attributes.ContainsKey(AttributeName))
                        {
                            AttributeTotal += CurrentNode.Attributes[AttributeName][0];
                        }
                    }
                    if (AttributeTotal >= 40.0f)
                    {
                        switch (AttrIndex)
                        {
                            case 1:
                                IsIntThreshold = true;
                                break;
                            case 2:
                                IsDexThreshold = true;
                                break;
                            default:
                                IsStrThreshold = true;
                                break;
                        }
                    }
                }
                if (IsStrThreshold)//No support for all 3 attribute types at once for now
                {
                    if (IsIntThreshold)
                    {
                        StrIntJewelSlots.Add(NodeID);
                        SkillTree.Skillnodes[NodeID].attributes = new[] { "+1 to Jewel Socket", "+1 Str JewelSlot", "+1 Int JewelSlot", IDLabel};
                        //SkillTree.Skillnodes[NodeID].Attributes.Add("+# to Int Based Jewel", new List<float>(1));
                    }
                    else if (IsDexThreshold)
                    {
                        StrDexJewelSlots.Add(NodeID);
                        SkillTree.Skillnodes[NodeID].attributes = new[] { "+1 to Jewel Socket", "+1 Str JewelSlot", "+1 Dex JewelSlot", IDLabel};
                        //SkillTree.Skillnodes[NodeID].Attributes.Add("+1 Dex JewelSlot", new List<float>(1));
                    }
                    else
                    {
                        StrJewelSlots.Add(NodeID);
                        SkillTree.Skillnodes[NodeID].attributes = new[] { "+1 to Jewel Socket", "+1 Str JewelSlot", IDLabel};
                    }
                    //SkillTree.Skillnodes[NodeID].Attributes.Add("+# to Str Based Jewel", new List<float>(1));
                }
                else if (IsIntThreshold)
                {
                    if (IsDexThreshold)
                    {
                        IntDexJewelSlots.Add(NodeID);
                        SkillTree.Skillnodes[NodeID].attributes = new[] { "+1 to Jewel Socket", "+1 Int JewelSlot", "+1 Dex JewelSlot", IDLabel };
                        //SkillTree.Skillnodes[NodeID].Attributes.Add("+1 Dex JewelSlot", new List<float>(1));
                    }
                    else
                    {
                        IntJewelSlots.Add(NodeID);
                        SkillTree.Skillnodes[NodeID].attributes = new[] { "+1 to Jewel Socket", "+1 Int JewelSlot", IDLabel};
                    }
                    //SkillTree.Skillnodes[NodeID].Attributes.Add("+# to Int Based Jewel", new List<float>(1));
                }
                else if (IsDexThreshold)
                {
                    DexJewelSlots.Add(NodeID);
                    SkillTree.Skillnodes[NodeID].attributes = new[] { "+1 to Jewel Socket", "+1 Dex JewelSlot", IDLabel};
                    //SkillTree.Skillnodes[NodeID].Attributes.Add("+1 Dex JewelSlot", new List<float>(1));

                }
                else//Neutral(often ineffective corner jewels)
                {
                    NeutralJewelSlots.Add(NodeID);
                    SkillTree.Skillnodes[NodeID].attributes = new[] { "+1 to Jewel Socket", IDLabel};
                }
                //SkillTree.Skillnodes[NodeID].Attributes.Add("Jewel Socket ID: #", new List<float>(NodeID));
                foreach (string s in SkillTree.Skillnodes[NodeID].attributes)
                {
                    if (s != "+1 to Jewel Socket")//Skip +1 to Jewel Socket
                    {
                        var values = new List<float>();

                        foreach (Match m in regexAttrib.Matches(s))
                        {
                            if (m.Value == "")
                                values.Add(float.NaN);
                            else
                                values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
                        }
                        string cs = (regexAttrib.Replace(s, "#"));
                        SkillTree.Skillnodes[NodeID].Attributes[cs] = values;
                    }
                }
            }
        }

        public static explicit operator System.Collections.Generic.List<ushort>(JewelData self)
        {
            return new List<ushort>(self.Keys);
        }

        /// <summary>
        /// Property that converts JewelDictionary into List of node ids
        /// </summary>
        public System.Collections.Generic.List<ushort> JewelIds { get { return (System.Collections.Generic.List<ushort>) this; } }

      //(Most of JewelData node searching code based on https://github.com/PoESkillTree/PoESkillTree/issues/163)
      //Point p = ((MouseEventArgs)e.OriginalSource).GetPosition(zbSkillTreeBackground.Child);
      //var v = new Vector2D(p.X, p.Y);
      //v = v * _multransform + _addtransform;
      //IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
      //    SkillTree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();

        /// <summary>
        /// Calculates the total of target attribute inside jewel area.
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        /// <param name="AttributeName">Name of Attribute to search for</param>
        /// <param name="SkilledNodes">The skilled nodes.</param>
        /// <param name="JewelRadiusType">Jewel Radius Type(Large/Medium/Small)(Default:Large"")</param>
        /// <returns></returns>
        static public float CalculateTotalOfAttributeInJewelArea(SkillNode TargetNode, string AttributeName, Utils.ObservableSet<SkillNode> SkilledNodes, string JewelRadiusType = "")
        {
            int JewelRadius;
            switch (JewelRadiusType)
            {
                case "Medium":
                    JewelRadius = 1200;
                    break;

                case "Small":
                    JewelRadius = 800;
                    break;

                default://"Large"
                    JewelRadius = 1500;
                    break;
            }
            SkillNode CurrentNode;
            float AttributeTotal = 0.0f;
            Vector2D nodePosition = TargetNode.Position;
            IEnumerable<KeyValuePair<ushort, SkillNode>> affectedNodes =
                SkillTree.Skillnodes.Where(n => ((n.Value.Position - nodePosition).Length < JewelRadius)).ToList();
            foreach (KeyValuePair<ushort, SkillNode> NodePair in affectedNodes)
            {
                CurrentNode = NodePair.Value;
                if (CurrentNode.Attributes.ContainsKey(AttributeName) && SkilledNodes.Contains(CurrentNode))
                {
                    AttributeTotal += CurrentNode.Attributes[AttributeName][0];
                }
            }
            return AttributeTotal;
        }

        /// <summary>
        /// Calculates the total of target attribute inside jewel area.
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        /// <param name="AttributeName">Name of Attribute to search for</param>
        /// <param name="SkilledNodes">The skilled nodes.</param>
        /// <param name="JewelRadiusType">Jewel Radius Type(Large/Medium/Small)(Default:Large"")</param>
        /// <returns></returns>
        static public float CalculateTotalUnallocAttributeInJewelArea(SkillNode TargetNode, string AttributeName, Utils.ObservableSet<SkillNode> SkilledNodes, string JewelRadiusType = "")
        {
            int JewelRadius;
            switch (JewelRadiusType)
            {
                case "Medium":
                    JewelRadius = 1200;
                    break;

                case "Small":
                    JewelRadius = 800;
                    break;

                default://"Large"
                    JewelRadius = 1500;
                    break;
            }
            SkillNode CurrentNode;
            float AttributeTotal = 0.0f;
            Vector2D nodePosition = TargetNode.Position;
            IEnumerable<KeyValuePair<ushort, SkillNode>> affectedNodes =
                SkillTree.Skillnodes.Where(n => ((n.Value.Position - nodePosition).Length < JewelRadius)).ToList();
            foreach (KeyValuePair<ushort, SkillNode> NodePair in affectedNodes)
            {
                CurrentNode = NodePair.Value;
                if (CurrentNode.Attributes.ContainsKey(AttributeName) && !SkilledNodes.Contains(CurrentNode))
                {
                    AttributeTotal += CurrentNode.Attributes[AttributeName][0];
                }
            }
            return AttributeTotal;
        }

        /// <summary>
        /// Applies the fake Intuitive Leap Support attribute to nodes in effected area
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        static public void ApplyIntuitiveLeapSupport(POESKillTree.SkillTreeFiles.SkillNode TargetNode)
        {
            List<float> BlankList = new List<float>(0);
            string[] ExtendedAttribute;
            int attributeSize;
            Vector2D nodePosition = TargetNode.Position;
            SkillNode CurrentNode;
            IEnumerable<KeyValuePair<ushort, SkillNode>> affectedNodes =
                SkillTree.Skillnodes.Where(n => ((n.Value.Position - nodePosition).Length < 800)).ToList();//Small Jewel Radius
            foreach (KeyValuePair<ushort, SkillNode> NodePair in affectedNodes)
            {
                CurrentNode = NodePair.Value;
                attributeSize = CurrentNode.attributes.Length;
                if (!CurrentNode.Attributes.ContainsKey(GlobalSettings.FakeIntuitiveLeapSupportAttribute) && attributeSize != 0)
                {
                    ExtendedAttribute = new string[attributeSize + 1];
                    for (int index = 0; index < attributeSize; ++index)
                    {
                        ExtendedAttribute[index] = CurrentNode.attributes[index];
                    }
                    ExtendedAttribute[attributeSize] = GlobalSettings.FakeIntuitiveLeapSupportAttribute;
                    CurrentNode.attributes = ExtendedAttribute;
                    CurrentNode.Attributes.Add(GlobalSettings.FakeIntuitiveLeapSupportAttribute, BlankList);
                }
            }
        }

        /// <summary>
        /// Removes the fake Intuitive Leap Support attribute to nodes in effected area
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        static public void RemoveIntuitiveLeapSupport(POESKillTree.SkillTreeFiles.SkillNode TargetNode)
        {
            string[] ExtendedAttribute;
            int attributeSize;
            int NewAttributeSize;
            int NewIndex;
            string CurrentAttri;
            Vector2D nodePosition = TargetNode.Position;
            SkillNode CurrentNode;
            IEnumerable<KeyValuePair<ushort, SkillNode>> affectedNodes =
                SkillTree.Skillnodes.Where(n => ((n.Value.Position - nodePosition).Length < 800)).ToList();//Small Jewel Radius
            foreach (KeyValuePair<ushort, SkillNode> NodePair in affectedNodes)
            {
                CurrentNode = NodePair.Value;
                if (CurrentNode.Attributes.ContainsKey(GlobalSettings.FakeIntuitiveLeapSupportAttribute))
                {
                    CurrentNode.Attributes.Remove(GlobalSettings.FakeIntuitiveLeapSupportAttribute);
                    attributeSize = CurrentNode.attributes.Length;
                    NewAttributeSize = attributeSize - 1;
                    ExtendedAttribute = new string[NewAttributeSize];
                    NewIndex = 0;
                    for (int index = 0; index < attributeSize; ++index)
                    {
                        CurrentAttri = CurrentNode.attributes[index];
                        if (CurrentAttri != GlobalSettings.FakeIntuitiveLeapSupportAttribute)
                        {
                            ExtendedAttribute[NewIndex] = CurrentNode.attributes[index];
                            ++NewIndex;
                        }
                    }
                    Array.Copy(CurrentNode.attributes, attributeSize, ExtendedAttribute, NewAttributeSize, 0);
                    CurrentNode.attributes = ExtendedAttribute;
                }
            }
        }

        /// <summary>
        /// Updates stats based on Unique Jewels Slotted
        /// </summary>
        /// <param name="attrlist">The attrlist.</param>
        /// <param name="Tree">The tree.</param>
        /// <returns></returns>
        static public Dictionary<string, List<float>> StatUpdater(Dictionary<string, List<float>> attrlist, SkillTree Tree)
        {
            float AreaStats;
            ushort NodeID;
            SkillNode CurrentNode;
            JewelItem CurrentJewelData;
            int GrandSpectrumTotal = 0;
            int ElemGrandSpectrums = 0;
            int ArmourGrandSpectrums = 0;
            int ManaGrandSpectrums = 0;

            foreach (KeyValuePair<ushort, JewelNodeData> JewelElement in GlobalSettings.JewelInfo)
            {
                NodeID = JewelElement.Key;
                CurrentNode = SkillTree.Skillnodes[NodeID];
                CurrentJewelData = JewelElement.Value.JewelData;
                if (Tree.SkilledNodes.Contains(CurrentNode))
                {
                    if (CurrentJewelData == null)//Jewel Not Equipped
                    {
                        if (CurrentNode.Attributes.ContainsKey(GlobalSettings.FakeIntuitiveLeapSupportAttribute))//Check to make sure Intuitive Leap Effect is removed when jewel is removed
                        {
                            RemoveIntuitiveLeapSupport(CurrentNode);
                        }
                        //if(CurrentNode.Icon != CurrentNode.DefaultJewelImage)
                        //{
                        //    CurrentNode.Icon = CurrentNode.DefaultJewelImage;
                        //}
                        continue;
                    }
                    else
                    {
                        //Add Attributes from Jewel into AttributeTotal
                        foreach (var attrMod in CurrentJewelData.Mods)
                        {
                            if (attrMod.Attribute.Contains("Elemental Damage per Grand Spectrum"))
                            {
                                if (GrandSpectrumTotal < 3)
                                {
                                    ElemGrandSpectrums++;
                                    GrandSpectrumTotal++;
                                }
                            }
                            else if (attrMod.Attribute.Contains("Armour per Grand Spectrum"))
                            {
                                if (GrandSpectrumTotal < 3)
                                {
                                    ArmourGrandSpectrums++;
                                    GrandSpectrumTotal++;
                                }
                            }
                            else if (attrMod.Attribute.Contains("Mana per Grand Spectrum"))
                            {
                                if (GrandSpectrumTotal < 3)
                                {
                                    ManaGrandSpectrums++;
                                    GrandSpectrumTotal++;
                                }
                            }
                            else
                            {
                                if (attrlist.ContainsKey(attrMod.Attribute))
                                {
                                    if (attrlist[attrMod.Attribute].Count == 1)
                                    {
                                        attrlist[attrMod.Attribute][0] += attrMod.Values[0];
                                    }
                                    for (int Index = 0; Index < attrlist[attrMod.Attribute].Count; ++Index)
                                    {
                                        attrlist[attrMod.Attribute][Index] += attrMod.Values[Index];
                                    }
                                }
                                else
                                {
                                    List<float> TempList = new List<float>();
                                    foreach (var row in attrMod.Values)
                                    {
                                        TempList.Add(row);
                                    }
                                    attrlist.Add(attrMod.Attribute, TempList);
                                }
                            }
                        }
                        //Update Jewel Slot Appearance
                        //    string CurrentJewelIcon = CurrentJewelData.Image.ToString();
                        //    string CurrentNodeIcon = CurrentNode.Icon;
                        //    string CurrentNodeIconKey = CurrentNode.IconKey;
                        //    //CurrentNode.Icon = CurrentJewelIcon;
                        //    //Change Node Image to match Jewel
                        //    //CurrentNode.Icon = CurrentJewelData.Image.ToString();
                    }
                    if (CurrentJewelData.Name == "Intuitive Leap")
                    {
                        if (!CurrentNode.Attributes.ContainsKey(GlobalSettings.FakeIntuitiveLeapSupportAttribute))//Only Apply IntuitiveLeap Area effect once equipped instead of even when only nearby nodes skilled etc
                        {
                            ApplyIntuitiveLeapSupport(CurrentNode);
                        }
                        continue;
                    }
                    else
                    {
                        if (CurrentNode.Attributes.ContainsKey(GlobalSettings.FakeIntuitiveLeapSupportAttribute))//Remove Intuitive Leap Area effect once switched for other jewel
                        {
                            RemoveIntuitiveLeapSupport(CurrentNode);
                        }
                    }
                    if (CurrentJewelData.Name == "Brute Force Solution")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Strength"][0] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Intelligence"))
                            {
                                attrlist["+# to Intelligence"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Intelligence", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fluid Motion")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Strength"][0] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Dexterity"))
                            {
                                attrlist["+# to Dexterity"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Dexterity", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fertile Mind")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Dexterity"][0] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Intelligence"))
                            {
                                attrlist["+# to Intelligence"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Intelligence", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Inertia")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Dexterity"][0] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Strength"))
                            {
                                attrlist["+# to Strength"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Strength", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Efficient Training")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Intelligence"][0] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Strength"))
                            {
                                attrlist["+# to Strength"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Strength", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Careful Planning")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Intelligence"][0] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Dexterity"))
                            {
                                attrlist["+# to Dexterity"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Dexterity", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Energised Armour")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Energy Shield", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["#% increased Energy Shield"][0] -= AreaStats;
                            AreaStats *= 2.0f;
                            if (attrlist.ContainsKey("#% increased Armour"))
                            {
                                attrlist["#% increased Armour"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Armour", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Energy From Within")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Life", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["#% increased Life"][0] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Dexterity"))
                            {
                                attrlist["#% increased Energy Shield"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Energy Shield", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fireborn")
                    {
                        AreaStats = 0.0f;
                        float CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Elemental Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Elemental Damage"][0] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Cold Damage"][0] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Chaos Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Chaos Damage"][0] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Global Physical Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Global Physical Damage"][0] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Lightning Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Lightning Damage"][0] -= CurrentTotal; AreaStats += CurrentTotal; }
                        if (AreaStats != 0.0f)
                        {
                            if (attrlist.ContainsKey("#% increased Fire Damage"))
                            {
                                attrlist["#% increased Fire Damage"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Fire Damage", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Might in All Forms")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes, "Medium");
                        AreaStats += CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes, "Medium");
                        if (AreaStats != 0.0f)
                        {
                            if (attrlist.ContainsKey("#% increased Melee Damage Bonus(from Might in All Forms)"))
                            {
                                attrlist["#% increased Melee Damage Bonus(from Might in All Forms)"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Melee Damage Bonus(from Might in All Forms)", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Anatomical Knowledge")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes, "Large");
                        AreaStats /= 3.0f;
                        if (AreaStats != 0.0f)
                        {
                            if (attrlist.ContainsKey("+# to maximum Life"))
                            {
                                attrlist["+# to maximum Life"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to maximum Life", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Cold Steel")
                    {
                        float ColdDamageTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage", Tree.SkilledNodes);
                        if (ColdDamageTotal != 0) { attrlist["#% increased Cold Damage"][0] -= ColdDamageTotal; }
                        float ColdDamageTotalAttacks = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage with Attack Skills", Tree.SkilledNodes);
                        if (ColdDamageTotalAttacks != 0) { attrlist["#% increased Cold Damage with Attack Skills"][0] -= ColdDamageTotalAttacks; }

                        float PhysDamageTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Global Physical Damage", Tree.SkilledNodes);
                        if (PhysDamageTotal != 0) { attrlist["#% increased Global Physical Damage"][0] -= PhysDamageTotal; }

                        float PhysDamageTotalTwoWeap = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Two Handed Melee Weapons", Tree.SkilledNodes);
                        if (PhysDamageTotalTwoWeap != 0) { attrlist["#% increased Physical Damage with Two Handed Melee Weapons"][0] -= PhysDamageTotalTwoWeap; }
                        float PhysDamageTotalOneWeap = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with One Handed Melee Weapons", Tree.SkilledNodes);
                        if (PhysDamageTotalOneWeap != 0) { attrlist["#% increased Physical Damage with One Handed Melee Weapons"][0] -= PhysDamageTotalOneWeap; }
                        float PhysDamageTotalDual = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Weapon Damage while Dual Wielding", Tree.SkilledNodes);
                        if (PhysDamageTotalDual != 0) { attrlist["#% increased Physical Weapon Damage while Dual Wielding"][0] -= PhysDamageTotalDual; }
                        float PhysDamageMelee = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Melee Physical Damage", Tree.SkilledNodes);
                        if (PhysDamageMelee != 0) { attrlist["#% increased Melee Physical Damage"][0] -= PhysDamageMelee; }
                        float PhysDamageShield = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Melee Physical Damage while holding a Shield", Tree.SkilledNodes);
                        if (PhysDamageShield != 0) { attrlist["#% increased Physical Damage while holding a Shield"][0] -= PhysDamageShield; }
                        float PhysDamageMace = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Maces", Tree.SkilledNodes);
                        if (PhysDamageMace != 0) { attrlist["#% increased Physical Damage with Maces"][0] -= PhysDamageMace; }
                        float PhysDamageStaves = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Staves", Tree.SkilledNodes);
                        if (PhysDamageStaves != 0) { attrlist["#% increased Physical Damage with Staves"][0] -= PhysDamageMace; }
                        float PhysDamageSwords = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Swords", Tree.SkilledNodes);
                        if (PhysDamageSwords != 0) { attrlist["#% increased Physical Damage with Swords"][0] -= PhysDamageSwords; }
                        float PhysDamageDaggers = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Daggers", Tree.SkilledNodes);
                        if (PhysDamageDaggers != 0) { attrlist["#% increased Physical Damage with Daggers"][0] -= PhysDamageDaggers; }
                        float PhysDamageClaws = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Claws", Tree.SkilledNodes);
                        if (PhysDamageClaws != 0) { attrlist["#% increased Physical Damage with Claws"][0] -= PhysDamageClaws; }
                        float PhysDamageBows = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Bows", Tree.SkilledNodes);
                        if (PhysDamageBows != 0) { attrlist["#% increased Physical Damage with Bows"][0] -= PhysDamageBows; }

                        if (ColdDamageTotal != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Global Physical Damage"))
                            {
                                attrlist["#% increased Global Physical Damage"][0] += ColdDamageTotal;
                            }
                            else
                            {
                                attrlist.Add("#% increased Global Physical Damage", new List<float>(1) { ColdDamageTotal });
                            }
                        }
                        if (PhysDamageTotal != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage"))
                            {
                                attrlist["#% increased Cold Damage"][0] += PhysDamageTotal;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage", new List<float>(1) { PhysDamageTotal });
                            }
                        }
                        if (ColdDamageTotalAttacks != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Physical Damage with Attack Skills"))
                            {
                                attrlist["#% increased Physical Damage with Attack Skills"][0] += ColdDamageTotalAttacks;
                            }
                            else
                            {
                                attrlist.Add("#% increased Physical Damage with Attack Skills", new List<float>(1) { ColdDamageTotalAttacks });
                            }
                        }
                        if (PhysDamageTotalTwoWeap != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Two Handed Melee Weapons"))
                            {
                                attrlist["#% increased Cold Damage with Two Handed Melee Weapons"][0] += PhysDamageTotalTwoWeap;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Two Handed Melee Weapons", new List<float>(1) { PhysDamageTotalTwoWeap });
                            }
                        }
                        if (PhysDamageTotalOneWeap != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with One Handed Melee Weapons"))
                            {
                                attrlist["#% increased Cold Damage with One Handed Melee Weapons"][0] += PhysDamageTotalOneWeap;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with One Handed Melee Weapons", new List<float>(1) { PhysDamageTotalOneWeap });
                            }
                        }
                        if (PhysDamageTotalDual != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Weapon Damage while Dual Wielding"))
                            {
                                attrlist["#% increased Cold Weapon Damage while Dual Wielding"][0] += PhysDamageTotalDual;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Weapon Damage while Dual Wielding", new List<float>(1) { PhysDamageTotalDual });
                            }
                        }
                        if (PhysDamageMelee != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Melee Cold Damage"))
                            {
                                attrlist["#% increased Melee Cold Damage"][0] += PhysDamageMelee;
                            }
                            else
                            {
                                attrlist.Add("#% increased Melee Cold Damage", new List<float>(1) { PhysDamageMelee });
                            }
                        }
                        if (PhysDamageShield != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage while holding a Shield"))
                            {
                                attrlist["#% increased Cold Damage while holding a Shield"][0] += PhysDamageShield;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage while holding a Shield", new List<float>(1) { PhysDamageShield });
                            }
                        }
                        if (PhysDamageMace != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Maces"))
                            {
                                attrlist["#% increased Cold Damage with Maces"][0] += PhysDamageMace;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Maces", new List<float>(1) { PhysDamageMace });
                            }
                        }
                        if (PhysDamageStaves != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Staves"))
                            {
                                attrlist["#% increased Cold Damage with Staves"][0] += PhysDamageStaves;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Staves", new List<float>(1) { PhysDamageStaves });
                            }
                        }
                        if (PhysDamageSwords != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Swords"))
                            {
                                attrlist["#% increased Cold Damage with Swords"][0] += PhysDamageSwords;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Swords", new List<float>(1) { PhysDamageSwords });
                            }
                        }
                        if (PhysDamageDaggers != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Daggers"))
                            {
                                attrlist["#% increased Cold Damage with Daggers"][0] += PhysDamageDaggers;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Daggers", new List<float>(1) { PhysDamageDaggers });
                            }
                        }
                        if (PhysDamageClaws != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Claws"))
                            {
                                attrlist["#% increased Cold Damage with Claws"][0] += PhysDamageClaws;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Claws", new List<float>(1) { PhysDamageClaws });
                            }
                        }
                        if (PhysDamageBows != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Bows"))
                            {
                                attrlist["#% increased Cold Damage with Bows"][0] += PhysDamageBows;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Bows", new List<float>(1) { PhysDamageBows });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Transcendent Mind")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        AreaStats /= 10;
                        AreaStats = (int)AreaStats;
                        if (AreaStats != 0)
                        {
                            AreaStats *= 0.4f;
                            if (attrlist.ContainsKey("+#% to Energy Shield Regenerated Per Second"))
                            {
                                attrlist["+#% to Energy Shield Regenerated Per Second"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+#% to Energy Shield Regenerated Per Second", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Transcendent Flesh")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        AreaStats /= 10;
                        AreaStats = (int)AreaStats;
                        if (AreaStats != 0)
                        {
                            if (attrlist.ContainsKey("+# additional Physical Damage Reduction"))
                            {
                                attrlist["+# additional Physical Damage Reduction"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# additional Physical Damage Reduction", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Transcendent Mind")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        AreaStats /= 10;
                        AreaStats = (int)AreaStats;
                        if (AreaStats != 0)
                        {
                            AreaStats *= 2f;
                            if (attrlist.ContainsKey("+#% increased Movement Speed"))
                            {
                                attrlist["+#% increased Movement Speed"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+#% increased Movement Speed", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Tempered Spirit")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Dexterity"][0] -= AreaStats;
                            AreaStats = CalculateTotalUnallocAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                            AreaStats /= 10;
                            AreaStats = (int)AreaStats;
                            AreaStats *= 15;
                            if (attrlist.ContainsKey("+# to Maximum Mana"))
                            {
                                attrlist["+# to Maximum Mana"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Maximum Mana", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Tempered Flesh")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Strength"][0] -= AreaStats;
                            AreaStats = CalculateTotalUnallocAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                            AreaStats /= 10;
                            AreaStats = (int)AreaStats;
                            AreaStats *= 5;
                            if (attrlist.ContainsKey("+#% to Critical Strike Multiplier"))
                            {
                                attrlist["+#% to Critical Strike Multiplier"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+#% to Critical Strike Multiplier", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Tempered Mind")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Intelligence"][0] -= AreaStats;
                            AreaStats = CalculateTotalUnallocAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                            AreaStats /= 10;
                            AreaStats = (int)AreaStats;
                            AreaStats *= 100;
                            if (attrlist.ContainsKey("+# to Accuracy Rating"))
                            {
                                attrlist["+# to Accuracy Rating"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Accuracy Rating", new List<float>(1) { AreaStats });
                            }
                        }
                    }
                }
                else//SkillNode not active
                {
                    if (CurrentNode.Attributes.ContainsKey(GlobalSettings.FakeIntuitiveLeapSupportAttribute))//Check to make sure Intuitive Leap Effect is removed when jewel is removed
                    {
                        RemoveIntuitiveLeapSupport(CurrentNode);
                    }
                }
            }

            if (GrandSpectrumTotal != 0)
            {
                float Total;
                if (ElemGrandSpectrums > 0)
                {
                    Total = 12.0f * ElemGrandSpectrums;
                    Total *= GrandSpectrumTotal;
                    if (attrlist.ContainsKey("#% increased Elemental Damage"))
                    {
                        attrlist["#% increased Elemental Damage"][0] += Total;
                    }
                    else
                    {
                        attrlist.Add("#% increased Elemental Damage", new List<float>(1) { Total });
                    }
                }
                if (ArmourGrandSpectrums > 0)
                {
                    Total = 200.0f * ArmourGrandSpectrums;
                    Total *= GrandSpectrumTotal;
                    if (attrlist.ContainsKey("+# to Armour"))
                    {
                        attrlist["+# to Armour"][0] += Total;
                    }
                    else
                    {
                        attrlist.Add("+# to Armour", new List<float>(1) { Total });
                    }
                }
                if (ManaGrandSpectrums > 0)
                {
                    Total = 30.0f * ManaGrandSpectrums;
                    Total *= GrandSpectrumTotal;
                    if (attrlist.ContainsKey("+# to maximum Mana"))
                    {
                        attrlist["+# to maximum Mana"][0] += Total;
                    }
                    else
                    {
                        attrlist.Add("+# to maximum Mana", new List<float>(1) { Total });
                    }
                }
            }
            float Subtotal = 0.0f;
            float TotalIncrease = 0.0f;
            if (attrlist.ContainsKey("+# Accuracy Rating"))
            {
                Subtotal += attrlist["+# Accuracy Rating"][0];
            }
            if (attrlist.ContainsKey("+# to Accuracy Rating"))
            {
                Subtotal += attrlist["+# to Accuracy Rating"][0];
            }
            if (attrlist.ContainsKey("#% increased Accuracy Rating with Wands"))
            {
                TotalIncrease += attrlist["#% increased Accuracy Rating with Wands"][0];
            }
            if (attrlist.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
            {
                TotalIncrease += attrlist["#% increased Accuracy Rating while Dual Wielding"][0];
            }
            if (attrlist.ContainsKey("#% increased Global Accuracy Rating"))
            {
                TotalIncrease += attrlist["#% increased Global Accuracy Rating"][0];
            }
            if (TotalIncrease!=0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease) / 100.0f;
                Subtotal *= TotalIncrease;
            }
            if (attrlist.ContainsKey(GlobalSettings.DualWandAccKey))//"# Accuracy Subtotal"
            {
                attrlist[GlobalSettings.DualWandAccKey][0] = Subtotal;
            }
            else
            {
                attrlist.Add(GlobalSettings.DualWandAccKey, new List<float>(1) { Subtotal });
            }
            //MaxLife combined with increased life
            Subtotal = 0.0f;
            TotalIncrease = 0.0f;
            if (attrlist.ContainsKey("+# to maximum Life"))
            {
                Subtotal = attrlist["+# to maximum Life"][0];
            }
            if (attrlist.ContainsKey("#% increased maximum Life"))
            {
                TotalIncrease = attrlist["#% increased maximum Life"][0];
            }
            if (TotalIncrease != 0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease) / 100.0f;
                Subtotal *= TotalIncrease;
            }
            if (attrlist.ContainsKey(GlobalSettings.HPTotalKey))
            {
                attrlist[GlobalSettings.HPTotalKey][0] = Subtotal;
            }
            else
            {
                attrlist.Add(GlobalSettings.HPTotalKey, new List<float>(1) { Subtotal });
            }
            float ESSubtotal = 0.0f;
            float ESIncrease = 0.0f;
            if (attrlist.ContainsKey("+# to maximum Energy Shield"))
            {
                ESSubtotal = attrlist["+# to maximum Energy Shield"][0];
            }
            if (attrlist.ContainsKey("#% increased maximum Energy Shield"))
            {
                ESIncrease = attrlist["#% increased maximum Energy Shield"][0];
            }
            if (attrlist.ContainsKey("+# to Intelligence"))
            {
                ESIncrease += attrlist["+# to Intelligence"][0] / 10.0f;
            }
            if (ESIncrease != 0.0f)
            {
                ESIncrease = (100.0f + ESIncrease) / 100.0f;
                ESSubtotal *= ESIncrease;
            }
            Subtotal += ESSubtotal;
            if (attrlist.ContainsKey(GlobalSettings.HybridHPKey))
            {
                attrlist[GlobalSettings.HybridHPKey][0] = Subtotal;
            }
            else
            {
                attrlist.Add(GlobalSettings.HybridHPKey, new List<float>(1) { Subtotal });
            }
            return attrlist;
        }

        /// <summary>
        /// Updates stats based on Unique Jewels Slotted
        /// </summary>
        /// <param name="attrlist">The attrlist.</param>
        /// <param name="Tree">The tree.</param>
        /// <param name="InvModel">The item information sent from SkillTreeGenerator.</param>
        /// <returns>Dictionary&lt;System.String, System.Single&gt;</returns>
        public Dictionary<string, float> StatUpdater(Dictionary<string, float> attrlist, SkillTree Tree, InventoryViewModel InvModel = null)
        {
            float AreaStats;
            ushort NodeID;
            SkillNode CurrentNode;
            JewelItem CurrentJewelData;
            int GrandSpectrumTotal = 0;
            int ElemGrandSpectrums = 0;
            int ArmourGrandSpectrums = 0;
            int ManaGrandSpectrums = 0;

            foreach (KeyValuePair<ushort, JewelNodeData> JewelElement in GlobalSettings.JewelInfo)
            {
                NodeID = JewelElement.Key;
                CurrentNode = SkillTree.Skillnodes[NodeID];
                CurrentJewelData = JewelElement.Value.JewelData;
                if (Tree.SkilledNodes.Contains(CurrentNode))
                {
                    if (CurrentJewelData == null)//Jewel Not Equipped
                    {
                        continue;
                    }
                    else
                    {
                        //Add Attributes from Jewel into AttributeTotal (Only allow single attribute Mods)
                        foreach (var attrMod in CurrentJewelData.Mods)
                        {
                            if (attrMod.Attribute.Contains("Elemental Damage per Grand Spectrum"))
                            {
                                if (GrandSpectrumTotal < 3)
                                {
                                    ElemGrandSpectrums++;
                                    GrandSpectrumTotal++;
                                }
                            }
                            else if (attrMod.Attribute.Contains("Armour per Grand Spectrum"))
                            {
                                if (GrandSpectrumTotal < 3)
                                {
                                    ArmourGrandSpectrums++;
                                    GrandSpectrumTotal++;
                                }
                            }
                            else if (attrMod.Attribute.Contains("Mana per Grand Spectrum"))
                            {
                                if (GrandSpectrumTotal < 3)
                                {
                                    ManaGrandSpectrums++;
                                    GrandSpectrumTotal++;
                                }
                            }
                            else
                            {
                                if (attrlist.ContainsKey(attrMod.Attribute))
                                {
                                    attrlist[attrMod.Attribute] += attrMod.Values[0];
                                }
                                else
                                {
                                    if (attrMod.Values.Count == 1)
                                    {
                                        attrlist.Add(attrMod.Attribute, attrMod.Values[0]);
                                    }
                                    else if (attrMod.Values.Count == 0)//Treat non-Value Attributes as value of one
                                    {
                                        string ModifiedName = attrMod.Attribute + " (#)";
                                        if (attrlist.ContainsKey(attrMod.Attribute))
                                        {
                                            attrlist[ModifiedName] += 1;
                                        }
                                        else
                                        {
                                            attrlist.Add(ModifiedName, 1);
                                        }
                                    }
                                    //List<float> TempList = new List<float>();
                                    //foreach (var row in attrMod.Values)
                                    //{
                                    //	TempList.Add(row);
                                    //}
                                    //attrlist.Add(attrMod.Attribute, TempList);
                                }
                            }
                        }
                    }
                    if (CurrentJewelData.Name == "Intuitive Leap") { }
                    else if (CurrentJewelData.Name == "Brute Force Solution")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Strength"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Intelligence"))
                            {
                                attrlist["+# to Intelligence"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Intelligence", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fluid Motion")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Strength"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Dexterity"))
                            {
                                attrlist["+# to Dexterity"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Dexterity", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fertile Mind")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Dexterity"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Intelligence"))
                            {
                                attrlist["+# to Intelligence"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Intelligence", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Inertia")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Dexterity"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Strength"))
                            {
                                attrlist["+# to Strength"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Strength", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Efficient Training")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Intelligence"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Strength"))
                            {
                                attrlist["+# to Strength"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Strength", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Careful Planning")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Intelligence"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Dexterity"))
                            {
                                attrlist["+# to Dexterity"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Dexterity", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Energised Armour")
                    {//
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Energy Shield", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["#% increased Energy Shield"] -= AreaStats;
                            AreaStats *= 2.0f;
                            if (attrlist.ContainsKey("#% increased Armour"))
                            {
                                attrlist["#% increased Armour"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Armour", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Energy From Within")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Life", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["#% increased Life"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Dexterity"))
                            {
                                attrlist["#% increased Energy Shield"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Energy Shield", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fireborn")
                    {
                        AreaStats = 0.0f;
                        float CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Elemental Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Elemental Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Cold Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Chaos Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Chaos Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Global Physical Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Global Physical Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Lightning Damage", Tree.SkilledNodes, "Medium");
                        if (CurrentTotal != 0) { attrlist["#% increased Lightning Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        if (AreaStats != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Fire Damage"))
                            {
                                attrlist["#% increased Fire Damage"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Fire Damage", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Might in All Forms")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes, "Medium");
                        AreaStats += CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes, "Medium");
                        if (AreaStats != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Melee Damage Bonus(from Might in All Forms)"))
                            {
                                attrlist["#% increased Melee Damage Bonus(from Might in All Forms)"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Melee Damage Bonus(from Might in All Forms)", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Anatomical Knowledge")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes, "Large");
                        AreaStats /= 3.0f;
                        if (AreaStats != 0.0f)
                        {
                            if (attrlist.ContainsKey("+# to maximum Life"))
                            {
                                attrlist["+# to maximum Life"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to maximum Life", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Cold Steel")
                    {
                        float ColdDamageTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage", Tree.SkilledNodes);
                        if (ColdDamageTotal != 0) { attrlist["#% increased Cold Damage"] -= ColdDamageTotal; }
                        float ColdDamageTotalAttacks = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage with Attack Skills", Tree.SkilledNodes);
                        if (ColdDamageTotalAttacks != 0) { attrlist["#% increased Cold Damage with Attack Skills"] -= ColdDamageTotalAttacks; }

                        float PhysDamageTotal = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Global Physical Damage", Tree.SkilledNodes);
                        if (PhysDamageTotal != 0) { attrlist["#% increased Global Physical Damage"] -= PhysDamageTotal; }

                        float PhysDamageTotalTwoWeap = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Two Handed Melee Weapons", Tree.SkilledNodes);
                        if (PhysDamageTotalTwoWeap != 0) { attrlist["#% increased Physical Damage with Two Handed Melee Weapons"] -= PhysDamageTotalTwoWeap; }
                        float PhysDamageTotalOneWeap = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with One Handed Melee Weapons", Tree.SkilledNodes);
                        if (PhysDamageTotalOneWeap != 0) { attrlist["#% increased Physical Damage with One Handed Melee Weapons"] -= PhysDamageTotalOneWeap; }
                        float PhysDamageTotalDual = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Weapon Damage while Dual Wielding", Tree.SkilledNodes);
                        if (PhysDamageTotalDual != 0) { attrlist["#% increased Physical Weapon Damage while Dual Wielding"] -= PhysDamageTotalDual; }
                        float PhysDamageMelee = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Melee Physical Damage", Tree.SkilledNodes);
                        if (PhysDamageMelee != 0) { attrlist["#% increased Melee Physical Damage"] -= PhysDamageMelee; }
                        float PhysDamageShield = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Melee Physical Damage while holding a Shield", Tree.SkilledNodes);
                        if (PhysDamageShield != 0) { attrlist["#% increased Physical Damage while holding a Shield"] -= PhysDamageShield; }
                        float PhysDamageMace = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Maces", Tree.SkilledNodes);
                        if (PhysDamageMace != 0) { attrlist["#% increased Physical Damage with Maces"] -= PhysDamageMace; }
                        float PhysDamageStaves = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Staves", Tree.SkilledNodes);
                        if (PhysDamageStaves != 0) { attrlist["#% increased Physical Damage with Staves"] -= PhysDamageMace; }
                        float PhysDamageSwords = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Swords", Tree.SkilledNodes);
                        if (PhysDamageSwords != 0) { attrlist["#% increased Physical Damage with Swords"] -= PhysDamageSwords; }
                        float PhysDamageDaggers = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Daggers", Tree.SkilledNodes);
                        if (PhysDamageDaggers != 0) { attrlist["#% increased Physical Damage with Daggers"] -= PhysDamageDaggers; }
                        float PhysDamageClaws = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Claws", Tree.SkilledNodes);
                        if (PhysDamageClaws != 0) { attrlist["#% increased Physical Damage with Claws"] -= PhysDamageClaws; }
                        float PhysDamageBows = CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Bows", Tree.SkilledNodes);
                        if (PhysDamageBows != 0) { attrlist["#% increased Physical Damage with Bows"] -= PhysDamageBows; }

                        if (ColdDamageTotal != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Global Physical Damage"))
                            {
                                attrlist["#% increased Global Physical Damage"] += ColdDamageTotal;
                            }
                            else
                            {
                                attrlist.Add("#% increased Global Physical Damage", ColdDamageTotal);
                            }
                        }
                        if (PhysDamageTotal != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage"))
                            {
                                attrlist["#% increased Cold Damage"] += PhysDamageTotal;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage", PhysDamageTotal);
                            }
                        }
                        if (ColdDamageTotalAttacks != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Physical Damage with Attack Skills"))
                            {
                                attrlist["#% increased Physical Damage with Attack Skills"] += ColdDamageTotalAttacks;
                            }
                            else
                            {
                                attrlist.Add("#% increased Physical Damage with Attack Skills", ColdDamageTotalAttacks);
                            }
                        }
                        if (PhysDamageTotalTwoWeap != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Two Handed Melee Weapons"))
                            {
                                attrlist["#% increased Cold Damage with Two Handed Melee Weapons"] += PhysDamageTotalTwoWeap;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Two Handed Melee Weapons", PhysDamageTotalTwoWeap);
                            }
                        }
                        if (PhysDamageTotalOneWeap != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with One Handed Melee Weapons"))
                            {
                                attrlist["#% increased Cold Damage with One Handed Melee Weapons"] += PhysDamageTotalOneWeap;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with One Handed Melee Weapons", PhysDamageTotalOneWeap);
                            }
                        }
                        if (PhysDamageTotalDual != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Weapon Damage while Dual Wielding"))
                            {
                                attrlist["#% increased Cold Weapon Damage while Dual Wielding"] += PhysDamageTotalDual;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Weapon Damage while Dual Wielding", PhysDamageTotalDual);
                            }
                        }
                        if (PhysDamageMelee != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Melee Cold Damage"))
                            {
                                attrlist["#% increased Melee Cold Damage"] += PhysDamageMelee;
                            }
                            else
                            {
                                attrlist.Add("#% increased Melee Cold Damage", PhysDamageMelee);
                            }
                        }
                        if (PhysDamageShield != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage while holding a Shield"))
                            {
                                attrlist["#% increased Cold Damage while holding a Shield"] += PhysDamageShield;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage while holding a Shield", PhysDamageShield);
                            }
                        }
                        if (PhysDamageMace != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Maces"))
                            {
                                attrlist["#% increased Cold Damage with Maces"] += PhysDamageMace;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Maces", PhysDamageMace);
                            }
                        }
                        if (PhysDamageStaves != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Staves"))
                            {
                                attrlist["#% increased Cold Damage with Staves"] += PhysDamageStaves;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Staves", PhysDamageStaves);
                            }
                        }
                        if (PhysDamageSwords != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Swords"))
                            {
                                attrlist["#% increased Cold Damage with Swords"] += PhysDamageSwords;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Swords", PhysDamageSwords);
                            }
                        }
                        if (PhysDamageDaggers != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Daggers"))
                            {
                                attrlist["#% increased Cold Damage with Daggers"] += PhysDamageDaggers;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Daggers", PhysDamageDaggers);
                            }
                        }
                        if (PhysDamageClaws != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Claws"))
                            {
                                attrlist["#% increased Cold Damage with Claws"] += PhysDamageClaws;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Claws", PhysDamageClaws);
                            }
                        }
                        if (PhysDamageBows != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Cold Damage with Bows"))
                            {
                                attrlist["#% increased Cold Damage with Bows"] += PhysDamageBows;
                            }
                            else
                            {
                                attrlist.Add("#% increased Cold Damage with Bows", PhysDamageBows);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Transcendent Mind")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        AreaStats /= 10;
                        AreaStats = (int)AreaStats;
                        if (AreaStats != 0)
                        {
                            AreaStats *= 0.4f;
                            if (attrlist.ContainsKey("+#% to Energy Shield Regenerated Per Second"))
                            {
                                attrlist["+#% to Energy Shield Regenerated Per Second"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+#% to Energy Shield Regenerated Per Second", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Transcendent Flesh")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        AreaStats /= 10;
                        AreaStats = (int)AreaStats;
                        if (AreaStats != 0)
                        {
                            if (attrlist.ContainsKey("+# additional Physical Damage Reduction"))
                            {
                                attrlist["+# additional Physical Damage Reduction"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# additional Physical Damage Reduction", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Transcendent Mind")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        AreaStats /= 10;
                        AreaStats = (int)AreaStats;
                        if (AreaStats != 0)
                        {
                            AreaStats *= 2f;
                            if (attrlist.ContainsKey("+#% increased Movement Speed"))
                            {
                                attrlist["+#% increased Movement Speed"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+#% increased Movement Speed", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Tempered Spirit")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Dexterity"] -= AreaStats;
                            AreaStats = CalculateTotalUnallocAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                            AreaStats /= 10;
                            AreaStats = (int)AreaStats;
                            AreaStats *= 15;
                            if (attrlist.ContainsKey("+# to Maximum Mana"))
                            {
                                attrlist["+# to Maximum Mana"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Maximum Mana", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Tempered Flesh")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Strength"] -= AreaStats;
                            AreaStats = CalculateTotalUnallocAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                            AreaStats /= 10;
                            AreaStats = (int)AreaStats;
                            AreaStats *= 5;
                            if (attrlist.ContainsKey("+#% to Critical Strike Multiplier"))
                            {
                                attrlist["+#% to Critical Strike Multiplier"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+#% to Critical Strike Multiplier", AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Tempered Mind")
                    {
                        AreaStats = CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Intelligence"] -= AreaStats;
                            AreaStats = CalculateTotalUnallocAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                            AreaStats /= 10;
                            AreaStats = (int)AreaStats;
                            AreaStats *= 100;
                            if (attrlist.ContainsKey("+# to Accuracy Rating"))
                            {
                                attrlist["+# to Accuracy Rating"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Accuracy Rating", AreaStats);
                            }
                        }
                    }
                }
            }

            if (GrandSpectrumTotal != 0)
            {
                float Total;
                if (ElemGrandSpectrums > 0)
                {
                    Total = 12.0f * ElemGrandSpectrums;
                    Total *= GrandSpectrumTotal;
                    if (attrlist.ContainsKey("#% increased Elemental Damage"))
                    {
                        attrlist["#% increased Elemental Damage"] += Total;
                    }
                    else
                    {
                        attrlist.Add("#% increased Elemental Damage", Total);
                    }
                }
                if (ArmourGrandSpectrums > 0)
                {
                    Total = 200.0f * ArmourGrandSpectrums;
                    Total *= GrandSpectrumTotal;
                    if (attrlist.ContainsKey("+# to Armour"))
                    {
                        attrlist["+# to Armour"] += Total;
                    }
                    else
                    {
                        attrlist.Add("+# to Armour", Total);
                    }
                }
                if (ManaGrandSpectrums > 0)
                {
                    Total = 30.0f * ManaGrandSpectrums;
                    Total *= GrandSpectrumTotal;
                    if (attrlist.ContainsKey("+# to maximum Mana"))
                    {
                        attrlist["+# to maximum Mana"] += Total;
                    }
                    else
                    {
                        attrlist.Add("+# to maximum Mana", Total);
                    }
                }
            }

            //Calculate Equipment Stats+Jewels on SkilledNodes
            if (InvModel != null)
            {
                Dictionary<string, float> ItemDictionary = new Dictionary<string, float>();
                POESKillTree.Model.Items.Item ItemData;
                bool ContinueCalc = true;
                for (int Index = 0; ContinueCalc; ++Index)
                {
                    switch (Index)
                    {
                        case 0:
                            ItemData = InvModel.Armor.Item; break;
                        case 1:
                            ItemData = InvModel.MainHand.Item; break;
                        case 2:
                            ItemData = InvModel.OffHand.Item; break;
                        case 3:
                            ItemData = InvModel.Ring.Item; break;
                        case 4:
                            ItemData = InvModel.Ring2.Item; break;
                        case 5:
                            ItemData = InvModel.Amulet.Item; break;
                        case 6:
                            ItemData = InvModel.Helm.Item; break;
                        case 7:
                            ItemData = InvModel.Gloves.Item; break;
                        case 8:
                            ItemData = InvModel.Boots.Item; break;
                        case 9:
                            ItemData = InvModel.Belt.Item; break;
                        default:
                            JewelItem JewelItemData;
                            foreach (KeyValuePair<ushort, JewelNodeData> JewelSlotData in this)
                            {
                                NodeID = JewelSlotData.Key;
                                CurrentNode = SkillTree.Skillnodes[NodeID];
                                JewelItemData = JewelSlotData.Value.JewelData;
                                if (JewelItemData != null&&Tree.SkilledNodes.Contains(CurrentNode))
                                {
                                    foreach (var TargetMod in JewelItemData.Mods)
                                    {
                                        if (TargetMod.Values.Count == 1)//Only single value Mods added to dictionary for solver use
                                        {
                                            if (ItemDictionary.ContainsKey(TargetMod.Attribute))
                                            {
                                                ItemDictionary[TargetMod.Attribute] += TargetMod.Values[0];
                                            }
                                            else
                                            {
                                                ItemDictionary.Add(TargetMod.Attribute, TargetMod.Values[0]);
                                            }
                                        }
                                    }
                                }
                            }
                            ItemData = null; ContinueCalc = false; break;
                    }
                    if (ItemData != null)
                    {
                        foreach (var TargetMod in ItemData.Mods)
                        {
                            if (TargetMod.Values.Count == 1)//Only single value Mods added to dictionary for solver use
                            {
                                if (ItemDictionary.ContainsKey(TargetMod.Attribute))
                                {
                                    ItemDictionary[TargetMod.Attribute] += TargetMod.Values[0];
                                }
                                else
                                {
                                    ItemDictionary.Add(TargetMod.Attribute, TargetMod.Values[0]);
                                }
                            }
                        }
                    }
                }
                string StatName;
                foreach (var StatElement in ItemDictionary)
                {
                    StatName = StatElement.Key;
                    if (attrlist.ContainsKey(StatName))
                    {
                        attrlist[StatName] += StatElement.Value;
                    }
                    else
                    {
                        attrlist.Add(StatName, StatElement.Value);
                    }
                }
            }

            float Subtotal = 0.0f;
            float TotalIncrease = 0.0f;
            if (attrlist.ContainsKey("+# Accuracy Rating"))
            {
                Subtotal += attrlist["+# Accuracy Rating"];
            }
            if (attrlist.ContainsKey("+# to Accuracy Rating"))
            {
                Subtotal += attrlist["+# to Accuracy Rating"];
            }
            if (attrlist.ContainsKey("#% increased Accuracy Rating with Wands"))
            {
                TotalIncrease += attrlist["#% increased Accuracy Rating with Wands"];
            }
            if (attrlist.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
            {
                TotalIncrease += attrlist["#% increased Accuracy Rating while Dual Wielding"];
            }
            if (attrlist.ContainsKey("#% increased Global Accuracy Rating"))
            {
                TotalIncrease += attrlist["#% increased Global Accuracy Rating"];
            }
            if (TotalIncrease != 0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease)/100;
                Subtotal *= TotalIncrease;
            }
            if (attrlist.ContainsKey(GlobalSettings.DualWandAccKey))//"# Accuracy Subtotal"
            {
                attrlist[GlobalSettings.DualWandAccKey] = Subtotal;
            }
            else
            {
                attrlist.Add(GlobalSettings.DualWandAccKey, Subtotal);
            }
            //MaxLife combined with increased life
            Subtotal = 0.0f;
            TotalIncrease = 0.0f;
            if (attrlist.ContainsKey("+# to maximum Life"))
            {
                Subtotal = attrlist["+# to maximum Life"];
            }
            if (attrlist.ContainsKey("#% increased maximum Life"))
            {
                TotalIncrease = attrlist["#% increased maximum Life"];
            }
            if (TotalIncrease != 0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease) / 100.0f;
                Subtotal *= TotalIncrease;
            }
            if (attrlist.ContainsKey(GlobalSettings.HPTotalKey))
            {
                attrlist[GlobalSettings.HPTotalKey] = Subtotal;
            }
            else
            {
                attrlist.Add(GlobalSettings.HPTotalKey, Subtotal);
            }
            float ESSubtotal = 0.0f;
            float ESIncrease = 0.0f;
            if (attrlist.ContainsKey("+# to maximum Energy Shield"))
            {
                ESSubtotal = attrlist["+# to maximum Energy Shield"];
            }
            if (attrlist.ContainsKey("#% increased maximum Energy Shield"))
            {
                ESIncrease = attrlist["#% increased maximum Energy Shield"];
            }
            if (attrlist.ContainsKey("+# to Intelligence"))
            {
                ESIncrease += attrlist["+# to Intelligence"] / 10.0f;
            }
            if (ESIncrease != 0.0f)
            {
                ESIncrease = (100.0f + ESIncrease) / 100.0f;
                ESSubtotal *= ESIncrease;
            }
            Subtotal += ESSubtotal;
            if (attrlist.ContainsKey(GlobalSettings.HybridHPKey))
            {
                attrlist[GlobalSettings.HybridHPKey] = Subtotal;
            }
            else
            {
                attrlist.Add(GlobalSettings.HybridHPKey, Subtotal);
            }
            return attrlist;
        }

    //    /// <summary>
    //    /// Calculates the total single attributes on Equipment.
    //    /// </summary>
    //    /// <returns></returns>
    //    private Dictionary<string, float> CalculateTotalSingleAttributes(InventoryViewModel InvModel)
    //    {
    //        Dictionary<string, float> ItemDictionary = new Dictionary<string, float>();
    //        POESKillTree.Model.Items.Item ItemData;
    //        bool ContinueCalc = true;
    //        for (int Index = 0; ContinueCalc; ++Index)
    //        {
    //            switch (Index)
    //            {
    //                case 0:
    //                    ItemData = InvModel.Armor.Item; break;
    //                case 1:
    //                    ItemData = InvModel.MainHand.Item; break;
    //                case 2:
    //                    ItemData = InvModel.OffHand.Item; break;
    //                case 3:
    //                    ItemData = InvModel.Ring.Item; break;
    //                case 4:
    //                    ItemData = InvModel.Ring2.Item; break;
    //                case 5:
    //                    ItemData = InvModel.Amulet.Item; break;
    //                case 6:
    //                    ItemData = InvModel.Helm.Item; break;
    //                case 7:
    //                    ItemData = InvModel.Gloves.Item; break;
    //                case 8:
    //                    ItemData = InvModel.Boots.Item; break;
    //                case 9:
    //                    ItemData = InvModel.Belt.Item; break;
    //                default:
    //                    JewelItem JewelItemData;
    //                    foreach (KeyValuePair<ushort, JewelNodeData> JewelSlotData in this)
    //                    {
    //                        JewelItemData = JewelSlotData.Value.JewelData;
    //                        if (JewelItemData != null)
    //                        {
    //                            foreach (var TargetMod in JewelItemData.Mods)
    //                            {
    //                                if (TargetMod.Values.Count == 1)//Only single value Mods added to dictionary for solver use
    //                                {
    //                                    if (ItemDictionary.ContainsKey(TargetMod.Attribute))
    //                                    {
    //                                        ItemDictionary[TargetMod.Attribute] += TargetMod.Values[0];
    //                                    }
    //                                    else
    //                                    {
    //                                        ItemDictionary.Add(TargetMod.Attribute, TargetMod.Values[0]);
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                    ItemData = null; ContinueCalc = false; break;
    //            }
    //            if (ItemData != null && ContinueCalc)
    //            {
    //                foreach (var TargetMod in ItemData.Mods)
    //                {
    //                    if (TargetMod.Values.Count == 1)//Only single value Mods added to dictionary for solver use
    //                    {
    //                        if (ItemDictionary.ContainsKey(TargetMod.Attribute))
    //                        {
    //                            ItemDictionary[TargetMod.Attribute] += TargetMod.Values[0];
    //                        }
    //                        else
    //                        {
    //                            ItemDictionary.Add(TargetMod.Attribute, TargetMod.Values[0]);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        return ItemDictionary;
    //    }
    }

    public class TrackedAttributes : System.Collections.Generic.List<PseudoAttribute>
    {
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
            for (int Index = 0; Index < this.Count; ++Index)
            {
                if (this[Index].Name == Name)
                {
                    return Index;
                }
            }
            return -1;
        }

        public int IndexOf(string Name) => GetIndexOfAttribute(Name);

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
            PseudoAttribute CurrentAttribute = this[Index];
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
        public void StartTracking(Dictionary<string, Tuple<float, double>> attributeConstraints, Dictionary<PseudoAttribute, System.Tuple<float, double>> pseudoAttributeConstraints, WeaponClass value, OffHand value1, SkillTree treeInfo)
        {
            int Index;
            Dictionary<string, List<float>> attrlist = treeInfo.SelectedAttributes;
            string StatName;
            string CustomName;
            //bool CreateCustomTrackedAttribute = false;
            foreach (PseudoAttribute Attribute in pseudoAttributeConstraints.Keys)//Don't need target value and weight
            {
                Index = GetIndexOfAttribute(Attribute);
                if (Index == -1)
                {//Make sure TrackedPseudoAttribute doesn't conflict with normal attributes(Tagged PseudoAttributes etc)
                    StatName = Attribute.Name;
                    if (attrlist.ContainsKey(StatName))
                    {
                        CustomName = StatName + " (TrackedAttr)";
                        this.Add(new PseudoAttribute(Attribute,CustomName));
                    }
                    else
                    {
                        this.Add(Attribute);
                    }
                }
                else
                {
                    this[Index] = Attribute;
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
        public PseudoAttribute this[string IndexKey, PseudoAttribute data]
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
                        this.Add(data);
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

    public static class GlobalSettings
    {
        //Check once during creation of AdvancedSolver
        #region AdvancedSolverToggles
        public const string HybridHPKey = "# HybridHP Subtotal";
        public const string DualWandAccKey = "# DualWand Accuracy Subtotal";
        public const string HPTotalKey = "# HP Subtotal";
        public const string PseudoAccKey = "# PseudoAccuracy Subtotal";

        public static bool AdvancedTreeSearch = false;
        public static bool ScanDualWandAcc = false;
        public static bool ScanHybridHP = false;
        public static bool ScanHPTotal = false;
        public static bool ScanPseudoAccuracy = false;
        #endregion AdvancedSolverToggles

        /// <summary>
        /// Stored JewelInfo
        /// </summary>
        public static JewelData JewelStorage;
        /// <summary>
        /// Static Property Event(http://10rem.net/blog/2011/11/29/wpf-45-binding-and-change-notification-for-static-properties)
        /// </summary>
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        /// <summary>
        /// Static Property Event(http://10rem.net/blog/2011/11/29/wpf-45-binding-and-change-notification-for-static-properties)
        /// </summary>
        public static void NotifyStaticPropertyChanged(string propertyName)
        {
            if (StaticPropertyChanged != null)
                StaticPropertyChanged(null, new PropertyChangedEventArgs(propertyName));
        }

        public static Dictionary<string, float> CalculateSubtotalAttributes(SkillTree treeInfo)
        {
            Dictionary<string, float> StatTotals = new Dictionary<string, float>();
            Dictionary<string, List<float>> attrlist = treeInfo.SelectedAttributes;

            //"# Accuracy Subtotal" = Dex Based Accuracy x Accuracy increase
            float Subtotal = 0.0f;
            float TotalIncrease = 0.0f;
            if (ScanDualWandAcc)
            {
                if (attrlist.ContainsKey("+# Accuracy Rating"))
                {
                    Subtotal += attrlist["+# Accuracy Rating"][0];
                }
                if (attrlist.ContainsKey("+# to Accuracy Rating"))
                {
                    Subtotal += attrlist["+# to Accuracy Rating"][0];
                }
                if (attrlist.ContainsKey("#% increased Accuracy Rating with Wands"))
                {
                    TotalIncrease += attrlist["#% increased Accuracy Rating with Wands"][0];
                }
                if (attrlist.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
                {
                    TotalIncrease += attrlist["#% increased Accuracy Rating while Dual Wielding"][0];
                }
                if (attrlist.ContainsKey("#% increased Global Accuracy Rating"))
                {
                    TotalIncrease += attrlist["#% increased Global Accuracy Rating"][0];
                }
                if (TotalIncrease != 0.0f)
                {
                    TotalIncrease = (100.0f + TotalIncrease) / 100.0f;
                    Subtotal *= TotalIncrease;
                }
                if (StatTotals.ContainsKey(DualWandAccKey))//"# Accuracy Subtotal"
                {
                    StatTotals[DualWandAccKey] = Subtotal;
                }
                else
                {
                    StatTotals.Add(DualWandAccKey, Subtotal);
                }
            }
            //MaxLife combined with increased life
            Subtotal = 0.0f;
            TotalIncrease = 0.0f;
            if (ScanHPTotal || ScanHybridHP)
            {
                if (attrlist.ContainsKey("+# to maximum Life"))
                {
                    Subtotal = attrlist["+# to maximum Life"][0];
                }
                if (attrlist.ContainsKey("#% increased maximum Life"))
                {
                    TotalIncrease = attrlist["#% increased maximum Life"][0];
                }
                if (TotalIncrease != 0.0f)
                {
                    TotalIncrease = (100.0f + TotalIncrease) / 100.0f;
                    Subtotal *= TotalIncrease;
                }
                if (ScanHPTotal)
                {
                    if (StatTotals.ContainsKey(HPTotalKey))
                    {
                        StatTotals[HPTotalKey] = Subtotal;
                    }
                    else
                    {
                        StatTotals.Add(HPTotalKey, Subtotal);
                    }
                }
                if (ScanHybridHP)
                {
                    float ESSubtotal = 0.0f;
                    float ESIncrease = 0.0f;
                    if (attrlist.ContainsKey("+# to maximum Energy Shield"))
                    {
                        ESSubtotal = attrlist["+# to maximum Energy Shield"][0];
                    }
                    if (attrlist.ContainsKey("#% increased maximum Energy Shield"))
                    {
                        ESIncrease = attrlist["#% increased maximum Energy Shield"][0];
                    }
                    if (attrlist.ContainsKey("+# to Intelligence"))
                    {
                        ESIncrease += attrlist["+# to Intelligence"][0] / 10.0f;
                    }
                    if (ESIncrease != 0.0f)
                    {
                        ESIncrease = (100.0f + ESIncrease) / 100.0f;
                        ESSubtotal *= ESIncrease;
                    }
                    Subtotal += ESSubtotal;
                    if (StatTotals.ContainsKey(HybridHPKey))
                    {
                        StatTotals[HybridHPKey] = Subtotal;
                    }
                    else
                    {
                        StatTotals.Add(HybridHPKey, Subtotal);
                    }
                }
            }
            return StatTotals;
        }

        /// <summary>
        /// Updates the subtotals.
        /// </summary>
        /// <param name="attrlist">The stat totals.</param>
        /// <returns>Dictionary&lt;System.String, System.Single&gt;</returns>
        public static Dictionary<string, float> UpdateSubtotals(Dictionary<string, float> attrlist)
        {
            float BaseAccuracy = 0.0f;
            float Subtotal = 0.0f;
            float TotalIncrease = 0.0f;
            //"# Accuracy Subtotal" = Dex Based Accuracy x Accuracy increase
            if (attrlist.ContainsKey("+# Accuracy Rating"))
            {
                BaseAccuracy += attrlist["+# Accuracy Rating"];
            }
            if (attrlist.ContainsKey("+# to Accuracy Rating"))
            {
                BaseAccuracy += attrlist["+# to Accuracy Rating"];
            }
            Subtotal += BaseAccuracy;
            if (attrlist.ContainsKey("#% increased Accuracy Rating with Wands"))
            {
                TotalIncrease += attrlist["#% increased Accuracy Rating with Wands"];
            }
            if (attrlist.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
            {
                TotalIncrease += attrlist["#% increased Accuracy Rating while Dual Wielding"];
            }
            if (attrlist.ContainsKey("#% increased Global Accuracy Rating"))
            {
                TotalIncrease += attrlist["#% increased Global Accuracy Rating"];
            }
            if (TotalIncrease != 0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease) / 100.0f;
                Subtotal *= TotalIncrease;
            }
            if (attrlist.ContainsKey(DualWandAccKey))
            {
                attrlist[DualWandAccKey] = Subtotal;
            }
            else
            {
                attrlist.Add(DualWandAccKey, Subtotal);
            }
            Subtotal = 0.0f;
            TotalIncrease = 0.0f;
            if (attrlist.ContainsKey("+# to maximum Life"))
            {
                Subtotal = attrlist["+# to maximum Life"];
            }
            if (attrlist.ContainsKey("#% increased maximum Life"))
            {
                TotalIncrease = attrlist["#% increased maximum Life"];
            }
            if (TotalIncrease != 0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease) / 100.0f;
                Subtotal *= TotalIncrease;
            }
            if (attrlist.ContainsKey(HPTotalKey))
            {
                attrlist[HPTotalKey] = Subtotal;
            }
            else
            {
                attrlist.Add(HPTotalKey, Subtotal);
            }
            float ESSubtotal = 0.0f;
            float ESIncrease = 0.0f;
            if (attrlist.ContainsKey("+# to maximum Energy Shield"))
            {
                ESSubtotal = attrlist["+# to maximum Energy Shield"];
            }
            if (attrlist.ContainsKey("#% increased maximum Energy Shield"))
            {
                ESIncrease = attrlist["#% increased maximum Energy Shield"];
            }
            if (attrlist.ContainsKey("+# to Intelligence"))
            {
                ESIncrease += attrlist["+# to Intelligence"] / 10.0f;
            }
            if (ESIncrease != 0.0f)
            {
                ESIncrease = (100.0f + ESIncrease) / 100.0f;
                ESSubtotal *= ESIncrease;
            }
            Subtotal += ESSubtotal;
            if (attrlist.ContainsKey(HybridHPKey))
            {
                attrlist[HybridHPKey] = Subtotal;
            }
            else
            {
                attrlist.Add(HybridHPKey, Subtotal);
            }
            return attrlist;
        }

        public static Dictionary<string, float> UpdateSubtotals(Dictionary<string, List<float>> attrlist)
        {
            Dictionary<string, float> AttributeTotals = new Dictionary<string, float>(2);
            float BaseAccuracy = 0.0f;
            float Subtotal = 0.0f;
            float TotalIncrease = 0.0f;
            //"# Accuracy Subtotal" = Dex Based Accuracy x Accuracy increase
            if (attrlist.ContainsKey("+# Accuracy Rating"))
            {
                BaseAccuracy += attrlist["+# Accuracy Rating"][0];
            }
            if (attrlist.ContainsKey("+# to Accuracy Rating"))
            {
                BaseAccuracy += attrlist["+# to Accuracy Rating"][0];
            }
            Subtotal += BaseAccuracy;
            if (attrlist.ContainsKey("#% increased Accuracy Rating with Wands"))
            {
                TotalIncrease += attrlist["#% increased Accuracy Rating with Wands"][0];
            }
            if (attrlist.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
            {
                TotalIncrease += attrlist["#% increased Accuracy Rating while Dual Wielding"][0];
            }
            if (attrlist.ContainsKey("#% increased Global Accuracy Rating"))
            {
                TotalIncrease += attrlist["#% increased Global Accuracy Rating"][0];
            }
            if (TotalIncrease != 0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease) / 100.0f;
                Subtotal *= TotalIncrease;
            }
            if (attrlist.ContainsKey(DualWandAccKey))
            {
                AttributeTotals[DualWandAccKey] = Subtotal;
            }
            else
            {
                AttributeTotals.Add(DualWandAccKey, Subtotal);
            }
            Subtotal = 0.0f;
            TotalIncrease = 0.0f;
            if (attrlist.ContainsKey("+# to maximum Life"))
            {
                Subtotal = attrlist["+# to maximum Life"][0];
            }
            if (attrlist.ContainsKey("#% increased maximum Life"))
            {
                TotalIncrease = attrlist["#% increased maximum Life"][0];
            }
            if (TotalIncrease != 0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease) / 100.0f;
                Subtotal *= TotalIncrease;
            }
            if (attrlist.ContainsKey(HPTotalKey))
            {
                AttributeTotals[HPTotalKey] = Subtotal;
            }
            else
            {
                AttributeTotals.Add(HPTotalKey, Subtotal);
            }
            float ESSubtotal = 0.0f;
            float ESIncrease = 0.0f;
            if (attrlist.ContainsKey("+# to maximum Energy Shield"))
            {
                ESSubtotal = attrlist["+# to maximum Energy Shield"][0];
            }
            if (attrlist.ContainsKey("#% increased maximum Energy Shield"))
            {
                ESIncrease = attrlist["#% increased maximum Energy Shield"][0];
            }
            if (attrlist.ContainsKey("+# to Intelligence"))
            {
                ESIncrease += attrlist["+# to Intelligence"][0] / 10.0f;
            }
            if (ESIncrease != 0.0f)
            {
                ESIncrease = (100.0f + ESIncrease) / 100.0f;
                ESSubtotal *= ESIncrease;
            }
            Subtotal += ESSubtotal;
            if (attrlist.ContainsKey(HybridHPKey))
            {
                AttributeTotals[HybridHPKey] = Subtotal;
            }
            else
            {
                AttributeTotals.Add(HybridHPKey, Subtotal);
            }
            return AttributeTotals;
        }

        /// <summary>
        /// Stored JewelInfo
        /// </summary>
        public static JewelData JewelInfo
        {
            get { return JewelStorage; }
            set
            {
                if (value == JewelStorage)
                    return;
                JewelStorage = value;
                NotifyStaticPropertyChanged("JewelInfo");
            }
        }
        /// <summary>
        /// The tracked stats
        /// </summary>
        public static TrackedAttributes TrackedStats = new TrackedAttributes();

        /// <summary>
        /// The default tracking directory
        /// </summary>
        public static string DefaultTrackingDir = Path.Combine(AppData.ProgramDirectory, "StatTracking" + Path.DirectorySeparatorChar);
        /// <summary>
        /// The stat tracking save path (Shared between TrackedStatsMenuModel and TrackedStatsMenu)
        /// </summary>
        public static string StatTrackingSavePathVal = DefaultTrackingDir;
        /// <summary>
        /// The stat tracking save path (Shared between TrackedStatsMenuModel and TrackedStatsMenu)
        /// </summary>
        public static string StatTrackingSavePath
        {
            get
            {
                if (GlobalSettings.StatTrackingSavePathVal == null)
                {
                    return GlobalSettings.DefaultTrackingDir;
                }
                else
                {
                    return GlobalSettings.StatTrackingSavePathVal;
                }
            }
            set
            {
                if (value != null && value != "" && StatTrackingSavePathVal != value)
                {
                    GlobalSettings.StatTrackingSavePathVal = value;
                    NotifyStaticPropertyChanged("StatTrackingSavePath");
                }
            }
        }
        /// <summary>
        /// Saved slot for item slot that are removing Intuitive leap support from node id on tree
        /// </summary>
        public static PoESkillTree.GameModel.Items.ItemSlot RemovingIntLeapJewels = 0;
        /// <summary>
        /// If true, automatically adds skilltree pseudo attributes to stat tracking (Use menu to turn on)(Default:false)
        /// </summary>
        public static bool AutoTrackStats = true;
        /// <summary>
        /// Auto-updated value of types of weapons equipped (0:None;
        //  1: 2H Axe; 2: 2H Sword; 3: 2H Mace;
        // 5+ values = Shield+Weapon (5 = shield+fist)
        // 6: Axe+Shield; 7: Sword+Shield;
        // 500+ values = Dual Wielding;
        // 501 = Dual Wand; 502 = Dual Axe; 503 = Dual Sword; 504 = Dual Mace
        /// </summary>
        public static int WeaponComboType = 0;

        public static InventoryViewModel ItemInfoVal;

        /// <summary>
        /// The item information equipped in skilltree
        /// </summary>
        public static InventoryViewModel ItemInfo
        {
            get { return ItemInfoVal; }
            set
            {
                if (value == ItemInfoVal)
                    return;
                ItemInfoVal = value;
                NotifyStaticPropertyChanged("ItemInfo");
            }
        }

        /// <summary>
        /// The fake intuitive leap support attribute
        /// </summary>
        public const string FakeIntuitiveLeapSupportAttribute = "IntuitiveLeapSupported";

        public const string LeapedNode = "Intuitive Leaped";

        /// <summary>
        /// The dialog coordinator
        /// </summary>
        private static IExtendedDialogCoordinator _dialogCoordinatorVal;

        /// <summary>
        /// The dialog coordinator
        /// </summary>
        public static IExtendedDialogCoordinator SharedDialogCoordinator
        {
            get { return _dialogCoordinatorVal; }
            set
            {
                if (value == _dialogCoordinatorVal)
                    return;
                _dialogCoordinatorVal = value;
                NotifyStaticPropertyChanged("SharedDialogCoordinator");
            }
        }
    }
}