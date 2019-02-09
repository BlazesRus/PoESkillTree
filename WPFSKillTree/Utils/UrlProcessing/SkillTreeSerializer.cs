using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Serializes provided instance of the <see cref="SkillTree"/> class into url representation.
    /// </summary>
    public class SkillTreeSerializer
    {
        private static int HeaderSize { get; } = 7;
        private static int Version { get; } = 4;
        private static byte Fullscreen { get; } = 0;

        private readonly SkillTree _skillTree;
        private readonly BuildUrlData _buildUrlData;
        private readonly ICollection<ushort> _skillNodes;

        public SkillTreeSerializer(BuildUrlData buildUrlData, ICollection<ushort> skillNodes)
        {
            _buildUrlData = buildUrlData;
            _skillNodes = skillNodes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillTreeSerializer"/> class.
        /// </summary>
        /// <param name="skillTree">The skill tree.</param>
        public SkillTreeSerializer(SkillTree skillTree)
        {
            _skillTree = skillTree;
        }

        /// <summary>
        /// Serializes specified skill tree into the https://pathofexile.com url.
        /// </summary>
        public string ToUrl()
        {
            return _skillTree != null ? ToUrlFromTree() : ToUrlFromData();
        }

        private string ToUrlFromData()
        {
            // Ordering provides nice exact Url match, but is not strictly needed.
            // Performance impact is minimal even on tree with all 1.3K nodes allocated.
            var skillNodes = _buildUrlData.SkilledNodesIds;
            skillNodes.Sort();

            var bytes = new byte[HeaderSize + skillNodes.Count() * 2];
            bytes = GetCharacterBytes((byte)_buildUrlData.CharacterClass, (byte)_buildUrlData.AscendancyClassId, bytes);

            int unknownNodes = 0;
            int i = HeaderSize;
            foreach (var id in skillNodes)
            {
                if (_skillNodes.Contains(id))
                {
                    bytes[i++] = (byte)(id >> 8 & 0xFF);
                    bytes[i++] = (byte)(id & 0xFF);
                }
                else
                {
                    unknownNodes++;
                }
            }

            if (unknownNodes > 0)
            {
                var usedBytes = bytes.Length - unknownNodes * 2;
                byte[] result = new byte[usedBytes];
                Array.Copy(bytes, result, usedBytes);
                bytes = result;
            }

            return Constants.TreeAddress + Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-");
        }

        private string ToUrlFromTree()
        {
            // Ordering provides nice exact Url match, but is not strictly needed.
            // Performance impact is minimal even on tree with all 1.3K nodes allocated.
            var skillNodes = _skillTree.SkilledNodes
                .Where(node => !node.IsAscendancyStart && !node.IsRootNode)
                .OrderBy(node => node.Id)
                .ToList();

            var bytes = new byte[HeaderSize + skillNodes.Count * 2];
            bytes = GetCharacterBytes((byte)_skillTree.CharClass, (byte)_skillTree.AscType, bytes);

            int i = HeaderSize;
            foreach (var inn in skillNodes)
            {
                bytes[i++] = (byte)(inn.Id >> 8 & 0xFF);
                bytes[i++] = (byte)(inn.Id & 0xFF);
            }

            return Constants.TreeAddress + Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-");
        }

        private static byte[] GetCharacterBytes(byte characterClassId = 0, byte ascendancyClassId = 0, byte[] target = null)
        {
            target = target ?? new byte[7];

            target[0] = (byte)(Version >> 24 & 0xFF);
            target[1] = (byte)(Version >> 16 & 0xFF);
            target[2] = (byte)(Version >> 8 & 0xFF);
            target[3] = (byte)(Version >> 0 & 0xFF);
            target[4] = characterClassId;
            target[5] = ascendancyClassId;
            target[6] = Fullscreen;

            return target;
        }
    }
}