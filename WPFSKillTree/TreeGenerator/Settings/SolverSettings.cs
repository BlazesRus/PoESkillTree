﻿using System;
using System.Collections.Generic;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.ViewModels.PassiveTree;

namespace PoESkillTree.TreeGenerator.Settings
{
    /// <summary>
    /// Data class for the settings shared by all tree generator solvers.
    /// </summary>
    public class SolverSettings
    {

        /// <summary>
        /// Set of Nodes that must be included in the result tree.
        /// </summary>
        public readonly ISet<PassiveNodeViewModel> Checked;

        /// <summary>
        /// Set of Nodes that must not be included in the result tree.
        /// </summary>
        public readonly ISet<PassiveNodeViewModel> Crossed;

        /// <summary>
        /// The number of iterations the solver should run. Each iteration is independent of each other
        /// and the best solution over all iterations is kept.
        /// </summary>
        public readonly int Iterations;

        /// <param name="settings">The settings to create a (shallow) copy of.</param>
        protected SolverSettings(SolverSettings settings)
            : this(settings.Checked, settings.Crossed, settings.Iterations)
        {
        }

        /// <summary>
        /// Creates new SolverSettings.
        /// </summary>
        /// <param name="checked">Set of Nodes that must be included in the result tree.</param>
        /// <param name="crossed">Set of Nodes that must not be included in the result tree.</param>
        /// <param name="iterations">The number of iterations the solver should run. (>= 1)</param>
        public SolverSettings(ISet<PassiveNodeViewModel>? @checked, ISet<PassiveNodeViewModel>? crossed,
            int iterations)
        {
            if (iterations < 1) throw new ArgumentOutOfRangeException(nameof(iterations), iterations, "must be >= 1");

            Checked = @checked ?? new HashSet<PassiveNodeViewModel>();
            Crossed = crossed ?? new HashSet<PassiveNodeViewModel>();
            Iterations = iterations;
        }
    }
}