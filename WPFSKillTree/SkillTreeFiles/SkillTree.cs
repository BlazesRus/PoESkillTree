﻿using EnumsNET;
using PoESkillTree.Common;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Localization;
using PoESkillTree.Model;
using PoESkillTree.Utils;
using PoESkillTree.Utils.UrlProcessing;
using PoESkillTree.Utils.UrlProcessing.Decoders;
using PoESkillTree.Utils.UrlProcessing.Encoders;
using PoESkillTree.Utils.Wpf;
using PoESkillTree.ViewModels.PassiveTree;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static PoESkillTree.SkillTreeFiles.Constants;
using HighlightState = PoESkillTree.SkillTreeFiles.NodeHighlighter.HighlightState;
using PoESkillTree.ViewModels;
using System.Windows.Controls;

namespace PoESkillTree.SkillTreeFiles
{
    public partial class SkillTree : Notifier, ISkillTree
    {
        public Rect AscendancyButtonRect = new Rect();

#pragma warning disable CS8618 // Initialized in InitializeAsync or CreateAsync
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

        private static readonly Dictionary<string, string> RenameImplicitAttributes = new Dictionary<string, string>
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
            {"ascendancyNormal", "AscendancyFrameSmallNormal"},
            {"ascendancyNotable", "AscendancyFrameLargeNormal"},
            {"ascendancyStart", "AscendancyMiddle"},
            {"blighted", "BlightedNotableFrameUnallocated"},
            {"clusterJewel", "JewelSocketAltNormal"},
        };

        private static readonly Dictionary<string, string> NodeBackgroundsActive = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrameActive"},
            {"notable", "NotableFrameAllocated"},
            {"keystone", "KeystoneFrameAllocated"},
            {"jewel", "JewelFrameAllocated"},
            {"ascendancyNormal", "AscendancyFrameSmallAllocated"},
            {"ascendancyNotable", "AscendancyFrameLargeAllocated"},
            {"ascendancyStart", "AscendancyMiddle"},
            {"blighted", "BlightedNotableFrameAllocated"},
            {"clusterJewel", "JewelSocketAltActive"},
        };

        private static SkillIcons IconActiveSkills { get; set; }
        private static SkillIcons IconInActiveSkills { get; set; }
        public static Dictionary<ushort, PassiveNodeViewModel> Skillnodes => PoESkillTree.PassiveNodes;

        private static IEnumerable<string>? _allAttributes;
        /// <summary>
        /// Gets an Array of all the attributes of SkillNodes.
        /// </summary>
        public static IEnumerable<string> AllAttributes
        {
            get { return _allAttributes ??= Skillnodes.Values.SelectMany(n => n.Attributes.Keys).Distinct().ToArray(); }
        }

        public static Dictionary<CharacterClass, IReadOnlyList<(string stat, float value)>> CharBaseAttributes
        {
            get;
            private set;
        }

        public static List<ushort> RootNodeList { get; } = new List<ushort>();
        private static HashSet<PassiveNodeViewModel> AscRootNodeList { get; } = new HashSet<PassiveNodeViewModel>();
        public static Rect2D SkillTreeRect { get; private set; }
        private static Dictionary<CharacterClass, ushort> RootNodeClassDictionary { get; } = new Dictionary<CharacterClass, ushort>();
        private static Dictionary<ushort, ushort> StartNodeDictionary { get; } = new Dictionary<ushort, ushort>();

        private static Dictionary<string, BitmapImage> Assets { get; } = new Dictionary<string, BitmapImage>();

        public readonly ObservableSet<PassiveNodeViewModel> SkilledNodes = new ObservableSet<PassiveNodeViewModel>();
        public readonly ObservableSet<PassiveNodeViewModel> HighlightedNodes = new ObservableSet<PassiveNodeViewModel>();

        private readonly ObservableSet<PassiveNodeViewModel> _itemAllocatedNodes = new ObservableSet<PassiveNodeViewModel>();

        public IEnumerable<ushort> ItemAllocatedNodes
        {
            set => _itemAllocatedNodes.ResetTo(value.Select(n => Skillnodes[n]));
        }

        public Func<IReadOnlyCollection<ushort>, IEnumerable<ushort>> ItemConnectedNodesSelector { private get; set; }

        private IEnumerable<PassiveNodeViewModel> SelectItemConnectedNodes(IEnumerable<PassiveNodeViewModel> sourceNodes) =>
            ItemConnectedNodesSelector(sourceNodes.Select(n => n.Id).ToHashSet())
                .Select(n => Skillnodes[n]);
        public IAscendancyClasses AscendancyClasses { get; private set; }

        private CharacterClass _charClass;
        private int _asctype;

        public static PassiveTreeViewModel PoESkillTree { get; private set; }

        private static bool _initialized;

#if (PoESkillTree_DisableStatTracking == false)
        //public ContextMenu trackingContextMenu;

#if PoESkillTree_UseSwordfishDictionary || PoESkillTree_UseIXDictionary
        public ConcurrentObservableDictionary<string, PseudoTotal> selectedPseudoTotal;
#elif PoeSkillTree_DontUseKeyedTrackedStats==false
        public ObservableKeyedPseudoStat selectedPseudoTotal;
#else
        public ObservableDictionary<string, PseudoTotal> selectedPseudoTotal;
#endif

        /// <summary>
        /// Tracked Stat Totals for Current Skill-Tree(Attribute Keys with Tracked Attribute values)
        /// </summary>
