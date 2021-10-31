// ***********************************************************************
// Code Created by James Michael Armstrong (https://github.com/BlazesRus)
// Latest GlobalCode Release at https://github.com/BlazesRus/MultiPlatformGlobalCode
// ***********************************************************************
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;
using PoESkillTree.ViewModels;
using PoESkillTree.ViewModels.PassiveTree;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;//Needed for Notifier parts of JewelData
//using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
//using PoESkillTree.ViewModels;
//using PoESkillTree.ViewModels.Equipment;
//using PoESkillTree.TreeGenerator.Model.PseudoAttributes;
//using PoESkillTree.TrackedStatViews;


namespace PoESkillTree
{
    /// <summary>Threshold jewel radius has 40 or more of stat(s)</summary>
    public enum ThresholdTypes : ushort
    {
        Neutral,
        Strength,
        Intelligence,
        Dexterity,
        StrIntHybrid,
        StrDexHybrid,
        IntDexHybrid,
        OmniType
    }

    public class JewelNodeData
    {
        public CharacterClass ClosestClassStartType;
        public ThresholdTypes JewelStatType;
        /// <summary>The slot description of Jewel Node on Tree</summary>
        public string SlotDesc;

        /// <summary>Generates the slot description using PassiveNodeViewModels information from Skill Tree.</summary>
        /// <param name="NodeId">NodeID for target node</param>
        public void GenerateSlotDesc(ushort NodeId)
        {
            PassiveNodeViewModel NullNode = new PassiveNodeViewModel(0);
            PassiveNodeViewModel ClosestKeystone = NullNode;
            PassiveNodeViewModel ClosestNotable = NullNode;
            PassiveNodeViewModel CurrentNode = NullNode;
            Vector2D nodePosition;
            IEnumerable<KeyValuePair<ushort, PassiveNodeViewModel>> affectedNodes;
            double MinRange = 0.0;
            double MaxRange = 1200.0;
            double ClosestKSRange = 99999.0;
            double ClosestNotableRange = 99999.0;
            double range;
            nodePosition = SkillTree.Skillnodes[NodeId].Position;
            do
            {
                affectedNodes = SkillTree.Skillnodes.Where(n => (((n.Value.Position - nodePosition).Length < MaxRange)) && ((n.Value.Position - nodePosition).Length > MinRange)).ToList();
                foreach (KeyValuePair<ushort, PassiveNodeViewModel> NodePair in affectedNodes)
                {
                    CurrentNode = NodePair.Value;
                    if (CurrentNode.PassiveNodeType == PassiveNodeType.Notable)
                    {
                        range = (CurrentNode.Position - nodePosition).Length;
                        if (range < ClosestNotableRange)
                        {
                            ClosestNotable = CurrentNode;
                            ClosestNotableRange = range;
                        }
                    }
                    else if (CurrentNode.PassiveNodeType == PassiveNodeType.Keystone)
                    {
                        range = (CurrentNode.Position - nodePosition).Length;
                        if (range < ClosestKSRange)
                        {
                            ClosestKeystone = CurrentNode;
                            ClosestKSRange = range;
                        }
                    }
                }
                if (ClosestNotable== NullNode || ClosestKeystone == NullNode)
                {
                    MinRange = MaxRange;
                    MaxRange += 600.0;
                }
            } while(ClosestNotable == null||ClosestKeystone == null);
            SlotDesc = "Closest Keystone to jewel-slot is "+ClosestKeystone.Name+".";
            SlotDesc += "\nClosest Notable to jewel-slot is "+ClosestNotable.Name+".";
            //Detect which class root node belongs to and then display it


            switch (JewelStatType)//Display Medium Jewel Threshold Type(if have 40+ of attribute(s))
            {
                case ThresholdTypes.Strength:
                    SlotDesc += "\n(Strength Threshold Slot)";
                    break;
                case ThresholdTypes.Intelligence:
                    SlotDesc += "\n(Intelligence Threshold Slot)";
                    break;
                case ThresholdTypes.Dexterity:
                    SlotDesc += "\n(Dexterity Threshold Slot)";
                    break;
                case ThresholdTypes.StrDexHybrid:
                    SlotDesc += "\n(Str+Dex Threshold Slot)";
                    break;
                case ThresholdTypes.StrIntHybrid:
                    SlotDesc += "\n(Str+Int Threshold Slot)";
                    break;
                case ThresholdTypes.IntDexHybrid:
                    SlotDesc += "\n(Int+Dex Threshold Slot)";
                    break;
                case ThresholdTypes.OmniType:
                    SlotDesc += "\n(Omni-Threshold Slot)";
                    break;
                default:
                    SlotDesc += "\n(Neutral Threshold Slot)";
                    break;
            }

        }

