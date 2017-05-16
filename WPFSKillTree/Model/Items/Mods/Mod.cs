using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Mods
{
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class Mod : IMod
    {
        public string Id { get; }

        private readonly ISet<ItemClass> _itemClasses = new HashSet<ItemClass>();
        private readonly IList<Tuple<Tags, bool>> _spawnTags = new List<Tuple<Tags, bool>>();

        public JsonMod JsonMod { get; }

        public IReadOnlyList<IStat> Stats { get; }
        public string Name => JsonMod.Name;
        public ModDomain Domain => JsonMod.Domain;
        public int RequiredLevel => JsonMod.RequiredLevel;

        public Mod(string id, JsonMod jsonMod, IEnumerable<JsonCraftingBenchOption> jsonBenchOptions,
            IEnumerable<IReadOnlyDictionary<string, bool>> spawnTagsReplacement)
        {
            Id = id;
            foreach (var jsonMasterMod in jsonBenchOptions)
            {
                foreach (var itemClass in jsonMasterMod.ItemClasses)
                {
                    ItemClass enumClass;
                    if (ItemClassEx.TryParse(itemClass, out enumClass))
                    {
                        _itemClasses.Add(enumClass);
                    }
                }
            }
            var spawnTags = spawnTagsReplacement ?? jsonMod.SpawnTags;
            foreach (var spawnTagDict in spawnTags)
            {
                foreach (var spawnTagPair in spawnTagDict)
                {
                    Tags tag;
                    if (TagsEx.TryParse(spawnTagPair.Key, out tag))
                    {
                        _spawnTags.Add(Tuple.Create(tag, spawnTagPair.Value));
                    }
                }
            }
            JsonMod = jsonMod;
            Stats = jsonMod.Stats.Select(s => new Stat(s)).ToList();
        }

        public bool Matches(Tags tags, ItemClass itemClass)
        {
            // the ModDomains Item and Master match everything but Flask, Jewel and Gem
            if (tags.HasFlag(Tags.Flask))
            {
                if (Domain != ModDomain.Flask)
                {
                    return false;
                }
            }
            else if (tags.HasFlag(Tags.Jewel))
            {
                if (Domain != ModDomain.Jewel)
                {
                    return false;
                }
            }
            else if (tags.HasFlag(Tags.Gem))
            {
                return false;
            }
            else
            {
                if (Domain != ModDomain.Item && Domain != ModDomain.Master)
                {
                    return false;
                }
            }

            if (_itemClasses.Contains(itemClass))
            {
                return true;
            }
            return (
                from spawnTag in _spawnTags
                where tags.HasFlag(spawnTag.Item1)
                select spawnTag.Item2
            ).FirstOrDefault();
        }
    }
}