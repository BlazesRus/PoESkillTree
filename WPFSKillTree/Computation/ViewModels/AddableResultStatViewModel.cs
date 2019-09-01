﻿using PoESkillTree.Computation.Common;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public class AddableResultStatViewModel : Notifier
    {
        private IStat _stat;
        private NodeType _nodeType = NodeType.Total;

        public IStat Stat
        {
            get => _stat;
            set => SetProperty(ref _stat, value);
        }

        public NodeType NodeType
        {
            get => _nodeType;
            set => SetProperty(ref _nodeType, value);
        }
    }
}