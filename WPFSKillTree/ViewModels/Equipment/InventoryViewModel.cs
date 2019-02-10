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

  //      public InventoryItemViewModel JSlot_Int_Witch { get; }
  //      public InventoryItemViewModel JSlot_Int_Scion { get; }
  //      public InventoryItemViewModel JSlot_Int_WitchShadow { get; }
  //      public InventoryItemViewModel JSlot_DexInt_Scion { get; }
  //      public InventoryItemViewModel JSlot_StrInt_Scion { get; }
  //      public InventoryItemViewModel JSlot_StrDex_Scion { get; }
  //      public InventoryItemViewModel JSlot_Neutral_Acrobatics { get; }
  //      public InventoryItemViewModel JSlot_Dex_ShadowRanger { get; }
  //      public InventoryItemViewModel JSlot_DexInt_Shadow { get; }
  //      public InventoryItemViewModel JSlot_Dex_Ranger { get; }
  //      public InventoryItemViewModel JSlot_Dex_RangerDuelist { get; }
  //      public InventoryItemViewModel JSlot_Str_WarriorDuelist { get; }
  //      public InventoryItemViewModel JSlot_Str_WarriorTemplarScion { get; }
  //      public InventoryItemViewModel JSlot_Int_TemplarWitch { get; }
  //      public InventoryItemViewModel JSlot_Str_FarWarTempScion { get; }
  //      public InventoryItemViewModel JSlot_StrInt_Templar { get; }
  //      public InventoryItemViewModel JSlot_StrDex_Duelist { get; }
  //      public InventoryItemViewModel JSlot_Neutral_IronGrip { get; }
  //      public InventoryItemViewModel JSlot_Neutral_PointBlank { get; }
  //      public InventoryItemViewModel JSlot_Neutral_MinionInstability { get; }
  //      public InventoryItemViewModel JSlot_Str_Warrior { get; }

        public InventoryViewModel(IExtendedDialogCoordinator dialogCoordinator, EquipmentData equipmentData,
            ItemAttributes itemAttributes)
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

   //         JSlot_Int_Witch = CreateSlotVm(ItemSlot.JSlot_Int_Witch);
   //         JSlot_Int_Scion = CreateSlotVm(ItemSlot.JSlot_Int_Scion);
   //         JSlot_Int_WitchShadow = CreateSlotVm(ItemSlot.JSlot_Int_WitchShadow);
   //         JSlot_DexInt_Scion = CreateSlotVm(ItemSlot.JSlot_DexInt_Scion);
   //         JSlot_StrInt_Scion = CreateSlotVm(ItemSlot.JSlot_StrInt_Scion);
   //         JSlot_StrDex_Scion = CreateSlotVm(ItemSlot.JSlot_StrDex_Scion);
   //         JSlot_Neutral_Acrobatics = CreateSlotVm(ItemSlot.JSlot_Neutral_Acrobatics);
   //         JSlot_Dex_ShadowRanger = CreateSlotVm(ItemSlot.JSlot_Dex_ShadowRanger);
   //         JSlot_DexInt_Shadow = CreateSlotVm(ItemSlot.JSlot_DexInt_Shadow);
   //         JSlot_Dex_Ranger = CreateSlotVm(ItemSlot.JSlot_Dex_Ranger);
   //         JSlot_Dex_RangerDuelist = CreateSlotVm(ItemSlot.JSlot_Dex_RangerDuelist);
   //         JSlot_Str_WarriorDuelist = CreateSlotVm(ItemSlot.JSlot_Str_WarriorDuelist);
   //         JSlot_Str_WarriorTemplarScion = CreateSlotVm(ItemSlot.JSlot_Str_WarriorTemplarScion);
   //         JSlot_Int_TemplarWitch = CreateSlotVm(ItemSlot.JSlot_Int_TemplarWitch);
   //         JSlot_Str_FarWarTempScion = CreateSlotVm(ItemSlot.JSlot_Str_FarWarTempScion);
   //         JSlot_StrInt_Templar = CreateSlotVm(ItemSlot.JSlot_StrInt_Templar);
   //         JSlot_StrDex_Duelist = CreateSlotVm(ItemSlot.JSlot_StrDex_Duelist);
   //         JSlot_Neutral_IronGrip = CreateSlotVm(ItemSlot.JSlot_Neutral_IronGrip);
   //         JSlot_Neutral_PointBlank = CreateSlotVm(ItemSlot.JSlot_Neutral_PointBlank);
   //         JSlot_Neutral_MinionInstability = CreateSlotVm(ItemSlot.JSlot_Neutral_MinionInstability);
   //         JSlot_Str_Warrior = CreateSlotVm(ItemSlot.JSlot_Str_Warrior);
        }

        private InventoryItemViewModel CreateSlotVm(ItemSlot slot)
        {
            var imageName = SlotToImageName(slot);
            return new InventoryItemViewModel(_dialogCoordinator, _itemAttributes, slot)
            {
                imageName = "TwoHandSword";
            }
            else if (slot == ItemSlot.OffHand)
            {
                imageName = "Shield";
            }
            else if (slot == ItemSlot.Ring2)
            {
                imageName = "Ring";
            }
            else if (slot == ItemSlot.Helm)
            {
                imageName = "Helmet";
            }
   //         //(replace with dynamic jewel slots later)
   //         else if(slot == ItemSlot.JSlot_Int_Witch || slot == ItemSlot.JSlot_Int_Scion || slot == ItemSlot.JSlot_Int_WitchShadow || slot == ItemSlot.JSlot_DexInt_Scion || slot == ItemSlot.JSlot_StrInt_Scion
   //         || slot == ItemSlot.JSlot_StrDex_Scion || slot == ItemSlot.JSlot_Neutral_Acrobatics || slot == ItemSlot.JSlot_Dex_ShadowRanger || slot == ItemSlot.JSlot_DexInt_Shadow || slot == ItemSlot.JSlot_Dex_Ranger
   //         || slot == ItemSlot.JSlot_Dex_RangerDuelist || slot == ItemSlot.JSlot_Str_WarriorDuelist || slot == ItemSlot.JSlot_Str_WarriorTemplarScion || slot == ItemSlot.JSlot_Int_TemplarWitch || slot == ItemSlot.JSlot_Str_FarWarTempScion
   //         || slot == ItemSlot.JSlot_StrInt_Templar || slot == ItemSlot.JSlot_StrDex_Duelist || slot == ItemSlot.JSlot_Neutral_IronGrip || slot == ItemSlot.JSlot_Neutral_PointBlank || slot == ItemSlot.JSlot_Neutral_MinionInstability
   //         || slot == ItemSlot.JSlot_Str_Warrior)
   //         {
   //             imageName = "Jewel";
   //         }

            return new InventoryItemViewModel(_dialogCoordinator, _equipmentData, _itemAttributes, slot)
            {
                EmptyBackgroundImagePath = $"/POESKillTree;component/Images/EquipmentUI/ItemDefaults/{imageName}.png"
            };
        }

        private static string SlotToImageName(ItemSlot slot)
        {
            Dictionary<string, float> ItemDictionary = new Dictionary<string, float>();
            POESKillTree.Model.Items.Item ItemData;
            int JewelIndex = 0;
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
                        foreach (KeyValuePair<int, JewelNodeData> JewelSlotData in GlobalSettings.JewelInfo)
                        {
                            ItemData = JewelSlotData.JewelData;
                            if (ItemData != null)
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
                        ItemData = null;ContinueCalc=false; break;
                }
                if (ItemData != null)
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
        }
    }
}