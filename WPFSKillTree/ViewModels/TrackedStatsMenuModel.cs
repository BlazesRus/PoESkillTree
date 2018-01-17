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

    public class TrackedStatsMenuModel : CloseableViewModel, INotifyPropertyChanged, INotifyPropertyChanging
    {
        private static class AsyncFileCommands
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
        }

        private readonly IPersistentData _persistentData;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly BuildsControlViewModel _buildsControlViewModel;

        public Options Options { get; }
        public TrackedStatOptions TrackedStatOptions { get; }

        //private readonly PseudoAttributeLoader _pseudoAttributeLoader = new PseudoAttributeLoader();

        /// <summary>
        /// The SkillTree instance to operate on.
        /// </summary>
        public SkillTree Tree { get; }

        //private string Key { get; } = "Tracked Stats Menu";
        //private IReadOnlyList<ISetting> SubSettings { get; }

        #region ICommand Initialization

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
        /// Gets the change stat tracking path command.
        /// </summary>
        /// <value>
        /// The change stat tracking path command.
        /// </value>
        public ICommand ChangeStatTrackingPathCommand { get; }

        #endregion ICommand Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedStatsMenuModel"/> class.
        /// </summary>
        /// <param name="persistentData">The persistent data.</param>
        /// <param name="dialogCoordinator">The dialog coordinator.</param>
        /// <param name="buildsControlViewModel">The builds control view model.</param>
        /// <param name="tree">The tree.</param>
        public TrackedStatsMenuModel(IPersistentData persistentData, IDialogCoordinator dialogCoordinator, BuildsControlViewModel buildsControlViewModel, SkillTree tree)
        {
            _persistentData = persistentData;
            _dialogCoordinator = dialogCoordinator;
            _buildsControlViewModel = buildsControlViewModel;
            Options = persistentData.Options;
            TrackedStatOptions = persistentData.TrackedStatOptions;
            Tree = tree;
            DisplayName = L10n.Message("Tracked Stat Settings");

            ChangeStatTrackingPathCommand = new AsyncRelayCommand(ChangeStatTrackingPath);
            LoadTrackedStatFileNamesCommand = new AsyncRelayCommand(LoadTrackedStatFileNames);
            LoadTrackedStatsCommand = new AsyncRelayCommand(LoadTrackedStats);

            Options.PropertyChanged += OptionsOnPropertyChanged;

            //_attributes = CreatePossibleAttributes().ToList();
            //AttributesView = new ListCollectionView(_attributes)
            //{
            //    Filter = item => !_addedAttributes.Contains(item),
            //};
            //AttributesView.GroupDescriptions.Add(new PropertyGroupDescription(".", AttrToGroupConverter));
            //AttributesView.MoveCurrentToFirst();
            //AttributeConstraints = new ObservableCollection<AttributeConstraint>();
            //NewAttributeConstraint = new AttributeConstraint(AttributesView.CurrentItem as string);

            //PseudoAttributesView = new ListCollectionView(_pseudoAttributes)
            //{
            //    Filter = item => !_addedPseudoAttributes.Contains((PseudoAttribute)item)
            //};
            //PseudoAttributesView.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            //PseudoAttributesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            //PseudoAttributesView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));

            //PseudoAttributeConstraints = new ObservableCollection<PseudoAttributeConstraint>();

            //ReloadPseudoAttributes();

            ////SubSettings = new ISetting[]
            ////{
            ////    new ConstraintsSetting(this)
            ////};
        }

        ///// <summary>
        ///// Setting that converts between JSON and AttributeConstraints/PseudoAttributeConstraints.
        ///// </summary>
        //private class ConstraintsSetting : ISetting
        //{
        //    private const string AttributeKey = "Attribute";
        //    private const string TargetValueKey = "TargetValue";
        //    private const string WeightKey = "Weight";

        //    private readonly TrackedStatsMenuModel _vm;

        //    public ConstraintsSetting(TrackedStatsMenuModel vm)
        //    {
        //        _vm = vm;
        //    }

        //    public void LoadFrom(JObject jObject)
        //    {
        //        JToken token;
        //        _vm.ClearAttributeConstraints();
        //        if (jObject.TryGetValue(nameof(AttributeConstraints), out token) && token.Any())
        //        {
        //            var newConstraints = new List<AttributeConstraint>();
        //            foreach (var element in token)
        //            {
        //                var obj = element as JObject;
        //                if (obj == null)
        //                    continue;
        //                JToken attrToken, targetToken, weightToken;
        //                if (!obj.TryGetValue(AttributeKey, out attrToken)
        //                    || !obj.TryGetValue(TargetValueKey, out targetToken)
        //                    || !obj.TryGetValue(WeightKey, out weightToken))
        //                    continue;

        //                var attr = attrToken.ToObject<string>();
        //                newConstraints.Add(new AttributeConstraint(attr)
        //                {
        //                    TargetValue = targetToken.ToObject<float>(),
        //                    Weight = weightToken.ToObject<int>()
        //                });
        //                _vm._addedAttributes.Add(attr);
        //            }

        //            _vm.AttributesView.Refresh();
        //            _vm.AttributesView.MoveCurrentToFirst();
        //            _vm.NewAttributeConstraint.Data = _vm.AttributesView.CurrentItem as string;
        //            _vm.AttributeConstraints.AddRange(newConstraints);
        //        }

        //        _vm.ClearPseudoAttributeConstraints();
        //        if (jObject.TryGetValue(nameof(PseudoAttributeConstraints), out token) && token.Any())
        //        {
        //            var pseudoDict = _vm._pseudoAttributes.ToDictionary(p => p.Name);

        //            var newConstraints = new List<PseudoAttributeConstraint>();
        //            foreach (var element in token)
        //            {
        //                var obj = element as JObject;
        //                if (obj == null)
        //                    continue;
        //                JToken attrToken, targetToken, weightToken;
        //                if (!obj.TryGetValue(AttributeKey, out attrToken)
        //                    || !obj.TryGetValue(TargetValueKey, out targetToken)
        //                    || !obj.TryGetValue(WeightKey, out weightToken))
        //                    continue;

        //                PseudoAttribute attr;
        //                if (!pseudoDict.TryGetValue(attrToken.ToObject<string>(), out attr))
        //                    continue;
        //                newConstraints.Add(new PseudoAttributeConstraint(attr)
        //                {
        //                    TargetValue = targetToken.ToObject<float>(),
        //                    Weight = weightToken.ToObject<int>()
        //                });
        //                _vm._addedPseudoAttributes.Add(attr);
        //            }

        //            _vm.PseudoAttributesView.Refresh();
        //            _vm.PseudoAttributesView.MoveCurrentToFirst();
        //            _vm.NewPseudoAttributeConstraint.Data = _vm.PseudoAttributesView.CurrentItem as PseudoAttribute;
        //            _vm.PseudoAttributeConstraints.AddRange(newConstraints);
        //        }
        //    }

        //    public bool SaveTo(JObject jObject)
        //    {
        //        var changed = false;
        //        var attrArray = new JArray();
        //        _vm.AttributeConstraints.ForEach(c => AddTo(attrArray, c.Data, c.TargetValue, c.Weight));
        //        JToken oldToken;
        //        if (jObject.TryGetValue(nameof(AttributeConstraints), out oldToken))
        //        {
        //            changed = !JToken.DeepEquals(attrArray, oldToken);
        //        }
        //        jObject[nameof(AttributeConstraints)] = attrArray;

        //        var pseudoArray = new JArray();
        //        _vm.PseudoAttributeConstraints.ForEach(c => AddTo(pseudoArray, c.Data.Name, c.TargetValue, c.Weight));
        //        if (!changed && jObject.TryGetValue(nameof(PseudoAttributeConstraints), out oldToken)
        //            && !JToken.DeepEquals(pseudoArray, oldToken))
        //        {
        //            changed = true;
        //        }
        //        jObject[nameof(PseudoAttributeConstraints)] = pseudoArray;
        //        return changed;
        //    }

        //    private static void AddTo(JArray array, string attribute, float targetValue, int weight)
        //    {
        //        array.Add(new JObject
        //        {
        //            {AttributeKey, new JValue(attribute)},
        //            {TargetValueKey, new JValue(targetValue)},
        //            {WeightKey, new JValue(weight)}
        //        });
        //    }

        //    public void Reset()
        //    {
        //        _vm.ClearAttributeConstraints();
        //        _vm.ClearPseudoAttributeConstraints();
        //    }
        //}


        protected override void OnClose()
        {
            Options.PropertyChanged -= OptionsOnPropertyChanged;
            _persistentData.Save();
        }

        private async void OptionsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            //            string propertyName = args.PropertyName;
            //            if (propertyName == nameof(Options.Language) ||
            //                (propertyName == nameof(Options.DownloadMissingItemImages) && !Options.DownloadMissingItemImages))
            //            {
            //                await _dialogCoordinator.ShowInfoAsync(this,
            //                    L10n.Message("You will need to restart the program for all changes to take effect."),
            //                    title: L10n.Message("Restart is needed"));
            //
            //                if (propertyName == nameof(Options.Language))
            //                    L10n.Initialize(Options.Language);
            //            }
        }

        #region TrackingCommand Code

        /// <summary>
        /// Changes the stat tracking path.
        /// </summary>
        /// <returns></returns>
        private async Task ChangeStatTrackingPath()
        {
            var dialogSettings = new FileSelectorDialogSettings
            {
                DefaultPath = TrackedStatOptions.StatTrackingSavePath,
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

            TrackedStatOptions.StatTrackingSavePath = path;
        }

        /// <summary>
        /// Loads the tracked stat file names.
        /// </summary>
        /// <returns></returns>
        private async Task LoadTrackedStatFileNames()
        {
            await TrackedStatOptions.LoadTrackedStatFileNames();
        }

        /// <summary>
        /// Loads the tracked stats.
        /// </summary>
        /// <returns></returns>
        private async Task LoadTrackedStats()
        {
            if (TrackedStatOptions.StatTrackingSavePath != "" && GlobalSettings.CurrentTrackedFileName != "")
            {
                string TargetFile = TrackedStatOptions.StatTrackingSavePath + GlobalSettings.CurrentTrackedFileName;
                string[] TrackedAttributeNames = await AsyncFileCommands.ReadAllLinesAsync(TrackedStatOptions.StatTrackingSavePath + GlobalSettings.CurrentTrackedFileName);

                //foreach (PseudoAttribute item in PseudoAttributesView)
                //{
                //    if (TrackedAttributeNames.Any(s => TargetFile.Contains(s)) && GlobalSettings.TrackedStats.GetIndexOfAttribute(item.Name) == -1)
                //    {
                //        GlobalSettings.TrackedStats.Add(item);
                //    }
                //}
            }
        }

        #endregion TrackingCommand Code

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

        #region Attribute constants

        private static readonly string PopularGroupName = L10n.Message("Popular");

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

        /// <summary>
        /// Converts attribute strings to their group names.
        /// </summary>
        private static readonly AttributeToGroupConverter AttrToGroupConverter = new AttributeToGroupConverter();

        #endregion Attribute constants
        #region Presentation

        private readonly HashSet<string> _addedAttributes = new HashSet<string>();

        /// <summary>
        /// The collection of attributes that can be used in AttributeConstraints.
        /// </summary>
        private readonly List<string> _attributes;

        private AttributeConstraint _newAttributeConstraint;

        /// <summary>
        /// Gets the AttributeConstraint used for creating new AttributeConstraints by the user.
        /// </summary>
        public AttributeConstraint NewAttributeConstraint
        {
            get { return _newAttributeConstraint; }
            private set { SetProperty(ref _newAttributeConstraint, value); }
        }

        //    /// <summary>
        //    /// Gets the CollectionView to the attribute names the user can use.
        //    /// </summary>
        //    public ICollectionView AttributesView { get; }

        /// <summary>
        /// HashSet of PseudoAttributes already added as PseudoAttributeConstraint.
        /// </summary>
        private readonly HashSet<PseudoAttribute> _addedPseudoAttributes = new HashSet<PseudoAttribute>();

        public HashSet<PseudoAttribute> AddedPseudoAttributes
        {
            get { return _addedPseudoAttributes; }
        }

        //    /// <summary>
        //    /// Gets the collection of AttributeConstraints the user specified.
        //    /// </summary>
        //    public ObservableCollection<AttributeConstraint> AttributeConstraints { get; }

        /// <summary>
        /// Placeholder for the PseudoAttributeConstraint the user is editing that can be added.
        /// </summary>
        private PseudoAttributeConstraint _newPseudoAttributeConstraint;

        /// <summary>
        /// Gets the PseudoAttributeConstraint used for creating new ones by the user.
        /// </summary>
        public PseudoAttributeConstraint NewPseudoAttributeConstraint
        {
            get { return _newPseudoAttributeConstraint; }
            private set { SetProperty(ref _newPseudoAttributeConstraint, value); }
        }

        //    /// <summary>
        //    /// Collection of pseudo attributes that can be used in PseudoAttributeConstraints.
        //    /// </summary>
        //    private readonly ObservableCollection<PseudoAttribute> _pseudoAttributes = new ObservableCollection<PseudoAttribute>();

        //    /// <summary>
        //    /// Gets the collection of PseudoAttributeConstraints the user specified.
        //    /// </summary>
        //    public ObservableCollection<PseudoAttributeConstraint> PseudoAttributeConstraints { get; }

        //    public ObservableCollection<PseudoAttribute> PseudoAttributes
        //    {
        //        get { return _pseudoAttributes; }
        //    }

        //    /// <summary>
        //    /// Gets the CollectionView to the PseudoAttributes the user can use.
        //    /// </summary>
        //    public ICollectionView PseudoAttributesView { get; }

        #endregion Presentation
        #region Other PseudoAttribute Related Code

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

        //    private void ClearAttributeConstraints()
        //    {
        //        _addedAttributes.Clear();
        //        AttributeConstraints.Clear();
        //        AttributesView.Refresh();
        //        AttributesView.MoveCurrentToFirst();
        //        NewAttributeConstraint = new AttributeConstraint(AttributesView.CurrentItem as string);
        //    }

        //    private void ClearPseudoAttributeConstraints()
        //    {
        //        _addedPseudoAttributes.Clear();
        //        PseudoAttributeConstraints.Clear();
        //        PseudoAttributesView.Refresh();
        //        PseudoAttributesView.MoveCurrentToFirst();
        //        NewPseudoAttributeConstraint =
        //            new PseudoAttributeConstraint(PseudoAttributesView.CurrentItem as PseudoAttribute);
        //    }

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

        ///// <summary>
        ///// Reloads the possible PseudoAttributes from the filesystem.
        ///// Resets PseudoAttributeConstraints entered by the user.
        ///// </summary>
        //private void ReloadPseudoAttributes()
        //{
        //    _addedPseudoAttributes.Clear();
        //    _pseudoAttributes.Clear();
        //    foreach (var pseudo in _pseudoAttributeLoader.LoadPseudoAttributes())
        //    {
        //        _pseudoAttributes.Add(pseudo);
        //    }
        //    PseudoAttributeConstraints.Clear();
        //    PseudoAttributesView.MoveCurrentToFirst();
        //    NewPseudoAttributeConstraint = new PseudoAttributeConstraint(PseudoAttributesView.CurrentItem as PseudoAttribute);
        //}

        #endregion Other PseudoAttribute Related Code
    }
}