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
using PoESkillTree.GameModel.Skills;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils;
using POESKillTree.ViewModels;

namespace POESKillTree.Model.Items
{
    public class JewelItemAttributes : Notifier
    {
        #region slotted items

        public JewelItem GetItemInSlot(ushort slot)
            => Equip.FirstOrDefault(i => i.Slot == slot);

        public void SetItemInSlot(JewelItem value, ushort slot)
        {
            if (!CanEquip(value, slot))
                return;

            var old = Equip.FirstOrDefault(i => i.Slot == slot);
            if (old != null)
            {
                Equip.Remove(old);
                old.Slot = 0;
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

        public bool CanEquip(JewelItem item, ushort slot)
        {
            if (item == null) return true;
            if (slot == 0) return false;
            return (item.Slot & slot) != 0;
        }
        #endregion

        public ObservableCollection<JewelItem> Equip { get; }

        private ListCollectionView _attributes;
        public ListCollectionView Attributes
        {
            get => _attributes;
            private set => SetProperty(ref _attributes, value);
        }

        public IReadOnlyList<ItemMod> NonLocalMods { get; private set; }

        private readonly EquipmentData _equipmentData;
        public event EventHandler ItemDataChanged;

        private void SlottedItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Item.JsonBase))
            {
                ItemDataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public JewelItemAttributes()
        {
            Equip = new ObservableCollection<JewelItem>();
            RefreshItemAttributes();
        }

        public JewelItemAttributes(EquipmentData equipmentData, string itemData)
        {
            _equipmentData = equipmentData;
            Equip = new ObservableCollection<JewelItem>();

            var jObject = JObject.Parse(itemData);
            //foreach (JObject jobj in (JArray)jObject["items"])
            //{
            //    var inventoryId = jobj.Value<string>("inventoryId");
            //    switch (inventoryId)
            //    {
            //        case "Weapon":
            //            inventoryId = "MainHand";
            //            break;
            //        case "Offhand":
            //            inventoryId = "OffHand";
            //            break;
            //        case "Flask":
            //            inventoryId = $"Flask{jobj.Value<int>("x") + 1}";
            //            break;
            //    }

            //    if (EnumsNET.Enums.TryParse(inventoryId, out ushort slot))
            //    {
            //        AddItem(jobj, slot);
            //    }
            //}

            RefreshItemAttributes();
        }

        public string ToJsonString()
        {
            var items = new JArray();
            foreach (var item in Equip)
            {
                var jItem = item.JsonBase;
                jItem["inventoryId"] = item.Slot.ToString();
                items.Add(jItem);
            }
            var jObj = new JObject { ["items"] = items };
            return jObj.ToString(Formatting.None);
        }

        private void RefreshItemAttributes()
        {
            NonLocalMods = (from item in Equip
                            from mod in SelectNonLocalMods(item)
                            group mod by mod.Attribute into modsForAttr
                            select modsForAttr.Aggregate((m1, m2) => m1.Sum(m2))
                           ).ToList();
            var aList = new List<Attribute>();
            var independent = new List<Attribute>();
            foreach (var item in Equip)
            {
                LoadItemAttributes(item, aList, independent);
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

        private static void LoadItemAttributes(JewelItem item, List<Attribute> attributes, List<Attribute> independentAttributes)
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

        private static IEnumerable<ItemMod> SelectNonLocalMods(JewelItem item)
        {
            var mods = item.Mods.Where(m => !m.IsLocal);
            // Weapons are treated differently, their properties do not count towards global mods.
            if (!item.Tags.HasFlag(Tags.Weapon))
                return mods.Union(item.Properties.Where(p => p.Attribute != "Quality: +#%"));
            return mods;
        }

        private void AddItem(JObject val, ushort islot)
        {
            JewelItem item = new JewelItem(_equipmentData, val, islot);
            Equip.Add(item);
            item.PropertyChanged += SlottedItemOnPropertyChanged;
        }


        public class Attribute : Notifier
        {
            public static readonly Regex Backreplace = new Regex("#");

            private readonly List<float> _value;

            public string Group { get; }

            public string TextAttribute { get; }

            public string ValuedAttribute
            {
                get { return _value.Aggregate(TextAttribute, (current, f) => Backreplace.Replace(current, f + "", 1)); }
            }

            public Attribute(string s, IEnumerable<float> val, string grp)
            {
                TextAttribute = s;
                _value = new List<float>(val);
                Group = grp;
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
