using System.Collections.Generic;
using System.Linq;
using PoESkillTree.GameModel.Items;
using POESKillTree.Model.Items;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for the inventory. Only a collection of the the InventoryItemViewModels for the slots.
    /// </summary>
    public class InventoryViewModel : Notifier
    {
        private readonly IExtendedDialogCoordinator _dialogCoordinator;
        private readonly ItemAttributes _itemAttributes;

        public InventoryItemViewModel Armor { get; }
        public InventoryItemViewModel MainHand { get; }
        public InventoryItemViewModel OffHand { get; }
        public InventoryItemViewModel Ring { get; }
        public InventoryItemViewModel Ring2 { get; }
        public InventoryItemViewModel Amulet { get; }
        public InventoryItemViewModel Helm { get; }
        public InventoryItemViewModel Gloves { get; }
        public InventoryItemViewModel Boots { get; }
        public InventoryItemViewModel Belt { get; }
        public IReadOnlyList<InventoryItemViewModel> Flasks { get; }

        public InventoryViewModel(IExtendedDialogCoordinator dialogCoordinator, ItemAttributes itemAttributes)
        {
            _dialogCoordinator = dialogCoordinator;
            _itemAttributes = itemAttributes;
            Armor = CreateSlotVm(ItemSlot.BodyArmour);
            MainHand = CreateSlotVm(ItemSlot.MainHand);
            OffHand = CreateSlotVm(ItemSlot.OffHand);
            Ring = CreateSlotVm(ItemSlot.Ring);
            Ring2 = CreateSlotVm(ItemSlot.Ring2);
            Amulet = CreateSlotVm(ItemSlot.Amulet);
            Helm = CreateSlotVm(ItemSlot.Helm);
            Gloves = CreateSlotVm(ItemSlot.Gloves);
            Boots = CreateSlotVm(ItemSlot.Boots);
            Belt = CreateSlotVm(ItemSlot.Belt);
            Flasks = ItemSlotExtensions.Flasks.Select(CreateSlotVm).ToList();
        }

        private InventoryItemViewModel CreateSlotVm(ItemSlot slot)
        {
            var imageName = SlotToImageName(slot);
            return new InventoryItemViewModel(_dialogCoordinator, _itemAttributes, slot)
            {
                EmptyBackgroundImagePath = $"/POESKillTree;component/Images/EquipmentUI/ItemDefaults/{imageName}.png"
            };
        }

        private static string SlotToImageName(ItemSlot slot)
        {
            if (slot.IsFlask())
                return "LifeFlask";
            switch (slot)
            {
                case ItemSlot.MainHand:
                    return "TwoHandSword";
                case ItemSlot.OffHand:
                    return "Shield";
                case ItemSlot.Ring2:
                    return "Ring";
                case ItemSlot.Helm:
                    return "Helmet";
                default:
                    return slot.ToString();
            }
        }

        /// <summary>
        /// Calculates the total single attributes.(Mostly for SkillTree Generation Totals)
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, float> CalculateTotalSingleAttributes()
        {
            Dictionary<string, float> ItemDictionary = new Dictionary<string, float>();
            POESKillTree.Model.Items.Item ItemData;
            bool ContinueCalc = true;
            for (int Index = 0; ContinueCalc; ++Index)
            {
                switch (Index)
                {
                    case 0:
                        ItemData = Armor.Item; break;
                    case 1:
                        ItemData = MainHand.Item; break;
                    case 2:
                        ItemData = OffHand.Item; break;
                    case 3:
                        ItemData = Ring.Item; break;
                    case 4:
                        ItemData = Ring2.Item; break;
                    case 5:
                        ItemData = Amulet.Item; break;
                    case 6:
                        ItemData = Helm.Item; break;
                    case 7:
                        ItemData = Gloves.Item; break;
                    case 8:
                        ItemData = Boots.Item; break;
                    case 9:
                        ItemData = Belt.Item; break;
                    default:
                        JewelItem JewelItemData;
                        foreach (KeyValuePair<ushort, JewelNodeData> JewelSlotData in GlobalSettings.JewelInfo)
                        {
                            JewelItemData = JewelSlotData.Value.JewelData;
                            if (JewelItemData != null)
                            {
                                foreach (var TargetMod in JewelItemData.Mods)
                                {
                                    if (TargetMod.Values.Count == 1)//Only single value Mods added to dictionary for solver use
                                    {
                                        if(ItemDictionary.ContainsKey(TargetMod.Attribute))
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
                        ItemData = null;ContinueCalc=false; break;
                }
                if (ItemData != null&& ContinueCalc)
                {
                    foreach (var TargetMod in ItemData.Mods)
                    {
                        if (TargetMod.Values.Count == 1)//Only single value Mods added to dictionary for solver use
                        {
                            if(ItemDictionary.ContainsKey(TargetMod.Attribute))
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
            return ItemDictionary;
        }
    }
}