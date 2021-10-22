﻿using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.GameModel.PassiveTree.Base;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.SkillTreeFiles;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace PoESkillTree.ViewModels.PassiveTree
{
    public class PassiveNodeViewModel
    {
        private readonly JsonPassiveNode JsonPassiveNode;
        public readonly PassiveNodeGroupViewModel? PassiveNodeGroup;

        public PassiveNodeViewModel(ushort id) : this(new JsonPassiveNode() { Id = id }) { }
        public PassiveNodeViewModel(JsonPassiveNode jsonPassiveNode, PassiveNodeGroupViewModel? group = null)
        {
            JsonPassiveNode = jsonPassiveNode;
            PassiveNodeGroup = group;
            InitializeAttributes();
        }

        public PassiveNodeDefinition PassiveNodeDefinition => PassiveNodeDefinition.Convert(JsonPassiveNode);

        public ushort Id { get => JsonPassiveNode.Id; }
        public ushort Skill { get => JsonPassiveNode.Skill; set => JsonPassiveNode.Skill = value; }
        public string Name { get => JsonPassiveNode.Name; }
        public CharacterClass? StartingCharacterClass { get => JsonPassiveNode.StartingCharacterClass; }
        public string[]? Recipe { get => JsonPassiveNode.Recipe; }
        public JsonExpansionJewelSocket? ExpansionJewelSocket { get => JsonPassiveNode.ExpansionJewelSocket; }
        public HashSet<ushort> OutPassiveNodeIds => JsonPassiveNode.OutPassiveNodeIds;
        public HashSet<ushort> InPassiveNodeIds => JsonPassiveNode.InPassiveNodeIds;
        public float[] OrbitRadii { get => JsonPassiveNode.OrbitRadii; }
        public int OrbitRadiiIndex { get => JsonPassiveNode.OrbitRadiiIndex; }
        public bool IsAscendancyNode => JsonPassiveNode.IsAscendancyNode;
        public bool IsRootNode => JsonPassiveNode.IsRootNode;
        public PassiveNodeType PassiveNodeType { get => JsonPassiveNode.PassiveNodeType; }
        public string[] ReminderText { get => JsonPassiveNode.ReminderText; }
        public string[] StatDescriptions { get => JsonPassiveNode.StatDescriptions; private set => JsonPassiveNode.StatDescriptions = value; }
        public int PassivePointsGranted { get => JsonPassiveNode.PassivePointsGranted; }
        public string Icon
        {
            get
            {
                if (PassiveNodeType == PassiveNodeType.Mastery)
                {
                    if (IsSkilled && !(JsonPassiveNode.ActiveIcon is null))
                    {
                        return JsonPassiveNode.ActiveIcon;
                    }

                    if (!(JsonPassiveNode.InactiveIcon is null))
                    {
                        return JsonPassiveNode.InactiveIcon;
                    }
                }

                return JsonPassiveNode.Icon;
            }
        }
        public string? ActiveEffectIcon { get => JsonPassiveNode.ActiveEffectImage; }
        public string? AscendancyName { get => JsonPassiveNode.AscendancyName; }
        public bool IsBlighted { get => JsonPassiveNode.IsBlighted; }
        public bool IsProxy { get => JsonPassiveNode.IsProxy; }
        public bool IsAscendancyStart { get => JsonPassiveNode.IsAscendancyStart; }
        public bool IsMultipleChoice { get => JsonPassiveNode.IsMultipleChoice; }
        public bool IsMultipleChoiceOption { get => JsonPassiveNode.IsMultipleChoiceOption; }
        public bool IsSkilled { get; set; } = false;
        public int Strength { get => JsonPassiveNode.Strength; }
        public int Dexterity { get => JsonPassiveNode.Dexterity; }
        public int Intelligence { get => JsonPassiveNode.Intelligence; }
        public double Arc => JsonPassiveNode.Arc;

        public string IconKey => $"{IconKeyPrefix}_{Icon}";
        private string IconKeyPrefix => PassiveNodeType switch
        {
            PassiveNodeType.Keystone => $"keystone",
            PassiveNodeType.Notable => $"notable",
            PassiveNodeType.Mastery => ActiveEffectIcon is null ? "mastery" : (IsSkilled ? "masterySelected" : (NeighborPassiveNodes.Count(x => x.Value.IsSkilled) > 0 ? "masteryConnected" : "mastery")),
            _ => $"normal"
        };
        public Dictionary<string, IReadOnlyList<float>> Attributes { get; } = new Dictionary<string, IReadOnlyList<float>>();
        public Dictionary<ushort, PassiveNodeViewModel> NeighborPassiveNodes { get; } = new Dictionary<ushort, PassiveNodeViewModel>();
        public Dictionary<ushort, PassiveNodeViewModel> VisibleNeighborPassiveNodes { get; } = new Dictionary<ushort, PassiveNodeViewModel>();

        public float ZoomLevel { get => JsonPassiveNode.ZoomLevel; }
        private Vector2D? _position = null;
        public Vector2D Position
        {
            get
            {
                if (_position?.X != JsonPassiveNode.Position.X || _position?.Y != JsonPassiveNode.Position.Y)
                {
                    _position = new Vector2D(JsonPassiveNode.Position.X, JsonPassiveNode.Position.Y);
                }
                return _position.Value;
            }
        }

        private bool? _isScionAscendancyNotable = null;
        public bool IsAscendantClassStartNode
        {
            get
            {
                if (!_isScionAscendancyNotable.HasValue)
                {
                    _isScionAscendancyNotable = false;
                    if (PassiveNodeType == PassiveNodeType.Notable)
                    {
                        /// <summary>
                        /// Nodes with an attribute matching this regex are one of the "Path of the ..." nodes connection Scion
                        /// Ascendant with other classes.
                        /// </summary>
                        var regexString = new Regex(@"Can Allocate Passives from the .* starting point");
                        foreach (var attibute in StatDescriptions)
                        {
                            if (regexString.IsMatch(attibute))
                            {
                                _isScionAscendancyNotable = true;
                                break;
                            }
                        }
                    }
                }

                return _isScionAscendancyNotable.Value;
            }
        }

        public void ClearPositionCache()
        {
            _position = null;
            JsonPassiveNode.ClearPositionCache();
        }

        private void InitializeAttributes()
        {
            if (PassiveNodeType == PassiveNodeType.JewelSocket || PassiveNodeType == PassiveNodeType.ExpansionJewelSocket)
            {
                StatDescriptions = new[] { "+1 Jewel Socket" };
            }

            var regexAttrib = new Regex("[0-9]*\\.?[0-9]+");
            foreach (string s in StatDescriptions)
            {
                var values = new List<float>();

                foreach (var m in regexAttrib.Matches(s).WhereNotNull())
                {
                    if (m.Value == "")
                        values.Add(float.NaN);
                    else
                        values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
                }
                string cs = (regexAttrib.Replace(s, "#"));
                Attributes[cs] = values;
            }
        }
    }
}
