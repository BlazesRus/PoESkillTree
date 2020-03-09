﻿using PoESkillTree.Model.Items;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Skills
{
    public class SkillViewModel : Notifier
    {
        private bool _isEnabled;
        private IHasItemToolTip _toolTip;

        public SkillViewModel(GemViewModel? gem, int skillIndex, SkillDefinitionViewModel definition)
        {
            Gem = gem;
            SkillIndex = skillIndex;
            Definition = definition;
            _toolTip = CreateToolTip();
        }

        public GemViewModel? Gem { get; }

        public int SkillIndex { get; }

        public SkillDefinitionViewModel Definition { get; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public string DisplayName => Definition.Model.DisplayName ?? "";

        public IHasItemToolTip ToolTip
        {
            get => _toolTip;
            private set => SetProperty(ref _toolTip, value);
        }

        public SkillViewModel Clone() =>
            new SkillViewModel(Gem, SkillIndex, Definition)
            {
                IsEnabled = IsEnabled,
            };

        public void ReCreateToolTip()
        {
            ToolTip = CreateToolTip();
        }

        private IHasItemToolTip CreateToolTip()
        {
            if (Gem != null && Definition.Model.Levels.TryGetValue(Gem.Level, out var levelDefinition))
            {
                return new SkillItem(levelDefinition.Tooltip, Gem.Quality);
            }
            else
            {
                return new SkillItem(DisplayName);
            }
        }
    }
}