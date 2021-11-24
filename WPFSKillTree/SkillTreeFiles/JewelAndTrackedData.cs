// ***********************************************************************
// Code Created by James Michael Armstrong (https://github.com/BlazesRus)
// ***********************************************************************
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.TreeGenerator.Model.PseudoAttributes;
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
#if PoESkillTree_EnableItemInfluencedGenerator
using PoESkillTree.ViewModels;
using PoESkillTree.ViewModels.Equipment;
#endif
#if (PoESkillTree_DisableStatTracking==false)
//using PoESkillTree.TreeGenerator.Model.PseudoAttributes;
#endif


/// <summary>
/// The PoESkillTree namespace
/// </summary>
namespace PoESkillTree.SkillTreeFiles
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
                affectedNodes = SkillTree.Skillnodes.Where(n => (n.Value.Position - nodePosition).Length < MaxRange && (n.Value.Position - nodePosition).Length > MinRange).ToList();//Need to update search code here
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
                if (ClosestNotable == NullNode || ClosestKeystone == NullNode)
                {
                    MinRange = MaxRange;
                    MaxRange += 600.0;
                }
            } while (ClosestNotable == null || ClosestKeystone == null);
            SlotDesc = "Closest Keystone to jewel-slot is " + ClosestKeystone.Name + ".";
            SlotDesc += "\nClosest Notable to jewel-slot is " + ClosestNotable.Name + ".";
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
    /// Implements the <see cref="Dictionary{ushort, JewelNodeData}" />
    /// </summary>
    /// <seealso cref="Dictionary{ushort, JewelNodeData}" />
    [Serializable]
    public class JewelData : Dictionary<ushort, JewelNodeData>
    {
        /// <summary>  Initialize JewelData with CategorizeJewelNodes method once(and then set to false) after skilltree nodes are finished generating onto tree</summary>
        public bool NotSetup;
#if DEBUG
        public int NumberOfJewelsFound;
#endif

        public void SetThresholdType(ushort Id, ThresholdTypes Value)
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
#if DEBUG
            NumberOfJewelsFound = 0;
#endif
        }

        /// <summary>
        /// Adds the jewel slot.
        /// </summary>
        /// <param name="nodeID">The node identifier.</param>
        public void AddJewelSlot(ushort nodeID)
        {
            Add(nodeID, new JewelNodeData());
#if DEBUG
            ++NumberOfJewelsFound;
#endif
        }

        /// <summary>
        /// Generate JewelDictionary Categories from  Data from SkillTree and add extra fake attributes to label threshold type and Node id for identifying node in inventory view
        /// </summary>
        public void CategorizeJewelSlots()
        {
            bool IsStrThreshold;
            bool IsIntThreshold;
            bool IsDexThreshold;

            string StrThresholdLabel = "+# Str JewelSlot";
            string IntThresholdLabel = "+# Int JewelSlot";
            string DexThresholdLabel = "+# Dex JewelSlot";
            string AttributeName = "";
            float AttributeTotal;
            List<float> SingleVal = new List<float>(1);
            SingleVal.Add(1);
            IEnumerable<PassiveNodeViewModel>? affectedNodes;
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

                    //New Area scan code based on JewelRadiusDrawer.cs::DrawNodeHighlights
                    Engine.GameModel.Items.JewelRadius mediumJewelArea = Engine.GameModel.Items.JewelRadius.Medium;
                    var radius = Engine.GameModel.Items.JewelRadiusExtensions.GetRadius(mediumJewelArea, SkillTree.Skillnodes[NodeId].ZoomLevel);
                    affectedNodes = SkillTree.Skillnodes.Values
                        .Where(n => !n.IsRootNode && !n.IsAscendancyNode)
                        .Where(n => Distance(n.Position, SkillTree.Skillnodes[NodeId].Position) <= radius * 1.2f);
                    foreach (var n in affectedNodes)
                    {
                        if (n.Attributes != null && n.Attributes.ContainsKey(AttributeName))
                        {
                            AttributeTotal += n.Attributes[AttributeName][0];
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
                //this[NodeId].GenerateSlotDesc(NodeId);//Not needed at moment
            }
#if DEBUG
            Console.WriteLine("Number of Large+Normal JewelSlots found:" + NumberOfJewelsFound);
#endif
        }

        private static float Distance(Vector2D a, Vector2D b)
        {
            var xDistance = a.X - b.X;
            var yDistance = a.Y - b.Y;
            return (float)Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }

        public static explicit operator List<ushort>(JewelData self)
        {
            return new List<ushort>(self.Keys);
        }

        /// <summary>
        /// Property that converts JewelDictionary into List of node ids
        /// </summary>
        public List<ushort> JewelIds { get { return (List<ushort>)this; } }

        /// <summary>
        /// Calculates the total of target attribute inside jewel area.
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        /// <param name="AttributeName">Name of Attribute to search for</param>
        /// <param name="SkilledNodes">The skilled nodes.</param>
        /// <param name="JewelRadiusType">Jewel Radius Type(Large/Medium/Small)(Default:Large"")</param>
        /// <returns></returns>
        static public float CalculateTotalOfAttributeInJewelArea(PassiveNodeViewModel TargetNode, string AttributeName, Dictionary<ushort, PassiveNodeViewModel> SkilledNodes, Engine.GameModel.Items.JewelRadius JewelArea = Engine.GameModel.Items.JewelRadius.Large)
        {
            float AttributeTotal = 0.0f;

            //New Area scan code based on JewelRadiusDrawer.cs::DrawNodeHighlights
            IEnumerable<PassiveNodeViewModel>? affectedNodes;
            var radius = Engine.GameModel.Items.JewelRadiusExtensions.GetRadius(JewelArea, TargetNode.ZoomLevel);
            affectedNodes = SkilledNodes.Values
                .Where(n => !n.IsRootNode && !n.IsAscendancyNode)
                .Where(n => Distance(n.Position, TargetNode.Position) <= radius);
            foreach (var n in affectedNodes)
            {
                if (n.Attributes.ContainsKey(AttributeName) && SkilledNodes.ContainsValue(n))
                {
                    AttributeTotal += n.Attributes[AttributeName][0];
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
        static public float CalculateTotalUnallocAttributeInJewelArea(PassiveNodeViewModel TargetNode, string AttributeName, Dictionary<ushort, PassiveNodeViewModel> SkilledNodes, Engine.GameModel.Items.JewelRadius JewelArea = Engine.GameModel.Items.JewelRadius.Large)
        {
            float AttributeTotal = 0.0f;
            IEnumerable<PassiveNodeViewModel>? affectedNodes;
            var radius = Engine.GameModel.Items.JewelRadiusExtensions.GetRadius(JewelArea, TargetNode.ZoomLevel);
            affectedNodes = SkilledNodes.Values
                .Where(n => !n.IsRootNode && !n.IsAscendancyNode)
                .Where(n => Distance(n.Position, TargetNode.Position) <= radius * 1.2f);
            foreach (var n in affectedNodes)
            {
                if (n.Attributes.ContainsKey(AttributeName) && !SkilledNodes.ContainsValue(n))
                {
                    AttributeTotal += n.Attributes[AttributeName][0];
                }
            }
            return AttributeTotal;
        }
    }

#if (PoESkillTree_DisableStatTracking == false)
    /// <summary>
    /// Derived from PseudoAttribute with only the Attributes(Values and display stored separately)
    /// </summary>
    public class PseudoStat//Referring to as Stat instead of Attribute to reduce class conflicts
    {
        /// <summary>
        /// Gets the list of Attributes this PseudoAttribute contains.
        /// </summary>
        public List<TreeGenerator.Model.PseudoAttributes.Attribute> RelatedAttributes { get; set; }

        /// <summary>
        /// Creates a new PseudoAttribute with the given name and group
        /// and an empty list of Attributes.
        /// </summary>
        /// <param name="name">Name (not null)</param>
        /// <param name="relatedAttributes">Listed of Attributes related to stat</param>
        public PseudoStat(List<TreeGenerator.Model.PseudoAttributes.Attribute> relatedAttributes)
        {
            //Name = name ?? throw new ArgumentNullException(nameof(name));
            RelatedAttributes = new List<TreeGenerator.Model.PseudoAttributes.Attribute>();
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
            List<float> RetrievedVal = new List<float>() { };
            foreach (var Attribute in RelatedAttributes)
            {
                AttributeName = Attribute.Name;
                Multiplier = Attribute.ConversionMultiplier;
                if(attrlist.ContainsKey(AttributeName))//attrlist.TryGetValue(AttributeName, out RetrievedVal);//Causes error so using less effective version instead for now
                {
                    TotalStat += Multiplier * attrlist[AttributeName][0];
                }
            }
            return TotalStat;
        }

        public PseudoStat(PseudoAttribute Target)
        {
            RelatedAttributes = Target.Attributes;
        }
    }

    /// <summary>
    /// Stores Dictionary linked to PseudoStats (Stat Calculations stored inside Dictionary<string, float> values)
    /// </summary>
    public class TrackedAttributes : Dictionary<string, PseudoStat>
    {
        public TrackedAttributes CloneSelf()
        {
            TrackedAttributes NewSelf = this;
            return NewSelf;
        }

        /// <summary>
        /// Starts the tracking of Pseudo-attributes(to display in stat calculations).
        /// </summary>
        /// <param name="pseudoAttributeConstraints">The pseudo attribute constraints.</param>
        public void StartTracking(Dictionary<string, Tuple<float, double>> attributeConstraints, Dictionary<PseudoAttribute, Tuple<float, double>> pseudoAttributeConstraints, WeaponClass primaryWeapon, OffHand offHandType, Tags tags)
        {
            PseudoCalcGlobals.PrimaryWeapon = primaryWeapon;
            PseudoCalcGlobals.OffHandType = offHandType;
            PseudoCalcGlobals.Tags = tags;
            foreach (PseudoAttribute Attribute in pseudoAttributeConstraints.Keys)//Don't need target value and weight
            {
                Add(Attribute.Name, new PseudoStat(Attribute.Attributes));
            }
        }

        //internal void Add(string name, PseudoAttribute item)
        //{
        //    PseudoStat newStat = new PseudoStat(item);
        //    Add(name, newStat);
        //}
    }
#endif

    /// <summary>
    /// Class PseudoCalcGlobals.
    /// </summary>
    public static class PseudoCalcGlobals
    {

        /// <summary>
        /// Static Property Event(http://10rem.net/blog/2011/11/29/wpf-45-binding-and-change-notification-for-static-properties)
        /// </summary>
        public static event EventHandler<PropertyChangedEventArgs>? StaticPropertyChanged;
        /// <summary>
        /// Static Property Event(http://10rem.net/blog/2011/11/29/wpf-45-binding-and-change-notification-for-static-properties)
        /// </summary>
        public static void NotifyStaticPropertyChanged(string propertyName)
        {
            if (StaticPropertyChanged != null)
                StaticPropertyChanged(null, new PropertyChangedEventArgs(propertyName));
        }

#if PoESkillTree_EnableSubtotalFeatures
        public const string HybridHPKey = "# HybridHP Subtotal(PseudoCalc)";
        public const string AccKey = "# Accuracy Subtotal(PseudoCalc)";
        public const string HPTotalKey = "# HP Subtotal(PseudoCalc)";
        public const string CritsPerSecKey = "# critical hits per second(PseudoCalc)";
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

        /// <summary>Add Whispering Ice based calculations(only for Staff Primary)</summary>
        /// <value>
        ///   <c>true</c> if [applying whispering ice stats]; otherwise, <c>false</c>.</value>
        public static bool ApplyWhisperingIceStats { get => applyWhisperingIceStats; set => applyWhisperingIceStats = value; }

        /// <summary>  Quality 20 Level 20 Nightblade active for attack critical chance (Only for claws and daggers) </summary>
        public static bool NightbladeActive { get => nightbladeActive; set => nightbladeActive = value; }

        public static bool CalculateAcc { get => calculateAcc; set { calculateAcc = value; NotifyStaticPropertyChanged("CalculateAcc"); } }
        public static bool CalculateHP { get => calculateHP; set { calculateHP = value; NotifyStaticPropertyChanged("CalculateHP"); } }
        public static bool CalculateHybridHP { get => calculateHybridHP; set { calculateHybridHP = value; NotifyStaticPropertyChanged("CalculateHybridHP"); } }
        public static bool CalculateCritsPerSec { get => calculateCritsPerSec; set { calculateCritsPerSec = value; NotifyStaticPropertyChanged("CalculateCritsPerSec"); } }
        public static bool CalculateCritSpellMult { get => calculateCritSpellMult; set { calculateCritSpellMult = value; NotifyStaticPropertyChanged("CalculateCritSpellMult"); } }
        public static bool LuckyCrits { get => luckyCrits; set { luckyCrits = value; NotifyStaticPropertyChanged("LuckyCrits"); } }
        public static bool IncreasedCritActive { get => increasedCritActive; set { increasedCritActive = value; NotifyStaticPropertyChanged("IncreasedCritActive"); } }
        public static int LevelPrecisionActive { get => levelPrecisionActive; set { levelPrecisionActive = value; NotifyStaticPropertyChanged("LevelPrecisionActive"); } }
#endif

#if PoESkillTree_EnableCritsPerSecondFeatures
        private static float primaryATKSpeed = 1.20f;
        private static float primaryCrit = 6.50f;
        private static float secondaryATKSpeed = 1.20f;
        private static float secondaryCrit = 6.50f;

        public static float PrimaryATKSpeed { get => primaryATKSpeed; set { primaryATKSpeed = value; NotifyStaticPropertyChanged("PrimaryATKSpeed"); } }
        public static float PrimaryCrit { get => primaryCrit; set { primaryCrit = value; NotifyStaticPropertyChanged("PrimaryCrit"); } }
        public static float SecondaryATKSpeed { get => secondaryATKSpeed; set { secondaryATKSpeed = value; NotifyStaticPropertyChanged("SecondaryATKSpeed"); } }
        public static float SecondaryCrit { get => secondaryCrit; set { secondaryCrit = value; NotifyStaticPropertyChanged("SecondaryCrit"); } }
#endif
        /*
                public enum SpellBehaviorTypes
                {
                    [Description("ProjectileSpell")]
                    ProjectileSpell,
                    [Description("AreaSpell")]
                    AreaSpell,
                    [Description("ChannelledAreaSpell")]
                    ChannelledAreaSpell
                }

                private static SpellBehaviorTypes spellBehaviorType = SpellBehaviorTypes.ProjectileSpell;


                public static SpellBehaviorTypes SpellBehaviorType { get => spellBehaviorType; set { spellBehaviorType = value; NotifyStaticPropertyChanged("SpellBehaviorType"); } }
        */

        public static WeaponClass primaryWeapon = WeaponClass.Dagger;
        public static WeaponClass secondaryWeapon = WeaponClass.Dagger;
        public static OffHand offHandType = OffHand.DualWield;

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

        private static DamageScaling dMScaling = DamageScaling.Ice;
        private static int numberOfPoisonStacks = 0;

        public static WeaponClass PrimaryWeapon
        {
            get => primaryWeapon;
            set
            {
                primaryWeapon = value; NotifyStaticPropertyChanged("PrimaryWeapon");
            }
        }

        public static void SetPrimaryType(WeaponClass value)
        {
            if (primaryWeapon != value)
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

        public static void SetSecondaryType(WeaponClass value)
        {
            if(secondaryWeapon != value)
            {
                secondaryWeapon = value; NotifyStaticPropertyChanged("SecondaryWeapon");
            }
        }

        public static OffHand OffHandType
        {
            //get => offHandType;
            get
            {
                if (SecondaryWeapon != WeaponClass.Unarmed)
                    SetOffHandType(OffHand.DualWield);
                return offHandType;
            }
            set => SetOffHandType(value);
        }

        public static void SetOffHandType(OffHand value)
        {
            if(offHandType!= value)
            {
                if (value == OffHand.TwoHanded || value == OffHand.Shield)
                    SetSecondaryType(WeaponClass.Unarmed);//Updates on Change but UI will not display at current implementation of code
                offHandType = value;
                NotifyStaticPropertyChanged("OffHandType");
            }
        }

        public static Tags tags = Tags.None;

        /// <summary>
        /// Tags used for pseudo attribute calculations.
        /// </summary>
        public static Tags Tags
        {
            get => tags;
            set
            {
                tags = value; NotifyStaticPropertyChanged("Tags");
            }
        }

        public static void SetTags(Tags value)
        {
            if (tags != value)
            {
                tags = value; NotifyStaticPropertyChanged("Tags");
            }
        }

        //public static bool NotUsingShield { get => SecondaryWeapon == Model.PseudoAttributes.OffHand.Shield; }
        public static DamageScaling DMScaling { get => dMScaling; set { dMScaling = value; NotifyStaticPropertyChanged("DMScaling"); } }
        public static int NumberOfPoisonStacks { get => numberOfPoisonStacks; set { numberOfPoisonStacks = value; NotifyStaticPropertyChanged("NumberOfPoisonStacks"); } }

        public static bool enableTrackedStatDisplay = true;

        public static bool EnableTrackedStatDisplay
        {
            get => enableTrackedStatDisplay;
            set
            {
                enableTrackedStatDisplay = value; NotifyStaticPropertyChanged("EnableTrackedStatDisplay");
            }
        }
    }

    public static class GlobalSettings
    {
#if PoESkillTree_UseStaticPointSharing
//        public static Dictionary<string, int> points = new Dictionary<string, int>()
//        {
//            {"NormalUsed", 0},
//            {"NormalTotal", 22},
//            {"AscendancyUsed", 0},
//            {"AscendancyTotal", 8},
//        };
#endif

        /// <summary>
        /// Stored JewelInfo
        /// </summary>
        public static JewelData JewelStorage = new JewelData();
        /// <summary>
        /// Static Property Event(http://10rem.net/blog/2011/11/29/wpf-45-binding-and-change-notification-for-static-properties)
        /// </summary>
        public static event EventHandler<PropertyChangedEventArgs>? StaticPropertyChanged;
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
                if (StatTrackingSavePathVal == null|| StatTrackingSavePathVal == "")
                    SetTrackedPathFolder(AppData.ProgramDirectory);
#pragma warning disable CS8603 // Possible null reference return.(Handled in previous line)
                return StatTrackingSavePathVal;
#pragma warning restore CS8603 // Possible null reference return.
            }
            set
            {
                if (value != null && value != "" && StatTrackingSavePathVal != value)
                {
                    StatTrackingSavePathVal = value;
                    NotifyStaticPropertyChanged("StatTrackingSavePath");
                }
            }
        }

        public static void SetTrackedPathFolder(string currentPath)
        {
            if (currentPath != null && currentPath != "" && StatTrackingSavePathVal != currentPath)
            {
                StatTrackingSavePathVal = currentPath;
                NotifyStaticPropertyChanged("StatTrackingSavePath");
            }
        }


        /// <summary>
        /// If true, automatically adds skill-tree pseudo attributes to stat tracking (Use menu to turn on)(Default:false)
        /// </summary>
        private static bool autoTrackStats = false;

        /// <summary>
        /// If true, automatically adds skill-tree pseudo attributes to stat tracking (Use menu to turn on)(Default:false)
        /// </summary>
        public static bool AutoTrackStats
        {
            get { return autoTrackStats; }
            set
            {
                if (AutoTrackStats != value)
                {
                    AutoTrackStats = value; NotifyStaticPropertyChanged("AutoTrackStats");
                }
            }
        }

        public static void SetAutoTrackStats(bool value)
        {
            if (AutoTrackStats != value)
            {
                AutoTrackStats = value; NotifyStaticPropertyChanged("AutoTrackStats");
            }
        }

        public static string TrackedFileFallbackValue = Path.Combine(GlobalSettings.StatTrackingSavePath, "StatTracking" + Path.DirectorySeparatorChar + "CurrentTrackedAttributes.txt");

        private static string _CurrentTrackedFile = TrackedFileFallbackValue;

        public static string CurrentTrackedFile
        {
            get { return _CurrentTrackedFile; }
            set
            {
                if (value != "" && value != null && value != CurrentTrackedFile)
                {
                    _CurrentTrackedFile = value;
                    NotifyStaticPropertyChanged("CurrentTrackedFile");
                }
            }
        }

        public static void SetCurrentTrackedFile(string value)
        {
            if (value != "" && value != null && _CurrentTrackedFile != value)
            {
                _CurrentTrackedFile = value; NotifyStaticPropertyChanged("CurrentTrackedFile");
            }
        }

        public static readonly Regex _backreplace = new Regex("#");

#if PoESkillTree_EnableItemInfluencedGenerator
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
#endif

        /// <summary>
        /// The dialog coordinator
        /// </summary>
        private static IExtendedDialogCoordinator? _dialogCoordinatorVal;

        /// <summary>
        /// The dialog coordinator
        /// </summary>
        public static IExtendedDialogCoordinator? SharedDialogCoordinator
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

        //To-Do Store Ascendancy ids inside dictionary or list and use for displaying build skill point total without including ascendancy node points
        //Tree.AscRootNodeList holds current ascendancy root nodes
    }
}