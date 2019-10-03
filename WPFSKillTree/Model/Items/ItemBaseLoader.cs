﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;

namespace PoESkillTree.Model.Items
{
    public class ItemBaseLoader
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private static readonly ISet<ItemClass> ItemClassWhitelist = new HashSet<ItemClass>
        {
            ItemClass.OneHandSword,
            ItemClass.ThrustingOneHandSword,
            ItemClass.OneHandAxe,
            ItemClass.OneHandMace,
            ItemClass.Sceptre,
            ItemClass.Dagger,
            ItemClass.RuneDagger,
            ItemClass.Claw,
            ItemClass.Wand,

            ItemClass.TwoHandSword,
            ItemClass.TwoHandAxe,
            ItemClass.TwoHandMace,
            ItemClass.Bow,
            ItemClass.Staff,
            ItemClass.Warstaff,
            ItemClass.FishingRod,

            ItemClass.Belt,
            ItemClass.Ring,
            ItemClass.Amulet,
            ItemClass.Quiver,

            ItemClass.Shield,
            ItemClass.Boots,
            ItemClass.BodyArmour,
            ItemClass.Gloves,
            ItemClass.Helmet,

            ItemClass.LifeFlask,
            ItemClass.ManaFlask,
            ItemClass.HybridFlask,
            ItemClass.UtilityFlask,
            ItemClass.UtilityFlaskCritical,

            ItemClass.Jewel,
            ItemClass.AbyssJewel,
        };

        private readonly HashSet<string> _unknownTags = new HashSet<string>();

        public static async Task<IEnumerable<ItemBaseDto>> LoadAsync()
        {
            var json = await DataUtils.LoadRePoEAsObjectAsync("base_items", true);
            var loader = new ItemBaseLoader();
            var xmlBases = loader.CreateXmlItemBases(json).ToList();
            Log.Info("Unknown tags: " + string.Join(", ", loader._unknownTags));
            return xmlBases;
        }

        private IEnumerable<ItemBaseDto> CreateXmlItemBases(JObject json)
        {
            return
                from property in json.Properties()
                let itemBaseJson = property.Value
                where HasValidItemClass(itemBaseJson)
                      && HasValidReleaseState(itemBaseJson)
                let metadataId = property.Name
                let itemBase = CreateXmlItemBaseFromJson(metadataId, itemBaseJson)
                orderby itemBase.MetadataId
                select itemBase;
        }

        private static bool HasValidItemClass(JToken itemBaseJson)
        {
            return ItemClassEx.TryParse(itemBaseJson.Value<string>("item_class"), out var itemClass)
                   && ItemClassWhitelist.Contains(itemClass);
        }

        private static bool HasValidReleaseState(JToken itemBaseJson)
        {
            return itemBaseJson.Value<string>("release_state") != "unreleased";
        }