#if PoESkillTree_UseSwordfishDictionary || PoESkillTree_UseIXDictionary
        public ConcurrentObservableDictionary<string, PseudoTotal> SelectedPseudoTotal
#elif PoeSkillTree_DontUseKeyedTrackedStats == false
        public ObservableKeyedPseudoStat SelectedPseudoTotal
#else
        public ObservableDictionary<string, PseudoTotal> SelectedPseudoTotal
#endif
        {
            get => selectedPseudoTotal;
            set => SetProperty(ref selectedPseudoTotal, value);
        }

        /// <summary>
        /// Tracked Stat Totals for highlighted build (Attribute Keys with Tracked Attribute values)
        /// </summary>
        public Dictionary<string, float>? HighlightedPseudoTotal { get; set; }

/*
        /// <summary>
        /// Unfinished(Still need current implementation to first display the context menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RemoveTrackedAttr(object sender, RoutedEventArgs e)
        {
            MenuItem CurrentMenuItem = (MenuItem)sender;
            //for (var i = 0; i < cmDeleteGroup.Items.Count; i++)
            //{
            //    if (((MenuItem)cmDeleteGroup.Items[i]).Header.ToString()!.ToLower().Equals(((MenuItem)sender).Header.ToString()!))
            //    {
            //        cmDeleteGroup.Items.RemoveAt(i);
            //        if (cmDeleteGroup.Items.Count == 0)
            //            cmDeleteGroup.IsEnabled = false;
            //        break;
            //    }
            //}

            //_attributeGroups.DeleteGroup(((MenuItem)sender).Header.ToString()!);
            //RefreshAttributeLists();
        }
*/
#endif

        //public Dictionary<string, float> SelectedPseudoTotalWithItems;
        //public Dictionary<string, float> TrackedStatTotalWithItems;

        private SkillTree(IPersistentData persistentData)
