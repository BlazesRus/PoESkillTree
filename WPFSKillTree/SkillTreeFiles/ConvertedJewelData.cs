using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POESKillTree.Model.Items;
using POESKillTree.ViewModels.Equipment;

namespace POESKillTree.SkillTreeFiles
{
    //(Most of ConvertedJewelData node searching code based on https://github.com/PoESkillTree/PoESkillTree/issues/163)
    //Point p = ((MouseEventArgs)e.OriginalSource).GetPosition(zbSkillTreeBackground.Child);
    //var v = new Vector2D(p.X, p.Y);
    //v = v * _multransform + _addtransform;
    //IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
    //    SkillTree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();

    /// <summary>
    /// 
    /// </summary>
    public class ConvertedJewelData
    {
        /// <summary>
        /// Jewels the name of the slot.
        /// </summary>
        /// <param name="NodeId">The node identifier.</param>
        /// <returns></returns>
        public static string JewelSlotName(int NodeId)
        {
            string JewelName;
            switch (NodeId)
            {
                //Int Jewels
                case JSlot_Int_WitchID://Jewel Slot directly north of Witch starting area
                    JewelName = "JSlot_Int_Witch"; break;
                case JSlot_Int_ScionID://Jewel slot far NE of Scion Starting Area; Nearest Jewel to CI area (Int Threshold Jewel Slot)
                    JewelName = "JSlot_Int_Scion"; break;
                case JSlot_Int_WitchShadowID://NE from center jewel slot between Witch and shadow areas
                    JewelName = "JSlot_Int_WitchShadow"; break;
                case JSlot_Int_TemplarWitchID://Jewel slot north-west of Scion area; At road between Templar and Witch areas
                    JewelName = "JSlot_Int_TemplarWitch"; break;
                //Str Jewels
                case JSlot_Str_WarriorDuelistID://Jewel slot south-west of Scion area; At road between Marauder and Duelist areas
                    JewelName = "JSlot_Str_WarriorDuelist"; break;
                case JSlot_Str_WarriorTemplarScionID://Jewel slot west of Scion area; At road between Marauder and Templar areas
                    JewelName = "JSlot_Str_WarriorTemplarScion"; break;
                case JSlot_Str_FarWarTempScionID://Jewel slot far west of Scion area; At road between Marauder and Templar areas; Nearest jewel slot to Resolute Technique
                    JewelName = "JSlot_Str_FarWarTempScion"; break;
                case JSlot_Str_WarriorID://Jewel slot west of Marauder area
                    JewelName = "JSlot_Str_Warrior"; break;
                //Dex Jewels
                case JSlot_Dex_ShadowRangerID://Jewel Slot east of Scion starting area between Shadow and Ranger areas(above Ranger area); Nearest jewel slot to Charisma passive node
                    JewelName = "JSlot_Dex_ShadowRanger"; break;
                case JSlot_Dex_RangerID://Jewel slot east of Ranger area(Jewel10)
                    JewelName = "JSlot_Dex_Ranger"; break;
                case JSlot_Dex_RangerDuelistID://Jewel slot south-east of Scion area; At road between Ranger and Duelist areas
                    JewelName = "JSlot_Dex_RangerDuelist"; break;
                //Hybrid Jewels
                case JSlot_StrInt_TemplarID://Jewel slot west of Templar starting area
                    JewelName = "JSlot_StrInt_Templar"; break;
                case JSlot_StrInt_ScionID://Scion Jewel Slot west of starting area
                    JewelName = "JSlot_StrInt_Scion"; break;
                case JSlot_DexInt_ShadowID://Jewel slot east of Shadow starting area
                    JewelName = "JSlot_DexInt_Shadow"; break;
                case JSlot_DexInt_ScionID://Scion jewel slot east of starting area
                    JewelName = "JSlot_DexInt_Scion"; break;
                case JSlot_StrDex_ScionID://Scion Jewel Slot south of starting area
                    JewelName = "JSlot_StrDex_Scion"; break;
                case JSlot_StrDex_DuelistID://Jewel slot south of Duelist starting area
                    JewelName = "JSlot_StrDex_Duelist"; break;
                //Non-Threshold Jewel Slots
                case JSlot_Neutral_AcrobaticsID:
                    JewelName = "JSlot_Neutral_IronGrip"; break;
                case JSlot_Neutral_PointBlankID:
                    JewelName = "JSlot_Neutral_PointBlank"; break;
                case JSlot_Neutral_MinionInstabilityID:
                    JewelName = "JSlot_Neutral_MinionInstability"; break;
                case JSlot_Neutral_IronGripID:
                    JewelName = "JSlot_Neutral_IronGrip"; break;
                default:
                    JewelName = "JSlot_"+NodeId;
                    break;
            }
            return JewelName;
        }

