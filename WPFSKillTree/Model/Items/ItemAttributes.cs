using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoESkillTree.GameModel.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.ViewModels;

namespace POESKillTree.Model.Items
{
    public class ItemAttributes : Notifier
    {
        #region slotted items

        public Item Armor
        {
            get { return GetItemInSlot(ItemSlot.BodyArmour); }
            set { SetItemInSlot(value, ItemSlot.BodyArmour); }
        }

        public Item MainHand
        {
            get { return GetItemInSlot(ItemSlot.MainHand); }
            set { SetItemInSlot(value, ItemSlot.MainHand); }
        }

        public Item OffHand
        {
            get { return GetItemInSlot(ItemSlot.OffHand); }
            set { SetItemInSlot(value, ItemSlot.OffHand); }
        }

        public Item Ring
        {
            get { return GetItemInSlot(ItemSlot.Ring); }
            set { SetItemInSlot(value, ItemSlot.Ring); }
        }

        public Item Ring2
        {
            get { return GetItemInSlot(ItemSlot.Ring2); }
            set { SetItemInSlot(value, ItemSlot.Ring2); }
        }

        public Item Amulet
        {
            get { return GetItemInSlot(ItemSlot.Amulet); }
            set { SetItemInSlot(value, ItemSlot.Amulet); }
        }

        public Item Helm
        {
            get { return GetItemInSlot(ItemSlot.Helm); }
            set { SetItemInSlot(value, ItemSlot.Helm); }
        }

        public Item Gloves
        {
            get { return GetItemInSlot(ItemSlot.Gloves); }
            set { SetItemInSlot(value, ItemSlot.Gloves); }
        }

        public Item Boots
        {
            get { return GetItemInSlot(ItemSlot.Boots); }
            set { SetItemInSlot(value, ItemSlot.Boots); }
        }

        public Item Belt
        {
            get { return GetItemInSlot(ItemSlot.Belt); }
            set { SetItemInSlot(value, ItemSlot.Belt); }
        }