        public JewelNodeData()
        {
            ClosestClassStartType = CharacterClass.Scion;
            JewelStatType = ThresholdTypes.Neutral;
            SlotDesc = "";
        }

        public JewelNodeData(CharacterClass closestClassStartType, ThresholdTypes jewelStatType)
        {
            ClosestClassStartType = closestClassStartType;
            JewelStatType = jewelStatType;
            SlotDesc = "";
        }
    }

    /// <summary>
    /// Dictionary  holding NodeIDs for Jewel Slots  as keys and JewelItems as data
    /// Implements the <see cref="System.Collections.Generic.Dictionary{System.UInt16, PoESkillTree.JewelNodeData}" />
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.UInt16, PoESkillTree.JewelNodeData}" />
    [Serializable]
    public class JewelData : Dictionary<ushort, JewelNodeData>
    {//Notifier combined directly into class since can't have 2 base classes
        #region NotifierCode
        ///// <summary>
        ///// Sets <paramref name="backingStore"/> to <paramref name="value"/> and
        ///// raises <see cref="PropertyChanging"/> before and <see cref="PropertyChanged"/>
        ///// after setting the value.
        ///// </summary>
        ///// <param name="backingStore">Target variable</param>
        ///// <param name="value">Source variable</param>
        ///// <param name="onChanged">Called after changing the value but before raising <see cref="PropertyChanged"/>.</param>
        ///// <param name="onChanging">Called before changing the value and before raising <see cref="PropertyChanging"/> with <paramref name="value"/> as parameter.</param>
        ///// <param name="propertyName">Name of the changed property</param>
        //protected void SetProperty<T>(
        //    ref T backingStore, T value,
        //    Action onChanged,
        //    Action<T> onChanging,
        //    [CallerMemberName] string propertyName = "")
        //{
        //    if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;

        //    onChanging?.Invoke(value);
        //    OnPropertyChanging(propertyName);

        //    backingStore = value;

        //    onChanged?.Invoke();
        //    OnPropertyChanged(propertyName);
        //}

        ///// <summary>
        ///// INotifyPropertyChanged event that is called right before a property is changed.
        ///// </summary>
        //public event PropertyChangingEventHandler PropertyChanging;

        //private void OnPropertyChanging(string propertyName)
        //{
        //    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        //}

        ///// <summary>
        ///// INotifyPropertyChanged event that is called right after a property is changed.
        ///// </summary>
        //public event PropertyChangedEventHandler PropertyChanged;

        //protected virtual void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        ///// <summary>
        ///// Equivalent to <c>o.MemberwiseClone()</c> except that events are set to null.
        ///// Override if your subclass has events or if you need to re-register handlers.
        ///// </summary>
        //protected virtual JewelData SafeMemberwiseClone()
        //{
        //    var t = (JewelData)MemberwiseClone();
        //    t.PropertyChanged;
        //    t.PropertyChanging;
        //    return t;
        //}
        #endregion

        /// <summary>  Initialize JewelData with CategorizeJewelNodes method once(and then set to false) after skilltree nodes are finished generating onto tree</summary>
        public bool NotSetup;
        public void SetThresholdType(ushort Id,ThresholdTypes Value)
        {
            this[Id].JewelStatType = Value;
        }

        public ThresholdTypes GetThresholdType(ushort Id)
        {
            return this[Id].JewelStatType;
        }

        /// <summary>Initializes a new instance of the <see cref="JewelDictionary"/> class.</summary>
        public JewelData() : base(21)
        {
            NotSetup = true;
        }

        /// <summary>
        /// Adds the jewel slot.
        /// </summary>
        /// <param name="nodeID">The node identifier.</param>
        public void AddJewelSlot(ushort nodeID)
        {
            Add(nodeID, new JewelNodeData());
        }

        /// <summary>
        /// Generate JewelDictionary Categories from  Data from SkillTree and add extra fake attributes to label threshold type and Node id for identifying node in inventory view
        /// </summary>
        public void CategorizeJewelSlots()
        {
            bool IsStrThreshold;
            bool IsIntThreshold;
            bool IsDexThreshold;

            PassiveNodeViewModel CurrentNode;
            //string IDLabel = "Jewel Socket ID: #";
            string StrThresholdLabel = "+# Str JewelSlot";
            string IntThresholdLabel = "+# Int JewelSlot";
            string DexThresholdLabel = "+# Dex JewelSlot";
            string JewelSocketLabel = "+# NonCluster JewelSocket";
            string AttributeName = "";
            float AttributeTotal;
            List<float> SingleVal = new List<float>(1);
            SingleVal.Add(1);
            Vector2D nodePosition;
            IEnumerable<KeyValuePair<ushort, PassiveNodeViewModel>> affectedNodes;
            //Copying keys so can change elements during foreach loop
            List<ushort> ElementList = new List<ushort>(GlobalSettings.JewelInfo.Keys);
            foreach (ushort NodeId in ElementList)
            {
                //IDLabel = "Jewel Socket ID: " + NodeId;
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
                    nodePosition = SkillTree.Skillnodes[NodeId].Position;
                    affectedNodes = SkillTree.Skillnodes.Where(n => ((n.Value.Position - nodePosition).Length < 1200.0)).ToList();
                    foreach (KeyValuePair<ushort, PassiveNodeViewModel> NodePair in affectedNodes)
                    {
                        CurrentNode = NodePair.Value;
                        if (CurrentNode.Attributes != null && CurrentNode.Attributes.ContainsKey(AttributeName))
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
                if (IsDexThreshold || IsStrThreshold || IsIntThreshold)
                {
                    if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(JewelSocketLabel))
                        SkillTree.Skillnodes[NodeId].Attributes.Add(JewelSocketLabel, SingleVal);
                }
                if (IsDexThreshold && IsStrThreshold && IsIntThreshold)
                {
                    if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(StrThresholdLabel))
                        SkillTree.Skillnodes[NodeId].Attributes.Add(StrThresholdLabel, SingleVal);
                    if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(IntThresholdLabel))
                        SkillTree.Skillnodes[NodeId].Attributes.Add(IntThresholdLabel, SingleVal);
                    if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(DexThresholdLabel))
                        SkillTree.Skillnodes[NodeId].Attributes.Add(DexThresholdLabel, SingleVal);
                    SetThresholdType(NodeId, ThresholdTypes.OmniType);
                }
                else if (IsStrThreshold)
                {
                    if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(StrThresholdLabel))
                        SkillTree.Skillnodes[NodeId].Attributes.Add(StrThresholdLabel, SingleVal);
                    if (IsIntThreshold)
                    {
                        if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(IntThresholdLabel))
                        SkillTree.Skillnodes[NodeId].Attributes.Add(IntThresholdLabel, SingleVal);
                        SetThresholdType(NodeId, ThresholdTypes.StrIntHybrid);
                    }
                    else if (IsDexThreshold)
                    {
                        if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(DexThresholdLabel))
                            SkillTree.Skillnodes[NodeId].Attributes.Add(DexThresholdLabel, SingleVal);
                        SetThresholdType(NodeId, ThresholdTypes.StrDexHybrid);
                    }
                    else
                    {
                        SetThresholdType(NodeId, ThresholdTypes.Strength);
                    }
                }
                else if (IsIntThreshold)
                {
                    if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(IntThresholdLabel))
                        SkillTree.Skillnodes[NodeId].Attributes.Add(IntThresholdLabel, SingleVal);
                    if (IsDexThreshold)
                    {
                        if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(DexThresholdLabel))
                            SkillTree.Skillnodes[NodeId].Attributes.Add(DexThresholdLabel, SingleVal);
                        SetThresholdType(NodeId, ThresholdTypes.IntDexHybrid);
                    }
                    else
                    {
                        SetThresholdType(NodeId, ThresholdTypes.Intelligence);
                    }
                }
                else if (IsDexThreshold)
                {
                    if (!SkillTree.Skillnodes[NodeId].Attributes.ContainsKey(DexThresholdLabel))
                        SkillTree.Skillnodes[NodeId].Attributes.Add(DexThresholdLabel, SingleVal);
                    SetThresholdType(NodeId, ThresholdTypes.Dexterity);

                }
                else//Neutral(often ineffective corner jewels)//Normally Cluster Jewel Sockets
                {
                    //NeutralJewelSlots.Add(NodeId);
                }
                SkillTree.Skillnodes[NodeId].UpdateStatDescription();
                this[NodeId].GenerateSlotDesc(NodeId);
            }
        }

        public static explicit operator System.Collections.Generic.List<ushort>(JewelData self)
        {
            return new List<ushort>(self.Keys);
        }

        /// <summary>
        /// Property that converts JewelDictionary into List of node ids
        /// </summary>
        public System.Collections.Generic.List<ushort> JewelIds { get { return (System.Collections.Generic.List<ushort>)this; } }

        //(Most of JewelData node searching code based on https://github.com/PoESkillTree/PoESkillTree/issues/163)
        //Point p = ((MouseEventArgs)e.OriginalSource).GetPosition(zbSkillTreeBackground.Child);
        //var v = new Vector2D(p.X, p.Y);
        //v = v * _multransform + _addtransform;
        //IEnumerable<KeyValuePair<ushort, PassiveNodeViewModel>> nodes =
        //    SkillTree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();

        /// <summary>
        /// Calculates the total of target attribute inside jewel area.
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        /// <param name="AttributeName">Name of Attribute to search for</param>
        /// <param name="SkilledNodes">The skilled nodes.</param>
        /// <param name="JewelRadiusType">Jewel Radius Type(Large/Medium/Small)(Default:Large"")</param>
        /// <returns></returns>
        static public float CalculateTotalOfAttributeInJewelArea(PassiveNodeViewModel TargetNode, string AttributeName, ObservableSet<PassiveNodeViewModel> SkilledNodes, string JewelRadiusType = "")
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
            PassiveNodeViewModel CurrentNode;
            float AttributeTotal = 0.0f;
            Vector2D nodePosition = TargetNode.Position;
            IEnumerable<KeyValuePair<ushort, PassiveNodeViewModel>> affectedNodes = SkillTree.Skillnodes.Where(n => ((n.Value.Position - nodePosition).Length < JewelRadius)).ToList();
            ////Or use
            //var nodes = Skillnodes.Where(n =>
            //{
            //    var size = GetNodeSurroundBrushSize(n.Value, 0);
            //    var range = JewelRadius;//size.Width * size.Height * n.Value.ZoomLevel;
            //    var length = (n.Value.Position - mousePointer).Length;
            //    return length * length < range;
            //}).ToList();//Based on FindNodesInRange from SkillTree
            foreach (KeyValuePair<ushort, PassiveNodeViewModel> NodePair in affectedNodes)
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
        /// Calculates the total of unallocated target attributes inside jewel area.
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        /// <param name="AttributeName">Name of Attribute to search for</param>
        /// <param name="SkilledNodes">The skilled nodes.</param>
        /// <param name="JewelRadiusType">Jewel Radius Type(Large/Medium/Small)(Default:Large"")</param>
        /// <returns></returns>
        static public float CalculateTotalUnallocAttributeInJewelArea(PassiveNodeViewModel TargetNode, string AttributeName, ObservableSet<PassiveNodeViewModel> SkilledNodes, string JewelRadiusType = "")
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
            PassiveNodeViewModel CurrentNode;
            float AttributeTotal = 0.0f;
            Vector2D nodePosition = TargetNode.Position;
            IEnumerable<KeyValuePair<ushort, PassiveNodeViewModel>> affectedNodes = SkillTree.Skillnodes.Where(n => ((n.Value.Position - nodePosition).Length < JewelRadius)).ToList();
            foreach (KeyValuePair<ushort, PassiveNodeViewModel> NodePair in affectedNodes)
            {
                CurrentNode = NodePair.Value;
                if (CurrentNode.Attributes.ContainsKey(AttributeName) && !SkilledNodes.Contains(CurrentNode))
                {
                    AttributeTotal += CurrentNode.Attributes[AttributeName][0];
                }
            }
            return AttributeTotal;
        }