        /// <summary>
        /// Jewel Slot directly north of Witch starting area
        /// </summary>
        public const ushort JSlot_Int_WitchID = 61419;
        /// <summary>
        /// Jewel slot far NE of Scion Starting Area; Nearest Jewel to CI area (Int Threshold Jewel Slot)
        /// </summary>
        public const ushort JSlot_Int_ScionID = 21984;
        /// <summary>
        /// NE from center jewel slot between Witch and shadow areas
        /// </summary>
        public const ushort JSlot_Int_WitchShadowID = 41263;
        /// <summary>
        /// Jewel slot north-west of Scion area; At road between Templar and Witch areas
        /// </summary>
        public const ushort JSlot_Int_TemplarWitchID = 36634;

        /// <summary>
        /// Jewel slot south-west of Scion area; At road between Marauder and Duelist areas
        /// </summary>
        public const ushort JSlot_Str_WarriorDuelistID = 28475;
        /// <summary>
        /// Jewel slot west of Scion area; At road between Marauder and Templar areas
        /// </summary>
        public const ushort JSlot_Str_WarriorTemplarScionID = 33631;
        /// <summary>
        /// Jewel slot far west of Scion area; At road between Marauder and Templar areas; Nearest jewel slot to Resolute Technique
        /// </summary>
        public const ushort JSlot_Str_FarWarTempScionID = 55190;
        /// <summary>
        /// Jewel slot west of Marauder area
        /// </summary>
        public const ushort JSlot_Str_WarriorID = 26725;

        /// <summary>
        /// Jewel Slot east of Scion starting area between Shadow and Ranger areas(above Ranger area); Nearest jewel slot to Charisma passive node
        /// </summary>
        public const ushort JSlot_Dex_ShadowRangerID = 33989;
        /// <summary>
        /// Jewel slot east of Ranger area
        /// </summary>
        public const ushort JSlot_Dex_RangerID = 60735;
        /// <summary>
        /// Jewel slot south-east of Scion area; At road between Ranger and Duelist areas
        /// </summary>
        public const ushort JSlot_Dex_RangerDuelistID = 34483;

        /// <summary>
        /// Jewel slot west of Templar starting area
        /// </summary>
        public const ushort JSlot_StrInt_TemplarID = 26196;
        /// <summary>
        /// Scion Jewel Slot west of starting area
        /// </summary>
        public const ushort JSlot_StrInt_ScionID = 6230;
        /// <summary>
        /// Jewel slot east of Shadow starting area
        /// </summary>
        public const ushort JSlot_DexInt_ShadowID = 61834;
        /// <summary>
        /// Scion jewel slot east of starting area
        /// </summary>
        public const ushort JSlot_DexInt_ScionID = 48768;
        /// <summary>
        /// Scion Jewel Slot south of starting area
        /// </summary>
        public const ushort JSlot_StrDex_ScionID = 31683;
        /// <summary>
        /// Jewel slot south of Duelist starting area
        /// </summary>
        public const ushort JSlot_StrDex_DuelistID = 54127;

