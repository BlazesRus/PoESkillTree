﻿using Newtonsoft.Json;
using POESKillTree.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using log4net;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.Utils.Extensions;
using POESKillTree.Common;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Utils.UrlProcessing;
using HighlightState = POESKillTree.SkillTreeFiles.NodeHighlighter.HighlightState;
using static POESKillTree.SkillTreeFiles.Constants;

namespace POESKillTree.SkillTreeFiles
{
    public partial class SkillTree : Notifier, ISkillTree
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SkillTree));

        public Vector2D AscButtonPosition = new Vector2D();
        /// <summary>
        /// Nodes with an attribute matching this regex are one of the "Path of the ..." nodes connection Scion
        /// Ascendant with other classes.
        /// </summary>
        private static readonly Regex AscendantClassStartRegex = new Regex(@"Can Allocate Passives from the .* starting point");

        // The absolute path of Assets folder (contains trailing directory separator).
        private static string _assetsFolderPath;

        public static readonly IReadOnlyList<(string stat, float value)> BaseAttributes = new (string, float)[]
        {
            ("+# to maximum Mana", 34),
            ("+# to maximum Life", 38),
            ("Evasion Rating: #", 53),
            ("+# to Maximum Endurance Charges", 3),
            ("+# to Maximum Frenzy Charges", 3),
            ("+# to Maximum Power Charges", 3),
            ("#% Additional Elemental Resistance per Endurance Charge", 4),
            ("#% Physical Damage Reduction per Endurance Charge", 4),
            ("#% Attack Speed Increase per Frenzy Charge", 4),
            ("#% Cast Speed Increase per Frenzy Charge", 4),
            ("#% More Damage per Frenzy Charge", 4),
            ("#% Critical Strike Chance Increase per Power Charge", 40),
        };

        private static readonly Dictionary<string, List<string>> HybridAttributes = new Dictionary<string, List<string>>
        {
            {
               "+# to Strength and Intelligence",
               new List<string> {"+# to Strength", "+# to Intelligence"}
            },
            {
                "+# to Strength and Dexterity",
                new List<string> {"+# to Strength", "+# to Dexterity"}
            },
            {
                "+# to Dexterity and Intelligence",
                new List<string> {"+# to Dexterity", "+# to Intelligence"}
            }
        };

        public static readonly Dictionary<string, string> RenameImplicitAttributes = new Dictionary<string, string>
        {
            {
                "#% increased Evasion Rating",
                "#% increased Evasion Rating from Dexterity"
            }, {
                "#% increased maximum Energy Shield",
                "#% increased maximum Energy Shield from Intelligence"
            }, {
                "#% increased Melee Physical Damage",
                "#% increased Melee Physical Damage from Strength"
            }
        };

        private static readonly IReadOnlyDictionary<string, CharacterClass> PassiveNodeNameToClass =
            new Dictionary<string, CharacterClass>
            {
                { "SEVEN", CharacterClass.Scion },
                { "MARAUDER", CharacterClass.Marauder },
                { "RANGER", CharacterClass.Ranger },
                { "WITCH", CharacterClass.Witch },
                { "DUELIST", CharacterClass.Duelist },
                { "TEMPLAR", CharacterClass.Templar },
                { "SIX", CharacterClass.Shadow }
            };

        private static readonly List<string> CharacterFaceNames = new List<string>
        {
            "centerscion",
            "centermarauder",
            "centerranger",
            "centerwitch",
            "centerduelist",
            "centertemplar",
            "centershadow"
        };

        private static readonly Dictionary<string, string> NodeBackgrounds = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrame"},
            {"notable", "NotableFrameUnallocated"},
            {"keystone", "KeystoneFrameUnallocated"},
            {"jewel", "JewelFrameUnallocated"},
            {"ascendancyNormal", "PassiveSkillScreenAscendancyFrameSmallNormal"},
            {"ascendancyNotable", "PassiveSkillScreenAscendancyFrameLargeNormal"}
        };

        private static readonly Dictionary<string, string> NodeBackgroundsActive = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrameActive"},
            {"notable", "NotableFrameAllocated"},
            {"keystone", "KeystoneFrameAllocated"},
            {"jewel", "JewelFrameAllocated"},
            {"ascendancyNormal", "PassiveSkillScreenAscendancyFrameSmallAllocated"},
            {"ascendancyNotable", "PassiveSkillScreenAscendancyFrameLargeAllocated"}
        };

        private static SkillIcons IconActiveSkills { get; set; }
        private static SkillIcons IconInActiveSkills { get; set; }
        public static Dictionary<ushort, SkillNode> Skillnodes { get; private set; }

        private static IEnumerable<string> _allAttributes;
        /// <summary>
        /// Gets an Array of all the attributes of SkillNodes.
        /// </summary>
        public static IEnumerable<string> AllAttributes
        {
            get { return _allAttributes ?? (_allAttributes = Skillnodes.Values.SelectMany(n => n.Attributes.Keys).Distinct().ToArray()); }
        }

        public static Dictionary<CharacterClass, IReadOnlyList<(string stat, float value)>> CharBaseAttributes
        {
            get;
            private set;
        }

        public static List<ushort> RootNodeList { get; private set; }
        private static HashSet<SkillNode> AscRootNodeList { get; set; }
        public static List<SkillNodeGroup> NodeGroups { get; private set; }
        public static Rect2D SkillTreeRect { get; private set; }
        private static Dictionary<CharacterClass, ushort> RootNodeClassDictionary { get; set; }
        private static Dictionary<ushort, ushort> StartNodeDictionary { get; set; }

        private static Dictionary<string, BitmapImage> Assets { get; } = new Dictionary<string, BitmapImage>();

        private static readonly List<ushort[]> Links = new List<ushort[]>();
        public readonly ObservableSet<SkillNode> SkilledNodes = new ObservableSet<SkillNode>();
        public readonly ObservableSet<SkillNode> HighlightedNodes = new ObservableSet<SkillNode>();
        public SkillTreeSerializer Serializer { get; }
        public IAscendancyClasses AscendancyClasses { get; private set; }
        public IBuildConverter BuildConverter { get; private set; }

        private CharacterClass _charClass;
        private int _asctype;
        public static int UndefinedLevel => 0;
        public static int MaximumLevel => 100;
        private int _level = UndefinedLevel;

        private static bool _initialized;

        private SkillTree(IPersistentData persistentData)
        {
            _persistentData = persistentData;

            Serializer = new SkillTreeSerializer(this);
        }

        private async Task InitializeAsync(string treestring, string opsstring, [CanBeNull] ProgressDialogController controller,
            AssetLoader assetLoader)
        {
            if (!_initialized)
            {
                var jss = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        // There are many errors in "oo" elements and we can't fix them anyway
                        if (args.ErrorContext.Path == null || !args.ErrorContext.Path.EndsWith(".oo"))
                            Log.Error("Exception while deserializing Json tree", args.ErrorContext.Error);
                        args.ErrorContext.Handled = true;
                    }
                };
                treestring = treestring.Replace("\"nodes\":{", "\"nodesDict\":{");
                var inTree = JsonConvert.DeserializeObject<PoESkillTree>(treestring, jss);
                var inOpts = JsonConvert.DeserializeObject<Opts>(opsstring, jss);

                controller?.SetProgress(0.25);
                await assetLoader.DownloadSkillNodeSpritesAsync(inTree, d => controller?.SetProgress(0.25 + d * 0.30));
                IconInActiveSkills = new SkillIcons();
                IconActiveSkills = new SkillIcons();
                foreach (var obj in inTree.skillSprites)
                {
                    SkillIcons icons;
                    string prefix;
                    foreach(var i in obj.Value)
                    {
                        if (i.filename.Contains('?'))
                            i.filename = i.filename.Remove(i.filename.IndexOf('?'));
                    }
                    if (obj.Key.EndsWith("Active"))
                    {
                        // Adds active nodes to IconActiveSkills
                        icons = IconActiveSkills;
                        prefix = obj.Key.Substring(0, obj.Key.Length - "Active".Length);
                    }
                    else if (obj.Key.EndsWith("Inactive"))
                    {
                        // Adds inactive nodes to IconInActiveSkills
                        icons = IconInActiveSkills;
                        prefix = obj.Key.Substring(0, obj.Key.Length - "Inactive".Length);
                    }
                    else
                    {
                        // Adds masteries to IconInActiveSkills
                        icons = IconInActiveSkills;
                        prefix = obj.Key;
                    }
                    var sprite = obj.Value[AssetZoomLevel];
                    var path = _assetsFolderPath + sprite.filename;
                    icons.Images[sprite.filename] = ImageHelper.OnLoadBitmapImage(new Uri(path, UriKind.Absolute));
                    foreach (var o in sprite.coords)
                    {
                        var iconKey = prefix + "_" + o.Key;
                        icons.SkillPositions[iconKey] = new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h);
                        icons.SkillImages[iconKey] = sprite.filename;
                    }
                }

                controller?.SetProgress(0.55);
                // The last percent progress is reserved for rounding errors as progress must not get > 1.
                await assetLoader.DownloadAssetsAsync(inTree, d => controller?.SetProgress(0.55 + d * 0.44));
                foreach (var ass in inTree.assets)
                {
                    var path = _assetsFolderPath + ass.Key + ".png";
                    Assets[ass.Key] = ImageHelper.OnLoadBitmapImage(new Uri(path, UriKind.Absolute));
                }

                RootNodeList = new List<ushort>();
                if (inTree.root != null)
                {
                    foreach (var i in inTree.root.ot)
                    {
                        RootNodeList.Add(i);
                    }
                }
                else if (inTree.main != null)
                {
                    foreach (var i in inTree.main.ot)
                    {
                        RootNodeList.Add(i);
                    }
                }

                AscendancyClasses = new AscendancyClasses(inOpts.ascClasses);

                BuildConverter = new BuildConverter(AscendancyClasses);
                BuildConverter.RegisterDefaultDeserializer(url => new NaivePoEUrlDeserializer(url, AscendancyClasses));
                BuildConverter.RegisterDeserializersFactories(
                    PoeplannerUrlDeserializer.TryCreate,
                    PathofexileUrlDeserializer.TryCreate
                );

                CharBaseAttributes = new Dictionary<CharacterClass, IReadOnlyList<(string stat, float value)>>();
                foreach (var (key, value) in inTree.characterData)
                {
                    CharBaseAttributes[(CharacterClass) key] = new (string stat, float value)[]
                    {
                        ("+# to Strength", value.base_str),
                        ("+# to Dexterity", value.base_dex),
                        ("+# to Intelligence", value.base_int)
                    };
                }

                Skillnodes = new Dictionary<ushort, SkillNode>();
                RootNodeClassDictionary = new Dictionary<CharacterClass, ushort>();
                StartNodeDictionary = new Dictionary<ushort, ushort>();
                AscRootNodeList = new HashSet<SkillNode>();

                GlobalSettings.JewelStorage = new JewelData();

                if (inTree.nodes != null && inTree.nodes.Any())
                    BuildNodeList(inTree.nodes);
                else if (inTree.nodesDict != null && inTree.nodesDict.Any())
                    BuildNodeList(inTree.nodesDict.Values.ToArray());

                void BuildNodeList(Node[] nodes)
                {
                    foreach (var nd in nodes)
                    {
                        var skillNode = new SkillNode
                        {
                            Id = nd.id,
                            Name = nd.dn,
                            //this value should not be split on '\n' as it causes the attribute list to separate nodes
                            attributes = nd.dn.Contains("Jewel Socket") ? new[] { "+1 Jewel Socket" } : nd.sd,
                            Orbit = nd.o,
                            OrbitIndex = nd.oidx,
                            Icon = nd.icon,
                            LinkId = nd._out,
                            G = nd.g,
                            Da = nd.da,
                            Ia = nd.ia,
                            Sa = nd.sa,
                            Spc = nd.spc.Length > 0 ? (int?)nd.spc[0] : null,
                            IsMultipleChoice = nd.isMultipleChoice,
                            IsMultipleChoiceOption = nd.isMultipleChoiceOption,
                            passivePointsGranted = nd.passivePointsGranted,
                            ascendancyName = nd.ascendancyName,
                            IsAscendancyStart = nd.isAscendancyStart,
                            reminderText = nd.reminderText
                        };
                        if (nd.ks && !nd.not && !nd.isJewelSocket && !nd.m)
                        {
                            skillNode.Type = PassiveNodeType.Keystone;
                        }
                        else if (!nd.ks && nd.not && !nd.isJewelSocket && !nd.m)
                        {
                            skillNode.Type = PassiveNodeType.Notable;
                        }
                        else if (!nd.ks && !nd.not && nd.isJewelSocket && !nd.m)
                        {
                            skillNode.Type = PassiveNodeType.JewelSocket;
                            //Sending Node Id into List to dynamically add threshold stat
                            GlobalSettings.JewelInfo.AddJewelSlot(skillNode.Id);
                        }
                        else if (!nd.ks && !nd.not && !nd.isJewelSocket && nd.m)
                        {
                            skillNode.Type = PassiveNodeType.Mastery;
                        }
                        else if (!nd.ks && !nd.not && !nd.isJewelSocket && !nd.m)
                        {
                            skillNode.Type = PassiveNodeType.Normal;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Invalid node type for node {skillNode.Name}");
                        }
                        Skillnodes.Add(nd.id, skillNode);
                        if (skillNode.IsAscendancyStart)
                            if (!AscRootNodeList.Contains(skillNode))
                                AscRootNodeList.Add(skillNode);
                        if (RootNodeList.Contains(nd.id))
                        {
                            skillNode.IsRootNode = true;
                            var characterClass = PassiveNodeNameToClass[nd.dn.ToUpperInvariant()];
                            if (!RootNodeClassDictionary.ContainsKey(characterClass))
                            {
                                RootNodeClassDictionary.Add(characterClass, nd.id);
                            }
                            foreach (var linkedNode in nd._out)
                            {
                                if (!StartNodeDictionary.ContainsKey(nd.id) && !nd.isAscendancyStart)
                                {
                                    StartNodeDictionary.Add(linkedNode, nd.id);
                                }
                            }
                        }
                        foreach (var node in nd._out)
                        {
                            if (!StartNodeDictionary.ContainsKey(nd.id) && RootNodeList.Contains(node))
                            {
                                StartNodeDictionary.Add(nd.id, node);
                            }
                        }
                    }
                }

                foreach (var skillNode in Skillnodes)
                {
                    foreach (var i in skillNode.Value.LinkId)
                    {
                        if (Links.Count(nd => (nd[0] == i && nd[1] == skillNode.Key) || nd[0] == skillNode.Key && nd[1] == i) != 1)
                            Links.Add(new[] { skillNode.Key, i });
                    }
                }
                foreach (var ints in Links)
                {
                    Regex regexString = new Regex(@"Can Allocate Passives from the .* starting point");
                    bool isScionAscendancyNotable = false;
                    foreach (var attibute in Skillnodes[ints[0]].attributes)
                    {
                        if (regexString.IsMatch(attibute))
                            isScionAscendancyNotable = true;
                    }
                    foreach (var attibute in Skillnodes[ints[1]].attributes)
                    {
                        if (regexString.IsMatch(attibute))
                            isScionAscendancyNotable = true;
                    }

                    if (isScionAscendancyNotable && StartNodeDictionary.Keys.Contains(ints[0]))
                    {
                        if (!Skillnodes[ints[1]].Neighbor.Contains(Skillnodes[ints[0]]))
                            Skillnodes[ints[1]].Neighbor.Add(Skillnodes[ints[0]]);
                    }
                    else if (isScionAscendancyNotable && StartNodeDictionary.Keys.Contains(ints[1]))
                    {
                        if (!Skillnodes[ints[0]].Neighbor.Contains(Skillnodes[ints[1]]))
                            Skillnodes[ints[0]].Neighbor.Add(Skillnodes[ints[1]]);
                    }
                    else
                    {
                        if (!Skillnodes[ints[0]].Neighbor.Contains(Skillnodes[ints[1]]))
                            Skillnodes[ints[0]].Neighbor.Add(Skillnodes[ints[1]]);
                        if (!Skillnodes[ints[1]].Neighbor.Contains(Skillnodes[ints[0]]))
                            Skillnodes[ints[1]].Neighbor.Add(Skillnodes[ints[0]]);
                    }
                }

                var regexAttrib = new Regex("[0-9]*\\.?[0-9]+");
                foreach (var skillnode in Skillnodes)
                {
                    //add each other as visible neighbors
                    foreach (var snn in skillnode.Value.Neighbor)
                    {
                        if (snn.IsAscendancyStart && skillnode.Value.LinkId.Contains(snn.Id))
                            continue;
                        skillnode.Value.VisibleNeighbors.Add(snn);
                    }

                    //populate the Attributes fields with parsed attributes
                    skillnode.Value.Attributes = new Dictionary<string, IReadOnlyList<float>>();
                    foreach (string s in skillnode.Value.attributes)
                    {
                        var values = new List<float>();

                        foreach (Match m in regexAttrib.Matches(s))
                        {
                            if (m.Value == "")
                                values.Add(float.NaN);
                            else
                                values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
                        }
                        string cs = (regexAttrib.Replace(s, "#"));

                        skillnode.Value.Attributes[cs] = values;
                    }
                }

                NodeGroups = new List<SkillNodeGroup>();
                foreach (var gp in inTree.groups)
                {
                    var ng = new SkillNodeGroup();

                    ng.OcpOrb = gp.Value.oo;
                    ng.Position = new Vector2D(gp.Value.x, gp.Value.y);
                    foreach (var node in gp.Value.n)
                    {
                        ng.Nodes.Add(Skillnodes[node]);
                    }
                    NodeGroups.Add(ng);
                }
                foreach (SkillNodeGroup group in NodeGroups)
                {
                    foreach (SkillNode node in group.Nodes)
                    {
                        node.SkillNodeGroup = group;
                    }
                }

                const int padding = 500; //This is to account for jewel range circles. Might need to find a better way to do it.
                SkillTreeRect = new Rect2D(new Vector2D(inTree.min_x * 1.1 - padding, inTree.min_y * 1.1 - padding),
                    new Vector2D(inTree.max_x * 1.1 + padding, inTree.max_y * 1.1 + padding));
            }

            if (_persistentData.Options.ShowAllAscendancyClasses)
                DrawAscendancy = true;

            InitialSkillTreeDrawing();
            controller?.SetProgress(1);

            _initialized = true;
        }

        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        /// <summary>
        /// This will get all skill points related to the tree both Normal and Ascendancy
        /// </summary>
        /// <returns>A Dictionary with keys of "NormalUsed", "NormalTotal", "AscendancyUsed", "AscendancyTotal", and "ScionAscendancyChoices"</returns>
        public Dictionary<string, int> GetPointCount()
        {
            Dictionary<string, int> points = new Dictionary<string, int>()
            {
                {"NormalUsed", 0},
                {"NormalTotal", 22},
                {"AscendancyUsed", 0},
                {"AscendancyTotal", 8},
            };

            var bandits = _persistentData.CurrentBuild.Bandits;
            points["NormalTotal"] += Level - 1;
            if (bandits.Choice == Bandit.None)
                points["NormalTotal"] += 2;

            foreach (var node in SkilledNodes)
            {
                if (node.ascendancyName == null && !node.IsRootNode)
                    points["NormalUsed"] += 1;
                else if (node.ascendancyName != null && !node.IsAscendancyStart && !node.IsMultipleChoiceOption)
                {
                    points["AscendancyUsed"] += 1;
                    points["NormalTotal"] += node.passivePointsGranted;
                }
            }
            return points;
        }

        public bool UpdateAscendancyClasses = true;

        public CharacterClass CharClass
        {
            get => _charClass;
            private set => SetProperty(ref _charClass, value);
        }

        public int AscType
        {
            get => _asctype;
            set
            {
                if (value < 0 || value > 3) return;
                ChangeAscClass(value);
            }
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument",
            Justification = "Would notify changes for the not existing property 'ChangeAscClass'")]
        private void ChangeAscClass(int toType)
        {
            var remove = new List<SkillNode>();
            var add = new HashSet<SkillNode>();
            var changedType = _asctype != toType;
            if (toType == 0)
            {
                remove = SkilledNodes.Where(n => n.ascendancyName != null).ToList();
                if (!_persistentData.Options.ShowAllAscendancyClasses)
                    DrawAscendancy = false;
                SetProperty(ref _asctype, toType, propertyName: nameof(AscType));
            }
            else
            {
                SetProperty(ref _asctype, toType, propertyName: nameof(AscType));
                var sn = GetAscNode();
                if (sn != null)
                {
                    add.Add(sn);
                    foreach (var n in SkilledNodes)
                    {
                        if (sn.ascendancyName != n.ascendancyName && n.ascendancyName != null)
                            remove.Add(n);
                        add.Add(n);
                    }
                }
            }

            add.ExceptWith(SkilledNodes);
            add.ExceptWith(remove);

            SkilledNodes.ExceptWith(remove);
            SkilledNodes.UnionWith(add);
            if (changedType)
                DrawAscendancyLayers();
        }

        public void SwitchClass(CharacterClass charClass)
        {
            if (charClass == CharClass) return;
            if(!CanSwitchClass(charClass))
                SkilledNodes.Clear();
            CharClass = charClass;
            var remove = SkilledNodes.Where(n => n.ascendancyName != null || n.IsRootNode).ToList();
            var add = Skillnodes[RootNodeClassDictionary[charClass]];
            SkilledNodes.ExceptWith(remove);
            SkilledNodes.Add(add);
            _asctype = 0;
        }

        public string AscendancyClassName
            => AscendancyClasses.GetAscendancyClassName(CharClass, AscType);

        public Dictionary<string, List<float>> HighlightedAttributes;

        public Dictionary<string, List<float>> SelectedAttributes
            => GetAttributes(SkilledNodes, CharClass, Level, _persistentData.CurrentBuild.Bandits);

        public static Dictionary<string, List<float>> GetAttributes(
            IEnumerable<SkillNode> skilledNodes, CharacterClass charClass, int level, BanditSettings banditSettings)
        {
            var temp = GetAttributesWithoutImplicit(skilledNodes, charClass, banditSettings);

            foreach (var a in ImplicitAttributes(temp, level))
            {
                var key = RenameImplicitAttributes.ContainsKey(a.Key) ? RenameImplicitAttributes[a.Key] : a.Key;

                if (!temp.ContainsKey(key))
                    temp[key] = new List<float>();
                for (var i = 0; i < a.Value.Count; i++)
                {
                    if (temp.ContainsKey(key) && temp[key].Count > i)
                        temp[key][i] += a.Value[i];
                    else
                    {
                        temp[key].Add(a.Value[i]);
                    }
                }
            }
            float Subtotal = 0.0f;
            float TotalIncrease = 0.0f;
            if (temp.ContainsKey("+# Accuracy Rating"))
            {
                Subtotal += temp["+# Accuracy Rating"][0];
            }
            if (temp.ContainsKey("+# to Accuracy Rating"))
            {
                Subtotal += temp["+# to Accuracy Rating"][0];
            }
            if (temp.ContainsKey("+# increased Accuracy Rating with Wands"))
            {
                TotalIncrease += temp["+# increased Accuracy Rating with Wands"][0];
            }
            if (temp.ContainsKey("+# increased Accuracy Rating while Dual Wielding"))
            {
                TotalIncrease += temp["+# increased Accuracy Rating while Dual Wielding"][0];
            }
            if (temp.ContainsKey("+# increased Global Accuracy Rating"))
            {
                TotalIncrease += temp["+# increased Global Accuracy Rating"][0];
            }
            if (TotalIncrease != 0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease) / 100;
                Subtotal *= TotalIncrease;
            }
            if (temp.ContainsKey("# DualWand Accuracy Subtotal"))//"# Accuracy Subtotal"
            {
                temp["# DualWand Accuracy Subtotal"][0] = Subtotal;
            }
            else
            {
                temp.Add("# DualWand Accuracy Subtotal", new List<float>(1) { Subtotal });
            }
            //MaxLife combined with increased life
            Subtotal = 0.0f;
            TotalIncrease = 0.0f;
            if (temp.ContainsKey("+# to maximum Life"))
            {
                Subtotal = temp["+# to maximum Life"][0];
            }
            if (temp.ContainsKey("#% increased maximum Life"))
            {
                TotalIncrease = temp["#% increased maximum Life"][0];
            }
            if (TotalIncrease != 0.0f)
            {
                TotalIncrease = (100.0f + TotalIncrease) / 100;
                Subtotal *= TotalIncrease;
            }
            if (temp.ContainsKey("# HP Subtotal"))
            {
                temp["# HP Subtotal"][0] = Subtotal;
            }
            else
            {
                temp.Add("# HP Subtotal", new List<float>(1) { Subtotal });
            }
            if (GlobalSettings.TrackedStats.Count != 0)
            {
                temp = GlobalSettings.TrackedStats.PlaceIntoAttributeDic(temp);
            }

            return temp;
        }

        public Dictionary<string, List<float>> SelectedAttributesWithoutImplicit
            => GetAttributesWithoutImplicit(SkilledNodes, CharClass, _persistentData.CurrentBuild.Bandits);

        private static Dictionary<string, List<float>> GetAttributesWithoutImplicit(
            IEnumerable<SkillNode> skilledNodes, CharacterClass charClass, BanditSettings banditSettings)
        {
            var temp = new Dictionary<string, List<float>>();

            foreach (var attr in CharBaseAttributes[chartype].Union(BaseAttributes).Union(banditSettings.Rewards))
            {
                if (!temp.ContainsKey(stat))
                    temp[stat] = new List<float> { value };
                else if (temp[stat].Any())
                    temp[stat][0] += value;
            }

            foreach (var node in skilledNodes)
            {
                foreach (var attr in ExpandHybridAttributes(node.Attributes))
                {
                    if (!temp.ContainsKey(attr.Key))
                        temp[attr.Key] = new List<float>();
                    for (int i = 0; i < attr.Value.Count; i++)
                    {
                        if (temp.ContainsKey(attr.Key) && temp[attr.Key].Count > i)
                            temp[attr.Key][i] += attr.Value[i];
                        else
                        {
                            temp[attr.Key].Add(attr.Value[i]);
                        }
                    }
                }
            }

            if (GlobalSettings.TrackedStats.Count != 0)
            {
                temp = GlobalSettings.TrackedStats.PlaceIntoAttributeDic(temp);
            }

            return temp;
        }


        public static Dictionary<string, List<float>> GetAttributesWithoutImplicitNodesOnly(IEnumerable<SkillNode> skilledNodes)
        {
            var temp = new Dictionary<string, List<float>>();

            foreach (var node in skilledNodes)
            {
                foreach (var attr in ExpandHybridAttributes(node.Attributes))
                {
                    if (!temp.ContainsKey(attr.Key))
                        temp[attr.Key] = new List<float>();
                    for (int i = 0; i < attr.Value.Count; i++)
                    {
                        if (temp.ContainsKey(attr.Key) && temp[attr.Key].Count > i)
                            temp[attr.Key][i] += attr.Value[i];
                        else
                        {
                            temp[attr.Key].Add(attr.Value[i]);
                        }
                    }
                }
            }
            if (GlobalSettings.TrackedStats.Count != 0)
            {
                temp = GlobalSettings.TrackedStats.PlaceIntoAttributeDic(temp);
            }

            return temp;
        }


        /// <summary>
        /// Returns a task that finishes with a SkillTree object once it has been initialized.
        /// </summary>
        /// <param name="persistentData"></param>
        /// <param name="controller">Null if no initialization progress should be displayed.</param>
        /// <param name="assetLoader">Can optionally be provided if the caller wants to backup assets.</param>
        /// <returns></returns>
        public static async Task<SkillTree> CreateAsync(IPersistentData persistentData,
            ProgressDialogController controller = null, AssetLoader assetLoader = null)
        {
            controller?.SetProgress(0);

            var dataFolderPath = AppData.GetFolder("Data", true);
            _assetsFolderPath = dataFolderPath + "Assets/";

            if (assetLoader == null)
                assetLoader = new AssetLoader(new HttpClient(), dataFolderPath, false);

            var skillTreeTask = LoadTreeFileAsync(dataFolderPath + "Skilltree.txt",
                () => assetLoader.DownloadSkillTreeToFileAsync());
            var optsTask = LoadTreeFileAsync(dataFolderPath + "Opts.txt",
                () => assetLoader.DownloadOptsToFileAsync());
            await Task.WhenAny(skillTreeTask, optsTask);
            controller?.SetProgress(0.1);

            var skillTreeObj = await skillTreeTask;
            var optsObj = await optsTask;
            controller?.SetProgress(0.25);

            var tree = new SkillTree(persistentData);
            await tree.InitializeAsync(skillTreeObj, optsObj, controller, assetLoader);
            return tree;
        }

        private static async Task<string> LoadTreeFileAsync(string path, Func<Task<string>> downloadFile)
        {
            var treeObj = "";
            if (File.Exists(path))
            {
                treeObj = await FileEx.ReadAllTextAsync(path);
            }
            if (treeObj == "")
            {
                treeObj = await downloadFile();
            }
            return treeObj;
        }

        private IEnumerable<KeyValuePair<ushort, SkillNode>> FindNodesInRange(Vector2D mousePointer, int range = 50)
        {
            var nodes =
              Skillnodes.Where(n => ((n.Value.Position - mousePointer).Length < range)).ToList();
            if (!DrawAscendancy || AscType <= 0) return nodes;
            var asn = GetAscNode();
            var bitmap = Assets["Classes" + asn.ascendancyName];
            nodes = Skillnodes.Where(n => (n.Value.ascendancyName != null || (Math.Pow(n.Value.Position.X - asn.Position.X, 2) + Math.Pow(n.Value.Position.Y - asn.Position.Y, 2)) > Math.Pow((bitmap.Height * 1.25 + bitmap.Width * 1.25) / 2, 2)) && ((n.Value.Position - mousePointer).Length < range)).ToList();
            return nodes;
        }

        public SkillNode FindNodeInRange(Vector2D mousePointer, int range = 50)
        {
            var nodes = FindNodesInRange(mousePointer, range);
            var nodeList = nodes as IList<KeyValuePair<ushort, SkillNode>> ?? nodes.ToList();
            if (!nodeList.Any()) return null;

            if (DrawAscendancy)
            {
                var dnode = nodeList.First();
                return nodeList
                    .Where(x => x.Value.ascendancyName == AscendancyClassName)
                    .DefaultIfEmpty(dnode)
                    .First()
                    .Value;
            }
            return nodeList.First().Value;
        }

        public void AllocateSkillNodes(IEnumerable<SkillNode> nodes)
        {
            if (nodes == null) return;
            var skillNodes = nodes as IList<SkillNode> ?? nodes.ToList();
            foreach (var i in skillNodes)
            {
                AllocateSkillNode(i, true);
            }
            SkilledNodes.UnionWith(skillNodes);
        }

        private void AllocateSkillNode(SkillNode node, bool bulk = false)
        {
            if (node == null) return;
            if (node.IsAscendancyStart)
            {
                var remove = SkilledNodes.Where(x => x.ascendancyName != null && x.ascendancyName != node.ascendancyName).ToArray();
                ChangeAscClass(AscendancyClasses.GetAscendancyClassNumber(node.ascendancyName));
                SkilledNodes.ExceptWith(remove);
            }
            else if (node.IsMultipleChoiceOption)
            {
                var remove = SkilledNodes.Where(x => x.IsMultipleChoiceOption && AscendancyClasses.GetStartingClass(node.Name) == AscendancyClasses.GetStartingClass(x.Name)).ToArray();
                SkilledNodes.ExceptWith(remove);
            }
            if (!bulk)
                SkilledNodes.Add(node);
        }

        public void ForceRefundNode(SkillNode node)
        {
            if (!SkilledNodes.Contains(node)) return;
            var charStartNode = GetCharNode();
            var front = new HashSet<SkillNode>() { charStartNode };
            foreach (var i in charStartNode.Neighbor)
                if (SkilledNodes.Contains(i) && i != node)
                    front.Add(i);
            var reachable = new HashSet<SkillNode>(front);

            while (front.Any())
            {
                var newFront = new HashSet<SkillNode>();
                foreach (var i in front)
                {
                    foreach (var j in i.Neighbor)
                    {
                        if (reachable.Contains(j) || !SkilledNodes.Contains(j) || j == node) continue;
                        newFront.Add(j);
                        reachable.Add(j);
                    }
                }
                front = newFront;
            }
            var removable = SkilledNodes.Except(reachable).ToList();
            SkilledNodes.ExceptWith(removable);
        }

        public HashSet<SkillNode> ForceRefundNodePreview(SkillNode node)
        {
            if (!SkilledNodes.Contains(node)) return new HashSet<SkillNode>();

            var charStartNode = GetCharNode();
            var front = new HashSet<SkillNode>() { charStartNode };
            foreach (var i in charStartNode.Neighbor)
                if (SkilledNodes.Contains(i) && i != node)
                    front.Add(i);
            var reachable = new HashSet<SkillNode>(front);

            while (front.Any())
            {
                var newFront = new HashSet<SkillNode>();
                foreach (var i in front)
                {
                    foreach (var j in i.Neighbor)
                    {
                        if (j == node || reachable.Contains(j) || !SkilledNodes.Contains(j)) continue;
                        newFront.Add(j);
                        reachable.Add(j);
                    }
                }
                front = newFront;
            }
            var unreachable = new HashSet<SkillNode>(SkilledNodes);
            unreachable.ExceptWith(reachable);
            return unreachable;
        }

        public List<SkillNode> GetShortestPathTo(SkillNode targetNode, IEnumerable<SkillNode> start)
        {
            var startNodes = start as IList<SkillNode> ?? start.ToList();
            for (int index = 0; index < startNodes.Count; ++index)
            {
                if (startNodes[index].Attributes.ContainsKey("Intuitive Leaped"))//Remove Leaped Nodes from possible start locations
                {
                    startNodes.RemoveAt(index);
                }
            }
            if (startNodes.Contains(targetNode))
                return new List<SkillNode>();
            var adjacent = GetAvailableNodes(startNodes);
            if (adjacent.Contains(targetNode))
                return new List<SkillNode> { targetNode };

            var visited = new HashSet<SkillNode>(startNodes);
            var distance = new Dictionary<SkillNode, int>();
            var parent = new Dictionary<SkillNode, SkillNode>();
            var newOnes = new Queue<SkillNode>();
            var toOmit = new HashSet<SkillNode>(
                         from entry in _nodeHighlighter.NodeHighlights
                         where entry.Value.HasFlag(HighlightState.Crossed)
                         select entry.Key);

            foreach (var node in adjacent)
            {
                if (toOmit.Contains(node))
                {
                    continue;
                }
                newOnes.Enqueue(node);
                distance.Add(node, 1);
            }

            while (newOnes.Count > 0)
            {
                var newNode = newOnes.Dequeue();
                var dis = distance[newNode];
                visited.Add(newNode);
                foreach (var connection in newNode.Neighbor)
                {
                    if (toOmit.Contains(connection))
                        continue;
                    if (visited.Contains(connection))
                        continue;
                    if (distance.ContainsKey(connection))
                        continue;
                    if (newNode.Spc.HasValue)
                        continue;
                    if (newNode.Type == PassiveNodeType.Mastery)
                        continue;
                    if (IsAscendantClassStartNode(newNode))
                        continue;
                    distance.Add(connection, dis + 1);
                    newOnes.Enqueue(connection);

                    parent.Add(connection, newNode);

                    if (connection == targetNode)
                    {
                        newOnes.Clear();
                        break;
                    }
                }
            }

            if (!distance.ContainsKey(targetNode))
                return new List<SkillNode>();

            var curr = targetNode;
            var result = new List<SkillNode> { curr };
            while (parent.ContainsKey(curr))
            {
                result.Add(parent[curr]);
                curr = parent[curr];
            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Returns true iff node is a Ascendant "Path of the ..." node.
        /// </summary>
        public static bool IsAscendantClassStartNode(SkillNode node)
        {
            return node.attributes.Any(s => AscendantClassStartRegex.IsMatch(s));
        }

        /// <summary>
        /// Changes the HighlightState of the node:
        /// None -> Checked -> Crossed -> None -> ...
        /// (preserves other HighlightStates than Checked and Crossed)
        /// </summary>
        /// <param name="node">Node to change the HighlightState for</param>
        public void CycleNodeTagForward(SkillNode node)
        {
            var id = node.Id;
            var build = _persistentData.CurrentBuild;
            if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Checked))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Checked);
                _nodeHighlighter.HighlightNode(node, HighlightState.Crossed);
                build.CheckedNodeIds.Remove(id);
                build.CrossedNodeIds.Add(id);
            }
            else if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Crossed))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Crossed);
                build.CrossedNodeIds.Remove(id);
            }
            else
            {
                _nodeHighlighter.HighlightNode(node, HighlightState.Checked);
                build.CheckedNodeIds.Add(id);
            }
            DrawHighlights();
        }

        /// <summary>
        /// Changes the HighlightState of the node:
        /// ... &lt;- None &lt;- Checked &lt;- Crossed &lt;- None
        /// (preserves other HighlightStates than Checked and Crossed)
        /// </summary>
        /// <param name="node">Node to change the HighlightState for</param>
        public void CycleNodeTagBackward(SkillNode node)
        {
            var id = node.Id;
            var build = _persistentData.CurrentBuild;
            if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Crossed))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Crossed);
                _nodeHighlighter.HighlightNode(node, HighlightState.Checked);
                build.CrossedNodeIds.Remove(id);
                build.CheckedNodeIds.Add(id);
            }
            else if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Checked))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Checked);
                build.CheckedNodeIds.Remove(id);
            }
            else
            {
                _nodeHighlighter.HighlightNode(node, HighlightState.Crossed);
                build.CrossedNodeIds.Add(id);
            }
            DrawHighlights();
        }

        /// <summary>
        /// Resets check and cross tagged node from <see cref="IPersistentData.CurrentBuild"/>.
        /// </summary>
        public void ResetTaggedNodes()
        {
            var build = _persistentData.CurrentBuild;
            _nodeHighlighter.ResetHighlights(SelectExistingNodesById(build.CheckedNodeIds), HighlightState.Checked);
            _nodeHighlighter.ResetHighlights(SelectExistingNodesById(build.CrossedNodeIds), HighlightState.Crossed);
            DrawHighlights();
        }

        private static IEnumerable<SkillNode> SelectExistingNodesById(IEnumerable<ushort> nodeIds)
        {
            return
                from id in nodeIds
                where Skillnodes.ContainsKey(id)
                select Skillnodes[id];
        }

        public void SetCheckTaggedNodes(IReadOnlyList<SkillNode> checkTagged)
        {
            _nodeHighlighter.ResetHighlights(checkTagged, HighlightState.Checked);
            _persistentData.CurrentBuild.CheckedNodeIds.Clear();
            _persistentData.CurrentBuild.CheckedNodeIds.UnionWith(checkTagged.Select(n => n.Id));
            DrawHighlights();
        }

        /// <param name="search">The string to search each node name and attribute for.</param>
        /// <param name="useregex">If the string should be interpreted as a regex.</param>
        /// <param name="flag">The flag to highlight found nodes with.</param>
        /// <param name="matchCount">The number of attributes of a node that must match the search to get highlighted, -1 if the count doesn't matter.</param>
        public void HighlightNodesBySearch(string search, bool useregex, HighlightState flag, int matchCount = -1)
        {
            if (search == "")
            {
                _nodeHighlighter.UnhighlightAllNodes(flag);
                DrawHighlights();
                return;
            }

            var matchFct = matchCount >= 0 ? (Func<string[], Func<string, bool>, bool>)
                 ((attributes, predicate) => attributes.Count(predicate) == matchCount)
                : (attributes, predicate) => attributes.Any(predicate);
            if (useregex)
            {
                try
                {
                    var regex = new Regex(search, RegexOptions.IgnoreCase);
                    var nodes =
                        Skillnodes.Values.Where(
                            nd => (matchFct(nd.attributes, att => regex.IsMatch(att)) ||
                                  regex.IsMatch(nd.Name) && nd.Type != PassiveNodeType.Mastery) &&
                                  (DrawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses || (nd.ascendancyName == null || nd.ascendancyName == AscendancyClassName)) : nd.ascendancyName == null));
                    _nodeHighlighter.ResetHighlights(nodes, flag);
                    DrawHighlights();
                }
                catch (Exception)
                {
                    // ?
                }
            }
            else
            {
                search = search.ToLowerInvariant();
                var nodes =
                    Skillnodes.Values.Where(
                        nd => (matchFct(nd.attributes, att => att.ToLowerInvariant().Contains(search)) ||
                              nd.Name.ToLowerInvariant().Contains(search) && nd.Type != PassiveNodeType.Mastery) &&
                              (DrawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses || (nd.ascendancyName == null || nd.ascendancyName == AscendancyClassName)) : nd.ascendancyName == null));
                _nodeHighlighter.ResetHighlights(nodes, flag);
                DrawHighlights();
            }
        }

        public void UnhighlightAllNodes()
        {
            _nodeHighlighter.UnhighlightAllNodes(HighlightState.Highlights);
        }

        public void UntagAllNodes()
        {
            _nodeHighlighter.UnhighlightAllNodes(HighlightState.Tags);
            _persistentData.CurrentBuild.CheckedNodeIds.Clear();
            _persistentData.CurrentBuild.CrossedNodeIds.Clear();
            DrawHighlights();
        }

        public void CheckAllHighlightedNodes()
        {
            var newlyChecked = _nodeHighlighter.HighlightNodesIf(HighlightState.Checked, HighlightState.Highlights)
                .Select(n => n.Id).ToList();
            _persistentData.CurrentBuild.CheckedNodeIds.UnionWith(newlyChecked);
            _persistentData.CurrentBuild.CrossedNodeIds.ExceptWith(newlyChecked);
            DrawHighlights();
        }

        public void CrossAllHighlightedNodes()
        {
            var newlyCrossed = _nodeHighlighter.HighlightNodesIf(HighlightState.Crossed, HighlightState.Highlights)
                .Select(n => n.Id).ToList();
            _persistentData.CurrentBuild.CrossedNodeIds.UnionWith(newlyCrossed);
            _persistentData.CurrentBuild.CheckedNodeIds.ExceptWith(newlyCrossed);
            DrawHighlights();
        }

        public static Dictionary<string, List<float>> ImplicitAttributes(Dictionary<string, List<float>> attribs, int level)
        {
            var retval = new Dictionary<string, List<float>>
            {
                ["+# to maximum Mana"] = new List<float>
                {
                    attribs["+# to Intelligence"][0]/IntPerMana + level*ManaPerLevel
                },
                ["#% increased maximum Energy Shield"] = new List<float>
                {
                    (float) Math.Round(attribs["+# to Intelligence"][0]/IntPerES, 0)
                },
                ["+# to maximum Life"] = new List<float>
                {
                    attribs["+# to Strength"][0]/StrPerLife + level*LifePerLevel
                }
            };
            // +# to Strength", co["base_str"].Value<int>() }, { "+# to Dexterity", co["base_dex"].Value<int>() }, { "+# to Intelligence", co["base_int"].Value<int>() } };

            // Every 10 strength grants 2% increased melee physical damage.
            var str = (int)attribs["+# to Strength"][0];
            if (str % (int)StrPerED > 0) str += (int)StrPerED - (str % (int)StrPerED);
            retval["#% increased Melee Physical Damage"] = new List<float> { str / StrPerED };
            // Every point of Dexterity gives 2 additional base accuracy, and characters gain 2 base accuracy when leveling up.
            // @see http://pathofexile.gamepedia.com/Accuracy
            retval["+# Accuracy Rating"] = new List<float>
            {
                attribs["+# to Dexterity"][0]/DexPerAcc + (level - 1)*AccPerLevel
            };
            retval["Evasion Rating: #"] = new List<float> { level * EvasPerLevel };

            // Dexterity value is not getting rounded up any more but rounded normally to the nearest multiple of 5.
            // @see http://pathofexile.gamepedia.com/Talk:Evasion
            float dex = attribs["+# to Dexterity"][0];
            dex = (float)Math.Round(dex / DexPerEvas, 0, MidpointRounding.AwayFromZero) * DexPerEvas;
            retval["#% increased Evasion Rating"] = new List<float> { dex / DexPerEvas };

            int frenzycharges, powercharges;
            var endurancecharges = frenzycharges = powercharges = 0;
            if (attribs.ContainsKey("+# to Maximum Endurance Charges"))
                endurancecharges = (int)(attribs["+# to Maximum Endurance Charges"][0]);
            if (attribs.ContainsKey("+# to Maximum Frenzy Charges"))
                frenzycharges = (int)(attribs["+# to Maximum Frenzy Charges"][0]);
            if (attribs.ContainsKey("+# to Maximum Power Charges"))
                powercharges = (int)(attribs["+# to Maximum Power Charges"][0]);
            foreach (var key in attribs.Keys)
            {
                string newkey;
                if (key.Contains("per Endurance Charge") && endurancecharges > 0)
                {
                    newkey = key.Replace("per Endurance Charge", "with all Endurance Charges");
                    retval.Add(newkey, new List<float>());
                    foreach (var f in attribs[key])
                    {
                        retval[newkey].Add(f * endurancecharges);
                    }
                }
                if (key.Contains("per Frenzy Charge") && endurancecharges > 0)
                {
                    newkey = key.Replace("per Frenzy Charge", "with all Frenzy Charges");
                    retval.Add(newkey, new List<float>());
                    foreach (var f in attribs[key])
                    {
                        retval[newkey].Add(f * frenzycharges);
                    }
                }
                if (key.Contains("per Power Charge") && endurancecharges > 0)
                {
                    newkey = key.Replace("per Power Charge", "with all Power Charges");
                    retval.Add(newkey, new List<float>());
                    foreach (var f in attribs[key])
                    {
                        retval[newkey].Add(f * powercharges);
                    }
                }
            }

            return retval;
        }

        public static void DecodeUrl(
            string url, out HashSet<SkillNode> skilledNodes, out CharacterClass charClass, ISkillTree skillTree)
            => DecodeUrlPrivate(url, out skilledNodes, out charClass, skillTree);

        private static BuildUrlData DecodeUrl(string url, out HashSet<SkillNode> skilledNodes, ISkillTree skillTree)
            => DecodeUrlPrivate(url, out skilledNodes, out _,  skillTree);

        public static BuildUrlData DecodeUrl(string url, ISkillTree skillTree)
            => DecodeUrlPrivate(url, out _, out _, skillTree);

        private static BuildUrlData DecodeUrlPrivate(
            string url, out HashSet<SkillNode> skilledNodes, out CharacterClass charClass, ISkillTree skillTree)
        {
            BuildUrlData buildData = skillTree.BuildConverter.GetUrlDeserializer(url).GetBuildData();

            charClass = buildData.CharacterClass;
            var ascType = (byte) buildData.AscendancyClassId;

            SkillNode startnode = Skillnodes[RootNodeClassDictionary[charClass]];
            skilledNodes = new HashSet<SkillNode> { startnode };

            if (ascType > 0)
            {
                string ascendancyClass = skillTree.AscendancyClasses.GetAscendancyClassName(charClass, ascType);
                SkillNode ascNode = AscRootNodeList.First(nd => nd.ascendancyName == ascendancyClass);
                skilledNodes.Add(ascNode);
            }

            var unknownNodes = 0;
            foreach (var nodeId in buildData.SkilledNodesIds)
            {
                if (Skillnodes.TryGetValue(nodeId, out var node))
                {
                    skilledNodes.Add(node);
                }
                else
                {
                    unknownNodes++;
                }
            }

            if (unknownNodes > 0)
            {
                buildData.CompatibilityIssues.Add(L10n.Message($"Some nodes ({unknownNodes}) are unknown and have been omitted."));
            }

            return buildData;
        }

        public void LoadFromUrl(string url)
        {
            var data = DecodeUrl(url, out var skillNodes, this);
            CharClass = data.CharacterClass;
            AscType = data.AscendancyClassId;
            SkilledNodes.Clear();
            AllocateSkillNodes(skillNodes);
        }

        public void Reset()
        {
            var prefs = _persistentData.Options.ResetPreferences;
            var ascNodes = SkilledNodes.Where(n => n.ascendancyName != null).ToList();
            if (prefs.HasFlag(ResetPreferences.MainTree))
            {
                SkilledNodes.Clear();
                if (prefs.HasFlag(ResetPreferences.AscendancyTree))
                    AscType = 0;
                else
                    SkilledNodes.UnionWith(ascNodes);
                var rootNode = Skillnodes[RootNodeClassDictionary[CharClass]];
                SkilledNodes.Add(rootNode);
            }
            else if (prefs.HasFlag(ResetPreferences.AscendancyTree))
            {
                SkilledNodes.ExceptWith(ascNodes);
                AscType = 0;
            }
            if (prefs.HasFlag(ResetPreferences.Bandits))
                _persistentData.CurrentBuild.Bandits.Reset();
            UpdateAscendancyClasses = true;
        }

        /// <summary>
        /// Returns all currently Check-tagged nodes.
        /// </summary>
        public HashSet<SkillNode> GetCheckedNodes()
        {
            var nodes = new HashSet<SkillNode>();
            foreach (var entry in _nodeHighlighter.NodeHighlights)
            {
                if (!entry.Key.IsRootNode && entry.Value.HasFlag(HighlightState.Checked))
                {
                    nodes.Add(entry.Key);
                }
            }
            return nodes;
        }

        /// <summary>
        /// Returns all currently Cross-tagged nodes.
        /// </summary>
        public HashSet<SkillNode> GetCrossedNodes()
        {
            var nodes = new HashSet<SkillNode>();
            foreach (var entry in _nodeHighlighter.NodeHighlights)
            {
                if (!entry.Key.IsRootNode && entry.Value.HasFlag(HighlightState.Crossed))
                {
                    nodes.Add(entry.Key);
                }
            }
            return nodes;
        }

        private SkillNode GetCharNode()
            => Skillnodes[GetCharNodeId()];

        private ushort GetCharNodeId()
            => RootNodeClassDictionary[CharClass];

        private SkillNode GetAscNode()
        {
            var ascNodeId = GetAscNodeId();
            if (ascNodeId != 0)
                return Skillnodes[ascNodeId];
            else
                return null;
        }

        private ushort GetAscNodeId()
        {
            if (_asctype <= 0 || _asctype > 3)
                return 0;
            var ascendancyClassName = AscendancyClassName;
            try
            {
                return AscRootNodeList.First(x => x.ascendancyName == ascendancyClassName).Id;
            }
            catch
            {
                return 0;
            }
        }

        private HashSet<SkillNode> GetAvailableNodes(IEnumerable<SkillNode> skilledNodes)
        {
            var availNodes = new HashSet<SkillNode>();

            foreach (var node in skilledNodes)
            {
                foreach (var skillNode in node.Neighbor)
                {
                    if (!RootNodeList.Contains(skillNode.Id) && !SkilledNodes.Contains(skillNode))
                        availNodes.Add(skillNode);
                }
            }
            return availNodes;
        }

        public static IEnumerable<KeyValuePair<string, IReadOnlyList<float>>> ExpandHybridAttributes(Dictionary<string, IReadOnlyList<float>> attributes)
        {
            return attributes.SelectMany(ExpandHybridAttributes);
        }

        public static IEnumerable<KeyValuePair<string, IReadOnlyList<float>>> ExpandHybridAttributes(KeyValuePair<string, IReadOnlyList<float>> attribute)
        {
            if (HybridAttributes.TryGetValue(attribute.Key, out List<string> expandedAttributes))
            {
                foreach (var expandedAttribute in expandedAttributes)
                {
                    yield return new KeyValuePair<string, IReadOnlyList<float>>(expandedAttribute, attribute.Value);
                }
            }
            else
            {
                yield return attribute;
            }
        }

        private bool CanSwitchClass(CharacterClass charClass)
        {
            RootNodeClassDictionary.TryGetValue(charClass, out var rootNodeValue);
            var classSpecificStartNodes = StartNodeDictionary.Where(kvp => kvp.Value == rootNodeValue).Select(kvp => kvp.Key).ToList();

            return (
                from nodeId in classSpecificStartNodes
                let temp = GetShortestPathTo(Skillnodes[nodeId], SkilledNodes)
                where !temp.Any() && Skillnodes[nodeId].ascendancyName == null
                select nodeId
            ).Any();
        }

        #region ISkillTree members



        #endregion
    }
}