        private ItemBaseDto CreateXmlItemBaseFromJson(string metadataId, JToken obj)
        {
            var converter = new ItemBaseJsonToXmlConverter(metadataId, obj);
            var itemBase = converter.Parse();
            _unknownTags.UnionWith(converter.UnknownTags);
            return itemBase;
        }
    }

    internal class ItemBaseJsonToXmlConverter
    {
        private readonly string _metadataId;
        private readonly JToken _json;

        private ItemBaseDto _xml;

        public IReadOnlyCollection<string> UnknownTags { get; private set; }

        public ItemBaseJsonToXmlConverter(string metadataId, JToken json)
        {
            _metadataId = metadataId;
            _json = json;
        }

        public ItemBaseDto Parse()
        {
            _xml = new ItemBaseDto();
            ParseSimpleFields();
            ParseItemClass();
            ParseImplicits();
            ParseRequirements();
            ParseTags();
            ParseProperties();
            return _xml;
        }

        private void ParseSimpleFields()
        {
            _xml.Name = _json.Value<string>("name");
            _xml.DropDisabled = _json.Value<string>("release_state") != "released";
            _xml.InventoryHeight = _json.Value<int>("inventory_height");
            _xml.InventoryWidth = _json.Value<int>("inventory_width");
            _xml.MetadataId = _metadataId;
        }

        private void ParseItemClass()
        {
            ItemClassEx.TryParse(_json.Value<string>("item_class"), out var itemClass);
            _xml.ItemClass = itemClass;
        }

        private void ParseImplicits()
        {
            _xml.Implicit = _json["implicits"].Values<string>().ToArray();
        }

        private void ParseRequirements()
        {
            var requirements = _json["requirements"];
            if (requirements.HasValues)
            {
                _xml.Dexterity = requirements.Value<int>("dexterity");
                _xml.Strength = requirements.Value<int>("strength");
                _xml.Intelligence = requirements.Value<int>("intelligence");
                _xml.Level = requirements.Value<int>("level");
            }
            else
            {
                _xml.Level = 1;
            }
        }

        private void ParseTags()
        {
            var unknownTags = new HashSet<string>();
            foreach (var s in _json["tags"].Values<string>())
            {
                if (TagsExtensions.TryParse(s, out var tag))
                {
                    _xml.Tags |= tag;
                }
                else
                {
                    unknownTags.Add(s);
                }
            }

            UnknownTags = unknownTags.ToList();
        }

        private void ParseProperties()
        {
            var properties = _json["properties"];
            if (_xml.Tags.HasFlag(Tags.Weapon))
            {
                _xml.Properties = FormatToArray(ParseWeaponProperties(properties));
            }
            else if (_xml.Tags.HasFlag(Tags.Armour))
            {
                _xml.Properties = FormatToArray(ParseArmourProperties(properties));
            }
            else if (_xml.Tags.HasFlag(Tags.Flask))
            {
                _xml.Properties = FormatToArray(ParseFlaskProperties(properties));
            }
            else
            {
                _xml.Properties = new string[0];
            }
        }

        private static string[] FormatToArray(IEnumerable<FormattableString> properties)
        {
            return properties.Select(FormattableString.Invariant).ToArray();
        }

        private static IEnumerable<FormattableString> ParseWeaponProperties(JToken properties)
        {
            yield return
                $"Physical Damage: {properties.Value<string>("physical_damage_min")}-{properties.Value<string>("physical_damage_max")}";
            yield return
                $"Critical Strike Chance: {properties.Value<int>("critical_strike_chance") / 100.0:##.##}%";
            yield return
                $"Attacks per Second: {1000.0 / properties.Value<int>("attack_time"):##.##}";
            yield return
                $"Weapon Range: {properties.Value<int>("range")}";
        }

        private static IEnumerable<FormattableString> ParseArmourProperties(JToken properties)
        {
            if (properties["block"]?.Value<int>() is int block)
                yield return $"Chance to Block: {block}%";

            if (properties["armour"]?.Value<int>() is int armour)
                yield return $"Armour: {armour}";

            if (properties["evasion"]?.Value<int>() is int evasion)
                yield return $"Evasion Rating: {evasion}";

            if (properties["energy_shield"]?.Value<int>() is int energyShield)
                yield return $"Energy Shield: {energyShield}";

            if (properties["movement_speed"]?.Value<int>() is int movementSpeed)
                yield return $"Movement Speed: {movementSpeed}%";
        }

        private static IEnumerable<FormattableString> ParseFlaskProperties(JToken properties)
        {
            var duration = properties.Value<double>("duration");
            yield return
                $"Consumes {properties.Value<int>("charges_per_use")} of {properties.Value<int>("charges_max")} Charges on use";

            var lifePerUse = properties["life_per_use"]?.Value<int>();
            if (lifePerUse.HasValue)
                yield return $"Recovers {lifePerUse} Life over {duration} seconds";
            var manaPerUse = properties["mana_per_use"]?.Value<int>();
            if (manaPerUse.HasValue)
                yield return $"Recovers {manaPerUse} Mana over {duration} seconds";
            if (!lifePerUse.HasValue && !manaPerUse.HasValue)
                yield return $"Lasts {duration} seconds";
        }
    }
}