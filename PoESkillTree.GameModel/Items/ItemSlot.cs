﻿using System;

namespace PoESkillTree.GameModel.Items
{
    /// <summary>
    /// Defines the slots which can be filled with items.
    /// </summary>
    [Flags]
    public enum ItemSlot: uint//Requires 2147483648 worth of space to fit all jewel sockets as flags (and 64 bit+ if want any extra jewel slots etc)
    {
        Unequipable = 0,
        BodyArmour = 1 << 0,
        MainHand = 1 << 1,
        OffHand = 1 << 2,
        Ring = 1 << 3,
        Ring2 = 1 << 4,
        Amulet = 1 << 5,
        Helm = 1 << 6,
        Gloves = 1 << 7,
        Boots = 1 << 8,
        Gem = 1 << 9,
        Belt = 1 << 10,
        Flask1 = 1 << 11,
        Flask2 = 1 << 12,
        Flask3 = 1 << 13,
        Flask4 = 1 << 14,
        Flask5 = 1 << 15,
    }
}
