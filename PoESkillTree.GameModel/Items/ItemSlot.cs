using System;

namespace PoESkillTree.GameModel.Items
{
    /// <summary>
    /// Defines the slots which can be filled with items.
    /// </summary>
    [Flags]
    public enum ItemSlot: uint//Requires 2147483648 worth of space to fit all jewel sockets as flags (and 64 bit+ if want any extra jewel slots etc)
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
        /// (Int Threshold Jewel Slot)
        /// </summary>
        JSlot_Int_Witch = 0x800,
        /// <summary>
        /// (Int Threshold Jewel Slot)
        /// </summary>
        JSlot_Int_Scion = 0x1000,
        /// <summary>
        /// (Int Threshold Jewel Slot)
        /// </summary>
        JSlot_Int_WitchShadow = 0x2000,
        /// <summary>
        /// (Both Int and Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_DexInt_Scion = 0x4000,
        /// <summary>
        /// (Both Str and Int Threshold Jewel Slot)
        /// </summary>
        JSlot_StrInt_Scion = 0x8000,
        /// <summary>
        /// (Both Str and Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_StrDex_Scion = 0x10000,
        /// <summary>
        /// (Neutral Jewel Slot)
        /// </summary>
        JSlot_Neutral_Acrobatics = 0x20000,
        /// <summary>
        /// (Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_Dex_ShadowRanger = 0x40000,
        /// <summary>
        /// (Both Int and Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_DexInt_Shadow = 0x80000,
        /// <summary>
        /// (Dex Threshold Jewel)
        /// </summary>
        JSlot_Dex_Ranger = 0x100000,
        /// <summary>
        /// (Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_Dex_RangerDuelist = 0x200000,
        /// <summary>
        /// (Str Threshold Jewel Slot)
        /// </summary>
        JSlot_Str_WarriorDuelist = 0x400000,
        /// <summary>
        /// (Str Threshold Jewel Slot)
        /// </summary>
        JSlot_Str_WarriorTemplarScion = 0x800000,
        /// <summary>
        /// (Int Threshold Jewel Slot)
        /// </summary>
        JSlot_Int_TemplarWitch = 0x1000000,
        /// <summary>
        /// (Str Threshold Jewel Slot)
        /// </summary>
        JSlot_Str_FarWarTempScion = 0x2000000,
        /// <summary>
        /// (Both Int and Str Threshold Jewel Slot)
        /// </summary>
        JSlot_StrInt_Templar = 0x4000000,
        /// <summary>
        /// (Both Str and Dex Threshold Jewel Slot)
        /// </summary>
        JSlot_StrDex_Duelist = 0x8000000,
        /// <summary>
        /// (Neutral Jewel Slot)
        /// </summary>
        JSlot_Neutral_IronGrip = 0x10000000,
        /// <summary>
        /// (Neutral Jewel Slot)
        /// </summary>
        JSlot_Neutral_PointBlank = 0x20000000,
        /// <summary>
        /// (Neutral Jewel Slot)
        /// </summary>
        JSlot_Neutral_MinionInstability = 0x40000000,
        /// <summary>
        /// (Str Threshold Jewel Slot)
        /// </summary>
        JSlot_Str_Warrior = 0x80000000
    }
}
