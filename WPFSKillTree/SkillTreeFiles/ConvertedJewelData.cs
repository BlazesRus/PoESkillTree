﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        static public Dictionary<string, List<float>> JewelBasedStatUpdater(Dictionary<string, List<float>> attrlist, InventoryViewModel InventoryViewModel, SkillTree Tree)
        {
            float AreaStats;
            ushort NodeID = ConvertedJewelData.JSlot_DexInt_ScionID;
            SkillNode CurrentNode = SkillTree.Skillnodes[NodeID];
            POESKillTree.Model.Items.Item CurrentJewelData = InventoryViewModel.JSlot_DexInt_Scion.Item;

            for (int JewelIndex = 0; JewelIndex < 21; ++JewelIndex)
            {
                if (JewelIndex != 0)
                {
                    switch (JewelIndex)
                    {
                        case 1:
                            NodeID = ConvertedJewelData.JSlot_DexInt_ShadowID;
                            CurrentJewelData = InventoryViewModel.JSlot_DexInt_Shadow.Item; break;
                        case 2:
                            NodeID = ConvertedJewelData.JSlot_Dex_RangerDuelistID;
                            CurrentJewelData = InventoryViewModel.JSlot_Dex_RangerDuelist.Item; break;
                        case 3:
                            NodeID = ConvertedJewelData.JSlot_Dex_RangerID;
                            CurrentJewelData = InventoryViewModel.JSlot_Dex_Ranger.Item; break;
                        case 4:
                            NodeID = ConvertedJewelData.JSlot_Dex_ShadowRangerID;
                            CurrentJewelData = InventoryViewModel.JSlot_Dex_ShadowRanger.Item; break;
                        case 5:
                            NodeID = ConvertedJewelData.JSlot_Int_ScionID;
                            CurrentJewelData = InventoryViewModel.JSlot_Int_Scion.Item; break;
                        case 6:
                            NodeID = ConvertedJewelData.JSlot_Int_TemplarWitchID;
                            CurrentJewelData = InventoryViewModel.JSlot_Int_TemplarWitch.Item; break;
                        case 7:
                            NodeID = ConvertedJewelData.JSlot_Int_WitchID;
                            CurrentJewelData = InventoryViewModel.JSlot_Int_Witch.Item; break;
                        case 8:
                            NodeID = ConvertedJewelData.JSlot_Int_WitchShadowID;
                            CurrentJewelData = InventoryViewModel.JSlot_Int_WitchShadow.Item; break;
                        case 9:
                            NodeID = ConvertedJewelData.JSlot_StrDex_DuelistID;
                            CurrentJewelData = InventoryViewModel.JSlot_StrDex_Duelist.Item; break;
                        case 10:
                            NodeID = ConvertedJewelData.JSlot_StrDex_ScionID;
                            CurrentJewelData = InventoryViewModel.JSlot_StrDex_Scion.Item; break;
                        case 11:
                            NodeID = ConvertedJewelData.JSlot_StrInt_ScionID;
                            CurrentJewelData = InventoryViewModel.JSlot_StrInt_Scion.Item; break;
                        case 12:
                            NodeID = ConvertedJewelData.JSlot_StrInt_TemplarID;
                            CurrentJewelData = InventoryViewModel.JSlot_StrInt_Templar.Item; break;
                        case 13:
                            NodeID = ConvertedJewelData.JSlot_Str_FarWarTempScionID;
                            CurrentJewelData = InventoryViewModel.JSlot_Str_FarWarTempScion.Item; break;
                        case 14:
                            NodeID = ConvertedJewelData.JSlot_Str_WarriorDuelistID;
                            CurrentJewelData = InventoryViewModel.JSlot_Str_WarriorDuelist.Item; break;
                        case 15:
                            NodeID = ConvertedJewelData.JSlot_Str_WarriorID;
                            CurrentJewelData = InventoryViewModel.JSlot_Str_Warrior.Item; break;
                        case 16:
                            NodeID = ConvertedJewelData.JSlot_Str_WarriorTemplarScionID;
                            CurrentJewelData = InventoryViewModel.JSlot_Str_WarriorTemplarScion.Item; break;
                        //Non-Threshold Jewel Slots below
                        case 17:
                            NodeID = ConvertedJewelData.JSlot_Neutral_AcrobaticsID;
                            CurrentJewelData = InventoryViewModel.JSlot_Neutral_Acrobatics.Item; break;
                        case 18:
                            NodeID = ConvertedJewelData.JSlot_Neutral_IronGripID;
                            CurrentJewelData = InventoryViewModel.JSlot_Neutral_IronGrip.Item; break;
                        case 19:
                            NodeID = ConvertedJewelData.JSlot_Neutral_MinionInstabilityID;
                            CurrentJewelData = InventoryViewModel.JSlot_Neutral_MinionInstability.Item; break;
                        case 20:
                            NodeID = ConvertedJewelData.JSlot_Neutral_PointBlankID;
                            CurrentJewelData = InventoryViewModel.JSlot_Neutral_PointBlank.Item; break;
                    }
                    if (NodeID == 0)
                    {
                        continue;
                    }
                    CurrentNode = SkillTree.Skillnodes[NodeID];
                }
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
                                attrlist["+# to Intelligence"].Add(AreaStats);
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
                                attrlist["+# to Dexterity"].Add(AreaStats);
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
                                attrlist["+# to Intelligence"].Add(AreaStats);
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
                                attrlist["+# to Strength"].Add(AreaStats);
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
                                attrlist["+# to Strength"].Add(AreaStats);
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
                                attrlist["+# to Dexterity"].Add(AreaStats);
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
                                attrlist["#% increased Armour"].Add(AreaStats);
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
                                attrlist["#% increased Energy Shield"].Add(AreaStats);
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
                        if (AreaStats != 0)
                        {
                            if (attrlist.ContainsKey("#% increased Fire Damage"))
                            {
                                attrlist["#% increased Fire Damage"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist["#% increased Fire Damage"].Add(AreaStats);
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
                                attrlist["#% increased Melee Damage Bonus(from Might in All Forms)"][0] += AreaStats;
                            }
                            else
                            {
                                attrlist["#% increased Melee Damage Bonus(from Might in All Forms)"].Add(AreaStats);
                            }
                        }
                    }
                    //else if (CurrentJewelData.Name == "Energy From Within")
                    //{
                    //    AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Energy Shield", Tree.SkilledNodes);
                    //    if (AreaStats != 0)
                    //    {
                    //        attrlist["#% increased Energy Shield"][0] -= AreaStats;
                    //        AreaStats *= 2.0f;
                    //        if (attrlist.ContainsKey("#% increased Armour"))
                    //        {
                    //            attrlist["#% increased Armour"][0] += AreaStats;
                    //        }
                    //        else
                    //        {
                    //            attrlist["#% increased Armour"].Add(AreaStats);
                    //        }
                    //    }
                    //}
                    //else if (CurrentJewelData.Name == "Anatomical Knowledge")
                    //{
                    //    AreaStats = ConvertedJewelData.CalculateTotalOfAttributeInJewelArea(CurrentNode, "#% increased Energy Shield", Tree.SkilledNodes);
                    //    if (AreaStats != 0)
                    //    {
                    //        attrlist["#% increased Energy Shield"][0] -= AreaStats;
                    //        AreaStats *= 2.0f;
                    //        if (attrlist.ContainsKey("#% increased Armour"))
                    //        {
                    //            attrlist["#% increased Armour"][0] += AreaStats;
                    //        }
                    //        else
                    //        {
                    //            attrlist["#% increased Armour"].Add(AreaStats);
                    //        }
                    //    }
                    //}
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
    }
}