/*
        /// <summary>
        /// Updates stats based on Unique Jewels Slotted
        /// </summary>
        /// <param name="attrlist">The attrlist.</param>
        /// <param name="Tree">The tree.</param>
        /// <returns></returns>
        static public Dictionary<string, List<float>> StatUpdater(Dictionary<string, List<float>> attrlist)
        {
            float Subtotal = 0.0f;
            float TotalIncrease = 0.0f;
            if (PseudoCalcGlobals.CalculateAcc)
            {
                if (attrlist.ContainsKey("+# Accuracy Rating"))
                {
                    Subtotal += attrlist["+# Accuracy Rating"][0];
                }
                if (attrlist.ContainsKey("+# to Accuracy Rating"))
                {
                    Subtotal += attrlist["+# to Accuracy Rating"][0];
                }
                string KeyName = "#% increased Accuracy Rating with ";
                switch (PseudoCalcGlobals.PrimaryWeapon)
                {
                    case WeaponClass.Staff:
                        KeyName += "Staves"; break;
                    default:
                        KeyName += PseudoCalcGlobals.PrimaryWeapon.GetDescription() + "s"; break;
                }
                if (attrlist.ContainsKey(KeyName))
                {
                    TotalIncrease += attrlist[KeyName][0];
                }
                if (PseudoCalcGlobals.NotUsingShield&&PseudoCalcGlobals.SecondaryWeapon != WeaponClass.Unarmed)
                {
                    if (attrlist.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
                    {
                        TotalIncrease += attrlist["#% increased Accuracy Rating while Dual Wielding"][0];
                    }
                }
                if (PseudoCalcGlobals.NotUsingShield && PseudoCalcGlobals.SecondaryWeapon == WeaponClass.Unarmed)
                {
                    if (attrlist.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
                    {
                        TotalIncrease += attrlist["#% increased Accuracy Rating while Dual Wielding"][0];
                    }
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
                if (attrlist.ContainsKey(PseudoCalcGlobals.AccKey))
                {
                    attrlist[PseudoCalcGlobals.AccKey][0] = Subtotal;
                }
                else
                {
                    attrlist.Add(PseudoCalcGlobals.AccKey, new List<float>(1) { Subtotal });
                }
            }
            if (PseudoCalcGlobals.CalculateHP||PseudoCalcGlobals.CalculateHybridHP)
            {
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
                if (PseudoCalcGlobals.CalculateHP)
                {
                    if (attrlist.ContainsKey(PseudoCalcGlobals.HPTotalKey))
                    {
                        attrlist[PseudoCalcGlobals.HPTotalKey][0] = Subtotal;
                    }
                    else
                    {
                        attrlist.Add(PseudoCalcGlobals.HPTotalKey, new List<float>(1) { Subtotal });
                    }
                }
                if (PseudoCalcGlobals.CalculateHybridHP)
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
                    if (attrlist.ContainsKey(PseudoCalcGlobals.HybridHPKey))
                    {
                        attrlist[PseudoCalcGlobals.HybridHPKey][0] = Subtotal;
                    }
                    else
                    {
                        attrlist.Add(PseudoCalcGlobals.HybridHPKey, new List<float>(1) { Subtotal });
                    }
                }
            }
            return attrlist;
        }

*/
 
