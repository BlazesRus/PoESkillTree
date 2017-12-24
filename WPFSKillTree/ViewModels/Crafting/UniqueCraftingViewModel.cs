using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MoreLinq;
using PoESkillTree.Common.Model.Items.Enums;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// View model for crafting unique items.
    /// </summary>
    public class UniqueCraftingViewModel : AbstractCraftingViewModel<UniqueBase>
    {

        private IReadOnlyList<ModSelectorViewModel> _msExplicits = new ModSelectorViewModel[0];
        public IReadOnlyList<ModSelectorViewModel> MsExplicits
        {
            get { return _msExplicits; }
            private set { SetProperty(ref _msExplicits, value); }
        }

        public UniqueCraftingViewModel(EquipmentData equipmentData)
            : base(equipmentData, equipmentData.UniqueBases)
        {
            Init();
        }

        protected override void UpdateBaseSpecific()
        {
            MsExplicits.ForEach(ms => ms.PropertyChanged -= MsOnPropertyChanged);
            var modSelectors = new List<ModSelectorViewModel>();
            foreach (var explicitMod in SelectedBase.ExplicitMods)
            {
                var modSelector = new ModSelectorViewModel(EquipmentData.StatTranslator, false)
                {
                    Affixes = new[]
                    {
                        new Affix(explicitMod)
                    }
                };
                modSelector.PropertyChanged += MsOnPropertyChanged;
                modSelectors.Add(modSelector);
            }
            MsExplicits = modSelectors;
        }

        protected override IEnumerable<IGrouping<ModLocation, StatIdValuePair>> RecalculateItemSpecific(out int requiredLevel)
        {
            Item.NameLine = SelectedBase.UniqueName;
            Item.Frame = FrameType.Unique;
            requiredLevel = MsExplicits
                .Select(ms => ms.Query().RequiredLevel)
                .DefaultIfEmpty()
                .Max();
            IEnumerable<IGrouping<ModLocation, StatIdValuePair>> NewSelf;
            try
            {
                NewSelf = MsExplicits
                    .SelectMany(ms => ms.GetStatValues())
                    .ToLookup(_ => ModLocation.Explicit);
            }
            catch
            {//Fix for creating Jewel uniques
                NewSelf = Enumerable.Empty<IGrouping<ModLocation, StatIdValuePair>>();
                List<IGrouping<ModLocation, StatIdValuePair>> SelfV2 = new List<IGrouping<ModLocation, StatIdValuePair>>(MsExplicits.Count);
                foreach(var Elem in MsExplicits)
                {
                    //Elem.
                }
                //NewSelf.
                return NewSelf;
            }
            return NewSelf;
        }

        private void MsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModSelectorViewModel.SelectedValues))
            {
                RecalculateItem();
            }
        }
    }
}