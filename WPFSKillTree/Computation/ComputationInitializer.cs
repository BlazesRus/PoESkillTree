﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using PoESkillTree.Computation.Model;
using PoESkillTree.Computation.ViewModels;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Core;
using PoESkillTree.Engine.Computation.Parsing;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Model;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;
using PoESkillTree.ViewModels.Equipment;

namespace PoESkillTree.Computation
{
    public class ComputationInitializer
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly GameDataWithOldTreeModel _gameData;

        private ICalculator _iCalculator;
        private IBuilderFactories _builderFactories;
        private IParser _parser;
        private ComputationSchedulerProvider _schedulers;
        private ComputationObservables _observables;
        private ObservableCalculator _calculator;

        private Task _initialParseTask;

        private ObservableSet<IReadOnlyList<Skill>> _skills;

#pragma warning disable CS8618 // The instance variables will be initialized if methods are called and awaited in the correct order
        private ComputationInitializer()
#pragma warning restore
        {
            _gameData = new GameDataWithOldTreeModel();
        }

        public static ComputationInitializer StartNew()
        {
            var instance = new ComputationInitializer();
            instance.GameData.StartAllTasks();
            return instance;
        }

        public GameData GameData => _gameData.Data;

        public async Task InitializeAsync(IEnumerable<SkillNode> skillNodes)
        {
            await InitializeFields(skillNodes);
            _initialParseTask = DoInitialParseAsync();
        }

        private async Task InitializeFields(IEnumerable<SkillNode> skillNodes)
        {
            _gameData.PassiveNodes = skillNodes;

            var computationFactory = new ComputationFactory(GameData);
            _iCalculator = computationFactory.CreateCalculator();
            _builderFactories = await computationFactory.CreateBuilderFactoriesAsync();
            _parser = await computationFactory.CreateParserAsync();

            _schedulers = new ComputationSchedulerProvider();
            _observables = new ComputationObservables(_parser, _schedulers.CalculationThread);
            _calculator = new ObservableCalculator(_iCalculator, _schedulers.CalculationThread);
        }

        private async Task DoInitialParseAsync()
        {
            var parserInitializationTask = _schedulers.NewThread.ScheduleAsync(_parser.Initialize);
            var passiveTree = await GameData.PassiveTree;
            var initialObservable = _observables.InitialParse(passiveTree, TimeSpan.FromMilliseconds(500));
            await _calculator.ForEachUpdateCalculatorAsync(initialObservable);
            await parserInitializationTask;
        }

        public async Task InitializeAfterBuildLoadAsync(
            ObservableSet<SkillNode> skilledNodes, ObservableSet<(Item, ItemSlot)> items,
            ObservableSet<(Item, ItemSlot, ushort, JewelRadius)> jewels, ObservableSet<IReadOnlyList<Skill>> skills)
        {
            _skills = skills;
            await Task.WhenAll(_initialParseTask,
                ConnectToSkilledPassiveNodesAsync(skilledNodes),
                ConnectToEquipmentAsync(items),
                ConnectToJewelsAsync(jewels),
                ConnectToSkillsAsync(skills));
        }

        private async Task ConnectToSkilledPassiveNodesAsync(ObservableSet<SkillNode> skilledNodes)
            => await ConnectAsync(
                _observables.ParseSkilledPassiveNodesAsync(skilledNodes),
                _observables.ObserveSkilledPassiveNodes(skilledNodes));

        private async Task ConnectToEquipmentAsync(ObservableSet<(Item, ItemSlot)> items)
            => await ConnectAsync(
                _observables.ParseItemsAsync(items),
                _observables.ObserveItems(items));

        private async Task ConnectToJewelsAsync(ObservableSet<(Item, ItemSlot, ushort, JewelRadius)> jewels)
            => await ConnectAsync(
                _observables.ParseJewelsAsync(jewels),
                _observables.ObserveJewels(jewels));

        private async Task ConnectToSkillsAsync(ObservableSet<IReadOnlyList<Skill>> skills)
            => await ConnectAsync(
                _observables.ParseSkillsAsync(skills),
                _observables.ObserveSkills(skills));

        private async Task ConnectAsync(
            Task<CalculatorUpdate> initialUpdate, IObservable<CalculatorUpdate> changeObservable)
        {
            await _calculator.UpdateCalculatorAsync(initialUpdate);
            _calculator.SubscribeTo(changeObservable);
        }

        public async Task<ComputationViewModel> CreateComputationViewModelAsync(IPersistentData persistentData)
        {
            var vm =
                await ComputationViewModel.CreateAsync(GameData, _builderFactories, _calculator, _schedulers, _skills);
            ConfigurationStatsConnector.Connect(persistentData, vm.ConfigurationStats.Stats,
                vm.MainSkillSelection.ConfigurationNodes);
            return vm;
        }

        public void SetupPeriodicActions()
            => _calculator.PeriodicallyRemoveUnusedNodes(
                ex => Log.Error(ex, "Exception while removing unused calculation nodes"));

        public AbyssalSocketObserver CreateAbyssalSocketObserver(
            IReadOnlyDictionary<ItemSlot, IReadOnlyList<InventoryItemViewModel>> jewels)
        {
            var observer = AbyssalSocketObserver.Create(_calculator, _schedulers.Dispatcher, _builderFactories);
            observer.SetItemJewelViewModels(jewels);
            return observer;
        }

        public async Task<IObservable<IEnumerable<ushort>>> CreateItemAllocatedPassiveNodesObservableAsync()
        {
            return ItemAllocatedPassiveNodesObservableFactory.Create(
                _iCalculator, _schedulers.CalculationThread, _schedulers.Dispatcher,
                _builderFactories.PassiveTreeBuilders, (await GameData.PassiveTree).Nodes);
        }
    }
}