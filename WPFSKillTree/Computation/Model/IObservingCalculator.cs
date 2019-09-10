using PoESkillTree.Engine.Computation.Core;
using System;

namespace PoESkillTree.Computation.Model
{
    public interface IObservingCalculator
    {
        void SubscribeTo(IObservable<CalculatorUpdate> updateObservable);
    }
}