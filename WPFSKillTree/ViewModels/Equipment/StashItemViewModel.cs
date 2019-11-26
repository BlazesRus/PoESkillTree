﻿using System.Windows;
using PoESkillTree.Model.Items;

namespace PoESkillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for items in the stash.
    /// </summary>
    public class StashItemViewModel : DraggableItemViewModel
    {
        private Item _item;
        public sealed override Item Item
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        private bool _highlight;
        /// <summary>
        /// Gets or sets whether this view model should be displayed highlighted.
        /// </summary>
        public bool Highlight
        {
            // ReSharper disable once UnusedMember.Global Used in styles
            get => _highlight;
            set => SetProperty(ref _highlight, value);
        }

        public override DragDropEffects DropOnInventoryEffect => DragDropEffects.Copy;

        public StashItemViewModel(Item item)
            => _item = item;
    }
}
