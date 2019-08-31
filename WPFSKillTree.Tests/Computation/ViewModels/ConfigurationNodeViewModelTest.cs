﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;
using POESKillTree.Computation.ViewModels;

namespace PoESkillTree.Tests.Computation.ViewModels
{
    [TestFixture]
    public class ConfigurationNodeViewModelTest
    {
        [Test]
        public void SubscribeCalculatorGeneratesCorrectValues()
        {
            var stat = new Stat("");
            var calculatorMock = new Mock<IObservingCalculator>();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat.Minimum, NodeType.Total, PathDefinition.MainPath) == new NodeValue(1) &&
                c.GetValue(stat.Maximum, NodeType.Total, PathDefinition.MainPath) == new NodeValue(3));
            var expectedValues = new (NodeValue? added, NodeValue? removed)[]
            {
                ((NodeValue?) true, null),
                (new NodeValue(3), (NodeValue?) true),
                (null, new NodeValue(3)),
                (new NodeValue(1), null),
                (null, new NodeValue(1)),
            };
            var actualUpdates = new List<CalculatorUpdate>();
            using (var sut = CreateSut(stat))
            {
                calculatorMock.Setup(
                        c => c.SubscribeTo(It.IsAny<IObservable<CalculatorUpdate>>(), It.IsAny<Action<Exception>>()))
                    .Callback<IObservable<CalculatorUpdate>, Action<Exception>>(
                        (observable, _) => observable.Subscribe(actualUpdates.Add))
                    .Returns(Disposable.Empty);

                sut.SubscribeCalculator(calculatorMock.Object);
                sut.BoolValue = true;
                sut.NumericValue = 4;
                sut.Value = null;
                sut.NumericValue = -2;
            }

            var actualValues = actualUpdates.Select(u =>
                (u.AddedModifiers.SingleOrDefault()?.Value.Calculate(context),
                    u.RemovedModifiers.SingleOrDefault()?.Value.Calculate(context)));
            Assert.AreEqual(expectedValues, actualValues);
        }

        [Test]
        public void ResetValueSetsValueToUserSpecifiedValue()
        {
            var stat = new Stat("",
                explicitRegistrationType: ExplicitRegistrationTypes.UserSpecifiedValue(true));
            var sut = CreateSut(stat);

            sut.ResetValue();

            Assert.AreEqual(true, sut.BoolValue);
        }

        [TestCase(null)]
        [TestCase(1.2)]
        public void ResetValueSetsValueToDefaultIfNotUserSpecified(double? defaultValue)
        {
            var stat = new Stat("");
            var sut = CreateSut(stat, defaultValue);
            sut.BoolValue = true;

            sut.ResetValue();

            Assert.AreEqual(defaultValue, sut.NumericValue);
        }

        private static ConfigurationNodeViewModel CreateSut(IStat stat, double? defaultValue = null)
            => new ConfigurationNodeViewModel(stat, (NodeValue?) defaultValue);
    }
}