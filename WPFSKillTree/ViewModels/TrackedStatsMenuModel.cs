using MoreLinq;
using Newtonsoft.Json.Linq;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.JsonSettings;
using POESKillTree.Model.Serialization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Model;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.Utils.Converter;
using POESKillTree.Utils.Extensions;
using POESKillTree.ViewModels.Builds;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace POESKillTree.ViewModels
{
    // Some aliases to make things clearer without the need of extra classes.
    using AttributeConstraint = TargetWeightConstraint<string>;
    using PseudoAttributeConstraint = TargetWeightConstraint<PseudoAttribute>;

    /// <summary>
    /// TrackedStatsMenuModel
    /// </summary>
    /// <seealso cref="POESKillTree.Common.ViewModels.CloseableViewModel" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanging" />
    /// <seealso cref="POESKillTree.Model.JsonSettings.ISetting" />
    public sealed class TrackedStatsMenuModel : SettingsMenuViewModel, INotifyPropertyChanged, INotifyPropertyChanging, ISetting
    {
        /// <summary>
        /// This is the same default buffer size as
        /// <see cref="StreamReader"/> and <see cref="FileStream"/>.
        /// </summary>
        private const int DefaultBufferSize = 4096;

        /// <summary>
        /// Indicates that
        /// 1. The file is to be used for asynchronous reading.
        /// 2. The file is to be accessed sequentially from beginning to end.
        /// </summary>
        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

        private readonly BuildsControlViewModel _buildsControlViewModel;

        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly IPersistentData _persistentData;

        private readonly PseudoAttributeLoader _pseudoAttributeLoader = new PseudoAttributeLoader();

        /// <summary>
        /// Gets all values of the WeaponClass Enum.
        /// </summary>
        public static IEnumerable<WeaponClass> WeaponClassValues
        {
            get { return Enum.GetValues(typeof(WeaponClass)).Cast<WeaponClass>(); }
        }

        /// <summary>
        /// Gets the change stat tracking path command.
        /// </summary>
        /// <value>
        /// The change stat tracking path command.
        /// </value>
        public ICommand ChangeStatTrackingPathCommand { get; }

        /// <summary>
        /// Gets the <see cref="IDialogCoordinator"/> used to display dialogs.
        /// </summary>
        public IDialogCoordinator DialogCoordinator
        {
            get { return _dialogCoordinator; }
            //set { SetProperty(ref _dialogCoordinator, value); }
        }

        /// <summary>
        /// Gets the load tracked stat file names command.
        /// </summary>
        /// <value>
        /// The load tracked stat file names command.
        /// </value>
        public ICommand LoadTrackedStatFileNamesCommand { get; }

        /// <summary>
        /// Gets the load tracked stats command.
        /// </summary>
        /// <value>
        /// The load tracked stats command.
        /// </value>
        public ICommand LoadTrackedStatsCommand { get; }

        /// <summary>
        /// Gets the TrackingOptions.
        /// </summary>
        /// <value>
        /// The TrackingOptions.
        /// </value>
        public TrackedStatOptions TrackingOptions { get; }

        /// <summary>
        /// The SkillTree instance to operate on.
        /// </summary>
        public SkillTree Tree { get; }
        private string Key { get; } = "Tracked Stats Menu";

        private IReadOnlyList<ISetting> SubSettings { get; }

        /// <summary>
        /// Instantiates a new TrackedStatsMenuModel.
        /// </summary>
        /// <param name="persistentData">The persistent data.</param>
        /// <param name="dialogCoordinator">The <see cref="IDialogCoordinator" /> used to display dialogs.</param>
        /// <param name="buildsControlViewModel">The builds control view model.</param>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        public TrackedStatsMenuModel(IPersistentData persistentData, IDialogCoordinator dialogCoordinator, BuildsControlViewModel buildsControlViewModel, SkillTree tree) : base(persistentData, dialogCoordinator, buildsControlViewModel)
        {
            DisplayName = L10n.Message("Tracked Stat Settings");
            _persistentData = persistentData;
            _dialogCoordinator = dialogCoordinator;
            _buildsControlViewModel = buildsControlViewModel;

            TrackingOptions = persistentData.TrackedStatOptions;

            ChangeStatTrackingPathCommand = new AsyncRelayCommand(ChangeStatTrackingPath);
            LoadTrackedStatFileNamesCommand = new AsyncRelayCommand(LoadTrackedStatFileNames);
            LoadTrackedStatsCommand = new AsyncRelayCommand(LoadTrackedStats);

            //Options.PropertyChanged += TrackingOptionsOnPropertyChanged;

            _attributes = CreatePossibleAttributes().ToList();
            AttributesView = new ListCollectionView(_attributes)
            {
                Filter = item => !_addedAttributes.Contains(item),
                //CustomSort = Comparer<string>.Create((s1, s2) =>
                //{
                //    // Sort by group as in AttrGroupOrder first and then by name.
                //    var groupCompare = AttrGroupOrder[AttrToGroupConverter.Convert(s1)].CompareTo(
                //        AttrGroupOrder[AttrToGroupConverter.Convert(s2)]);
                //    return groupCompare != 0 ? groupCompare : string.CompareOrdinal(s1, s2);
                //})
            };
            AttributesView.GroupDescriptions.Add(new PropertyGroupDescription(".", AttrToGroupConverter));
            AttributesView.MoveCurrentToFirst();
            AttributeConstraints = new ObservableCollection<AttributeConstraint>();
            NewAttributeConstraint = new AttributeConstraint(AttributesView.CurrentItem as string);

            PseudoAttributesView = new ListCollectionView(_pseudoAttributes)
            {
                Filter = item => !_addedPseudoAttributes.Contains((PseudoAttribute)item)
            };
            PseudoAttributesView.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            PseudoAttributesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            PseudoAttributesView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));

            PseudoAttributeConstraints = new ObservableCollection<PseudoAttributeConstraint>();

            ReloadPseudoAttributes();

            SubSettings = new ISetting[]
            {
                new ConstraintsSetting(this)
            };
        }

        /// <summary>
        /// Converts attributes to their groups. Similar to <see cref="GroupStringConverter"/>
        /// except that it additionally groups attributes in <see cref="PopularAttributes"/> together
        /// and caches the calculations to a dictionary.
        /// </summary>
        private class AttributeToGroupConverter : IValueConverter
        {
            private static readonly GroupStringConverter GroupStringConverter = new GroupStringConverter();

            private readonly Dictionary<string, string> _attributeToGroupDictionary = new Dictionary<string, string>();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Convert(value.ToString());
            }

            public string Convert(string attrName)
            {
                string groupName;
                if (!_attributeToGroupDictionary.TryGetValue(attrName, out groupName))
                {
                    groupName = PopularAttributes.Contains(attrName)
                        ? PopularGroupName
                        : GroupStringConverter.Convert(attrName).GroupName;
                    _attributeToGroupDictionary[attrName] = groupName;
                }
                return groupName;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Setting that converts between JSON and AttributeConstraints/PseudoAttributeConstraints.
        /// </summary>
        private class ConstraintsSetting : ISetting
        {
            private const string AttributeKey = "Attribute";
            private const string TargetValueKey = "TargetValue";
            private const string WeightKey = "Weight";

            private readonly TrackedStatsMenuModel _vm;

            public ConstraintsSetting(TrackedStatsMenuModel vm)
            {
                _vm = vm;
            }

            public void LoadFrom(JObject jObject)
            {
                JToken token;
                _vm.ClearAttributeConstraints();
                if (jObject.TryGetValue(nameof(AttributeConstraints), out token) && token.Any())
                {
                    var newConstraints = new List<AttributeConstraint>();
                    foreach (var element in token)
                    {
                        var obj = element as JObject;
                        if (obj == null)
                            continue;
                        JToken attrToken, targetToken, weightToken;
                        if (!obj.TryGetValue(AttributeKey, out attrToken)
                            || !obj.TryGetValue(TargetValueKey, out targetToken)
                            || !obj.TryGetValue(WeightKey, out weightToken))
                            continue;

                        var attr = attrToken.ToObject<string>();
                        newConstraints.Add(new AttributeConstraint(attr)
                        {
                            TargetValue = targetToken.ToObject<float>(),
                            Weight = weightToken.ToObject<int>()
                        });
                        _vm._addedAttributes.Add(attr);
                    }

                    _vm.AttributesView.Refresh();
                    _vm.AttributesView.MoveCurrentToFirst();
                    _vm.NewAttributeConstraint.Data = _vm.AttributesView.CurrentItem as string;
                    _vm.AttributeConstraints.AddRange(newConstraints);
                }

                _vm.ClearPseudoAttributeConstraints();
                if (jObject.TryGetValue(nameof(PseudoAttributeConstraints), out token) && token.Any())
                {
                    var pseudoDict = _vm._pseudoAttributes.ToDictionary(p => p.Name);

                    var newConstraints = new List<PseudoAttributeConstraint>();
                    foreach (var element in token)
                    {
                        var obj = element as JObject;
                        if (obj == null)
                            continue;
                        JToken attrToken, targetToken, weightToken;
                        if (!obj.TryGetValue(AttributeKey, out attrToken)
                            || !obj.TryGetValue(TargetValueKey, out targetToken)
                            || !obj.TryGetValue(WeightKey, out weightToken))
                            continue;

                        PseudoAttribute attr;
                        if (!pseudoDict.TryGetValue(attrToken.ToObject<string>(), out attr))
                            continue;
                        newConstraints.Add(new PseudoAttributeConstraint(attr)
                        {
                            TargetValue = targetToken.ToObject<float>(),
                            Weight = weightToken.ToObject<int>()
                        });
                        _vm._addedPseudoAttributes.Add(attr);
                    }

                    _vm.PseudoAttributesView.Refresh();
                    _vm.PseudoAttributesView.MoveCurrentToFirst();
                    _vm.NewPseudoAttributeConstraint.Data = _vm.PseudoAttributesView.CurrentItem as PseudoAttribute;
                    _vm.PseudoAttributeConstraints.AddRange(newConstraints);
                }
            }

            public void Reset()
            {
                _vm.ClearAttributeConstraints();
                _vm.ClearPseudoAttributeConstraints();
            }

            public bool SaveTo(JObject jObject)
            {
                var changed = false;
                var attrArray = new JArray();
                _vm.AttributeConstraints.ForEach(c => AddTo(attrArray, c.Data, c.TargetValue, c.Weight));
                JToken oldToken;
                if (jObject.TryGetValue(nameof(AttributeConstraints), out oldToken))
                {
                    changed = !JToken.DeepEquals(attrArray, oldToken);
                }
                jObject[nameof(AttributeConstraints)] = attrArray;

                var pseudoArray = new JArray();
                _vm.PseudoAttributeConstraints.ForEach(c => AddTo(pseudoArray, c.Data.Name, c.TargetValue, c.Weight));
                if (!changed && jObject.TryGetValue(nameof(PseudoAttributeConstraints), out oldToken)
                    && !JToken.DeepEquals(pseudoArray, oldToken))
                {
                    changed = true;
                }
                jObject[nameof(PseudoAttributeConstraints)] = pseudoArray;
                return changed;
            }

            private static void AddTo(JArray array, string attribute, float targetValue, int weight)
            {
                array.Add(new JObject
                {
                    {AttributeKey, new JValue(attribute)},
                    {TargetValueKey, new JValue(targetValue)},
                    {WeightKey, new JValue(weight)}
                });
            }
        }
        public static Task<string[]> ReadAllLinesAsync(string path)
        {
            return ReadAllLinesAsync(path, Encoding.UTF8);
        }

        public static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding)
        {
            var lines = new List<string>();

            // Open the FileStream with the same FileMode, FileAccess
            // and FileShare as a call to File.OpenText would've done.
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }

        /// <summary>
        /// Loads this component's values from <paramref name="jObject" />.
        /// </summary>
        /// <param name="jObject"></param>
        public void LoadFrom(JObject jObject)
        {
            JToken token;
            if (!jObject.TryGetValue(Key, out token) || !(token is JObject))
            {
                Reset();
                return;
            }
            SubSettings.ForEach(s => s.LoadFrom((JObject)token));
        }

        /// <summary>
        /// Resets this component's values to their default values.
        /// </summary>
        public void Reset()
        {
            SubSettings.ForEach(s => s.Reset());
        }

        /// <summary>
        /// Saves this component's values to <paramref name="jObject" />.
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns>
        /// True iff this operation changed <paramref name="jObject" />
        /// </returns>
        public bool SaveTo(JObject jObject)
        {
            JToken token;
            if (!jObject.TryGetValue(Key, out token) || !(token is JObject))
            {
                jObject[Key] = token = new JObject();
            }
            if (!SubSettings.Any())
                return false;
            var obj = (JObject)token;
            var changed = false;
            foreach (var s in SubSettings)
            {
                if (s.SaveTo(obj))
                    changed = true;
            }
            return changed;
        }
        protected override void OnClose()
        {
            //Options.PropertyChanged -= TrackingOptionsOnPropertyChanged;
            _persistentData.Save();
        }

        //private async void TrackingOptionsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        //{
        //    string propertyName = args.PropertyName;
        //    //if (propertyName == nameof(Options.Language) ||
        //    //    (propertyName == nameof(Options.DownloadMissingItemImages) && !Options.DownloadMissingItemImages))
        //    //{
        //    //    await _dialogCoordinator.ShowInfoAsync(this,
        //    //        L10n.Message("You will need to restart the program for all changes to take effect."),
        //    //        title: L10n.Message("Restart is needed"));

        //    //    if (propertyName == nameof(Options.Language))
        //    //        L10n.Initialize(Options.Language);
        //    //}
        //}

        /// <summary>
        /// Creates possible attributes from the SkillTree nodes.
        /// Unique and blacklisted attributes are not taken.
        /// Attributes of ascendancy nodes are ignored.
        /// Attributes must have at least one '#' in their name (which means they have a value).
        /// </summary>
        private static IEnumerable<string> CreatePossibleAttributes()
        {
            return from node in SkillTree.Skillnodes.Values
                   where node.ascendancyName == null
                   from attr in SkillTree.ExpandHybridAttributes(node.Attributes)
                   where attr.Key.Contains("#")
                   group attr by attr.Key into attrGroup
                   where attrGroup.Count() > 1
                   select attrGroup.First().Key;
        }

        /// <summary>
        /// Changes the stat tracking path.
        /// </summary>
        /// <returns></returns>
        private async Task ChangeStatTrackingPath()
        {
            var dialogSettings = new FileSelectorDialogSettings
            {
                DefaultPath = TrackingOptions.StatTrackingSavePath,
                IsFolderPicker = true,
                ValidationSubPath = SerializationConstants.EncodedDefaultBuildName
            };
            var path = await _dialogCoordinator.ShowFileSelectorAsync(this,
                L10n.Message("Select TrackedStat directory"),
                L10n.Message("Select the directory where builds will be stored.\n" +
                             "It will be created if it does not yet exist."),
                dialogSettings);
            if (path == null)
                return;

            TrackingOptions.StatTrackingSavePath = path;
        }

        private void ClearAttributeConstraints()
        {
            _addedAttributes.Clear();
            AttributeConstraints.Clear();
            AttributesView.Refresh();
            AttributesView.MoveCurrentToFirst();
            NewAttributeConstraint = new AttributeConstraint(AttributesView.CurrentItem as string);
        }

        private void ClearPseudoAttributeConstraints()
        {
            _addedPseudoAttributes.Clear();
            PseudoAttributeConstraints.Clear();
            PseudoAttributesView.Refresh();
            PseudoAttributesView.MoveCurrentToFirst();
            NewPseudoAttributeConstraint =
                new PseudoAttributeConstraint(PseudoAttributesView.CurrentItem as PseudoAttribute);
        }

        /// <summary>
        /// Creates the attributes the skill tree has with these settings initially
        /// (without any tree generating done).
        /// </summary>
        private Dictionary<string, float> CreateInitialAttributes()
        {
            // base attributes: SkillTree.BaseAttributes, SkillTree.CharBaseAttributes
            var stats = new Dictionary<string, float>(SkillTree.BaseAttributes);
            foreach (var attr in SkillTree.CharBaseAttributes[Tree.Chartype])
            {
                stats[attr.Key] = attr.Value;
            }
            return stats;
        }

        /// <summary>
        /// Loads the tracked stat file names.
        /// </summary>
        /// <returns></returns>
        private async Task LoadTrackedStatFileNames()
        {
            if (TrackingOptions.StatTrackingSavePath != "")
            {//string[] FilesInPath = System.IO.Directory.GetFiles(Options.StatTrackingSavePath);
             //SLightly async version of Getting files from http://writeasync.net/?p=2621
             // Avoid blocking the caller for the initial enumerate call.
                await Task.Yield();
                List<string> FileList = new List<string>();

                foreach (string file in Directory.EnumerateFiles(TrackingOptions.StatTrackingSavePath))
                {
                    FileList.Add(file);
                }

                TrackingOptions.TrackingFileList = FileList.ToArray();
            }
            else
            {
                await _dialogCoordinator.ShowInfoAsync(this,
                    L10n.Message("Select Tracked FilePath first."),
                    title: L10n.Message("Tracked Stats FilePath needed inputed first"));
                return;
            }
        }

        /// <summary>
        /// Loads the tracked stats.
        /// </summary>
        /// <returns></returns>
        private async Task LoadTrackedStats()
        {
            if (TrackingOptions.StatTrackingSavePath != "" && GlobalSettings.CurrentTrackedFileName != "")
            {
                string TargetFile = TrackingOptions.StatTrackingSavePath + GlobalSettings.CurrentTrackedFileName;
                string[] TrackedAttributeNames = await ReadAllLinesAsync(TrackingOptions.StatTrackingSavePath + GlobalSettings.CurrentTrackedFileName);

                foreach (PseudoAttribute item in PseudoAttributesView)
                {
                    if (TrackedAttributeNames.Contains(item.Name) && GlobalSettings.TrackedStats.GetIndexOfAttribute(item.Name) == -1)
                    {
                        GlobalSettings.TrackedStats.Add(item);
                    }
                }
            }
        }

        #region Attribute constants

        /// <summary>
        /// Order in which the attribute groups are shown.
        /// </summary>
        private static readonly Dictionary<string, int> AttrGroupOrder = new Dictionary<string, int>()
        {
            {PopularGroupName, -1},
            // General
            {L10n.Message("Core Attributes"), 0},
            {L10n.Message("General"), 1},
            {L10n.Message("Keystone"), 2},
            {L10n.Message("Charges"), 3},
            // Defense
            {L10n.Message("Defense"), 4},
            {L10n.Message("Block"), 5},
            {L10n.Message("Shield"), 6},
            // Offense
            {L10n.Message("Weapon"), 7},
            {L10n.Message("Spell"), 8},
            {L10n.Message("Critical Strike"), 9},
            // Alternate Spell groups
            {L10n.Message("Aura"), 10},
            {L10n.Message("Curse"), 11},
            {L10n.Message("Minion"), 12},
            {L10n.Message("Trap"), 13},
            {L10n.Message("Totem"), 14},
            {L10n.Message("Flasks"), 15 },
            {L10n.Message("Jewel Types"), 16},
            {L10n.Message("Tracked PseudoTotals"), 17},
            {L10n.Message("Everything Else"), 18},
            {L10n.Message("Hidden"), 19}
        };

        /// <summary>
        /// Converts attribute strings to their group names.
        /// </summary>
        private static readonly AttributeToGroupConverter AttrToGroupConverter = new AttributeToGroupConverter();

        /// <summary>
        /// List of attributes that should be displayed before others.
        /// </summary>
        private static readonly HashSet<string> PopularAttributes = new HashSet<string>()
        {
            "+# to Dexterity", "+# to Intelligence", "+# to Strength",
            "#% increased Movement Speed", "#% increased maximum Life", "#% of Life Regenerated per Second",
            "#% of Physical Attack Damage Leeched as Mana",
            "#% increased effect of Auras you Cast", "#% reduced Mana Reserved",
            "+# Jewel Socket", "+# Str Based Jewel", "+# Int Based Jewel", "+# Dex Based Jewel"
        };

        private static readonly string PopularGroupName = L10n.Message("Popular");
        #endregion Attribute constants
        #region Presentation

        private readonly HashSet<string> _addedAttributes = new HashSet<string>();

        /// <summary>
        /// HashSet of PseudoAttributes already added as PseudoAttributeConstraint.
        /// </summary>
        private readonly HashSet<PseudoAttribute> _addedPseudoAttributes = new HashSet<PseudoAttribute>();

        /// <summary>
        /// The collection of attributes that can be used in AttributeConstraints.
        /// </summary>
        private readonly List<string> _attributes;

        /// <summary>
        /// Collection of pseudo attributes that can be used in PseudoAttributeConstraints.
        /// </summary>
        private readonly ObservableCollection<PseudoAttribute> _pseudoAttributes = new ObservableCollection<PseudoAttribute>();

        private AttributeConstraint _newAttributeConstraint;

        /// <summary>
        /// Placeholder for the PseudoAttributeConstraint the user is editing that can be added.
        /// </summary>
        private PseudoAttributeConstraint _newPseudoAttributeConstraint;

        public HashSet<PseudoAttribute> AddedPseudoAttributes
        {
            get { return _addedPseudoAttributes; }
        }

        /// <summary>
        /// Gets the collection of AttributeConstraints the user specified.
        /// </summary>
        public ObservableCollection<AttributeConstraint> AttributeConstraints { get; }

        /// <summary>
        /// Gets the CollectionView to the attribute names the user can use.
        /// </summary>
        public ICollectionView AttributesView { get; }
        /// <summary>
        /// Gets the AttributeConstraint used for creating new AttributeConstraints by the user.
        /// </summary>
        public AttributeConstraint NewAttributeConstraint
        {
            get { return _newAttributeConstraint; }
            private set { SetProperty(ref _newAttributeConstraint, value); }
        }
        /// <summary>
        /// Gets the PseudoAttributeConstraint used for creating new ones by the user.
        /// </summary>
        public PseudoAttributeConstraint NewPseudoAttributeConstraint
        {
            get { return _newPseudoAttributeConstraint; }
            private set { SetProperty(ref _newPseudoAttributeConstraint, value); }
        }

        /// <summary>
        /// Gets the collection of PseudoAttributeConstraints the user specified.
        /// </summary>
        public ObservableCollection<PseudoAttributeConstraint> PseudoAttributeConstraints { get; }

        public ObservableCollection<PseudoAttribute> PseudoAttributes
        {
            get { return _pseudoAttributes; }
        }

        /// <summary>
        /// Gets the CollectionView to the PseudoAttributes the user can use.
        /// </summary>
        public ICollectionView PseudoAttributesView { get; }
        #endregion Presentation

        #region Commands

        private RelayCommand _addAttributeConstraintCommand;
        private RelayCommand _addPseudoConstraintCommand;
        private RelayCommand _reloadPseudoAttributesCommand;
        private ICommand _removeAttributeConstraintCommand;
        private ICommand _removePseudoConstraintCommand;
        private RelayCommand _resetCommand;

        /// <summary>
        /// Gets the command to add an AttributeConstraint to the collection.
        /// </summary>
        public ICommand AddAttributeConstraintCommand
        {
            get
            {
                return _addAttributeConstraintCommand ?? (_addAttributeConstraintCommand = new RelayCommand(
                    () =>
                    {
                        var newConstraint = (AttributeConstraint)NewAttributeConstraint.Clone();
                        _addedAttributes.Add(newConstraint.Data);
                        AttributesView.Refresh();

                        AttributesView.MoveCurrentToFirst();
                        NewAttributeConstraint.Data = AttributesView.CurrentItem as string;
                        AttributeConstraints.Add(newConstraint);
                    },
                    () => _addedAttributes.Count < _attributes.Count));
            }
        }

        /// <summary>
        /// Gets the command to add a PseudoAttributeConstraint to the collection.
        /// </summary>
        public ICommand AddPseudoConstraintCommand
        {
            get
            {
                return _addPseudoConstraintCommand ?? (_addPseudoConstraintCommand = new RelayCommand(
                    () =>
                    {
                        var newConstraint = (PseudoAttributeConstraint)NewPseudoAttributeConstraint.Clone();
                        _addedPseudoAttributes.Add(newConstraint.Data);
                        PseudoAttributesView.Refresh();

                        PseudoAttributesView.MoveCurrentToFirst();
                        NewPseudoAttributeConstraint.Data = PseudoAttributesView.CurrentItem as PseudoAttribute;
                        PseudoAttributeConstraints.Add(newConstraint);
                    },
                    () => _addedPseudoAttributes.Count < _pseudoAttributes.Count));
            }
        }

        /// <summary>
        /// Gets the command to reload the possible PseudoAttributes from the filesystem.
        /// Removes all user specified PseudoAttributeConstraints.
        /// </summary>
        public ICommand ReloadPseudoAttributesCommand
        {
            get
            {
                return _reloadPseudoAttributesCommand ??
                       (_reloadPseudoAttributesCommand = new RelayCommand(ReloadPseudoAttributes));
            }
        }

        /// <summary>
        /// Gets the command to remove an AttributeConstraint from the collection.
        /// </summary>
        public ICommand RemoveAttributeConstraintCommand
        {
            get
            {
                return _removeAttributeConstraintCommand ?? (_removeAttributeConstraintCommand = new RelayCommand<AttributeConstraint>(
                    param =>
                    {
                        var oldConstraint = param;
                        _addedAttributes.Remove(oldConstraint.Data);
                        AttributesView.Refresh();

                        NewAttributeConstraint = oldConstraint;
                        AttributeConstraints.Remove(oldConstraint);
                    }));
            }
        }

        /// <summary>
        /// Gets the command to remove a PseudoAttributeConstraint from the collection.
        /// </summary>
        public ICommand RemovePseudoConstraintCommand
        {
            get
            {
                return _removePseudoConstraintCommand ?? (_removePseudoConstraintCommand = new RelayCommand<PseudoAttributeConstraint>(
                    param =>
                    {
                        var oldConstraint = param;
                        _addedPseudoAttributes.Remove(oldConstraint.Data);
                        PseudoAttributesView.Refresh();

                        NewPseudoAttributeConstraint = oldConstraint;
                        PseudoAttributeConstraints.Remove(oldConstraint);
                    }));
            }
        }

        /// <summary>
        /// Resets all Properties to the values they had on construction.
        /// Calls <see cref="GeneratorTabViewModel.Reset"/> on all tabs.
        /// </summary>
        public ICommand ResetCommand
        {
            get { return _resetCommand ?? (_resetCommand = new RelayCommand(Reset)); }
        }
        #endregion Commands
        /// <summary>
        /// Reloads the possible PseudoAttributes from the filesystem.
        /// Resets PseudoAttributeConstraints entered by the user.
        /// </summary>
        private void ReloadPseudoAttributes()
        {
            _addedPseudoAttributes.Clear();
            _pseudoAttributes.Clear();
            foreach (var pseudo in _pseudoAttributeLoader.LoadPseudoAttributes())
            {
                _pseudoAttributes.Add(pseudo);
            }
            PseudoAttributeConstraints.Clear();
            PseudoAttributesView.MoveCurrentToFirst();
            NewPseudoAttributeConstraint = new PseudoAttributeConstraint(PseudoAttributesView.CurrentItem as PseudoAttribute);
        }
    }
}