/*
       /// <summary>
        /// Add Stats from Equipment to StatTotal
        /// </summary>
        /// <param name="attrlist">The attribute list.</param>
        /// <param name="InvModel">The item information sent from SkillTreeGenerator.</param>
        /// <returns>Dictionary&lt;System.String, System.Single&gt;</returns>
        public Dictionary<string, float> EquipmentStatUpdater(Dictionary<string, float> attrlist, InventoryViewModel InvModel)
        {
            Dictionary<string, float> ItemDictionary = new Dictionary<string, float>();
            PoESkillTree.Model.Items.Item ItemData = InvModel.Armor.Item;
            for (int Index = 0; Index < 10; ++Index)
            {
                switch (Index)
                {
                    case 0: break;
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
                        break;
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
            return attrlist;
        }

*/

/*
        /// <summary>
        /// Updates stats based on Unique Jewels Slotted
        /// </summary>
        /// <param name="attrlist">The attribute list.</param>
        /// <param name="Tree">The tree.</param>
        /// <param name="InvModel">The item information sent from SkillTreeGenerator.</param>
        /// <returns>Dictionary&lt;System.String, System.Single&gt;</returns>
        public Dictionary<string, float> PseudoCalcUpdater(Dictionary<string, float> attrlist)
        {
            float Subtotal = 0.0f;
            float TotalIncrease = 0.0f;
            if (PseudoCalcGlobals.CalculateAcc)
            {
                if (attrlist.ContainsKey("+# Accuracy Rating"))
                {
                    Subtotal += attrlist["+# Accuracy Rating"];
                }
                if (attrlist.ContainsKey("+# to Accuracy Rating"))
                {
                    Subtotal += attrlist["+# to Accuracy Rating"];
                }
                string KeyName = "#% increased Accuracy Rating with ";
                switch (PseudoCalcGlobals.PrimaryWeapon)
                {
                    case WeaponClass.Staff:
                        KeyName += "Staves"; break;
                    default:
                        KeyName += PseudoCalcGlobals.PrimaryWeapon.GetDescription() + "s"; break;
                }
                if (attrlist.ContainsKey(KeyName))
                {
                    TotalIncrease += attrlist[KeyName];
                }
                if (PseudoCalcGlobals.NotUsingShield && PseudoCalcGlobals.SecondaryWeapon != WeaponClass.Unarmed)
                {
                    if (attrlist.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
                    {
                        TotalIncrease += attrlist["#% increased Accuracy Rating while Dual Wielding"];
                    }
                }
                if (PseudoCalcGlobals.NotUsingShield && PseudoCalcGlobals.SecondaryWeapon == WeaponClass.Unarmed)
                {
                    if (attrlist.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
                    {
                        TotalIncrease += attrlist["#% increased Accuracy Rating while Dual Wielding"];
                    }
                }
                if (attrlist.ContainsKey("#% increased Global Accuracy Rating"))
                {
                    TotalIncrease += attrlist["#% increased Global Accuracy Rating"];
                }
                if (TotalIncrease != 0.0f)
                {
                    TotalIncrease = (100.0f + TotalIncrease) / 100;
                    Subtotal *= TotalIncrease;
                }
                if (attrlist.ContainsKey(PseudoCalcGlobals.AccKey))//"# Accuracy Subtotal"
                {
                    attrlist[PseudoCalcGlobals.AccKey] = Subtotal;
                }
                else
                {
                    attrlist.Add(PseudoCalcGlobals.AccKey, Subtotal);
                }
            }
            if (PseudoCalcGlobals.CalculateHP || PseudoCalcGlobals.CalculateHybridHP)
            {
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
                if (PseudoCalcGlobals.CalculateHP)
                {
                    if (attrlist.ContainsKey(PseudoCalcGlobals.HPTotalKey))
                    {
                        attrlist[PseudoCalcGlobals.HPTotalKey] = Subtotal;
                    }
                    else
                    {
                        attrlist.Add(PseudoCalcGlobals.HPTotalKey, Subtotal);
                    }
                }
                if (PseudoCalcGlobals.CalculateHybridHP)
                {
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
                    if (attrlist.ContainsKey(PseudoCalcGlobals.HybridHPKey))
                    {
                        attrlist[PseudoCalcGlobals.HybridHPKey] = Subtotal;
                    }
                    else
                    {
                        attrlist.Add(PseudoCalcGlobals.HybridHPKey, Subtotal);
                    }
                }
            }
            return attrlist;
        }
*/
    }