        /// <summary>
        /// Jewel Slot far east of Scion starting area between Shadow and Ranger areas; Nearest jewel slot to Acrobatics Jewel (Non-Threshold Jewel Slot)
        /// </summary>
        public const ushort JSlot_Neutral_AcrobaticsID = 32763;
        /// <summary>
        /// Jewel slot far south-west of center; Located between Marauder and Duelist areas next to Iron Grip (Non-Threshold jewel slot)
        /// </summary>
        public const ushort JSlot_Neutral_IronGripID = 2491;
        /// <summary>
        /// Jewel slot far south-east of center; Located between Duelist and Ranger areas next to Point Blank (Non-Threshold jewel slot)
        /// </summary>
        public const ushort JSlot_Neutral_PointBlankID = 46882;
        /// <summary>
        /// Jewel slot far north-west of center; Located between Templar and Witch areas next to Minion-Instability (Non-Threshold jewel slot)
        /// </summary>
        public const ushort JSlot_Neutral_MinionInstabilityID = 7960;

        /// <summary>
        /// The fake intuitive leap support attribute
        /// </summary>
        public static readonly string FakeIntuitiveLeapSupportAttribute = "+# IntuitiveLeapSupports";
        /// <summary>
        /// Calculates the total of target attribute inside jewel area.
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        /// <param name="AttributeName">Name of Attribute to search for</param>
        /// <param name="SkilledNodes">The skilled nodes.</param>
        /// <param name="JewelRadiusType">Jewel Radius Type(Large/Medium/Small)(Default:Large"")</param>
        /// <returns></returns>
        static public float CalculateTotalOfAttributeInJewelArea(SkillNode TargetNode, string AttributeName, Utils.ObservableSet<SkillNode> SkilledNodes, string JewelRadiusType="")
        {
            int JewelRadius;
            switch(JewelRadiusType)
            {
                case "Medium":
                    JewelRadius = 1200;
                    break;
                case "Small"://Large
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
        /// Applies the fake Intuitive Leap Support attribute to nodes in effected area
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        static public void ApplyIntuitizeLeapSupport(POESKillTree.SkillTreeFiles.SkillNode TargetNode)
        {
            List<float> BlankList = new List<float>(1);
            BlankList.Add(1.0f);
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
                if (!CurrentNode.Attributes.ContainsKey(FakeIntuitiveLeapSupportAttribute) && attributeSize != 0) //&& CurrentNode.ascendancyName =="")
                {
                    CurrentNode.Attributes.Add(FakeIntuitiveLeapSupportAttribute, BlankList);
                    ExtendedAttribute = new string[attributeSize + 1];
                    for (int index = 0; index < attributeSize; ++index)
                    {
                        ExtendedAttribute[index] = CurrentNode.attributes[index];
                    }
                    ExtendedAttribute[attributeSize] = "+1 IntuitiveLeapSupports";
                    CurrentNode.attributes = ExtendedAttribute;
                }
            }
        }
        /// <summary>
        /// Removes the fake Intuitive Leap Support attribute to nodes in effected area
        /// </summary>
        /// <param name="TargetNode">The target node.</param>
        static public void RemoveIntuitizeLeapSupport(POESKillTree.SkillTreeFiles.SkillNode TargetNode)
        {
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
                if (CurrentNode.Attributes.ContainsKey(FakeIntuitiveLeapSupportAttribute))
                {
                    CurrentNode.Attributes.Remove(FakeIntuitiveLeapSupportAttribute);
                    ExtendedAttribute = new string[attributeSize];
                    int index = CurrentNode.attributes.Length - 1;
                    Array.Copy(CurrentNode.attributes, CurrentNode.attributes.Length, ExtendedAttribute, CurrentNode.attributes.Length - 1, 0);
                    CurrentNode.attributes = ExtendedAttribute;
                }
            }
        }
        public class JewelUpdateData
        {
            public ushort NodeID;
            public POESKillTree.Model.Items.Item CurrentJewelData;
            public SkillNode CurrentNode;

            public JewelUpdateData(ushort nodeID, Item currentJewelData)
            {
                NodeID = nodeID;
                CurrentJewelData = currentJewelData;
                CurrentNode = null;
            }

            public JewelUpdateData(ushort nodeID, Item currentJewelData, SkillNode currentNode)
            {
                NodeID = nodeID;
                CurrentJewelData = currentJewelData;
                CurrentNode = currentNode;
            }
        }

