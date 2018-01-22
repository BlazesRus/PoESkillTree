﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using POESKillTree.Model.Items;
using POESKillTree.Model.JsonSettings;
using POESKillTree.SkillTreeFiles;
using POESKillTree.ViewModels.Equipment;

namespace POESKillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// ViewModel that enables setting up and running <see cref="Solver.ISolver"/> through
    /// contained <see cref="GeneratorTabViewModel"/>s.
    /// </summary>
    public sealed class SettingsViewModel : AbstractCompositeSetting
    {
        private readonly ISettingsDialogCoordinator _dialogCoordinator;
        private readonly object _dialogContext;

        /// <summary>
        /// Gets the tree.
        /// </summary>
        /// <value>
        /// The tree.
        /// </value>
        public SkillTree Tree { get; }

        /// <summary>
        /// The item information
        /// </summary>
        public InventoryViewModel ItemInfo;

        /// <summary>
        /// Gets or sets the observable collection of <see cref="GeneratorTabViewModel"/> contained in
        /// this ViewModel. Index 0 is the Steiner tab, 1 the Advanced tab.
        /// </summary>
        public ObservableCollection<GeneratorTabViewModel> Tabs { get; }

        protected override string Key { get; } = "TreeGenerator";

        protected override IReadOnlyList<ISetting> SubSettings { get; }

        #region Presentation

        /// <summary>
        /// The currently selected <see cref="GeneratorTabViewModel"/>.
        /// </summary>
        public LeafSetting<int> SelectedTabIndex { get; }

        #endregion

        /// <summary>
        /// Constructs a new SettingsViewModel that operates on the given skill tree.
        /// </summary>
        /// <param name="tree">The skill tree to operate on. (not null)</param>
        /// <param name="dialogCoordinator">The dialog coordinator.</param>
        /// <param name="itemAttributes">The item attributes.</param>
        /// <param name="dialogContext">The context used for <paramref name="dialogCoordinator" />.</param>
        public SettingsViewModel(SkillTree tree, ISettingsDialogCoordinator dialogCoordinator, InventoryViewModel itemInfo, object dialogContext)
        {
            Tree = tree;
            ItemInfo = itemInfo;
            _dialogCoordinator = dialogCoordinator;
            _dialogContext = dialogContext;

            SelectedTabIndex = new LeafSetting<int>(nameof(SelectedTabIndex), 0);

            Action<GeneratorTabViewModel> runCallback = async g => await RunAsync(g);
            Tabs = new ObservableCollection<GeneratorTabViewModel>
            {
                new SteinerTabViewModel(Tree, _dialogCoordinator, _dialogContext, runCallback),
                new AdvancedTabViewModel(Tree, _dialogCoordinator, _dialogContext, ItemInfo, runCallback),
                new AutomatedTabViewModel(Tree, _dialogCoordinator, _dialogContext, runCallback)
            };
            SubSettings = new ISetting[] {SelectedTabIndex}.Union(Tabs).ToArray();
        }

        public async Task RunAsync(GeneratorTabViewModel generator)
        {
            var savedHighlights = Tree.HighlightedNodes.ToList();

            var solver = await generator.CreateSolverAsync();
            if (solver == null)
                return;

            var controllerResult = await _dialogCoordinator
                .ShowControllerDialogAsync(_dialogContext, solver, generator.DisplayName, Tree);
            if (controllerResult != null)
            {
                Tree.SkilledNodes.Clear();
                Tree.AllocateSkillNodes(controllerResult.Select(n => SkillTree.Skillnodes[n]));
            }
            Tree.HighlightedNodes.Clear();
            Tree.HighlightedNodes.UnionWith(savedHighlights);
            Tree.DrawHighlights();

            RunFinished?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised when <see cref="RunAsync"/> execution is finished
        /// and the skill tree may have changed.
        /// </summary>
        public event EventHandler RunFinished;
    }
}