﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.SkillTreeFiles;

namespace PoESkillTree.Model
{
    public class GameDataWithOldTreeModel
    {
        public GameDataWithOldTreeModel()
            => Data = new GameData(new LazyPassiveNodeEnumerable(this), true);

        public GameData Data { get; }

        [DisallowNull]
        public IEnumerable<SkillNode>? PassiveNodes { private get; set; }

        private class LazyPassiveNodeEnumerable : IEnumerable<PassiveNodeDefinition>
        {
            private readonly GameDataWithOldTreeModel _loader;

            public LazyPassiveNodeEnumerable(GameDataWithOldTreeModel loader)
                => _loader = loader;

            public IEnumerator<PassiveNodeDefinition> GetEnumerator()
            {
                if (_loader.PassiveNodes == null)
                    throw new InvalidOperationException("GameDataLoader.PassiveNodes was not yet set");
                return _loader.PassiveNodes.Select(ModelConverter.Convert).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}