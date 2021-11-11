// ***********************************************************************
// Code Created by James Michael Armstrong (https://github.com/BlazesRus)
// ***********************************************************************
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.Utils.Extensions;
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
#if (PoESkillTree_DisableStatTracking == false)
//using PoESkillTree.TreeGenerator.Model.PseudoAttributes;
#endif

public static class MasteryDefinitions
{
    private static string MasteryLabel = "+# Mastery Nodes";
    private static string LifeMStat = "+# Life Mastery Nodes";
    private static string ManaMStat = "+# Mana Mastery Nodes";

    private static string AttackMStat = "+# Attack Mastery Nodes";
    private static string CasterMStat = "+# Caster Mastery Nodes";
    private static string LeechMStat = "+# Leech Mastery Nodes";
    private static string FlaskMStat = "+# Flask Mastery Nodes";
    private static string MinionAttackMStat = "+# Minion Offensive Mastery Nodes";

    private static string ResMStat = "+# Resistance Mastery Nodes";
    private static string MinionDefMStat = "+# Minion Defense Mastery Nodes";
    private static string BleedMStat = "+# Bleeding Mastery Nodes";
    private static string PoisonMStat = "+# Poison Mastery Nodes";
    private static string ProjMStat = "+# Projectile Mastery Nodes";
    private static string DOTMStat = "+# Damage Over Time Mastery Nodes";


    private static string ElemMStat = "+# Elemental Mastery Nodes";
    private static string FireMStat = "+# Fire Mastery Nodes";
    private static string ColdMStat = "+# Cold Mastery Nodes";
    private static string LightMStat = "+# Lightning Mastery Nodes";
    private static string ChaosMStat = "+# Chaos Mastery Nodes";
    private static string PhysicalMStat = "+# Physical Mastery Nodes";

    private static string WandMStat = "+# Wand Mastery Nodes";
    private static string StaffMStat = "+# Staff Mastery Nodes";
    private static string MaceMStat = "+# Mace Mastery Nodes";
    private static string DaggerMStat = "+# Dagger Mastery Nodes";
    private static string DualMStat = "+# Dual Mastery Nodes";
    private static string ShieldMStat = "+# Shield Mastery Nodes";
    private static string BowMStat = "+# Bow Mastery Nodes";
    private static string SwordMStat = "+# Sword Mastery Nodes";
    private static string AxeMStat = "+# Axe Mastery Nodes";
    private static string ClawMStat = "+# Claw Mastery Nodes";
    private static string TwoHMStat = "+# Two Hand Mastery Nodes";

    private static string BlockMStat = "+# Block Mastery Nodes";
    private static string CritMStat = "+# Critical Mastery Nodes";
    private static string LinkMStat = "+# Link Mastery Nodes";

    private static string MineMStat = "+# Mine Mastery Nodes";
    private static string TrapMStat = "+# Trap Mastery Nodes";
    private static string TotemMStat = "+# Totem Mastery Nodes";
    private static string BrandMStat = "+# Brand Mastery Nodes";
    private static string CurseMStat = "+# Curse Mastery Nodes";


    private static string FortifyMStat = "+# Fortify Mastery Nodes";
    private static string WarcryMStat = "+# Warcry Mastery Nodes";
    private static string AuraMStat = "+# Reservation Mastery Nodes";
    private static string HitMStat = "+# Accuracy Mastery Nodes";
    private static string ImpaleMStat = "+# Impale Mastery Nodes";
    private static string MarkMStat = "+# Mark Mastery Nodes";
    private static string DurationMStat = "+# Duration Mastery Nodes";
    private static string SpellDefMStat = "+# Spell Suppression Mastery Nodes";
    private static string BlindMStat = "+# Blind Mastery Nodes";
    private static string ChargeMStat = "+# Charge Mastery Nodes";