#pragma warning restore
        {
            _persistentData = persistentData;

            SkilledNodes.CollectionChanged += SkilledNodes_CollectionChanged;
#if PoESkillTree_UseSwordfishDictionary || PoESkillTree_UseIXDictionary
            SelectedPseudoTotal = = new ConcurrentObservableDictionary<string, PseudoTotal>() { };
#elif PoeSkillTree_DontUseKeyedTrackedStats==false
            SelectedPseudoTotal = new ObservableKeyedPseudoStat();
#endif
/*
            trackingContextMenu = new ContextMenu();
            var cmRemoveTrackedAttr = new MenuItem { Header = L10n.Message("Remove PseudoAttribute from Tracking") };
            cmRemoveTrackedAttr.Click += RemoveTrackedAttr;
            trackingContextMenu.Items.Add(cmRemoveTrackedAttr);
*/
        }

        private void SkilledNodes_CollectionChanged(object sender, CollectionChangedEventArgs<PassiveNodeViewModel> args)
        {
            foreach (var node in args.RemovedItems)
            {
                node.IsSkilled = false;
            }

            foreach (var node in args.AddedItems)
            {
                node.IsSkilled = true;
            }
        }

        private async Task InitializeAsync(string treestring, string? opsstring, ProgressDialogController? controller, AssetLoader assetLoader)
        {
            if (!_initialized)
            {
                RootNodeList.Clear();
                AscRootNodeList.Clear();
                RootNodeClassDictionary.Clear();
                StartNodeDictionary.Clear();

                PoESkillTree = new PassiveTreeViewModel(treestring, opsstring);

                controller?.SetProgress(0.25);
                await assetLoader.DownloadSkillNodeSpritesAsync(PoESkillTree, d => controller?.SetProgress(0.25 + d * 0.30));
                IconInActiveSkills = new SkillIcons();
                IconActiveSkills = new SkillIcons();
                var assetActions =
                    new List<(Task<BitmapImage>, Action<BitmapImage>)>(PoESkillTree.SkillSprites.Count + PoESkillTree.Assets.Count);
                foreach (var obj in PoESkillTree.SkillSprites)
                {
                    foreach (var i in obj.Value)
                    {
                        if (i.FileName.Contains('?'))
                            i.FileName = i.FileName.Remove(i.FileName.IndexOf('?'));
                    }

                    var value = obj.Value[PoESkillTree.MaxImageZoomLevelIndex];
                    if (obj.Key.Contains("Inactive"))
                    {
                        AddToIcons(IconInActiveSkills, value, obj.Key.Replace("Inactive", string.Empty), assetActions);
                    }
                    else if (obj.Key.Contains("Active"))
                    {
                        AddToIcons(IconActiveSkills, value, obj.Key.Replace("Active", string.Empty), assetActions);
                    }
                    else
                    {
                        AddToIcons(IconInActiveSkills, value, obj.Key, assetActions);
                        AddToIcons(IconActiveSkills, value, obj.Key, assetActions);
                    }
                }

                controller?.SetProgress(0.55);
                // The last percent progress is reserved for rounding errors as progress must not get > 1.
                await assetLoader.DownloadAssetsAsync(PoESkillTree, d => controller?.SetProgress(0.55 + d * 0.44));
                foreach (var ass in PoESkillTree.Assets)
                {
                    var key = ass.Key.Replace("PassiveSkillScreen", string.Empty);
                    var path = _assetsFolderPath + key + ".png";
                    assetActions.Add((Task.Run(() => BitmapImageFactory.Create(path)), i => Assets[key] = i));
                }

                foreach (var (task, action) in assetActions)
                {
                    action(await task);
                }

                AscendancyClasses = new AscendancyClasses(PoESkillTree.CharacterClasses);

                CharBaseAttributes = new Dictionary<CharacterClass, IReadOnlyList<(string stat, float value)>>();
                foreach (var character in PoESkillTree.CharacterClasses)
                {
                    var characterClass = Enums.Parse<CharacterClass>(character.Name);
                    CharBaseAttributes[characterClass] = new (string stat, float value)[]
                    {
                        ("+# to Strength", character.Strength),
                        ("+# to Dexterity", character.Dexterity),
                        ("+# to Intelligence", character.Intelligence)
                    };
                }

                GlobalSettings.JewelStorage = new JewelData();
                if (PoESkillTree.Root != null)
                {
                    foreach (var i in PoESkillTree.Root.OutPassiveNodeIds)
                    {
                        RootNodeList.Add(i);
                        if (Skillnodes.ContainsKey(i))
                        {
                            var node = Skillnodes[i];

                            var characterClass = PassiveNodeNameToClass[node.Name.ToUpperInvariant()];
                            if (!RootNodeClassDictionary.ContainsKey(characterClass))
                            {
                                RootNodeClassDictionary.Add(characterClass, node.Id);
                            }

                            foreach (var linkedNode in node.OutPassiveNodeIds)
                            {
                                if (!StartNodeDictionary.ContainsKey(node.Id) && !node.IsAscendancyStart)
                                {
                                    StartNodeDictionary.Add(linkedNode, node.Id);
                                }
                            }
                        }
                    }
                }

                string ClusterLabel = "+# Expansion JewelSlot";
                string LClusterLabel = "+# Large JewelSlot";
                string MClusterLabel = "+# Medium JewelSlot";
                string SClusterLabel = "+# Small JewelSlot";
                string JewelSocketLabel = "+# NonCluster JewelSocket";

                List<float> SingleVal = new List<float>(1);
                SingleVal.Add(1);
                foreach (var (_, node) in PoESkillTree.PassiveNodes)
                {
                    if (node.IsAscendancyStart && !AscRootNodeList.Contains(node))
                    {
                        AscRootNodeList.Add(node);
                    }
                    else if(node.PassiveNodeType == PassiveNodeType.JewelSocket)
                    {
                        if (!node.Attributes.ContainsKey(JewelSocketLabel))
                            node.Attributes.Add(JewelSocketLabel, SingleVal);
                        GlobalSettings.JewelInfo.AddJewelSlot(node.Id);
                    }
                    else if(node.PassiveNodeType == PassiveNodeType.ExpansionJewelSocket)//Cluster Jewel
                    {
                        if (!node.Attributes.ContainsKey(ClusterLabel))
                            node.Attributes.Add(ClusterLabel, SingleVal);
                        if(node.Name.Contains("Large Jewel Socket")&&!node.Attributes.ContainsKey(LClusterLabel))
                        {
                            node.Attributes.Add(LClusterLabel, SingleVal);
                            //Including Large Cluster Jewels as potential Threshold Jewel Slots(since at least one Dex Threshold jewel slot exists that is a cluster Jewel Slot)
                            GlobalSettings.JewelInfo.AddJewelSlot(node.Id);
                        }
                        else if(node.Name.Contains("Medium Jewel Socket")&&!node.Attributes.ContainsKey(MClusterLabel))
                            node.Attributes.Add(MClusterLabel, SingleVal);
                        else if(!node.Attributes.ContainsKey(SClusterLabel))
                            node.Attributes.Add(SClusterLabel, SingleVal);
                        node.UpdateStatDescription();
                    }
                    else if(node.PassiveNodeType == PassiveNodeType.Mastery)
                    {
                        MasteryDefinitions.SetMasteryLabel(node);
                    }
                    else if(node.IsAscendancyNode)
                    {
                        GlobalSettings.KnownAscendancyNodes.Add(node.Id);
                    }
                }

                SkillTreeRect = new Rect2D(
                    new Vector2D(PoESkillTree.MinX * PoESkillTree.MaxImageZoomLevel * 1.25, PoESkillTree.MinY * PoESkillTree.MaxImageZoomLevel * 1.25),
                    new Vector2D(PoESkillTree.MaxX * PoESkillTree.MaxImageZoomLevel * 1.25, PoESkillTree.MaxY * PoESkillTree.MaxImageZoomLevel * 1.25));
            }

            if (_persistentData.Options.ShowAllAscendancyClasses)
                DrawAscendancy = true;

            InitialSkillTreeDrawing();
            controller?.SetProgress(1);

            _initialized = true;
        }

        private static void AddToIcons(SkillIcons icons, in Engine.GameModel.PassiveTree.Base.JsonPassiveTreeSkillSprite sprite, in string prefix, in List<(Task<BitmapImage>, Action<BitmapImage>)> assetActions)
        {
            var filename = sprite.FileName.Replace("https://web.poecdn.com/image/passive-skill/", string.Empty);
            var path = _assetsFolderPath + filename;
            assetActions.Add((Task.Run(() => BitmapImageFactory.Create(path)), i => icons.Images[filename] = i));
            foreach (var o in sprite.Coords)
            {
                var iconKey = prefix + "_" + o.Key;
                icons.SkillPositions[iconKey] = new Rect(o.Value.X, o.Value.Y, o.Value.Width, o.Value.Height);
                icons.SkillImages[iconKey] = filename;
            }
        }

#if PoESkillTree_UseTreePointSharing
        Dictionary<string, int> points = new Dictionary<string, int>()
        {
            {"NormalUsed", 0},
            {"NormalTotal", 22},
            {"AscendancyUsed", 0},
            {"AscendancyTotal", 8},
        };
