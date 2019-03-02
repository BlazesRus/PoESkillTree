﻿using System;
using PoESkillTree.Computation.Core;

namespace POESKillTree.Computation.Model
{
    public interface IObservingCalculator
    {
        void SubscribeTo(IObservable<CalculatorUpdate> updateObservable);
    }
}