        public Item JSlot_Int_Witch
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Int_Witch); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Int_Witch); }
        }

        public Item JSlot_Int_Scion
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Int_Scion); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Int_Scion); }
        }

        public Item JSlot_Int_WitchShadow
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Int_WitchShadow); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Int_WitchShadow); }
        }

        public Item JSlot_DexInt_Scion
        {
            get { return GetItemInSlot(ItemSlot.JSlot_DexInt_Scion); }
            set { SetItemInSlot(value, ItemSlot.JSlot_DexInt_Scion); }
        }

        public Item JSlot_StrInt_Scion
        {
            get { return GetItemInSlot(ItemSlot.JSlot_StrInt_Scion); }
            set { SetItemInSlot(value, ItemSlot.JSlot_StrInt_Scion); }
        }

        public Item JSlot_StrDex_Scion
        {
            get { return GetItemInSlot(ItemSlot.JSlot_StrDex_Scion); }
            set { SetItemInSlot(value, ItemSlot.JSlot_StrDex_Scion); }
        }

        public Item JSlot_Neutral_Acrobatics
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Neutral_Acrobatics); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Neutral_Acrobatics); }
        }

        public Item JSlot_Dex_ShadowRanger
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Dex_ShadowRanger); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Dex_ShadowRanger); }
        }

        public Item JSlot_DexInt_Shadow
        {
            get { return GetItemInSlot(ItemSlot.JSlot_DexInt_Shadow); }
            set { SetItemInSlot(value, ItemSlot.JSlot_DexInt_Shadow); }
        }

        public Item JSlot_Dex_Ranger
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Dex_Ranger); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Dex_Ranger); }
        }

        public Item JSlot_Dex_RangerDuelist
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Dex_RangerDuelist); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Dex_RangerDuelist); }
        }

        public Item JSlot_Str_WarriorDuelist
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Str_WarriorDuelist); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Str_WarriorDuelist); }
        }

        public Item JSlot_Str_WarriorTemplarScion
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Str_WarriorTemplarScion); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Str_WarriorTemplarScion); }
        }

        public Item JSlot_Int_TemplarWitch
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Int_TemplarWitch); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Int_TemplarWitch); }
        }

        public Item JSlot_Str_FarWarTempScion
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Str_FarWarTempScion); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Str_FarWarTempScion); }
        }

        public Item JSlot_StrInt_Templar
        {
            get { return GetItemInSlot(ItemSlot.JSlot_StrInt_Templar); }
            set { SetItemInSlot(value, ItemSlot.JSlot_StrInt_Templar); }
        }

        public Item JSlot_StrDex_Duelist
        {
            get { return GetItemInSlot(ItemSlot.JSlot_StrDex_Duelist); }
            set { SetItemInSlot(value, ItemSlot.JSlot_StrDex_Duelist); }
        }

        public Item JSlot_Neutral_IronGrip
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Neutral_IronGrip); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Neutral_IronGrip); }
        }

        public Item JSlot_Neutral_PointBlank
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Neutral_PointBlank); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Neutral_PointBlank); }
        }

        public Item JSlot_Neutral_MinionInstability
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Neutral_MinionInstability); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Neutral_MinionInstability); }
        }

        public Item JSlot_Str_Warrior
        {
            get { return GetItemInSlot(ItemSlot.JSlot_Str_Warrior); }
            set { SetItemInSlot(value, ItemSlot.JSlot_Str_Warrior); }
        }

        public Item ReturnItemByName(string SlotName)
        {
            switch (SlotName)
            {
                //Int Jewels
                case "JSlot_Int_Witch"://Jewel Slot directly north of Witch starting area
                    return JSlot_Int_Witch;
                case "JSlot_Int_Scion"://Jewel slot far NE of Scion Starting Area; Nearest Jewel to CI area (Int Threshold Jewel Slot)
                    return JSlot_Int_Scion;
                case "JSlot_Int_WitchShadow"://NE from center jewel slot between Witch and shadow areas
                    return JSlot_Int_WitchShadow;
                case "JSlot_Int_TemplarWitch"://Jewel slot north-west of Scion area; At road between Templar and Witch areas
                    return JSlot_Int_TemplarWitch;
                //Str Jewels
                case "JSlot_Str_WarriorDuelist"://Jewel slot south-west of Scion area; At road between Marauder and Duelist areas
                    return JSlot_Str_WarriorDuelist;
                case "JSlot_Str_WarriorTemplarScion"://Jewel slot west of Scion area; At road between Marauder and Templar areas
                    return JSlot_Str_WarriorTemplarScion;
                case "JSlot_Str_FarWarTempScion"://Jewel slot far west of Scion area; At road between Marauder and Templar areas; Nearest jewel slot to Resolute Technique
                    return JSlot_Str_FarWarTempScion;
                case "JSlot_Str_Warrior"://Jewel slot west of Marauder area
                    return JSlot_Str_Warrior;
                //Dex Jewels
                case "JSlot_Dex_ShadowRanger"://Jewel Slot east of Scion starting area between Shadow and Ranger areas(above Ranger area); Nearest jewel slot to Charisma passive node
                    return JSlot_Dex_ShadowRanger;
                case "JSlot_Dex_Ranger"://Jewel slot east of Ranger area
                    return JSlot_Dex_Ranger;
                case "JSlot_Dex_RangerDuelist"://Jewel slot south-east of Scion area; At road between Ranger and Duelist areas
                    return JSlot_Dex_RangerDuelist;
                //Hybrid Jewels
                case "JSlot_StrInt_Templar"://Jewel slot west of Templar starting area
                    return JSlot_StrInt_Templar;
                case "JSlot_StrInt_Scion"://Scion Jewel Slot west of starting area
                    return JSlot_StrInt_Scion;
                case "JSlot_DexInt_Shadow"://Jewel slot east of Shadow starting area
                    return JSlot_DexInt_Shadow;
                case "JSlot_DexInt_Scion"://Scion jewel slot east of starting area
                    return JSlot_DexInt_Scion;
                case "JSlot_StrDex_Scion"://Scion Jewel Slot south of starting area
                    return JSlot_StrDex_Scion;
                case "JSlot_StrDex_Duelist"://Jewel slot south of Duelist starting area
                    return JSlot_StrDex_Duelist;
                //Non-Threshold Jewel Slots
                case "JSlot_Neutral_Acrobatics":
                    return JSlot_Neutral_IronGrip;
                case "JSlot_Neutral_PointBlank":
                    return JSlot_Neutral_PointBlank;
                case "JSlot_Neutral_MinionInstabilityID":
                    return JSlot_Neutral_MinionInstability;
                case "JSlot_Neutral_IronGrip":
                    return JSlot_Neutral_IronGrip;
                //Non-Threshold Jewel Slots
                default:
                    return null;
            }
        }

        public Item GetItemInSlot(ItemSlot slot)
        {
            return Equip.FirstOrDefault(i => i.Slot == slot);
        }

        public void SetItemInSlot(Item value, ItemSlot slot)
        {
            if (!CanEquip(value, slot))
                return;
            
            var old = Equip.FirstOrDefault(i => i.Slot == slot);
            if (old != null)
            {
                Equip.Remove(old);
                old.Slot = ItemSlot.Unequipable;
                old.PropertyChanged -= SlottedItemOnPropertyChanged;
            }

            if (value != null)
            {
                value.Slot = slot;
                Equip.Add(value);
                value.PropertyChanged += SlottedItemOnPropertyChanged;
            }
            OnPropertyChanged(slot.ToString());
            RefreshItemAttributes();
        }

        public bool CanEquip(Item item, ItemSlot slot)
        {
            if (item == null) return true;
            if (slot == ItemSlot.Unequipable) return false;
            // one handed -> only equippable if other hand is free, shield or matching one handed
            if (item.Tags.HasFlag(Tags.OneHand)
                && (slot == ItemSlot.MainHand || slot == ItemSlot.OffHand))
            {
                var other = slot == ItemSlot.MainHand ? OffHand : MainHand;
                if (other == null || other.ItemClass == ItemClass.Shield)
                    return true;
                if (!other.Tags.HasFlag(Tags.OneHand))
                    return false;
                if ((item.ItemClass == ItemClass.Wand && other.ItemClass != ItemClass.Wand)
                    || (other.ItemClass == ItemClass.Wand && item.ItemClass != ItemClass.Wand))
                    return false;
                return true;
            }
            // two handed and not bow -> only equippable if off hand is free
            if (item.Tags.HasFlag(Tags.TwoHand) && item.ItemClass != ItemClass.Bow
                && slot == ItemSlot.MainHand)
            {
                return OffHand == null;
            }
            // bow -> only equippable if off hand is free or quiver
            if (item.ItemClass == ItemClass.Bow && slot == ItemSlot.MainHand)
            {
                return OffHand == null || OffHand.ItemClass == ItemClass.Quiver;
            }
            // quiver -> only equippable if main hand is free or bow
            if (item.ItemClass == ItemClass.Quiver && slot == ItemSlot.OffHand)
            {
                return MainHand == null || MainHand.ItemClass == ItemClass.Bow;
            }
            // shield -> only equippable if main hand is free or one hand
            if (item.ItemClass == ItemClass.Shield && slot == ItemSlot.OffHand)
            {
                return MainHand == null || MainHand.Tags.HasFlag(Tags.OneHand);
            }
            return ((int) item.ItemClass.ItemSlots() & (int) slot) != 0;
        }
        #endregion

        public ObservableCollection<Item> Equip { get; }

        private ListCollectionView _attributes;
        public ListCollectionView Attributes
        {
            get { return _attributes; }
            private set { SetProperty(ref _attributes, value); }
        }

        public IReadOnlyList<ItemMod> NonLocalMods { get; private set; }

        private readonly IPersistentData _persistentData;

        public event EventHandler ItemDataChanged;

        private void SlottedItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Item.JsonBase))
            {
                ItemDataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ItemAttributes()
        {
            Equip = new ObservableCollection<Item>();
            RefreshItemAttributes();
        }

        public ItemAttributes(IPersistentData persistentData, string itemData)
        {
            _persistentData = persistentData;
            Equip = new ObservableCollection<Item>();

            var jObject = JObject.Parse(itemData);
            foreach (JObject jobj in (JArray)jObject["items"])
            {
                switch (jobj["inventoryId"].Value<string>())
                {
                    case "BodyArmour":
                        AddItem(jobj, ItemSlot.BodyArmour);
                        break;
                    case "Ring":
                        AddItem(jobj, ItemSlot.Ring);
                        break;
                    case "Ring2":
                        AddItem(jobj, ItemSlot.Ring2);
                        break;
                    case "Gloves":
                        AddItem(jobj, ItemSlot.Gloves);
                        break;
                    case "Weapon":
                        AddItem(jobj, ItemSlot.MainHand);
                        break;
                    case "Offhand":
                        AddItem(jobj, ItemSlot.OffHand);
                        break;
                    case "Helm":
                        AddItem(jobj, ItemSlot.Helm);
                        break;
                    case "Boots":
                        AddItem(jobj, ItemSlot.Boots);
                        break;
                    case "Amulet":
                        AddItem(jobj, ItemSlot.Amulet);
                        break;
                    case "Belt":
                        AddItem(jobj, ItemSlot.Belt);
                        break;
                    case "JSlot_Int_Witch":
                        AddItem(jobj, ItemSlot.JSlot_Int_Witch);
                        break;
                    case "JSlot_Int_Scion":
                        AddItem(jobj, ItemSlot.JSlot_Int_Scion);
                        break;
                    case "JSlot_Int_WitchShadow":
                        AddItem(jobj, ItemSlot.JSlot_Int_WitchShadow);
                        break;
                    case "JSlot_DexInt_Scion":
                        AddItem(jobj, ItemSlot.JSlot_DexInt_Scion);
                        break;
                    case "JSlot_StrInt_Scion":
                        AddItem(jobj, ItemSlot.JSlot_StrInt_Scion);
                        break;
                    case "JSlot_StrDex_Scion":
                        AddItem(jobj, ItemSlot.JSlot_StrDex_Scion);
                        break;
                    case "JSlot_Neutral_Acrobatics":
                        AddItem(jobj, ItemSlot.JSlot_Neutral_Acrobatics);
                        break;
                    case "JSlot_Dex_ShadowRanger":
                        AddItem(jobj, ItemSlot.JSlot_Dex_ShadowRanger);
                        break;
                    case "JSlot_DexInt_Shadow":
                        AddItem(jobj, ItemSlot.JSlot_DexInt_Shadow);
                        break;
                    case "JSlot_Dex_Ranger":
                        AddItem(jobj, ItemSlot.JSlot_Dex_Ranger);
                        break;
                    case "JSlot_Dex_RangerDuelist":
                        AddItem(jobj, ItemSlot.JSlot_Dex_RangerDuelist);
                        break;
                    case "JSlot_Str_WarriorDuelist":
                        AddItem(jobj, ItemSlot.JSlot_Str_WarriorDuelist);
                        break;
                    case "JSlot_Str_WarriorTemplarScion":
                        AddItem(jobj, ItemSlot.JSlot_Str_WarriorTemplarScion);
                        break;
                    case "JSlot_Int_TemplarWitch":
                        AddItem(jobj, ItemSlot.JSlot_Int_TemplarWitch);
                        break;
                    case "JSlot_Str_FarWarTempScion":
                        AddItem(jobj, ItemSlot.JSlot_Str_FarWarTempScion);
                        break;
                    case "JSlot_StrInt_Templar":
                        AddItem(jobj, ItemSlot.JSlot_StrInt_Templar);
                        break;
                    case "JSlot_StrDex_Duelist":
                        AddItem(jobj, ItemSlot.JSlot_StrDex_Duelist);
                        break;
                    case "JSlot_Neutral_IronGrip":
                        AddItem(jobj, ItemSlot.JSlot_Neutral_IronGrip);
                        break;
                    case "JSlot_Neutral_PointBlank":
                        AddItem(jobj, ItemSlot.JSlot_Neutral_PointBlank);
                        break;
                    case "JSlot_Neutral_MinionInstability":
                        AddItem(jobj, ItemSlot.JSlot_Neutral_MinionInstability);
                        break;
                    case "JSlot_Str_Warrior":
                        AddItem(jobj, ItemSlot.JSlot_Str_Warrior);
                        break;
                }
            }

            RefreshItemAttributes();
        }

        public string ToJsonString()
        {
            var items = new JArray();
            foreach (var item in Equip)
            {
                var jItem = item.JsonBase;
                switch (item.Slot)
                {
                    case ItemSlot.BodyArmour:
                        jItem["inventoryId"] = "BodyArmour";
                        break;
                    case ItemSlot.MainHand:
                        jItem["inventoryId"] = "Weapon";
                        break;
                    case ItemSlot.OffHand:
                        jItem["inventoryId"] = "Offhand";
                        break;
                    case ItemSlot.Ring:
                        jItem["inventoryId"] = "Ring";
                        break;
                    case ItemSlot.Ring2:
                        jItem["inventoryId"] = "Ring2";
                        break;
                    case ItemSlot.Amulet:
                        jItem["inventoryId"] = "Amulet";
                        break;
                    case ItemSlot.Helm:
                        jItem["inventoryId"] = "Helm";
                        break;
                    case ItemSlot.Gloves:
                        jItem["inventoryId"] = "Gloves";
                        break;
                    case ItemSlot.Boots:
                        jItem["inventoryId"] = "Boots";
                        break;
                    case ItemSlot.Belt:
                        jItem["inventoryId"] = "Belt";
                        break;
                    case ItemSlot.JSlot_DexInt_Scion:
                        jItem["inventoryId"] = "JSlot_DexInt_Scion";
                        break;
                    case ItemSlot.JSlot_DexInt_Shadow:
                        jItem["inventoryId"] = "JSlot_DexInt_Shadow";
                        break;
                    case ItemSlot.JSlot_Dex_Ranger:
                        jItem["inventoryId"] = "JSlot_Dex_Ranger";
                        break;
                    case ItemSlot.JSlot_Dex_RangerDuelist:
                        jItem["inventoryId"] = "JSlot_Dex_RangerDuelist";
                        break;
                    case ItemSlot.JSlot_Dex_ShadowRanger:
                        jItem["inventoryId"] = "JSlot_Dex_ShadowRanger";
                        break;
                    case ItemSlot.JSlot_Int_Scion:
                        jItem["inventoryId"] = "JSlot_Int_Scion";
                        break;
                    case ItemSlot.JSlot_Int_TemplarWitch:
                        jItem["inventoryId"] = "JSlot_Int_TemplarWitch";
                        break;
                    case ItemSlot.JSlot_Int_Witch:
                        jItem["inventoryId"] = "JSlot_Int_Witch";
                        break;
                    case ItemSlot.JSlot_Int_WitchShadow:
                        jItem["inventoryId"] = "JSlot_Int_WitchShadow";
                        break;
                    case ItemSlot.JSlot_Neutral_Acrobatics:
                        jItem["inventoryId"] = "JSlot_Neutral_Acrobatics";
                        break;
                    case ItemSlot.JSlot_Neutral_IronGrip:
                        jItem["inventoryId"] = "JSlot_Neutral_IronGrip";
                        break;
                    case ItemSlot.JSlot_Neutral_MinionInstability:
                        jItem["inventoryId"] = "JSlot_Neutral_MinionInstability";
                        break;
                    case ItemSlot.JSlot_Neutral_PointBlank:
                        jItem["inventoryId"] = "JSlot_Neutral_PointBlank";
                        break;
                    case ItemSlot.JSlot_StrDex_Duelist:
                        jItem["inventoryId"] = "JSlot_StrDex_Duelist";
                        break;
                    case ItemSlot.JSlot_StrDex_Scion:
                        jItem["inventoryId"] = "JSlot_StrDex_Scion";
                        break;
                    case ItemSlot.JSlot_StrInt_Scion:
                        jItem["inventoryId"] = "JSlot_StrInt_Scion";
                        break;
                    case ItemSlot.JSlot_StrInt_Templar:
                        jItem["inventoryId"] = "JSlot_StrInt_Templar";
                        break;
                    case ItemSlot.JSlot_Str_FarWarTempScion:
                        jItem["inventoryId"] = "JSlot_Str_FarWarTempScion";
                        break;
                    case ItemSlot.JSlot_Str_Warrior:
                        jItem["inventoryId"] = "JSlot_Str_Warrior";
                        break;
                    case ItemSlot.JSlot_Str_WarriorDuelist:
                        jItem["inventoryId"] = "JSlot_Str_WarriorDuelist";
                        break;
                    case ItemSlot.JSlot_Str_WarriorTemplarScion:
                        jItem["inventoryId"] = "JSlot_Str_WarriorTemplarScion";
                        break;
                }
                items.Add(jItem);
            }
            var jObj = new JObject();
            jObj["items"] = items;
            return jObj.ToString(Formatting.None);
        }

        private void RefreshItemAttributes()
        {
			int FirstJewelIndex = (int)ItemSlot.JSlot_Int_Witch;
			var ReducedEquip = Equip.TakeWhile(i => (int)i.Slot< FirstJewelIndex);
			NonLocalMods = (from item in ReducedEquip
							from mod in SelectNonLocalMods(item)
                            group mod by mod.Attribute into modsForAttr
                            select modsForAttr.Aggregate((m1, m2) => m1.Sum(m2))
                           ).ToList();

            var aList = new List<Attribute>();
            var independent = new List<Attribute>();
			int SlotIndex;

			foreach (var item in Equip)
			{
				SlotIndex = (int)item.Slot;
				if (SlotIndex < FirstJewelIndex)//Add Jewel Stats during Jewel Conversion code instead
				{
					LoadItemAttributes(item, aList, independent);
				}

			}
            aList.AddRange(independent);
            Attributes = new ListCollectionView(aList);

            var pgd = new PropertyGroupDescription("Group", new HeaderConverter());
            Attributes.GroupDescriptions.Add(pgd);

            Attributes.Refresh();
        }

		private static void AddAttribute(ItemMod mod, string group, ICollection<Attribute> attributes, Attribute existingAttribute)
        {
            if (existingAttribute == null)
            {
                attributes.Add(new Attribute(mod.Attribute, mod.Values, group));
            }
            else
            {
                existingAttribute.Add(mod.Values);
            }
        }

        private static void LoadItemAttributes(Item item, List<Attribute> attributes, List<Attribute> independentAttributes)
        {
            foreach (var attr in item.Properties)
            {
                // Show all properties except quality in the group for this slot.
                if (attr.Attribute == "Quality: +#%") continue;
                attributes.Add(new Attribute(attr.Attribute, attr.Values, item.Slot.ToString()));
            }

            var modsAffectingProperties = item.GetModsAffectingProperties().SelectMany(pair => pair.Value).ToList();
            foreach (var mod in item.Mods)
            {
                if (mod.IsLocal)
                {
                    // Show local mods in the group for this slot
                    // if they are not already represented by affecting properties.
                    if (mod.Attribute.StartsWith("Adds") || modsAffectingProperties.Contains(mod))
                        continue;
                    var attTo = attributes.Find(ad => ad.TextAttribute == mod.Attribute && ad.Group == item.Slot.ToString());
                    AddAttribute(mod, item.Slot.ToString(), attributes, attTo);
                }
                else
                {
                    // Show all non-local mods in the Independent group.
                    var attTo = independentAttributes.Find(ad => ad.TextAttribute == mod.Attribute && ad.Group == "Independent");
                    AddAttribute(mod, "Independent", independentAttributes, attTo);
                }
            }
        }

        private static IEnumerable<ItemMod> SelectNonLocalMods(Item item)
        {
            var mods = item.Mods.Where(m => !m.IsLocal);
            // Weapons are treated differently, their properties do not count towards global mods.
            if (!item.Tags.HasFlag(Tags.Weapon))
                return mods.Union(item.Properties.Where(p => p.Attribute != "Quality: +#%"));
            return mods;
        }

        private void AddItem(JObject val, ItemSlot islot)
        {
            var item = new Item(_persistentData, val, islot);
            Equip.Add(item);
            item.PropertyChanged += SlottedItemOnPropertyChanged;
        }


        public class Attribute : Notifier
        {
            public static readonly Regex Backreplace = new Regex("#");

            private readonly List<float> _value;

            private readonly string _group;
            public string Group
            {
                get { return _group; }
            }

            private readonly string _attribute;
            public string TextAttribute
            {
                get { return _attribute; }
            }

            public string ValuedAttribute
            {
                get { return _value.Aggregate(_attribute, (current, f) => Backreplace.Replace(current, f + "", 1)); }
            }

            public Attribute(string s, IEnumerable<float> val, string grp)
            {
                _attribute = s;
                _value = new List<float>(val);
                _group = grp;
            }

            public void Add(IReadOnlyList<float> val)
            {
                if (_value.Count != val.Count) throw new NotSupportedException();
                for (var i = 0; i < val.Count; i++)
                {
                    _value[i] += val[i];
                }
                OnPropertyChanged("ValuedAttribute");
            }
        }


        private class HeaderConverter : IValueConverter
        {
            private readonly Dictionary<string, AttributeGroup> _itemGroups = new Dictionary<string, AttributeGroup>();

            public HeaderConverter()
            {
                foreach (var slot in Enum.GetValues(typeof(ItemSlot)))
                {
                    if (!_itemGroups.ContainsKey(slot.ToString()))
                    {
                        _itemGroups.Add(slot.ToString(), new AttributeGroup(slot.ToString()));
                    }
                }

                _itemGroups.Add("Independent", new AttributeGroup("Independent"));
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return _itemGroups[value.ToString()];
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}