using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.SkillTreeFiles;

namespace PoESkillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents an object that extracts build information from the tree url.
    /// </summary>
    public abstract class BuildUrlDeserializer
    {
        private readonly IAscendancyClasses _ascendancyClasses;
        protected string BuildUrl { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildUrlDeserializer"/> class.
        /// </summary>
        /// <param name="buildUrl">The PoE build url.</param>
        /// <param name="ascendancyClasses">The instance of the <see cref="ascendancyClasses"/>
        /// to access general information about skill tree.</param>
        protected BuildUrlDeserializer(string buildUrl, IAscendancyClasses ascendancyClasses)
        {
            BuildUrl = buildUrl ?? string.Empty;
            _ascendancyClasses = ascendancyClasses;
        }

        /// <summary>
        /// Returns the basic build data.
        /// </summary>
        public abstract BuildUrlData GetBuildData();

        /// <summary>
        /// Returns the id of a character class decoded from the tree url.
        /// </summary>
        protected abstract int GetCharacterClassId();

        /// <summary>
        /// Returns the id of an ascendancy class decoded from the tree url.
        /// </summary>
        public abstract int GetAscendancyClassId();

        /// <summary>
        /// Validates that the build url can be deserialized without exceptions.
        /// </summary>
        /// <param name="exception">The exception that was thrown on deserializing the build url. Null if true is
        /// returned.</param>
        /// <returns>True iff the build url can be deserialized without exceptions</returns>
        public abstract bool ValidateBuildUrl([NotNullWhen(false)] out Exception? exception);

        /// <summary>
        /// Returns the number of non-ascendancy points the given tree url uses.
        /// </summary>
        public int GetPointsCount()
            => GetBuildData().SkilledNodesIds.Count(
                id => SkillTree.Skillnodes.TryGetValue(id, out var skillNode)
                      && (!skillNode.IsRootNode && !skillNode.IsAscendancyNode));

        /// <summary>
        /// Returns the character class of the given build url.
        /// </summary>
        public CharacterClass GetCharacterClass()
            => (CharacterClass) GetCharacterClassId();

        /// <summary>
        /// Returns the ascendancy class of the given build url.
        /// Returns null if the tree has no ascendancy class selected.
        /// </summary>
        public string? GetAscendancyClass()
        {
            var ascendancyId = GetAscendancyClassId();

            // No Ascendancy class selected.
            if (ascendancyId == 0)
                return null;

            return _ascendancyClasses.GetAscendancyClassName(GetCharacterClass(), ascendancyId);
        }
    }
}