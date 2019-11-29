﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using EnumsNET;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Modifiers;
using PoESkillTree.Engine.GameModel.StatTranslation;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Model.Items;
using PoESkillTree.Model.Items.Mods;
using PoESkillTree.Utils;
using Item = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.ViewModels.Crafting
{
    /// <summary>
    /// Base view model for crafting items from a list of bases. Contains all functionality not specific to the type
    /// of the base.
    /// </summary>
    /// <typeparam name="TBase">Type of crafted bases.</typeparam>
    public abstract class AbstractCraftingViewModel<TBase> : CloseableViewModel<bool>
        where TBase : class, IItemBase
    {
        private bool _showDropDisabledItems;
        public bool ShowDropDisabledItems
        {
            get => _showDropDisabledItems;
            set => SetProperty(ref _showDropDisabledItems, value, UpdateSecondLevel);
        }

        // Bases are filtered in three levels:
        // 1. BaseGroup
        // 2. ItemClass
        // 3. specific Tags, i.e. Str/Dex/Int for armour and jewels
        // All levels also support not being filtered (Any/Default)

        // Bases

        private readonly IReadOnlyList<TBase> _bases;
        private IEnumerable<TBase> EligibleBases
            => _bases.Where(b => ShowDropDisabledItems || !b.DropDisabled);

        private IReadOnlyList<TBase>? _baseList;
        public IReadOnlyList<TBase>? BaseList
        {
            get => _baseList;
            private set => SetProperty(ref _baseList, value);
        }

        private TBase _selectedBase;
        public TBase SelectedBase
        {
            get => _selectedBase;
            set => SetProperty(ref _selectedBase, value, UpdateBase);
        }

        // First level

        public IReadOnlyList<BaseGroup> FirstLevelList { get; } = Enums.GetValues<BaseGroup>().ToList();

        private BaseGroup _selectedFirstLevel;
        public BaseGroup SelectedFirstLevel
        {
            get => _selectedFirstLevel;
            set => SetProperty(ref _selectedFirstLevel, value, UpdateSecondLevel);
        }

        // Second level

        private IReadOnlyList<ItemClass> _secondLevelList;
        public IReadOnlyList<ItemClass> SecondLevelList
        {
            get => _secondLevelList;
            private set => SetProperty(ref _secondLevelList, value);
        }

        private ItemClass _selectedSecondLevel;
        public ItemClass SelectedSecondLevel
        {
            get => _selectedSecondLevel;
            set => SetProperty(ref _selectedSecondLevel, value, UpdateThirdLevel);
        }

        // Third level

        private readonly IEnumerable<Tags> _thirdLevelOptions = new[]
        {
            Tags.StrArmour, Tags.DexArmour, Tags.IntArmour, 
            Tags.StrDexArmour, Tags.StrIntArmour, Tags.DexIntArmour, 
            Tags.StrDexIntArmour,
            Tags.StrJewel, Tags.DexJewel, Tags.IntJewel
        };

        private IReadOnlyList<Tags> _thirdLevelList;
        public IReadOnlyList<Tags> ThirdLevelList
        {
            get => _thirdLevelList;
            private set => SetProperty(ref _thirdLevelList, value);
        }

        private Tags _selectedThirdLevel;
        public Tags SelectedThirdLevel
        {
            get => _selectedThirdLevel;
            set => SetProperty(ref _selectedThirdLevel, value, UpdateBaseList);
        }


        private Item _item;

        public Item Item
        {
            get => _item;
            private set => SetProperty(ref _item, value);
        }

        private IReadOnlyList<ModSelectorViewModel> _msImplicits = new ModSelectorViewModel[0];
        public IReadOnlyList<ModSelectorViewModel> MsImplicits
        {
            get => _msImplicits;
            private set => SetProperty(ref _msImplicits, value);
        }

        private readonly SliderViewModel _qualitySlider;
        public SliderGroupViewModel QualitySliderGroup { get; }

        private bool _showQualitySlider;
        public bool ShowQualitySlider
        {
            get => _showQualitySlider;
            private set => SetProperty(ref _showQualitySlider, value);
        }

        protected SimpleMonitor Monitor { get; } = new SimpleMonitor();

        protected EquipmentData EquipmentData { get; }

#pragma warning disable CS8618 // All levels are initialized in Init
        protected AbstractCraftingViewModel(EquipmentData equipmentData, IEnumerable<TBase> bases)
#pragma warning restore
        {
            EquipmentData = equipmentData;
            _bases = bases.ToList();
            // setting the underlying field directly, setting other properties is initiated in Init()
            _selectedFirstLevel = FirstLevelList[0];
            Monitor.Freed += (sender, args) => RecalculateItem();

            var qualityStat = new FormatTranslation("Quality: +{0}%");
            _qualitySlider = new SliderViewModel(0, Enumerable.Range(0, 21));
            QualitySliderGroup = new SliderGroupViewModel(new[] { _qualitySlider }, qualityStat);
            _qualitySlider.ValueChanged += QualitySliderOnValueChanged;
        }

        protected void Init()
        {
            UpdateSecondLevel();
        }

        private void UpdateSecondLevel()
        {
            using (Monitor.Enter())
            {
                var previousSecondLevel = SelectedSecondLevel;

                if (SelectedFirstLevel == BaseGroup.Any)
                {
                    SecondLevelList = EligibleBases.Select(b => b.ItemClass)
                        .Prepend(ItemClass.Any)
                        .Distinct()
                        .OrderBy(c => c).ToList();
                }
                else
                {
                    var list = EligibleBases
                        .Where(b => BaseGroupEx.FromTags(b.Tags) == SelectedFirstLevel)
                        .Select(b => b.ItemClass)
                        .Distinct()
                        .OrderBy(c => c).ToList();
                    switch (list.Count)
                    {
                        case 1:
                            // Only contains the only class of the selected group
                            // -> "any" makes no sense
                            SecondLevelList = list;
                            break;
                        default:
                            SecondLevelList = list.Prepend(ItemClass.Any).ToList();
                            break;
                    }
                }
                SelectedSecondLevel = SecondLevelList.Contains(previousSecondLevel)
                    ? previousSecondLevel
                    : SecondLevelList[0];

                if (previousSecondLevel == SelectedSecondLevel)
                {
                    UpdateThirdLevel();
                }
            }
        }

        private void UpdateThirdLevel()
        {
            var previousThirdLevel = SelectedThirdLevel;

            if (SelectedSecondLevel == ItemClass.Any)
            {
                ThirdLevelList = new[] { Tags.Default };
            }
            else
            {
                var matchingBases = EligibleBases
                    .Where(b => b.ItemClass == SelectedSecondLevel)
                    .ToList();
                var list = _thirdLevelOptions
                    .Where(t => matchingBases.Any(b => b.Tags.HasFlag(t)))
                    .ToList();
                switch (list.Count)
                {
                    case 0:
                        ThirdLevelList = new[] { Tags.Default };
                        break;
                    case 1:
                        ThirdLevelList = list;
                        break;
                    default:
                        ThirdLevelList = list.Prepend(Tags.Default).ToList();
                        break;
                }
            }
            SelectedThirdLevel = ThirdLevelList.Contains(previousThirdLevel)
                ? previousThirdLevel
                : ThirdLevelList[0];

            if (previousThirdLevel == SelectedThirdLevel)
            {
                UpdateBaseList();
            }
        }

        private void UpdateBaseList()
        {
            using (Monitor.Enter())
            {
                var previousSelectedBase = SelectedBase;

                var bases = EligibleBases;
                if (SelectedFirstLevel != BaseGroup.Any)
                {
                    bases = bases.Where(b => SelectedFirstLevel.Matches(b.Tags));
                }
                if (SelectedSecondLevel != ItemClass.Any)
                {
                    bases = bases.Where(b => b.ItemClass == SelectedSecondLevel);
                }
                if (SelectedThirdLevel != Tags.Default)
                {
                    bases = bases.Where(b => b.Tags.HasFlag(SelectedThirdLevel));
                }
                BaseList = bases.ToList();
                SelectedBase = BaseList.Contains(previousSelectedBase)
                    ? previousSelectedBase
                    : BaseList[0];

                if (SelectedBase == previousSelectedBase)
                {
                    UpdateBase();
                }
            }
        }

        private void UpdateBase()
        {
            if (SelectedBase == null)
            {
                return;
            }

            using (Monitor.Enter())
            {
                var ibase = SelectedBase;
                Item = new Item(ibase);

                foreach (var modSelector in MsImplicits)
                {
                    modSelector.PropertyChanged -= MsOnPropertyChanged;
                }

                var modSelectors = new List<ModSelectorViewModel>();
                foreach (var implicitMod in ibase.ImplicitMods)
                {
                    var modSelector = new ModSelectorViewModel(EquipmentData.StatTranslator, false)
                    {
                        Affixes = new[]
                        {
                            new Affix(implicitMod)
                        }
                    };
                    modSelector.PropertyChanged += MsOnPropertyChanged;
                    modSelectors.Add(modSelector);
                }
                MsImplicits = modSelectors;

                ShowQualitySlider = ibase.CanHaveQuality;

                UpdateBaseSpecific();
            }
        }

        /// <summary>
        /// Set up mod selectors for mods specific to <see cref="TBase"/>.
        /// </summary>
        protected abstract void UpdateBaseSpecific();

        protected void RecalculateItem()
        {
            if (Monitor.IsBusy)
                return;

            Item.NameLine = "";
            Item.TypeLine = Item.BaseType.Name;

            var (explicitStats, craftedStats) = RecalculateItemSpecific(out var requiredLevel);

            Item.ExplicitMods = CreateItemMods(explicitStats).ToList();
            Item.CraftedMods = CreateItemMods(craftedStats).ToList();
            Item.ImplicitMods = CreateItemMods(MsImplicits.SelectMany(ms => ms.GetStatValues())).ToList();

            var quality = SelectedBase.CanHaveQuality 
                ? _qualitySlider.Value
                : 0;
            var properties = Item.BaseType.GetRawProperties(quality)
                .Concat(GetAdditionalProperties());
            Item.Properties = new ObservableCollection<ItemMod>(properties);
            ApplyLocals();

            if (Item.IsWeapon)
            {
                ApplyElementalMods(Item.Mods);
            }

            if (MsImplicits.Any())
            {
                var implicits = MsImplicits.Select(ms => ms.Query());
                requiredLevel = Math.Max(requiredLevel, implicits.Max(m => m.RequiredLevel));
            }
            Item.UpdateRequirements((80 * requiredLevel) / 100);
        }

        /// <summary>
        /// (Re)calculate parts of the item in crafting specific to <see cref="TBase"/>.
        /// </summary>
        /// <param name="requiredLevel">the highest <see cref="IMod.RequiredLevel"/> of any selected mod</param>
        /// <returns>All explicit and crafted stats of the item and their values.</returns>
        protected abstract (IEnumerable<StatIdValuePair> explicitStats, IEnumerable<StatIdValuePair> craftedStats)
            RecalculateItemSpecific(out int requiredLevel);

        protected virtual IEnumerable<ItemMod> GetAdditionalProperties()
            => new ItemMod[0];

        private IEnumerable<ItemMod> CreateItemMods(IEnumerable<StatIdValuePair> statValuePairs)
        {
            if (statValuePairs == null)
            {
                yield break;
            }

            // this list is used to translate the stats in order of appearance,
            // using merged.Keys wouldn't guarantee that
            var statIds = new List<string>();
            var merged = new Dictionary<string, int>();
            foreach (var pair in statValuePairs)
            {
                var stat = pair.StatId;
                statIds.Add(stat);
                var value = pair.Value;
                if (merged.ContainsKey(stat))
                {
                    value += merged[stat];
                }
                merged[stat] = value;
            }

            var lines = EquipmentData.StatTranslator.GetTranslations(statIds)
                .Select(t => t.Translate(merged))
                .WhereNotNull();
            foreach (var line in lines)
            {
                var attr = ItemMod.Numberfilter.Replace(line, "#");
                var isLocal = ModifierLocalityTester.IsLocal(attr, SelectedBase.Tags);
                yield return new ItemMod(line, isLocal);
            }
        }

        private void ApplyElementalMods(IEnumerable<ItemMod> allMods)
        {
            var elementalMods = new List<ItemMod>();
            var chaosMods = new List<ItemMod>();
            foreach (var mod in allMods)
            {
                string attr = mod.Attribute;
                if (attr.StartsWith("Adds") && !attr.Contains("in Main Hand") && !attr.Contains("in Off Hand"))
                {
                    if (attr.Contains("Fire") || attr.Contains("Cold") || attr.Contains("Lightning"))
                    {
                        elementalMods.Add(mod);
                    }
                    if (attr.Contains("Chaos"))
                    {
                        chaosMods.Add(mod);
                    }
                }
            }

            if (elementalMods.Any())
            {
                var values = new List<float>();
                var mods = new List<string>();
                var cols = new List<ValueColoring>();

                var fmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Fire"));
                if (fmod != null)
                {
                    values.AddRange(fmod.Values);
                    mods.Add("#-#");
                    cols.Add(ValueColoring.Fire);
                    cols.Add(ValueColoring.Fire);
                }

                var cmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Cold"));
                if (cmod != null)
                {
                    values.AddRange(cmod.Values);
                    mods.Add("#-#");
                    cols.Add(ValueColoring.Cold);
                    cols.Add(ValueColoring.Cold);
                }

                var lmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Lightning"));
                if (lmod != null)
                {
                    values.AddRange(lmod.Values);
                    mods.Add("#-#");
                    cols.Add(ValueColoring.Lightning);
                    cols.Add(ValueColoring.Lightning);
                }

                Item.Properties.Add(new ItemMod("Elemental Damage: " + string.Join(", ", mods), true, values, cols));
            }

            if (chaosMods.Any())
            {
                Item.Properties.Add(new ItemMod("Chaos Damage: #-#", true, chaosMods[0].Values,
                    new[] { ValueColoring.Chaos, ValueColoring.Chaos }));
            }
        }

        private void ApplyLocals()
        {
            foreach (var pair in Item.GetModsAffectingProperties())
            {
                ItemMod prop = pair.Key;
                List<ItemMod> applymods = pair.Value;

                List<ItemMod> percm = applymods.Where(m => Regex.IsMatch(m.Attribute, @"(?<!\+)#%")).ToList();
                List<ItemMod> valuem = applymods.Except(percm).ToList();

                if (valuem.Count > 0)
                {
                    IReadOnlyList<float> val = valuem
                        .Select(m => m.Values)
                        .Aggregate((l1, l2) => l1.Zip(l2, (f1, f2) => f1 + f2)
                        .ToList());
                    IReadOnlyList<float> nval = prop.Values
                        .Zip(val, (f1, f2) => f1 + f2)
                        .ToList();
                    prop.ValueColors = prop.ValueColors
                        .Select((c, i) => val[i] == nval[i] ? prop.ValueColors[i] : ValueColoring.LocallyAffected)
                        .ToList();
                    prop.Values = nval;
                }

                Func<float, float> roundf = val => (float)Math.Round(val);

                if (prop.Attribute.Contains("Critical"))
                {
                    roundf = f => (float)(Math.Round(f * 10) / 10);
                }
                else if (prop.Attribute.Contains("per Second"))
                {
                    roundf = f => (float)(Math.Round(f * 100) / 100);
                }

                if (percm.Count > 0)
                {
                    var perc = 1f + percm.Select(m => m.Values[0]).Sum() / 100f;
                    prop.ValueColors = prop.ValueColors.Select(c => ValueColoring.LocallyAffected).ToList();
                    prop.Values = prop.Values.Select(v => roundf(v * perc)).ToList();
                }
            }
        }

        private void MsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModSelectorViewModel.SelectedValues))
            {
                RecalculateItem();
            }
        }

        private void QualitySliderOnValueChanged(object? sender, SliderValueChangedEventArgs e)
        {
            RecalculateItem();
        }


        /// <summary>
        /// ITranslation implementation that translates a single value with <see cref="string.Format(string,object)"/>.
        /// Used for the quality slider as the quality has no mod or stat behind it that could be translated.
        /// </summary>
        private class FormatTranslation : ITranslation
        {
            private readonly string _format;

            public FormatTranslation(string format)
            {
                _format = format;
            }

            public string Translate(IReadOnlyList<int> values)
            {
                if (values.Count != 1)
                {
                    throw new ArgumentException("Number of values does not match number of ranges");
                }
                return string.Format(CultureInfo.InvariantCulture, _format, values[0]);
            }
        }

    }
}
