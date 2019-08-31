﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;
using POESKillTree.Computation.Model;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ComputationViewModel : Notifier
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;

        public MainSkillSelectionViewModel MainSkillSelection { get; private set; }
        public ResultStatsViewModel OffensiveStats { get; }
        public ResultStatsViewModel DefensiveStats { get; }
        public ConfigurationStatsViewModel ConfigurationStats { get; }
        public SharedConfigurationViewModel SharedConfiguration { get; private set; }

        private ComputationViewModel(ObservableCalculator observableCalculator, ComputationSchedulerProvider schedulers)
        {
            _nodeFactory = new CalculationNodeViewModelFactory(observableCalculator, schedulers.Dispatcher);
            OffensiveStats = new ResultStatsViewModel(_nodeFactory);
            DefensiveStats = new ResultStatsViewModel(_nodeFactory);
            ConfigurationStats = new ConfigurationStatsViewModel(observableCalculator, _nodeFactory);
        }

        private async Task InitializeAsync(
            SkillDefinitions skillDefinitions, IBuilderFactories f, ObservableCollection<IReadOnlyList<Skill>> skills)
        {
            MainSkillSelection = MainSkillSelectionViewModel.Create(skillDefinitions, f, _nodeFactory, skills);

            AddStats(OffensiveStats, f.MetaStatBuilders.SkillDpsWithHits);
            AddStats(OffensiveStats, f.MetaStatBuilders.AverageHitDamage);
            AddStats(OffensiveStats, f.DamageTypeBuilders.Physical.Damage.WithSkills);
            AddStats(OffensiveStats, f.DamageTypeBuilders.Lightning.Damage.WithSkills);
            AddStats(OffensiveStats, f.StatBuilders.CastRate);
            AddStats(OffensiveStats, f.StatBuilders.HitRate);
            AddStats(OffensiveStats, f.MetaStatBuilders.SkillHitDamageSource);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillDpsWithDoTs);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.AverageDamage.WithSkills);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillUsesHand(AttackDamageHand.MainHand));
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillUsesHand(AttackDamageHand.OffHand));
            AddAvailableStats(OffensiveStats, f.StatBuilders.BaseCastTime.With(AttackDamageHand.MainHand));
            AddAvailableStats(OffensiveStats, f.EffectBuilders.Stun.Threshold.With(AttackDamageHand.MainHand), Entity.Enemy);
            AddAvailableStats(OffensiveStats, f.StatBuilders.AreaOfEffect);

            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.Life));
            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.Life), nodeType: NodeType.Increase);
            AddStats(DefensiveStats, f.StatBuilders.Armour);
            AddStats(DefensiveStats, f.StatBuilders.Evasion);
            AddStats(DefensiveStats, f.DamageTypeBuilders.AnyDamageType().Resistance);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Strength);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Dexterity);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Intelligence);
            AddStats(DefensiveStats, f.StatBuilders.PassivePoints, nodeType: NodeType.UncappedSubtotal);
            AddStats(DefensiveStats, f.StatBuilders.PassivePoints.Maximum);
            AddStats(DefensiveStats, f.StatBuilders.AscendancyPassivePoints, nodeType: NodeType.UncappedSubtotal);
            AddStats(DefensiveStats, f.StatBuilders.AscendancyPassivePoints.Maximum);
            AddAvailableStats(DefensiveStats, f.MetaStatBuilders.SelectedBandit);

            await AddConfigurationStatAsync(f.StatBuilders.Level, Entity.Enemy, false);
            await AddConfigurationStatAsync(f.MetaStatBuilders.SelectedQuestPart);
            ConfigurationStats.Observe();

            SharedConfiguration = SharedConfigurationViewModel.Create(_nodeFactory, f);
        }

        private static void AddAvailableStats(
            ResultStatsViewModel viewModel, IStatBuilder statBuilder, Entity entity = Entity.Character)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                viewModel.AddAvailableStat(stat);
            }
        }

        private static void AddStats(
            ResultStatsViewModel viewModel,
            IStatBuilder statBuilder, Entity entity = Entity.Character,
            NodeType nodeType = NodeType.Total)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                viewModel.AddStat(stat, nodeType);
            }
        }

        private async Task AddConfigurationStatAsync(IStatBuilder statBuilder, Entity entity = Entity.Character,
            bool initializeWithCurrentValue = true)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                await ConfigurationStats.AddPinnedAsync(stat, initializeWithCurrentValue);
            }
        }

        public static async Task<ComputationViewModel> CreateAsync(
            GameData gameData, IBuilderFactories builderFactories,
            ObservableCalculator observableCalculator, ComputationSchedulerProvider schedulers,
            ObservableCollection<IReadOnlyList<Skill>> skills)
        {
            var skillDefinitions = await gameData.Skills;
            var vm = new ComputationViewModel(observableCalculator, schedulers);
            await vm.InitializeAsync(skillDefinitions, builderFactories, skills);
            return vm;
        }
    }
}