#endif

        /// <summary>
        /// This will get all skill points related to the tree both Normal and Ascendancy
        /// </summary>
        /// <returns>A Dictionary with keys of "NormalUsed", "NormalTotal", "AscendancyUsed", "AscendancyTotal", and "ScionAscendancyChoices"</returns>
        public Dictionary<string, int> GetPointCount()
        {
#if PoESkillTree_UseTreePointSharing == false
            Dictionary<string, int> points = new Dictionary<string, int>()
            {
                {"NormalUsed", 0},
                {"NormalTotal", 22},
                {"AscendancyUsed", 0},
                {"AscendancyTotal", 8},
            };
#endif

            var bandits = _persistentData.CurrentBuild.Bandits;
#if PoESkillTree_UseTreePointSharing
            points["NormalUsed"] = 0;
            points["AscendancyUsed"] = 0;
            points["NormalTotal"] = bandits.Choice == Bandit.None ? _persistentData.CurrentBuild.Level + 23 : _persistentData.CurrentBuild.Level + 21;
#else
            points["NormalTotal"] += _persistentData.CurrentBuild.Level + bandits.Choice == Bandit.None ? 1 : -1;
#endif
            foreach (var node in SkilledNodes)
            {
                if (!node.IsAscendancyNode && !node.IsRootNode)
                    points["NormalUsed"] += 1;
                else if (node.IsAscendancyNode && !node.IsAscendancyStart && !node.IsMultipleChoiceOption)
                {
                    points["AscendancyUsed"] += 1;
                    points["NormalTotal"] += node.PassivePointsGranted;
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
            if (toType == 0)
            {
                var remove = SkilledNodes.Where(n => n.IsAscendancyNode).ToList();
                if (!_persistentData.Options.ShowAllAscendancyClasses)
                    DrawAscendancy = false;
                SetProperty(ref _asctype, toType, propertyName: nameof(AscType));
                SkilledNodes.ExceptWith(remove);
            }
            else
            {
                var remove = new List<PassiveNodeViewModel>();
                SetProperty(ref _asctype, toType, propertyName: nameof(AscType));
                var sn = GetAscNode();
                if (sn != null)
                {
                    foreach (var n in SkilledNodes)
                    {
                        if (sn.AscendancyName != n.AscendancyName && n.IsAscendancyNode)
                            remove.Add(n);
                    }
                    SkilledNodes.ExceptAndUnionWith(remove, new[] { sn });
                }
            }

            DrawAscendancyLayers();
        }

        public bool SwitchClass(CharacterClass charClass)
        {
            if (charClass == CharClass)
                return false;
            var canSwitch = CanSwitchClass(charClass);
            CharClass = charClass;

            var remove = canSwitch ? SkilledNodes.Where(n => n.IsAscendancyNode || n.IsRootNode) : SkilledNodes;
            var add = Skillnodes[RootNodeClassDictionary[charClass]];
            SkilledNodes.ExceptAndUnionWith(remove.ToList(), new[] { add });
            _asctype = 0;
            return true;
        }

        public string? AscendancyClassName
            => AscendancyClasses.GetAscendancyClassName(CharClass, AscType);

        public Dictionary<string, List<float>>? HighlightedAttributes { get; set; }

        public Dictionary<string, List<float>> SelectedAttributes
            => GetAttributes(SkilledNodes, CharClass, _persistentData.CurrentBuild.Level, _persistentData.CurrentBuild.Bandits);

        public static Dictionary<string, List<float>> GetAttributes(
            IEnumerable<PassiveNodeViewModel> skilledNodes, CharacterClass charClass, int level, BanditSettings banditSettings)
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
            return temp;
        }

#if PoESkillTree_DisableStatTracking == false
        private string InsertNumbersInTrackedDisplay(string statDisplay, float statTotal)
        {
            statDisplay = GlobalSettings._backreplace.Replace(statDisplay, statTotal + "", 1);
            return statDisplay;
        }
        public void UpdateTrackedStatCalculation()
        {
            if(GlobalSettings.TrackedStats.Count==0)
            {
                if(!SelectedPseudoTotal.IsEmpty())
                    SelectedPseudoTotal.Clear();
                if(HighlightedPseudoTotal!=null&& !HighlightedPseudoTotal.IsEmpty())
                    HighlightedPseudoTotal.Clear();
                return;
            }
            float statCalc;
            string displayTotal;
            foreach (var item in GlobalSettings.TrackedStats)
            {
                statCalc = item.Value.CalculateValue(SelectedAttributes);//trackedStat.Value.UpdateValue(Tree.SelectedAttributes, Tree.SelectedPseudoTotal);
                displayTotal = InsertNumbersInTrackedDisplay(item.Key, statCalc);
#if PoeSkillTree_DontUseKeyedTrackedStats == false
                if (SelectedPseudoTotal.Contains(item.Key))//Update Tracked Value if already in Display
#else
                if (SelectedPseudoTotal.ContainsKey(item.Key))//Update Tracked Value if already in Display
#endif
                {
                    SelectedPseudoTotal[item.Key].Text = displayTotal;
                    SelectedPseudoTotal[item.Key].Total = statCalc;
                }
                else
#if PoeSkillTree_DontUseKeyedTrackedStats == false
                    SelectedPseudoTotal.Add(new PseudoTotal(displayTotal, statCalc, item.Key));
#else
                    SelectedPseudoTotal.Add(item.Key, new PseudoTotal(displayTotal, statTotal));
#endif
            }
            //Updating Display to show attribute comparison
            if (HighlightedPseudoTotal != null)
            {
                //Reusing statCalc for Stat difference(how much additional compared to selected build)
                foreach (var item in HighlightedPseudoTotal)
                {
                    statCalc = SelectedPseudoTotal[item.Key].Total - item.Value;
                    if (statCalc >= 0)
                        SelectedPseudoTotal[item.Key].Text += " (+";
                    else
                        SelectedPseudoTotal[item.Key].Text += " (";
                    SelectedPseudoTotal[item.Key].Text += statCalc+")";
                }
            }
        }
#endif

        private static Dictionary<string, List<float>> GetAttributesWithoutImplicit(
            IEnumerable<PassiveNodeViewModel> skilledNodes, CharacterClass charClass, BanditSettings banditSettings)
        {
            var temp = new Dictionary<string, List<float>>();

            foreach (var (stat, value) in
                CharBaseAttributes[charClass].Union(BaseAttributes).Union(banditSettings.Rewards))
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

            return temp;
        }

        public static Dictionary<string, List<float>> GetAttributesWithoutImplicitNodesOnly(IEnumerable<PassiveNodeViewModel> skilledNodes)
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

            return temp;
        }