        static public JewelUpdateData JewelBasedStatInitialUpdate(int JewelIndex, InventoryViewModel ItemInfo)
        {
            ushort NodeID;
            POESKillTree.Model.Items.Item CurrentJewelData;

            switch (JewelIndex)
            {
                case 0:
                    NodeID = ConvertedJewelData.JSlot_DexInt_ScionID;
                    CurrentJewelData = ItemInfo.JSlot_DexInt_Scion.Item; break;
                case 1:
                    NodeID = ConvertedJewelData.JSlot_DexInt_ShadowID;
                    CurrentJewelData = ItemInfo.JSlot_DexInt_Shadow.Item; break;
                case 2:
                    NodeID = ConvertedJewelData.JSlot_Dex_RangerDuelistID;
                    CurrentJewelData = ItemInfo.JSlot_Dex_RangerDuelist.Item; break;
                case 3:
                    NodeID = ConvertedJewelData.JSlot_Dex_RangerID;
                    CurrentJewelData = ItemInfo.JSlot_Dex_Ranger.Item; break;
                case 4:
                    NodeID = ConvertedJewelData.JSlot_Dex_ShadowRangerID;
                    CurrentJewelData = ItemInfo.JSlot_Dex_ShadowRanger.Item; break;
                case 5:
                    NodeID = ConvertedJewelData.JSlot_Int_ScionID;
                    CurrentJewelData = ItemInfo.JSlot_Int_Scion.Item; break;
                case 6:
                    NodeID = ConvertedJewelData.JSlot_Int_TemplarWitchID;
                    CurrentJewelData = ItemInfo.JSlot_Int_TemplarWitch.Item; break;
                case 7:
                    NodeID = ConvertedJewelData.JSlot_Int_WitchID;
                    CurrentJewelData = ItemInfo.JSlot_Int_Witch.Item; break;
                case 8:
                    NodeID = ConvertedJewelData.JSlot_Int_WitchShadowID;
                    CurrentJewelData = ItemInfo.JSlot_Int_WitchShadow.Item; break;
                case 9:
                    NodeID = ConvertedJewelData.JSlot_StrDex_DuelistID;
                    CurrentJewelData = ItemInfo.JSlot_StrDex_Duelist.Item; break;
                case 10:
                    NodeID = ConvertedJewelData.JSlot_StrDex_ScionID;
                    CurrentJewelData = ItemInfo.JSlot_StrDex_Scion.Item; break;
                case 11:
                    NodeID = ConvertedJewelData.JSlot_StrInt_ScionID;
                    CurrentJewelData = ItemInfo.JSlot_StrInt_Scion.Item; break;
                case 12:
                    NodeID = ConvertedJewelData.JSlot_StrInt_TemplarID;
                    CurrentJewelData = ItemInfo.JSlot_StrInt_Templar.Item; break;
                case 13:
                    NodeID = ConvertedJewelData.JSlot_Str_FarWarTempScionID;
                    CurrentJewelData = ItemInfo.JSlot_Str_FarWarTempScion.Item; break;
                case 14:
                    NodeID = ConvertedJewelData.JSlot_Str_WarriorDuelistID;
                    CurrentJewelData = ItemInfo.JSlot_Str_WarriorDuelist.Item; break;
                case 15:
                    NodeID = ConvertedJewelData.JSlot_Str_WarriorID;
                    CurrentJewelData = ItemInfo.JSlot_Str_Warrior.Item; break;
                case 16:
                    NodeID = ConvertedJewelData.JSlot_Str_WarriorTemplarScionID;
                    CurrentJewelData = ItemInfo.JSlot_Str_WarriorTemplarScion.Item; break;
                //Non-Threshold Jewel Slots below
                case 17:
                    NodeID = ConvertedJewelData.JSlot_Neutral_AcrobaticsID;
                    CurrentJewelData = ItemInfo.JSlot_Neutral_Acrobatics.Item; break;
                case 18:
                    NodeID = ConvertedJewelData.JSlot_Neutral_IronGripID;
                    CurrentJewelData = ItemInfo.JSlot_Neutral_IronGrip.Item; break;
                case 19:
                    NodeID = ConvertedJewelData.JSlot_Neutral_MinionInstabilityID;
                    CurrentJewelData = ItemInfo.JSlot_Neutral_MinionInstability.Item; break;
                case 20:
                    NodeID = ConvertedJewelData.JSlot_Neutral_PointBlankID;
                    CurrentJewelData = ItemInfo.JSlot_Neutral_PointBlank.Item; break;
                default://Shouldn't ever use this part
                    NodeID = 0;
                    CurrentJewelData = null;
                    break;
            }

            if (NodeID == 0)
            {
                return new JewelUpdateData(NodeID, CurrentJewelData);
            }
            else
            {
                return new JewelUpdateData(NodeID, CurrentJewelData, SkillTree.Skillnodes[NodeID]);
            }
        }

