using System;

namespace PoESkillTree.Common.Model.Items.Enums
{
    /// <summary>
    /// Defines the slots which can be filled with items.
    /// </summary>
    [Flags]
    public enum ItemSlot : uint//Requires 2147483648 worth of space to fit all jewel sockets as flags (and 64 bit+ if want any extra jewel slots etc)
    {
        Unequipable = 0x0,
        BodyArmour = 0x1,
        MainHand = 0x2,
        OffHand = 0x4,
        Ring = 0x8,
        Ring2 = 0x10,
        Amulet = 0x20,
        Helm = 0x40,
        Gloves = 0x80,
        Boots = 0x100,
        Gem = 0x200,
        Belt = 0x400,
        /// <summary>
        /// Jewel Slot directly north of Witch starting area (Int Threshold Jewel Slot)
        /// </summary>
        JSlot_Int_Witch = 0x800,
        /// <summary>
        /// Jewel slot far NE of Scion Starting Area; Nearest Jewel to CI area (Int Threshold Jewel Slot)
        /// </summary>
        JSlot_Int_Scion = 0x1000,
        /// <summary>
        /// NE from center jewel slot between Witch and shadow areas (Int Threshold Jewel Slot)
        /// </summary>
        JSlot_Int_WitchShadow = 0x2000,
        /// <summary>
        /// Scion jewel slot east of starting area (Both Int and Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_DexInt_Scion = 0x4000,
        /// <summary>
        /// Scion Jewel Slot west of starting area (Both Str and Int Threshold Jewel Slot)
        /// </summary>
        JSlot_StrInt_Scion = 0x8000,
        /// <summary>
        /// Scion Jewel Slot south of starting area (Both Str and Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_StrDex_Scion = 0x10000,
        /// <summary>
        /// Jewel Slot far east of Scion starting area between Shadow and Ranger areas; Nearest jewel slot to Acrobatics Jewel (Dex Jewel Slot)
        /// </summary>
        JSlot_Dex_Acrobatics = 0x20000,
        /// <summary>
        /// Jewel Slot east of Scion starting area between Shadow and Ranger areas(above Ranger area); Nearest jewel slot to Charisma passive node
        /// (Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_Dex_ShadowRanger = 0x40000,
        /// <summary>
        /// Jewel slot east of Shadow starting area (Both Int and Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_DexInt_Shadow = 0x80000,
        /// <summary>
        /// Jewel slot east of Ranger area (Dex Threshold Jewel)
        /// </summary>
        JSlot_Dex_Ranger = 0x100000,
        /// <summary>
        /// Jewel slot south-east of Scion area; At road between Ranger and Duelist areas (Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_Dex_RangerDuelist = 0x200000,
        /// <summary>
        /// Jewel slot south-west of Scion area; At road between Marauder and Duelist areas 
        /// (Str Threshold Jewel Slot)
        /// </summary>
        JSlot_Str_WarriorDuelist = 0x400000,
        /// <summary>
        /// Jewel slot west of Scion area; At road between Marauder and Templar areas 
        /// (Str Threshold Jewel Slot)
        /// </summary>
        JSlot_Str_WarriorTemplarScion = 0x800000,
        /// <summary>
        /// Jewel slot north-west of Scion area; At road between Templar and Witch areas (Int Threshold Jewel Slot)
        /// </summary>
        JSlot_Int_TemplarWitch = 0x1000000,
        /// <summary>
        /// Jewel slot far west of Scion area; At road between Marauder and Templar areas; 
        /// Nearest jewel slot to Resolute Technique
        /// (Str Threshold Jewel Slot)
        /// </summary>
        JSlot_Str_FarWarTempScion = 0x2000000,
        /// <summary>
        /// Jewel slot west of Templar starting area 
        /// (Both Int and Str Threshold Jewel Slot)
        /// </summary>
        JSlot_StrInt_Templar = 0x4000000,
        /// <summary>
        /// Jewel slot south of Duelist starting area (Both Str and Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_StrDex_Duelist = 0x8000000,
        /// <summary>
        /// Jewel slot far south-west of center; Located between Marauder and Duelist areas next to Iron Grip (Non-Threshold jewel slot)
        /// </summary>
        JSlot_Neutral_IronGrip = 0x10000000,
        /// <summary>
        /// Jewel slot far south-east of center; Located between Duelist and Ranger areas next to Point Blank (Non-Threshold jewel slot)
        /// </summary>
        JSlot_Neutral_PointBlank = 0x20000000,
        /// <summary>
        /// Jewel slot far north-west of center; Located between Templar and Witch areas next to Minion-Instability (Non-Threshold jewel slot)
        /// </summary>
        JSlot_Neutral_MinionInstability = 0x40000000,
        /// <summary>
        /// Jewel slot west of Marauder area (Str Threshold Jewel Slot)
        /// </summary>
        JSlot_Str_Warrior = 0x80000000
    }
}