/*
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
        /// <param name="AttributeDic">The attribute dictionary.</param>
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
        /// <param name="AttributeDic">The attribute dictionary.</param>
        /// <returns></returns>
        public Dictionary<string, List<float>> PlaceIntoAttributeDic(Dictionary<string, List<float>> AttributeDic)
        {
            if (Count == 0) { return AttributeDic; }
            Dictionary<string, float> AttributeTotals = CreateAttributeDictionary(AttributeDic);
            foreach (var Element in AttributeTotals.Keys)
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
        /// Starts the tracking of Pseudo-attributes(to display in stat calculations).
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
                        this.Add(new PseudoAttribute(Attribute, CustomName));
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
                    if (data == null)
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
*/

/*
    /// <summary>
    /// Class PseudoCalcGlobals.
    /// </summary>
    public static class PseudoCalcGlobals
    {
        //public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanging;

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

        /// <summary>
        /// Sets <paramref name="backingStore"/> to <paramref name="value"/> and
        /// raises <see cref="PropertyChanging"/> before and <see cref="PropertyChanged"/>
        /// after setting the value.(Based on Notifier.cs)
        /// </summary>
        /// <param name="backingStore">Target variable</param>
        /// <param name="value">Source variable</param>
        /// <param name="onChanged">Called after changing the value but before raising <see cref="PropertyChanged"/>.</param>
        /// <param name="onChanging">Called before changing the value and before raising <see cref="PropertyChanging"/> with <paramref name="value"/> as parameter.</param>
        /// <param name="propertyName">Name of the changed property</param>
        public static void SetProperty<T>(
            ref T backingStore, T value,
            Action onChanged = null,
            Action<T> onChanging = null,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;

            //onChanging?.Invoke(value);
            //NotifyStaticPropertyChanging(propertyName);

            backingStore = value;

            onChanged?.Invoke();
            NotifyStaticPropertyChanged(propertyName);
        }

        public const string HybridHPKey = "# HybridHP Subtotal(PseudoCalc)";
        public const string AccKey = "# Accuracy Subtotal(PseudoCalc)";
        public const string HPTotalKey = "# HP Subtotal(PseudoCalc)";
        public const string CritsPerSecKey = "# Crits Per Second(PseudoCalc)";
        public const string PerfectCritSpellMultiplierKey = "#% Critical Spell DamageMultiplier(PseudoCalc)";

        private static bool calculateAcc = false;
        private static bool calculateHP = true;
        private static bool calculateHybridHP = true;
        private static bool calculateCritsPerSec = true;
        private static bool calculateCritSpellMult = true;
        private static bool applyWhisperingIceStats = false;

        private static bool luckyCrits = false;

        /// <summary>  Quality 20 Level 20 Increased Critical active for critical chance calculations </summary>
        private static bool increasedCritActive = false;

        private static bool nightbladeActive = false;

        private static int levelPrecisionActive = 0;

        private static float primaryATKSpeed = 1.20f;
        private static float primaryCrit = 6.50f;
        private static float secondaryATKSpeed = 1.20f;
        private static float secondaryCrit = 6.50f;
        private static WeaponClass primaryWeapon = WeaponClass.Dagger;
        private static WeaponClass secondaryWeapon = WeaponClass.Dagger;
        private static bool notUsingShield = true;

        public enum SpellBehaviorTypes
        {
            [Description("ProjectileSpell")]
            ProjectileSpell,
            [Description("AreaSpell")]
            AreaSpell,
            [Description("AreaSpell")]
            ChannelledAreaSpell
        }

        /// <summary>Damage Scaling of Spell</summary>
        public enum DamageScaling
        {
            [Description("100% Physical")]
            FullPhysical,
            [Description("Full Physical plus Fire Herald")]
            PhysicalPlusFireHeraldCon,
            [Description("Ice")]
            Ice,
            [Description("Physical with 60% Cold")]
            GlCasDamage,
            [Description("Physical with 60% Cold and Fire Herald")]
            GlCasDamageWithFHerald,
            [Description("Physical with 100% Cold")]
            FullConversionGLCas,
        }

        private static SpellBehaviorTypes spellBehaviorType = SpellBehaviorTypes.ProjectileSpell;
        private static DamageScaling dMScaling = DamageScaling.Ice;
        private static int numberOfPoisonStacks = 0;

        /// <summary>Add Whispering Ice based calculations(only for Staff Primary)</summary>
        /// <value>
        ///   <c>true</c> if [applying whispering ice stats]; otherwise, <c>false</c>.</value>
        public static bool ApplyWhisperingIceStats { get => applyWhisperingIceStats; set => applyWhisperingIceStats = value; }

        /// <summary>  Quality 20 Level 20 Nightblade active for attack critical chance (Only for claws and daggers) </summary>
        public static bool NightbladeActive { get => nightbladeActive; set => nightbladeActive = value; }

        public static bool CalculateAcc { get => calculateAcc; set { SetProperty(ref calculateAcc, value); } }
        public static bool CalculateHP { get => calculateHP; set { calculateHP = value; NotifyStaticPropertyChanged("CalculateHP"); } }
        public static bool CalculateHybridHP { get => calculateHybridHP; set { calculateHybridHP = value; NotifyStaticPropertyChanged("CalculateHybridHP"); } }
        public static bool CalculateCritsPerSec { get => calculateCritsPerSec; set { calculateCritsPerSec = value; NotifyStaticPropertyChanged("CalculateCritsPerSec"); } }
        public static bool CalculateCritSpellMult { get => calculateCritSpellMult; set { calculateCritSpellMult = value; NotifyStaticPropertyChanged("CalculateCritSpellMult"); } }
        public static bool LuckyCrits { get => luckyCrits; set { luckyCrits = value; NotifyStaticPropertyChanged("LuckyCrits"); } }
        public static bool IncreasedCritActive { get => increasedCritActive; set { increasedCritActive = value; NotifyStaticPropertyChanged("IncreasedCritActive"); } }
        public static int LevelPrecisionActive { get => levelPrecisionActive; set { levelPrecisionActive = value; NotifyStaticPropertyChanged("LevelPrecisionActive"); } }
        public static float PrimaryATKSpeed { get => primaryATKSpeed; set { primaryATKSpeed = value; NotifyStaticPropertyChanged("PrimaryATKSpeed"); } }
        public static float PrimaryCrit { get => primaryCrit; set { primaryCrit = value; NotifyStaticPropertyChanged("PrimaryCrit"); } }
        public static float SecondaryATKSpeed { get => secondaryATKSpeed; set { secondaryATKSpeed = value; NotifyStaticPropertyChanged("SecondaryATKSpeed"); } }
        public static float SecondaryCrit { get => secondaryCrit; set { secondaryCrit = value; NotifyStaticPropertyChanged("SecondaryCrit"); } }
        public static WeaponClass PrimaryWeapon { get => primaryWeapon;
            set
            {
                primaryWeapon = value; NotifyStaticPropertyChanged("PrimaryWeapon");
            }
        }
        public static WeaponClass SecondaryWeapon
        {
            get => secondaryWeapon;
            set
            {
                secondaryWeapon = value; NotifyStaticPropertyChanged("SecondaryWeapon");
            }
        }

        public static bool NotUsingShield { get => notUsingShield; set { notUsingShield = value; NotifyStaticPropertyChanged("NotUsingShield"); } }
        public static SpellBehaviorTypes SpellBehaviorType { get => spellBehaviorType; set { spellBehaviorType = value; NotifyStaticPropertyChanged("SpellBehaviorType"); } }
        public static DamageScaling DMScaling { get => dMScaling; set { dMScaling = value; NotifyStaticPropertyChanged("DMScaling"); } }
        public static int NumberOfPoisonStacks { get => numberOfPoisonStacks; set { numberOfPoisonStacks = value; NotifyStaticPropertyChanged("NumberOfPoisonStacks"); } }
    }
    */

    public static class GlobalSettings
    {
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

        public static ThresholdTypes GetThresholdType(ushort NodeId)
        {
            return JewelStorage.GetThresholdType(NodeId);
        }

/*
        /// <summary>
        /// The tracked stats
        /// </summary>
        public static TrackedAttributes TrackedStats = new TrackedAttributes();
*/

/*
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
*/

/*
        /// <summary>
        /// If true, automatically adds skill-tree pseudo attributes to stat tracking (Use menu to turn on)(Default:false)
        /// </summary>
        public static bool AutoTrackStats = true;
*/

/*
        public static InventoryViewModel ItemInfoVal;

        /// <summary>
        /// The item information equipped in skill-tree
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
*/

        /// <summary>
        /// The dialog coordinator
        /// </summary>
        private static IExtendedDialogCoordinator _dialogCoordinatorVal;

        /// <summary>
        /// The dialog coordinator
        /// </summary>
        public static PoESkillTree.ViewModels.IExtendedDialogCoordinator SharedDialogCoordinator
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

/*
        /// <summary>Updates the stat definitions from node attributes.</summary>
        /// <param name="TargetAttributes">The target attributes.</param>
        /// <returns>System.String[].</returns>
        public static string[] UpdateStatDefinitions(Dictionary<string, IReadOnlyList<float>> TargetAttributes)
        {
            //new Dictionary<string, IReadOnlyList<float>>();
            return null;
        }
*/
    }
}