        /// <summary>
        /// Updates stats based on Unique Jewels Slotted
        /// </summary>
        /// <param name="attrlist">The attrlist.</param>
        /// <param name="ItemInfo">The item information.</param>
        /// <param name="Tree">The tree.</param>
        /// <returns></returns>
        static public Dictionary<string, List<float>> JewelBasedStatUpdater(Dictionary<string, List<float>> attrlist, InventoryViewModel ItemInfo, SkillTree Tree)
        {
            float AreaStats;
            ushort NodeID;
            SkillNode CurrentNode;
            POESKillTree.Model.Items.Item CurrentJewelData;
            JewelUpdateData updateData;

            for (int JewelIndex = 0; JewelIndex < 21; ++JewelIndex)
            {
                updateData = JewelBasedStatInitialUpdate(JewelIndex, ItemInfo);
                NodeID = updateData.NodeID;
                CurrentNode = updateData.CurrentNode;
                CurrentJewelData = updateData.CurrentJewelData;
                if (Tree.SkilledNodes.Contains(CurrentNode))
                {
                    if (CurrentJewelData == null)//Jewel Not Equipped
                    {
                        continue;
                    }
                    //else
                    //{
                    //    //Change Node Image to match Jewel
                    //    //CurrentNode.Icon = CurrentJewelData.Image.ToString();
                    //}
                    if (CurrentJewelData.Name == "Intuitive Leap")
                    {
                        //ConvertedJewelData.ApplyIntuitizeLeapSupport(CurrentNode);
                    }
                    else if (CurrentJewelData.Name == "Brute Force Solution")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Strength"][0] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Intelligence"))
                            {
                                attrlist["+# to Intelligence"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Intelligence",new List<float>(1) { AreaStats});
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fluid Motion")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
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
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
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
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
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
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
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
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
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
                    {//
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Energy Shield", Tree.SkilledNodes);
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
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Life", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["#% increased Energy Shield"][0] -= AreaStats;
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
                        float CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Elemental Damage", Tree.SkilledNodes);
                        if (CurrentTotal != 0) { attrlist["#% increased Elemental Damage"][0] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage", Tree.SkilledNodes);
                        if (CurrentTotal != 0) { attrlist["#% increased Cold Damage"][0] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Chaos Damage", Tree.SkilledNodes);
                        if (CurrentTotal != 0) { attrlist["#% increased Chaos Damage"][0] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage", Tree.SkilledNodes);
                        if (CurrentTotal != 0) { attrlist["#% increased Physical Damage"][0] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Lightning Damage", Tree.SkilledNodes);
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
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        AreaStats += ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0.0f)
                        {
                            if (attrlist.ContainsKey("#% increased Melee Damage Bonus(from Might in All Forms)"))
                            {
                                attrlist["#% increased Melee Damage Bonus(from Might in All Forms)"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist["#% increased Melee Damage Bonus(from Might in All Forms)"].Add(, new List<float>(1) { AreaStats });
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Anatomical Knowledge")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes, "Large");
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
                    //else if (CurrentJewelData.Name == "Cold Steel")
                    //{
                    //    float ColdDamageTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage", Tree.SkilledNodes);
                    //    if (ColdDamageTotal != 0) { attrlist["#% increased Cold Damage"][0] -= ColdDamageTotal; }
                    //    float ColdDamageTotalWeap = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage with Weapons", Tree.SkilledNodes);
                    //    if (ColdDamageTotalWeap != 0) { attrlist["#% increased Cold Damage with Weapons"][0] -= ColdDamageTotalWeap; }
                    //    float ColdDamageTotalTwoWeap = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage with Two Handed Melee Weapons", Tree.SkilledNodes);
                    //    if (ColdDamageTotalTwoWeap != 0) { attrlist["#% increased Cold Damage with Two Handed Melee Weapons"][0] -= ColdDamageTotalTwoWeap; }
                    //    //float ColdDamageTotalOneWeap = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage with One Handed Melee Weapons", Tree.SkilledNodes);
                    //    //if (ColdDamageTotalOneWeap != 0) { attrlist["#% increased Cold Damage with One Handed Melee Weapons"][0] -= ColdDamageTotalOneWeap; }
                    //    float PhysDamageTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage", Tree.SkilledNodes);
                    //    if (PhysDamageTotal != 0) { attrlist["#% increased Physical Damage"][0] -= PhysDamageTotal; }
                    //    float PhysDamageTotalWeap = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Weapons", Tree.SkilledNodes);
                    //    if (PhysDamageTotalWeap != 0) { attrlist["#% increased Physical Damage with Weapons"][0] -= PhysDamageTotalWeap; }
                    //    float PhysDamageTotalTwoWeap = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Two Handed Melee Weapons", Tree.SkilledNodes);
                    //    if (PhysDamageTotalTwoWeap != 0) { attrlist["#% increased Physical Damage with Two Handed Melee Weapons"][0] -= PhysDamageTotalTwoWeap; }
                    //    float PhysDamageTotalDual = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Weapon Damage while Dual Wielding", Tree.SkilledNodes);
                    //    if (PhysDamageTotalDual != 0) { attrlist["#% increased Physical Weapon Damage while Dual Wielding"][0] -= PhysDamageTotalDual; }
                    //    float PhysDamageMelee = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Melee Physical Damage", Tree.SkilledNodes);
                    //    if (PhysDamageMelee != 0) { attrlist["#% increased Melee Physical Damage"][0] -= PhysDamageMelee; }
                    //    float PhysDamageShield = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Melee Physical Damage while holding a Shield", Tree.SkilledNodes);
                    //    if (PhysDamageShield != 0) { attrlist["#% increased Melee Physical Damage while holding a Shield"][0] -= PhysDamageShield; }
                    //    float PhysDamageMace = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Maces", Tree.SkilledNodes);
                    //    if (PhysDamageMace != 0) { attrlist["#% increased Melee Physical Damage while holding a Shield"][0] -= PhysDamageMace; }
                    //    float PhysDamageStaves = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Staves", Tree.SkilledNodes);
                    //    if (PhysDamageStaves != 0) { attrlist["#% increased Physical Damage with Staves"][0] -= PhysDamageMace; }
                    //    //float PhysDamageMace = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage with Maces", Tree.SkilledNodes);
                    //    //if (PhysDamageMace != 0) { attrlist["#% increased Melee Physical Damage while holding a Shield"][0] -= PhysDamageShield; }
                    //    if (ColdDamageTotal != 0)
                    //    {
                    //        if (attrlist.ContainsKey("#% increased Physical Damage"))
                    //        {
                    //            attrlist["#% increased Physical Damage"][0] += ColdDamageTotal;
                    //        }
                    //        else
                    //        {
                    //            attrlist["#% increased Physical Damage"].Add(ColdDamageTotal);
                    //        }
                    //    }
                    //    if (PhysDamageTotal != 0)
                    //    {
                    //        if (attrlist.ContainsKey("#% increased Cold Damage"))
                    //        {
                    //            attrlist["#% increased Cold Damage"][0] += PhysDamageTotal;
                    //        }
                    //        else
                    //        {
                    //            attrlist["#% increased Cold Damage"].Add(PhysDamageTotal);
                    //        }
                    //    }
                    //}

                }
            }
            return attrlist;
        }

        /// <summary>
        /// Updates stats based on Unique Jewels Slotted
        /// </summary>
        /// <param name="attrlist">The attrlist.</param>
        /// <param name="ItemInfo">The item information.</param>
        /// <param name="Tree">The tree.</param>
        /// <returns></returns>
        static public Dictionary<string, float> JewelBasedStatUpdater(Dictionary<string, float> attrlist, InventoryViewModel ItemInfo, SkillTree Tree)
        {
            float AreaStats;
            ushort NodeID;
            SkillNode CurrentNode;
            POESKillTree.Model.Items.Item CurrentJewelData;
            JewelUpdateData updateData;

            for (int JewelIndex = 0; JewelIndex < 21; ++JewelIndex)
            {
                updateData = JewelBasedStatInitialUpdate(JewelIndex, ItemInfo);
                NodeID = updateData.NodeID;
                CurrentNode = updateData.CurrentNode;
                CurrentJewelData = updateData.CurrentJewelData;
                if (Tree.SkilledNodes.Contains(CurrentNode))
                {
                    if (CurrentJewelData == null)//Jewel Not Equipped
                    {
                        continue;
                    }
                    //else
                    //{
                    //    //Change Node Image to match Jewel
                    //    //CurrentNode.Icon = CurrentJewelData.Image.ToString();
                    //}
                    if (CurrentJewelData.Name == "Intuitive Leap")
                    {
                        //ConvertedJewelData.ApplyIntuitizeLeapSupport(CurrentNode);
                    }
                    else if (CurrentJewelData.Name == "Brute Force Solution")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Strength"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Intelligence"))
                            {
                                attrlist["+# to Intelligence"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Intelligence",AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fluid Motion")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Strength", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Strength"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Dexterity"))
                            {
                                attrlist["+# to Dexterity"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Dexterity",AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fertile Mind")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Dexterity"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Intelligence"))
                            {
                                attrlist["+# to Intelligence"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Intelligence",AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Inertia")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Dexterity"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Strength"))
                            {
                                attrlist["+# to Strength"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Strength",AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Efficient Training")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Intelligence"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Strength"))
                            {
                                attrlist["+# to Strength"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Strength",AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Careful Planning")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["+# to Intelligence"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Dexterity"))
                            {
                                attrlist["+# to Dexterity"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("+# to Dexterity",AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Energised Armour")
                    {//
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Energy Shield", Tree.SkilledNodes);
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
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Life", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            attrlist["#% increased Energy Shield"] -= AreaStats;
                            if (attrlist.ContainsKey("+# to Dexterity"))
                            {
                                attrlist["#% increased Energy Shield"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Energy Shield",AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Fireborn")
                    {
                        AreaStats = 0.0f;
                        float CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Elemental Damage", Tree.SkilledNodes);
                        if (CurrentTotal != 0) { attrlist["#% increased Elemental Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Cold Damage", Tree.SkilledNodes);
                        if (CurrentTotal != 0) { attrlist["#% increased Cold Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Chaos Damage", Tree.SkilledNodes);
                        if (CurrentTotal != 0) { attrlist["#% increased Chaos Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Physical Damage", Tree.SkilledNodes);
                        if (CurrentTotal != 0) { attrlist["#% increased Physical Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        CurrentTotal = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Lightning Damage", Tree.SkilledNodes);
                        if (CurrentTotal != 0) { attrlist["#% increased Lightning Damage"] -= CurrentTotal; AreaStats += CurrentTotal; }
                        if (AreaStats != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Fire Damage"))
                            {
                                attrlist["#% increased Fire Damage"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Fire Damage",AreaStats);
                            }
                        }
                    }
                    else if (CurrentJewelData.Name == "Might in All Forms")
                    {
                        AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Intelligence", Tree.SkilledNodes);
                        AreaStats += ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "+# to Dexterity", Tree.SkilledNodes);
                        if (AreaStats != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Melee Damage Bonus(from Might in All Forms)"))
                            {
                                attrlist["#% increased Melee Damage Bonus(from Might in All Forms)"] += AreaStats;
                            }
                            else
                            {
                                attrlist.Add("#% increased Melee Damage Bonus(from Might in All Forms)",AreaStats);
                            }
                        }
                    }
                }
            }
            return attrlist;
        }
    }
}