#if (PoESkillTree_DisableStatTracking == false)
        public static Dictionary<string, float> GetCalculatedTrackedAttributesFromStatDictionary(Dictionary<string, List<float>> NonTrackedTotal)
        {
            float totalStat;
            var statTotals = new Dictionary<string, float>(GlobalSettings.TrackedStats.Count);
            foreach (var element in GlobalSettings.TrackedStats)
            {
                totalStat = element.Value.CalculateValue(NonTrackedTotal);
                statTotals.Add(element.Key, totalStat);
            }
            return statTotals;
        }
#endif

        /// <summary>
        /// Returns a task that finishes with a SkillTree object once it has been initialized.
        /// </summary>
        /// <param name="persistentData"></param>
        /// <param name="controller">Null if no initialization progress should be displayed.</param>
        /// <param name="assetLoader">Can optionally be provided if the caller wants to backup assets.</param>
        /// <returns></returns>
        public static async Task<SkillTree> CreateAsync(IPersistentData persistentData,
            ProgressDialogController? controller = null, AssetLoader? assetLoader = null)
        {
            controller?.SetProgress(0);

            var dataFolderPath = AppData.GetFolder("Data", true);
            _assetsFolderPath = dataFolderPath + "Assets/";

            if (assetLoader == null)
                assetLoader = new AssetLoader(new HttpClient(), dataFolderPath, false);

            if (File.Exists(dataFolderPath + "Skilltree.txt"))
                File.Move(dataFolderPath + "Skilltree.txt", dataFolderPath + AssetLoader.SkillTreeFile);

            if (File.Exists(dataFolderPath + "Opts.txt"))
                File.Move(dataFolderPath + "Opts.txt", dataFolderPath + AssetLoader.OptsFile);

            var skillTreeTask = LoadTreeFileAsync(dataFolderPath + AssetLoader.SkillTreeFile,
                () => assetLoader.DownloadSkillTreeToFileAsync());
            var optsTask = LoadTreeFileAsync(dataFolderPath + AssetLoader.OptsFile);
            await Task.WhenAny(skillTreeTask, optsTask);
            controller?.SetProgress(0.1);

            var skillTreeObj = await skillTreeTask;
            var optsObj = await optsTask;
            controller?.SetProgress(0.25);

            var tree = new SkillTree(persistentData);
            await tree.InitializeAsync(skillTreeObj, optsObj, controller, assetLoader);
            return tree;
        }

        private static async Task<string> LoadTreeFileAsync(string path, Func<Task<string>>? downloadFile = null)
        {
            var treeObj = string.Empty;
            if (File.Exists(path))
            {
                treeObj = await FileUtils.ReadAllTextAsync(path);
            }

            if (string.IsNullOrWhiteSpace(treeObj) && !(downloadFile is null))
            {
                treeObj = await downloadFile();
            }

            return treeObj;
        }

        private List<KeyValuePair<ushort, PassiveNodeViewModel>> FindNodesInRange(Vector2D mousePointer)
        {
            var nodes = Skillnodes.Where(n =>
            {
                var size = GetNodeSurroundBrushSize(n.Value, 0);
                var range = size.Width * size.Height * n.Value.ZoomLevel;
                var length = (n.Value.Position - mousePointer).Length;
                return length * length < range;
            });

            if (!DrawAscendancy || AscType <= 0) return nodes.ToList();
            if (GetAscNode() is PassiveNodeViewModel asn)
            {
                var bitmap = Assets["Classes" + asn.AscendancyName];
                var radius = (bitmap.Width * bitmap.Height) / Math.PI;
                nodes = nodes.Where(n => n.Value.IsAscendancyNode || (Math.Pow(n.Value.Position.X - asn.Position.X, 2) + Math.Pow((n.Value.Position.Y - asn.Position.Y), 2)) > radius).ToList();
            }
            return nodes.ToList();
        }

        public PassiveNodeViewModel? FindNodeInRange(Vector2D mousePointer)
        {
            var nodes = FindNodesInRange(mousePointer) ?? new List<KeyValuePair<ushort, PassiveNodeViewModel>>();
            if (!nodes.Any()) return null;

            if (DrawAscendancy)
            {
                var dnode = nodes.First();
                return nodes.Where(x => x.Value.AscendancyName == AscendancyClassName).DefaultIfEmpty(dnode).First().Value;
            }
            return nodes.First().Value;
        }

        public void ResetSkilledNodesTo(IEnumerable<PassiveNodeViewModel> nodes)
        {
            SkilledNodes.ResetTo(nodes);
            AscType = SelectAscendancyFromNodes(nodes) ?? 0;
        }

        public void ResetSkilledNodesWithClassInfo(IEnumerable<PassiveNodeViewModel> nodes, CharacterClass charClass, int ascendancyType)
        {
            SkilledNodes.ResetTo(nodes);
            if (charClass != CharClass)
            {
                var canSwitch = CanSwitchClass(charClass);
                CharClass = charClass;

                //Only remove Ascendancy nodes that do not match the Ascendancy of loaded url
                string? ascendancyName = AscendancyClasses.GetAscendancyClassName(charClass, ascendancyType); 
                IEnumerable<PassiveNodeViewModel>? remove = canSwitch ? SkilledNodes.Where(n => (n.IsAscendancyNode&&n.AscendancyName!= ascendancyName) || n.IsRootNode) : SkilledNodes;
                var add = Skillnodes[RootNodeClassDictionary[charClass]];
                SkilledNodes.ExceptAndUnionWith(remove.ToList(), new[] { add });
                _asctype = 0;
            }
            AscType = ascendancyType;//AscType = SelectAscendancyFromNodes(nodes) ?? 0;
        }

        public void AllocateSkillNodes(IReadOnlyCollection<PassiveNodeViewModel> toAdd)
        {
            toAdd = toAdd.Where(n => !SkilledNodes.Contains(n)).ToList();
            var toRemove = toAdd.SelectMany(SelectAscendancyNodesToRemove).ToList();
            SkilledNodes.ExceptAndUnionWith(toRemove, toAdd);
            if (SelectAscendancyFromNodes(toAdd) is int ascType)
                AscType = ascType;
        }

        public void AllocateNormalSkillNodes(IReadOnlyCollection<PassiveNodeViewModel> toAdd)
        {
            toAdd = toAdd.Where(n => !SkilledNodes.Contains(n)).ToList();
            SkilledNodes.UnionWith(toAdd);
        }

        public void AllocateAscSkillNodes(IReadOnlyCollection<PassiveNodeViewModel> toAdd)
        {
            toAdd = toAdd.Where(n => !SkilledNodes.Contains(n)).ToList();
            if (SelectAscendancyFromNodes(toAdd) is int ascType)
                AscType = ascType;
            var toRemove = toAdd.SelectMany(SelectAscendancyNodesToRemove).ToList();
            SkilledNodes.ExceptAndUnionWith(toRemove, toAdd);
        }

        private int? SelectAscendancyFromNodes(IEnumerable<PassiveNodeViewModel> nodes)
        {
            int? ascendancy = null;
            foreach (var node in nodes)
            {
                if (node.IsAscendancyStart)
                {
                    ascendancy = AscendancyClasses.GetAscendancyClassNumber(node.AscendancyName!);
                    break;//Exit loop after retrieve ascendancy info
                }
            }
            return ascendancy;
        }

        private IEnumerable<PassiveNodeViewModel> SelectAscendancyNodesToRemove(PassiveNodeViewModel node)
        {
            if (node.IsAscendancyStart)
                return SkilledNodes.Where(x => x.IsAscendancyNode && x.AscendancyName != node.AscendancyName);
            if (node.IsMultipleChoiceOption)
                return SkilledNodes
                    .Where(x => x.IsMultipleChoiceOption)
                    .Where(x => AscendancyClasses.GetStartingClass(node.Name)
                                == AscendancyClasses.GetStartingClass(x.Name));
            return Enumerable.Empty<PassiveNodeViewModel>();
        }

        public void ForceRefundNode(PassiveNodeViewModel node)
        {
            if (!SkilledNodes.Contains(node))
                return;

            SkilledNodes.ExceptWith(GetNodesToRefundWhenRefunding(node));
        }

        public IReadOnlyCollection<PassiveNodeViewModel> ForceRefundNodePreview(PassiveNodeViewModel node) =>
            GetNodesToRefundWhenRefunding(node);

        private IReadOnlyCollection<PassiveNodeViewModel> GetNodesToRefundWhenRefunding(PassiveNodeViewModel node)
        {
            if (!SkilledNodes.Contains(node))
                return Array.Empty<PassiveNodeViewModel>();

            var newSkilledNodes = SkilledNodes.ToHashSet();
            newSkilledNodes.Remove(node);

            var reachable = SelectNodesReachableFromStart(newSkilledNodes);
            reachable.UnionWith(SelectItemConnectedNodes(reachable));
            reachable.Remove(node);
            return SkilledNodes.Except(reachable).ToList();
        }

        private HashSet<PassiveNodeViewModel> SelectNodesReachableFromStart(IReadOnlyCollection<PassiveNodeViewModel> nodes)
        {
            var front = new HashSet<PassiveNodeViewModel> { GetCharNode() };

            var reachable = new HashSet<PassiveNodeViewModel>(front);
            while (front.Any())
            {
                front = front.SelectMany(n => n.NeighborPassiveNodes.Values)
                    .Where(n => !reachable.Contains(n))
                    .Where(nodes.Contains)
                    .ToHashSet();
                reachable.UnionWith(front);
            }

            return reachable;
        }

        public IReadOnlyCollection<PassiveNodeViewModel> GetShortestPathTo(PassiveNodeViewModel targetNode)
        {
            if (SkilledNodes.Contains(targetNode))
                return new List<PassiveNodeViewModel>();
            var reachableSkilled = SelectNodesReachableFromStart(SkilledNodes);
            var adjacent = GetAvailableNodes(reachableSkilled);
            if (adjacent.Contains(targetNode))
                return new List<PassiveNodeViewModel> { targetNode };

            var visited = new HashSet<PassiveNodeViewModel>(reachableSkilled);
            var distance = new Dictionary<PassiveNodeViewModel, int>();
            var parents = new Dictionary<PassiveNodeViewModel, PassiveNodeViewModel>();
            var newOnes = new Queue<PassiveNodeViewModel>();
            var toOmit = new HashSet<ushort>(
                         from entry in _nodeHighlighter.NodeHighlights
                         where entry.Value.HasFlag(HighlightState.Crossed)
                         select entry.Key.Id);

            foreach (var node in adjacent)
            {
                if (toOmit.Contains(node.Id))
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
                foreach (var (id, connection) in newNode.NeighborPassiveNodes)
                {
                    if (toOmit.Contains(id))
                        continue;
                    if (visited.Contains(connection))
                        continue;
                    if (distance.ContainsKey(connection))
                        continue;
                    if (newNode.PassiveNodeType == PassiveNodeType.Mastery)
                        continue;
                    if (newNode.StartingCharacterClass.HasValue)
                        continue;
                    if (newNode.IsAscendantClassStartNode)
                        continue;
                    distance.Add(connection, dis + 1);
                    newOnes.Enqueue(connection);

                    parents.Add(connection, newNode);

                    if (connection == targetNode)
                    {
                        newOnes.Clear();
                        break;
                    }
                }
            }

            if (!distance.ContainsKey(targetNode))
                return new List<PassiveNodeViewModel>();

            var curr = targetNode;
            var result = new List<PassiveNodeViewModel> { curr };
            while (parents.ContainsKey(curr))
            {
                result.Add(parents[curr]);
                curr = parents[curr];
            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Changes the HighlightState of the node:
        /// None -> Checked -> Crossed -> None -> ...
        /// (preserves other HighlightStates than Checked and Crossed)
        /// </summary>
        /// <param name="node">Node to change the HighlightState for</param>
        public void CycleNodeTagForward(PassiveNodeViewModel node)
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
        public void CycleNodeTagBackward(PassiveNodeViewModel node)
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

        private static IEnumerable<PassiveNodeViewModel> SelectExistingNodesById(IEnumerable<ushort> nodeIds)
        {
            return
                from id in nodeIds
                where Skillnodes.ContainsKey(id)
                select Skillnodes[id];
        }

        public void SetCheckTaggedNodes(IReadOnlyList<PassiveNodeViewModel> checkTagged)
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
                            nd => (matchFct(nd.StatDescriptions, att => regex.IsMatch(att)) || regex.IsMatch(nd.Name)) &&
                                  (DrawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses || (nd.AscendancyName == null || nd.AscendancyName == AscendancyClassName)) : nd.AscendancyName == null));
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
                        nd => (matchFct(nd.StatDescriptions, att => att.ToLowerInvariant().Contains(search)) || nd.Name.ToLowerInvariant().Contains(search)) &&
                              (DrawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses || (nd.AscendancyName == null || nd.AscendancyName == AscendancyClassName)) : nd.AscendancyName == null));
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

        public string EncodeUrl() => SkillTreeUrlEncoders.TryEncode((byte)CharClass, (byte)AscType, SkilledNodes) ?? throw new Exception(L10n.Message("Could not encode skill tree url. Something is very wrong..."));
        public string EncodeUrl(SkillTreeUrlData data) => SkillTreeUrlEncoders.TryEncode((byte)data.CharacterClass, (byte)data.AscendancyClassId, Skillnodes.Values.Where(x => data.SkilledNodesIds.Contains(x.Id))) ?? throw new Exception(L10n.Message("Could not encode skill tree url. Something is very wrong..."));
        public SkillTreeUrlData DecodeUrl(string url) => SkillTreeUrlDecoders.TryDecode(url);

        public static void DecodeUrl(
            string url, out HashSet<PassiveNodeViewModel> skilledNodes, out CharacterClass charClass, ISkillTree skillTree)
            => DecodeUrlPrivate(url, out skilledNodes, out charClass, skillTree);

        private static SkillTreeUrlData DecodeUrl(string url, out HashSet<PassiveNodeViewModel> skilledNodes, ISkillTree skillTree)
            => DecodeUrlPrivate(url, out skilledNodes, out _, skillTree);

        private static SkillTreeUrlData DecodeUrlPrivate(
            string url, out HashSet<PassiveNodeViewModel> skilledNodes, out CharacterClass charClass, ISkillTree skillTree)
        {
            var buildData = skillTree.DecodeUrl(url);

            charClass = buildData.CharacterClass;
            var ascType = (byte)buildData.AscendancyClassId;

            PassiveNodeViewModel startnode = Skillnodes[RootNodeClassDictionary[charClass]];
            skilledNodes = new HashSet<PassiveNodeViewModel> { startnode };

            if (ascType > 0)
            {
                var ascendancyClass = skillTree.AscendancyClasses.GetAscendancyClassName(charClass, ascType);
                PassiveNodeViewModel ascNode = AscRootNodeList.First(nd => nd.AscendancyName == ascendancyClass);
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

        public void SwitchAscendancy(int ascClassID)
        {
            string? newAscClassName = AscendancyClasses.GetAscendancyClassName(CharClass, ascClassID);
            ushort ascNodeId = AscRootNodeList.FirstOrDefault(x => x.AscendancyName == newAscClassName)?.Id ?? 0;
            var sn = ascNodeId != 0 ?Skillnodes[ascNodeId]:null;
            //Remove all Ascendancy nodes not matching the new Ascendancy Type
            IEnumerable<PassiveNodeViewModel>? nodesToRemove = SkilledNodes.Where(x => x.IsAscendancyNode && x.AscendancyName != newAscClassName);
            
            SkilledNodes.Except(nodesToRemove);
            if(sn!=null&& !SkilledNodes.Contains(sn))
                SkilledNodes.Add(sn);
            _asctype = ascClassID;
        }

        public void RemoveAllAscendancyNodes()
        {
            //Remove all Ascendancy nodes
            IEnumerable<PassiveNodeViewModel>? nodesToRemove = SkilledNodes.Where(x => x.IsAscendancyNode || x.IsAscendancyStart);// || x.IsAscendantClassStartNode);
            SkilledNodes.Except(nodesToRemove);
            _asctype = 0;
        }

        public void RemoveAllAscendancySkillNodes()
        {
            //Remove all ascendancy point nodes
            IEnumerable<PassiveNodeViewModel>? nodesToRemove = SkilledNodes.Where(x => x.IsAscendancyNode && x.IsAscendancyStart==false && x.IsAscendantClassStartNode==false);
            SkilledNodes.Except(nodesToRemove);
        }

        public void LoadFromUrl(string url)
        {
            var data = DecodeUrl(url);
            var skillNodes = Skillnodes.Values.Where(x => data.SkilledNodesIds.Contains(x.Id));
#if PoESkillTree_UseLegacyClassCode
            ResetSkilledNodesTo(skillNodes);
            SwitchClass(data.CharacterClass);
#else
            ResetSkilledNodesWithClassInfo(skillNodes, data.CharacterClass, data.AscendancyClassId);
#endif
            foreach (var pair in data.MasteryEffectPairs)
            {
                if (SkilledNodes.FirstOrDefault(x => x.Id == pair.Id) is PassiveNodeViewModel mastery)
                {
                    mastery.Skill = pair.Effect;
                }
            }
        }

        public void Reset()
        {
            var prefs = _persistentData.Options.ResetPreferences;
            if (prefs.HasFlag(ResetPreferences.MainTree))
            {
                var rootNode = Skillnodes[RootNodeClassDictionary[CharClass]];
                if (prefs.HasFlag(ResetPreferences.AscendancyTree))
                {
                    SkilledNodes.ResetTo(new[] { rootNode });
                }
                else
                {
                    var ascNodes = SkilledNodes.Where(n => n.IsAscendancyNode).ToList();
                    SkilledNodes.ResetTo(ascNodes.Append(rootNode));
                }
            }
            if (prefs.HasFlag(ResetPreferences.AscendancyTree))
            {
                AscType = 0;
            }
            if (prefs.HasFlag(ResetPreferences.Bandits))
                _persistentData.CurrentBuild.Bandits.Reset();
            UpdateAscendancyClasses = true;
        }

        /// <summary>
        /// Returns all currently Check-tagged nodes.
        /// </summary>
        public HashSet<PassiveNodeViewModel> GetCheckedNodes()
        {
            var nodes = new HashSet<PassiveNodeViewModel>();
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
        public HashSet<PassiveNodeViewModel> GetCrossedNodes()
        {
            var nodes = new HashSet<PassiveNodeViewModel>();
            foreach (var entry in _nodeHighlighter.NodeHighlights)
            {
                if (!entry.Key.IsRootNode && entry.Value.HasFlag(HighlightState.Crossed))
                {
                    nodes.Add(entry.Key);
                }
            }
            return nodes;
        }

        private PassiveNodeViewModel GetCharNode()
            => Skillnodes[GetCharNodeId()];

        private ushort GetCharNodeId()
            => RootNodeClassDictionary[CharClass];

        private PassiveNodeViewModel? GetAscNode()
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
            return AscRootNodeList.FirstOrDefault(x => x.AscendancyName == AscendancyClassName)?.Id ?? 0;
        }

        private HashSet<PassiveNodeViewModel> GetAvailableNodes(IEnumerable<PassiveNodeViewModel> skilledNodes)
        {
            var availNodes = new HashSet<PassiveNodeViewModel>();

            foreach (var node in skilledNodes)
            {
                if (node.PassiveNodeType == PassiveNodeType.Mastery)
                {
                    continue;
                }

                foreach (var skillNode in node.NeighborPassiveNodes.Values)
                {
                    if (!skillNode.IsRootNode && !SkilledNodes.Contains(skillNode))
                        availNodes.Add(skillNode);
                }
            }
            return availNodes;
        }

        public static IEnumerable<KeyValuePair<string, IReadOnlyList<float>>> ExpandHybridAttributes(Dictionary<string, IReadOnlyList<float>> attributes)
        {
            return attributes.SelectMany(ExpandHybridAttributes);
        }

        private static IEnumerable<KeyValuePair<string, IReadOnlyList<float>>> ExpandHybridAttributes(KeyValuePair<string, IReadOnlyList<float>> attribute)
        {
            if (HybridAttributes.TryGetValue(attribute.Key, out var expandedAttributes))
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
            var classSpecificStartNodes = StartNodeDictionary
                .Where(kvp => kvp.Value == rootNodeValue)
                .Select(kvp => Skillnodes[kvp.Key])
                .Where(n => !n.IsAscendancyNode);
            return classSpecificStartNodes.Any(n => SkilledNodes.Contains(n));
        }

        #region ISkillTree members



        #endregion
    }
}
