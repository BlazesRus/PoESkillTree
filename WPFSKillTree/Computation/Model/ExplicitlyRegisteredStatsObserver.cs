﻿using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core;
using PoESkillTree.Engine.Utils;

namespace PoESkillTree.Computation.Model
{
    public class ExplicitlyRegisteredStatsObserver
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ObservableCalculator _observableCalculator;

        private readonly HashSet<(ICalculationNode node, IStat stat)> _items =
            new HashSet<(ICalculationNode, IStat)>();

        public ExplicitlyRegisteredStatsObserver(ObservableCalculator observableCalculator)
            => _observableCalculator = observableCalculator;

        public event Action<ICalculationNode, IStat>? StatAdded;
        public event Action<ICalculationNode, IStat>? StatRemoved;

        public async Task InitializeAsync(IScheduler observeScheduler)
        {
            foreach (var item in await _observableCalculator.GetExplicitlyRegisteredStatsAsync())
                Add(item);

            _observableCalculator.ObserveExplicitlyRegisteredStats()
                .ObserveOn(observeScheduler)
                .Subscribe(OnNext, OnError);
        }

        private void OnNext(CollectionChangedEventArgs<(ICalculationNode node, IStat stat)> args)
        {
            foreach (var item in args.AddedItems)
                Add(item);
            foreach (var item in args.RemovedItems)
                Remove(item);
        }

        private static void OnError(Exception exception)
            => Log.Error(exception, "ObserveExplicitlyRegisteredStats failed");

        private void Add((ICalculationNode, IStat) element)
        {
            if (_items.Add(element))
                StatAdded?.Invoke(element.Item1, element.Item2);
        }

        private void Remove((ICalculationNode, IStat) element)
        {
            if (_items.Remove(element))
                StatRemoved?.Invoke(element.Item1, element.Item2);
        }
    }
}