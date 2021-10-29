using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MoreLinq;
using NLog;
using PoESkillTree.Utils;
using PoESkillTree.Controls;
using PoESkillTree.Model.Builds;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Model.Serialization
{
    /// <summary>
    /// Deserializes PersistentData using the old build saving structure and created without the versioning refactoring.
    /// Latest applicable release is 2.3.0. All commits after the release and before the versioning refactoring have
    /// used 2.3.0 as AppVersion.
    /// </summary>
    public class PersistentDataDeserializerUpTo230 : AbstractPersistentDataDeserializer
    {
        /// <summary>
        /// XML format used in these versions. Public as it's used for deserialization.
        /// </summary>
        [XmlRoot("PersistentData")]
        public class XmlPersistentData
        {
            [XmlElement]
            public Options? Options { get; set; }

            [XmlElement]
            public XmlBuild? CurrentBuild { get; set; }

            [XmlArray]
            public List<StashBookmark>? StashBookmarks { get; set; }

            [XmlArray]
            [XmlArrayItem("PoEBuild")]
            public List<XmlBuild>? Builds { get; set; }
        }

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public PersistentDataDeserializerUpTo230()
            : base(null, "2.3.0")
        {
        }

        public override void DeserializePersistentDataFile(string xmlString)
        {
            var obj = XmlSerializationUtils.DeserializeString<XmlPersistentData>(xmlString);
            PersistentData.Options = obj.Options ?? new Options();
            PersistentData.CurrentBuild = ConvertFromXmlBuild(obj.CurrentBuild) ?? CreateDefaultCurrentBuild();
            obj.StashBookmarks?.ForEach(PersistentData.StashBookmarks.Add);
            if (obj.Builds != null)
            {
                PersistentData.RootBuild.Builds.AddRange(obj.Builds.Select(b => ConvertFromXmlBuild(b)));
            }

            if (!SelectCurrentBuildByName(PersistentData.CurrentBuild.Name))
            {
                PersistentData.RootBuild.Builds.Add(PersistentData.CurrentBuild);
            }
            RenameBuilds();
        }

        protected override string GetLongestRequiredSubpath()
        {
            return PersistentData.RootBuild.Builds
                .Select(b => SerializationUtils.EncodeFileName(b.Name) + SerializationConstants.BuildFileExtension)
                .Aggregate((s1, s2) => s1.Length > s2.Length ? s1 : s2);
        }

        protected override Task DeserializeAdditionalFilesAsync()
        {
            return ImportLegacySavedBuildsAsync();
        }

        public override void SaveBuildChanges()
        {
            PersistentData.SaveBuild(PersistentData.RootBuild);
            PersistentData.RootBuild.Builds.ForEach(PersistentData.SaveBuild);
        }

        private bool SelectCurrentBuildByName(string name)
        {
            var buildNameMatch =
                (from PoEBuild build in PersistentData.RootBuild.BuildsPreorder()
                 where build.Name == name
                 select build).FirstOrDefault();
            if (buildNameMatch == null)
                return false;
            PersistentData.CurrentBuild = buildNameMatch;
            return true;
        }

        private void RenameBuilds()
        {
            var builds = PersistentData.RootBuild.Builds;
            // Rename empty builds
            foreach (var build in builds)
            {
                if (string.IsNullOrEmpty(build.Name))
                    build.Name = " ";
            }
            // Rename duplicates
            var names = new Dictionary<string, IBuild>();
            foreach (var build in builds)
            {
                if (!names.ContainsKey(build.Name))
                {
                    names[build.Name] = build;
                    continue;
                }
                build.Name = Util.FindDistinctName(build.Name, names.Keys);
                names[build.Name] = build;
            }
        }

        /// <summary>
        /// Import builds from legacy build save file "savedBuilds" to PersistentData.xml.
        /// Warning: This will remove the "savedBuilds"
        /// </summary>
        private async Task ImportLegacySavedBuildsAsync()
        {
            if (!File.Exists("savedBuilds"))
                return;
            try
            {
                var text = await FileUtils.ReadAllTextAsync("savedBuilds");
                foreach (var b in text.Split('\n'))
                {
                    var build = new PoEBuild
                    {
                        Name = b.Split(';')[0].Split('|')[0]
                    };
                    if (HasBuildNote(b))
                    {
                        build.TreeUrl = b.Split(';')[1].Split('|')[0];
                        build.Note = b.Split(';')[1].Split('|')[1];
                    }
                    else
                    {
                        build.TreeUrl = b.Split(';')[1];
                    }
                    PersistentData.RootBuild.Builds.Add(build);
                }
                File.Move("savedBuilds", "savedBuilds.old");
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not load legacy savedBuilds file");
            }
        }

        private static bool HasBuildNote(string b)
        {
            var buildNoteTest = b.Split(';')[1].Split('|');
            return buildNoteTest.Length > 1;
        }
    }
}