    private static string ArmourMStat = "+# Armour Mastery Nodes";
    private static string EvasionMStat = "+# Evasion Mastery Nodes";
    private static string ESMStat = "+# Energy Shield Mastery Nodes";
    private static string ArmourESMStat = "+# Armour&Energy Shield Mastery Nodes";
    private static string ArmourEvasionMStat = "+# Armour&Evasion Mastery Nodes";
    private static string EvasionESMStat = "+# Evasion&Energy Shield Mastery Nodes";

    private static string StatMStat = "+# Attribute Mastery Nodes";
    private static List<float> SingleVal = new List<float>(1) { 1 };

    private static string[] LifeMDesc = new string[2] { "+1 Mastery Nodes", "+1 Life Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> LifeMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { LifeMStat, SingleVal } };
    //9 Count
    private static string[] ManaMDesc = new string[2] { "+1 Mastery Nodes", "+1 Mana Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ManaMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ManaMStat, SingleVal } };
    //8 Count
    private static string[] AttackMDesc = new string[2] { "+1 Mastery Nodes", "+1 Attack Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> AttackMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { AttackMStat, SingleVal } };
    private static string[] FlaskMDesc = new string[2] { "+1 Mastery Nodes", "+1 Flask Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> FlaskMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { FlaskMStat, SingleVal } };
    private static string[] LeechMDesc = new string[2] { "+1 Mastery Nodes", "+1 Leech Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> LeechMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { LeechMStat, SingleVal } };
    private static string[] CasterMDesc = new string[2] { "+1 Mastery Nodes", "+1 Caster Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> CasterMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { CasterMStat, SingleVal } };
    private static string[] MinionAttackMDesc = new string[2] { "+1 Mastery Nodes", "+1 Minion Offensive Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> MinionAttackMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { MinionAttackMStat, SingleVal } };
    //7 Count
    private static string[] CritMDesc = new string[2] { "+1 Mastery Nodes", "+1 Critical Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> CritMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { CritMStat, SingleVal } };
    private static string[] FireMDesc = new string[2] { "+1 Mastery Nodes", "+1 Fire Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> FireMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { FireMStat, SingleVal } };
    //6 Count
    private static string[] ElemMDesc = new string[2] { "+1 Mastery Nodes", "+1 Elemental Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ElemMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ElemMStat, SingleVal } };
    private static string[] ESMDesc = new string[2] { "+1 Mastery Nodes", "+1 Energy Shield Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ESMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ESMStat, SingleVal } };
    private static string[] PhysicalMDesc = new string[2] { "+1 Mastery Nodes", "+1 Physical Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> PhysicalMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { PhysicalMStat, SingleVal } };
    private static string[] MineMDesc = new string[2] { "+1 Mastery Nodes", "+1 Mine Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> MineMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { MineMStat, SingleVal } };
    private static string[] AuraMDesc = new string[2] { "+1 Mastery Nodes", "+1 Reservation Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> AuraMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { AuraMStat, SingleVal } };
    private static string[] TotemMDesc = new string[2] { "+1 Mastery Nodes", "+1 Totem Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> TotemMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { TotemMStat, SingleVal } };
    private static string[] ResMDesc = new string[2] { "+1 Mastery Nodes", "+1 Resistance Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ResMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ResMStat, SingleVal } };
    //5 Count
    private static string[] ArmourMDesc = new string[2] { "+1 Mastery Nodes", "+1 Armour Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ArmourMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ArmourMStat, SingleVal } };
    private static string[] MinionDefMDesc = new string[2] { "+1 Mastery Nodes", "+1 Minion Defence Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> MinionDefMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { MinionDefMStat, SingleVal } };
    private static string[] ColdMDesc = new string[2] { "+1 Mastery Nodes", "+1 Cold Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ColdMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ColdMStat, SingleVal } };
    private static string[] CurseMDesc = new string[2] { "+1 Mastery Nodes", "+1 Curse Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> CurseMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { CurseMStat, SingleVal } };
    private static string[] BowMDesc = new string[2] { "+1 Mastery Nodes", "+1 Bow Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> BowMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { BowMStat, SingleVal } };
    private static string[] EvasionMDesc = new string[2] { "+1 Mastery Nodes", "+1 Evasion Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> EvasionMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { EvasionMStat, SingleVal } };
    private static string[] MaceMDesc = new string[2] { "+1 Mastery Nodes", "+1 Mace Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> MaceMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { MaceMStat, SingleVal } };
    private static string[] ShieldMDesc = new string[2] { "+1 Mastery Nodes", "+1 Shield Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ShieldMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ShieldMStat, SingleVal } };
    private static string[] StaffMDesc = new string[2] { "+1 Mastery Nodes", "+1 Staff Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> StaffMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { StaffMStat, SingleVal } };
    private static string[] TrapMDesc = new string[2] { "+1 Mastery Nodes", "+1 Trap Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> TrapMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { TrapMStat, SingleVal } };
    //4 Count
    private static string[] HitMDesc = new string[2] { "+1 Mastery Nodes", "+1 Accuracy Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> HitMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { HitMStat, SingleVal } };
    private static string[] WandMDesc = new string[2] { "+1 Mastery Nodes", "+1 Wand Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> WandMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { WandMStat, SingleVal } };
    private static string[] DaggerMDesc = new string[2] { "+1 Mastery Nodes", "+1 Dagger Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> DaggerMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { DaggerMStat, SingleVal } };
    private static string[] PoisonMDesc = new string[2] { "+1 Mastery Nodes", "+1 Poison Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> PoisonMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { PoisonMStat, SingleVal } };
    private static string[] ChargeMDesc = new string[2] { "+1 Mastery Nodes", "+1 Charge Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ChargeMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ChargeMStat, SingleVal } };
    private static string[] ClawMDesc = new string[2] { "+1 Mastery Nodes", "+1 Claw Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ClawMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ClawMStat, SingleVal } };
    private static string[] LightMDesc = new string[2] { "+1 Mastery Nodes", "+1 Lightning Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> LightMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { LightMStat, SingleVal } };
    private static string[] WarcryMDesc = new string[2] { "+1 Mastery Nodes", "+1 Warcry Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> WarcryMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { WarcryMStat, SingleVal } };
    private static string[] AxeMDesc = new string[2] { "+1 Mastery Nodes", "+1 Axe Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> AxeMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { AxeMStat, SingleVal } };
    private static string[] SwordMDesc = new string[2] { "+1 Mastery Nodes", "+1 Sword Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> SwordMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { SwordMStat, SingleVal } };
    private static string[] TwoHMDesc = new string[2] { "+1 Mastery Nodes", "+1 Two Hand Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> TwoHMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { TwoHMStat, SingleVal } };
    private static string[] DOTMDesc = new string[2] { "+1 Mastery Nodes", "+1 Damage Over Time Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> DOTMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { DOTMStat, SingleVal } };
    //3 Count
    private static string[] BlockMDesc = new string[2] { "+1 Mastery Nodes", "+1 Block Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> BlockMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { BlockMStat, SingleVal } };
    private static string[] ChaosMDesc = new string[2] { "+1 Mastery Nodes", "+1 Chaos Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ChaosMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ChaosMStat, SingleVal } };
    private static string[] DualMDesc = new string[2] { "+1 Mastery Nodes", "+1 Dual Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> DualMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { DualMStat, SingleVal } };
    private static string[] ProjMDesc = new string[2] { "+1 Mastery Nodes", "+1 Projectile Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ProjMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ProjMStat, SingleVal } };
    private static string[] StatMDesc = new string[2] { "+1 Mastery Nodes", "+1 Attribute Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> StatMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { StatMStat, SingleVal } };
    private static string[] BrandMDesc = new string[2] { "+1 Mastery Nodes", "+1 Brand Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> BrandMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { BrandMStat, SingleVal } };
    private static string[] SpellDefMDesc = new string[2] { "+1 Mastery Nodes", "+1 Spell Suppression Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> SpellDefMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { SpellDefMStat, SingleVal } };
    private static string[] ImpaleMDesc = new string[2] { "+1 Mastery Nodes", "+1 Impale Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ImpaleMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ImpaleMStat, SingleVal } };
    //2 Count
    private static string[] ArmourESMDesc = new string[2] { "+1 Mastery Nodes", "+1 Armour and Energy Shield Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ArmourESMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ArmourESMStat, SingleVal } };
    private static string[] ArmourEvasionMDesc = new string[2] { "+1 Mastery Nodes", "+1 Armour and Evasion Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> ArmourEvasionMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { ArmourEvasionMStat, SingleVal } };
    private static string[] EvasionESMDesc = new string[2] { "+1 Mastery Nodes", "+1 Evasion and Energy Shield Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> EvasionESMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { EvasionESMStat, SingleVal } };

    private static string[] FortifyMDesc = new string[2] { "+1 Mastery Nodes", "+1 Fortify Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> FortifyMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { FortifyMStat, SingleVal } };
    private static string[] BleedMDesc = new string[2] { "+1 Mastery Nodes", "+1 Bleeding Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> BleedMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { BleedMStat, SingleVal } };
    private static string[] MarkMDesc = new string[2] { "+1 Mastery Nodes", "+1 Mark Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> MarkMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { MarkMStat, SingleVal } };
    private static string[] DurationMDesc = new string[2] { "+1 Mastery Nodes", "+1 Duration Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> DurationMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { DurationMStat, SingleVal } };
    private static string[] LinkMDesc = new string[2] { "+1 Mastery Nodes", "+1 Link Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> LinkMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { LinkMStat, SingleVal } };
    private static string[] BlindMDesc = new string[2] { "+1 Mastery Nodes", "+1 Blind Mastery Nodes" };
    private static Dictionary<string, IReadOnlyList<float>> BlindMAttributes = new Dictionary<string, IReadOnlyList<float>>(2) { { MasteryLabel, SingleVal }, { BlindMStat, SingleVal } };

    public static void SetMasteryLabel(PassiveNodeViewModel node)
    {
        //19 Count
        if (node.Name.StartsWith("Life"))
            node.SetDescriptionWithStats(ref LifeMDesc, ref LifeMAttributes);
        //9 Count
        else if (node.Name.StartsWith("Mana") && !node.Attributes.ContainsKey(ManaMStat))
            node.SetDescriptionWithStats(ref ManaMDesc, ref ManaMAttributes);
        //8 Count
        else if (node.Name.StartsWith("Attack"))
            node.SetDescriptionWithStats(ref AttackMDesc, ref AttackMAttributes);
        else if (node.Name.StartsWith("Flask"))
            node.SetDescriptionWithStats(ref FlaskMDesc, ref FlaskMAttributes);
        else if (node.Name.StartsWith("Leech"))
            node.SetDescriptionWithStats(ref LeechMDesc, ref LeechMAttributes);
        else if (node.Name.StartsWith("Caster"))
            node.SetDescriptionWithStats(ref CasterMDesc, ref CasterMAttributes);
        else if (node.Name.StartsWith("Minion Offence"))
            node.SetDescriptionWithStats(ref MinionAttackMDesc, ref MinionAttackMAttributes);
        //7 Count
        else if (node.Name.StartsWith("Critical"))
            node.SetDescriptionWithStats(ref CritMDesc, ref CritMAttributes);
        else if (node.Name.StartsWith("Fire") && !node.Attributes.ContainsKey(FireMStat))
            node.SetDescriptionWithStats(ref FireMDesc, ref FireMAttributes);
        //6 Count
        else if (node.Name.StartsWith("Elemental"))
            node.SetDescriptionWithStats(ref ElemMDesc, ref ElemMAttributes);
        else if (node.Name.StartsWith("Energy Shield"))
            node.SetDescriptionWithStats(ref ESMDesc, ref ESMAttributes);
        else if (node.Name.StartsWith("Physical"))
            node.SetDescriptionWithStats(ref PhysicalMDesc, ref PhysicalMAttributes);
        else if (node.Name.StartsWith("Mine"))
            node.SetDescriptionWithStats(ref MineMDesc, ref MineMAttributes);
        else if (node.Name.StartsWith("Reservation"))
            node.SetDescriptionWithStats(ref AuraMDesc, ref AuraMAttributes);
        else if (node.Name.StartsWith("Totem"))
            node.SetDescriptionWithStats(ref TotemMDesc, ref TotemMAttributes);
        else if (node.Name.StartsWith("Resistance"))
            node.SetDescriptionWithStats(ref ResMDesc, ref ResMAttributes);
        //5 Count
        else if (node.Name.StartsWith("Armour"))
            node.SetDescriptionWithStats(ref ArmourMDesc, ref ArmourMAttributes);
        else if (node.Name.StartsWith("Minion Defence"))
            node.SetDescriptionWithStats(ref MinionDefMDesc, ref MinionDefMAttributes);
        else if (node.Name.StartsWith("Cold"))
            node.SetDescriptionWithStats(ref ColdMDesc, ref ColdMAttributes);
        else if (node.Name.StartsWith("Curse"))
            node.SetDescriptionWithStats(ref CurseMDesc, ref CurseMAttributes);
        else if (node.Name.StartsWith("Bow"))
            node.SetDescriptionWithStats(ref BowMDesc, ref BowMAttributes);
        else if (node.Name.StartsWith("Evasion"))
            node.SetDescriptionWithStats(ref EvasionMDesc, ref EvasionMAttributes);
        else if (node.Name.StartsWith("Mace"))
            node.SetDescriptionWithStats(ref MaceMDesc, ref MaceMAttributes);
        else if (node.Name.StartsWith("Shield"))
            node.SetDescriptionWithStats(ref ShieldMDesc, ref ShieldMAttributes);
        else if (node.Name.StartsWith("Staff"))
            node.SetDescriptionWithStats(ref StaffMDesc, ref StaffMAttributes);
        else if (node.Name.StartsWith("Trap"))
            node.SetDescriptionWithStats(ref TrapMDesc, ref TrapMAttributes);
        //4 Count
        else if (node.Name.StartsWith("Accuracy"))
            node.SetDescriptionWithStats(ref HitMDesc, ref HitMAttributes);
        else if (node.Name.StartsWith("Wand"))
            node.SetDescriptionWithStats(ref WandMDesc, ref WandMAttributes);
        else if (node.Name.StartsWith("Dagger"))
            node.SetDescriptionWithStats(ref DaggerMDesc, ref DaggerMAttributes);
        else if (node.Name.StartsWith("Poison"))
            node.SetDescriptionWithStats(ref PoisonMDesc, ref PoisonMAttributes);
        else if (node.Name.StartsWith("Charge"))
            node.SetDescriptionWithStats(ref ChargeMDesc, ref ChargeMAttributes);
        else if (node.Name.StartsWith("Claw"))
            node.SetDescriptionWithStats(ref ClawMDesc, ref ClawMAttributes);
        else if (node.Name.StartsWith("Lightning"))
            node.SetDescriptionWithStats(ref LightMDesc, ref LightMAttributes);
        else if (node.Name.StartsWith("Warcry"))
            node.SetDescriptionWithStats(ref WarcryMDesc, ref WarcryMAttributes);
        else if (node.Name.StartsWith("Axe"))
            node.SetDescriptionWithStats(ref AxeMDesc, ref AxeMAttributes);
        else if (node.Name.StartsWith("Sword"))
            node.SetDescriptionWithStats(ref SwordMDesc, ref SwordMAttributes);
        else if (node.Name.StartsWith("Two Hand"))
            node.SetDescriptionWithStats(ref TwoHMDesc, ref TwoHMAttributes);
        else if (node.Name.StartsWith("Damage Over Time"))
            node.SetDescriptionWithStats(ref DOTMDesc, ref DOTMAttributes);
        //3 Count
        else if (node.Name.StartsWith("Block"))
            node.SetDescriptionWithStats(ref BlockMDesc, ref BlockMAttributes);
        else if (node.Name.StartsWith("Chaos"))
            node.SetDescriptionWithStats(ref ChaosMDesc, ref ChaosMAttributes);
        else if (node.Name.StartsWith("Dual"))
            node.SetDescriptionWithStats(ref DualMDesc, ref DualMAttributes);
        else if (node.Name.StartsWith("Projectile"))
            node.SetDescriptionWithStats(ref ProjMDesc, ref ProjMAttributes);
        else if (node.Name.StartsWith("Attributes"))
            node.SetDescriptionWithStats(ref StatMDesc, ref StatMAttributes);
        else if (node.Name.StartsWith("Brand"))
            node.SetDescriptionWithStats(ref BrandMDesc, ref BrandMAttributes);
        else if (node.Name.StartsWith("Spell Suppression"))
            node.SetDescriptionWithStats(ref SpellDefMDesc, ref SpellDefMAttributes);
        else if (node.Name.StartsWith("Impale"))
            node.SetDescriptionWithStats(ref ImpaleMDesc, ref ImpaleMAttributes);
        //2 Count
        else if (node.Name.StartsWith("Armour and Energy Shield"))
            node.SetDescriptionWithStats(ref ArmourESMDesc, ref ArmourESMAttributes);
        else if (node.Name.StartsWith("Armour and Evasion"))
            node.SetDescriptionWithStats(ref ArmourEvasionMDesc, ref ArmourEvasionMAttributes);
        else if (node.Name.StartsWith("Evasion and Energy Shield"))
            node.SetDescriptionWithStats(ref EvasionESMDesc, ref EvasionESMAttributes);
        else if (node.Name.StartsWith("Fortify"))
            node.SetDescriptionWithStats(ref FortifyMDesc, ref FortifyMAttributes);
        else if (node.Name.StartsWith("Bleeding"))
            node.SetDescriptionWithStats(ref BleedMDesc, ref BleedMAttributes);
        else if (node.Name.StartsWith("Mark"))
            node.SetDescriptionWithStats(ref MarkMDesc, ref MarkMAttributes);
        else if (node.Name.StartsWith("Duration"))
            node.SetDescriptionWithStats(ref LifeMDesc, ref DurationMAttributes);
        else if (node.Name.StartsWith("Link"))
            node.SetDescriptionWithStats(ref LinkMDesc, ref LinkMAttributes);
        else if (node.Name.StartsWith("Blind"))
            node.SetDescriptionWithStats(ref BlindMDesc, ref BlindMAttributes);
    }

    /// <summary>Adds +1 Mastery Nodes to end of Selected Mastery stats before applying to node</summary>
    /// <param name="node">The node.</param>
    /// <param name="selectedStats">The selected stats.</param>
    /// <param name="EffectId">The effect identifier.</param>
    public static void ApplyMasterySelectionStats(PassiveNodeViewModel node, string[] selectedStats, ushort EffectId)
    {
        int LastIndex = selectedStats.Length;
        int FinalSize = LastIndex + 1;
        System.Array.Resize(ref selectedStats, FinalSize);
        selectedStats[LastIndex] = "+1 Mastery Nodes";
        Dictionary<string, IReadOnlyList<float>> statDictionary = new Dictionary<string, IReadOnlyList<float>>(FinalSize);
        var regexAttrib = new Regex("[0-9]*\\.?[0-9]+");
        var values = new List<float>();
        foreach (string statName in selectedStats)
        {
            foreach (var m in regexAttrib.Matches(statName).WhereNotNull())
            {
                if (m.Value == "")
                    values.Add(float.NaN);
                else
                    values.Add(float.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture));
            }
            string cs = (regexAttrib.Replace(statName, "#"));
            statDictionary.Add(cs, values);
            values.Clear();
        }
        node.SetDescriptionWithStats(ref selectedStats, ref statDictionary);
        node.ForceChangeSkill(EffectId);
    }
}