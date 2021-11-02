using EnumsNET;
using Fluent;
using MahApps.Metro.Controls;
using NLog;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Computation;
using PoESkillTree.Computation.ViewModels;
using PoESkillTree.Computation.Views;
using PoESkillTree.Controls;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Localization;
using PoESkillTree.Model;
using PoESkillTree.Model.Builds;
using PoESkillTree.Model.Items;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.TreeGenerator.ViewModels;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Converter;
using PoESkillTree.Utils.UrlProcessing;
using PoESkillTree.ViewModels;
using PoESkillTree.ViewModels.Builds;
using PoESkillTree.ViewModels.Crafting;
using PoESkillTree.ViewModels.Equipment;
using PoESkillTree.ViewModels.Import;
using PoESkillTree.ViewModels.PassiveTree;
using PoESkillTree.ViewModels.Skills;
using PoESkillTree.Views.Crafting;
using PoESkillTree.Views.Import;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ControlzEx.Theming;
using Attribute = PoESkillTree.ViewModels.Attribute;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Item = PoESkillTree.Model.Items.Item;
using MenuItem = System.Windows.Controls.MenuItem;
using ThemeManager = ControlzEx.Theming.ThemeManager;
using PoESkillTree.Views.PassiveTree;

namespace PoESkillTree.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged, IRibbonWindow
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The set of keys of which one needs to be pressed to highlight similar nodes on hover.
        /// </summary>
        private static readonly Key[] HighlightByHoverKeys = { Key.LeftShift, Key.RightShift };

        public event PropertyChangedEventHandler? PropertyChanged;

        private IExtendedDialogCoordinator _dialogCoordinator;
        public IPersistentData PersistentData { get; } = App.PersistentData;

        private ThemeManager ThemeManager => ThemeManager.Current;

        private readonly List<Attribute> _attiblist = new List<Attribute>();
        private readonly Regex _backreplace = new Regex("#");
        private readonly ToolTip _sToolTip = new ToolTip();
        private readonly SkillTreeUrlNormalizer _buildUrlNormalizer = new SkillTreeUrlNormalizer();
        private ListCollectionView _attributeCollection;

        private GroupStringConverter _attributeGroups;
        private ContextMenu _attributeContextMenu;
        private MenuItem cmAddToGroup, cmDeleteGroup;

        private readonly HttpClient _httpClient = new HttpClient();

        private GameData _gameData;

        private ItemAttributes _itemAttributes;

        private ItemAttributes ItemAttributes
        {
            get => _itemAttributes;
            set => SetProperty(ref _itemAttributes, value);
        }

/*
        /// <summary>
        /// The item information equipped in skilltree(Shared inside Static Instance)
        /// </summary>
        public InventoryViewModel InventoryViewModel
        {
            get => GlobalSettings.ItemInfoVal;
            private set => SetProperty(ref GlobalSettings.ItemInfoVal, value);
        }
*/
        private InventoryViewModel _inventoryViewModel;
        public InventoryViewModel InventoryViewModel
        {
            get => _inventoryViewModel;
            private set => SetProperty(ref _inventoryViewModel, value);
        }

        private SkillTreeAreaViewModel _skillTreeAreaViewModel;
        public SkillTreeAreaViewModel SkillTreeAreaViewModel
        {
            get => _skillTreeAreaViewModel;
            private set => SetProperty(ref _skillTreeAreaViewModel, value);
        }

        private JewelSocketObserver _jewelSocketObserver;
        private AbyssalSocketObserver _abyssalSocketObserver;

        public StashViewModel StashViewModel { get; } = new StashViewModel();

        private ImportViewModels _importViewModels;

        private ObservableItemCollectionConverter? _equipmentConverter;

        private ComputationViewModel? _computationViewModel;

        [DisallowNull]
        public ComputationViewModel? ComputationViewModel
        {
            get => _computationViewModel;
            private set
            {
                value!.SharedConfiguration.SetLevel(PersistentData.CurrentBuild.Level);
                value.SharedConfiguration.SetCharacterClass(Tree.CharClass);
                value.SharedConfiguration.SetBandit(PersistentData.CurrentBuild.Bandits.Choice);
                SetProperty(ref _computationViewModel, value);
            }
        }

        private SkillsEditingViewModel _skillsEditingViewModel;

        public SkillsEditingViewModel SkillsEditingViewModel
        {
            get => _skillsEditingViewModel;
            private set => SetProperty(ref _skillsEditingViewModel, value);
        }

        private SkillTree _tree;
        public SkillTree Tree
        {
            get => _tree;
            private set
            {
                if (_tree != null)
                    _tree.PropertyChanged -= Tree_PropertyChanged;
                SetProperty(ref _tree!, value);
            }
        }
        private async Task<SkillTree> CreateSkillTreeAsync(ProgressDialogController controller,
            AssetLoader? assetLoader = null)
        {
            var tree = await SkillTree.CreateAsync(PersistentData, controller, assetLoader);
            tree.PropertyChanged += Tree_PropertyChanged;
            if (BuildsControlViewModel != null)
                BuildsControlViewModel.SkillTree = tree;
            if (TreeGeneratorInteraction != null)
                TreeGeneratorInteraction.SkillTree = tree;
            if (InventoryViewModel != null)
            {
                tree.JewelViewModels = InventoryViewModel.TreeJewels;
                SkillTreeAreaViewModel.Dispose();
                SkillTreeAreaViewModel = new SkillTreeAreaViewModel(SkillTree.Skillnodes, InventoryViewModel.TreeJewels);
            }
            if (ComputationViewModel != null)
            {
                tree.ItemConnectedNodesSelector = ComputationViewModel.PassiveTreeConnections.GetConnectedNodes;
            }
            _jewelSocketObserver?.Dispose();
            _jewelSocketObserver = new JewelSocketObserver(tree.SkilledNodes);
            return tree;
        }

        private BuildsControlViewModel _buildsControlViewModel;

        public BuildsControlViewModel BuildsControlViewModel
        {
            get => _buildsControlViewModel;
            private set => SetProperty(ref _buildsControlViewModel, value);
        }

        public CommandCollectionViewModel LoadTreeButtonViewModel { get; } = new CommandCollectionViewModel();

        private Vector2D _addtransform;
        private bool _justLoaded;
        private string? _lasttooltip;

        private Vector2D _multransform;

        private IReadOnlyCollection<PassiveNodeViewModel>? _prePath;
        private IReadOnlyCollection<PassiveNodeViewModel>? _toRemove;

        private readonly Stack<string> _undoList = new Stack<string>();
        private readonly Stack<string> _redoList = new Stack<string>();

        private MouseButton _lastMouseButton;
        private bool _userInteraction;
        /// <summary>
        /// The node of the SkillTree that currently has the mouse over it.
        /// Null if no node is under the mouse.
        /// </summary>
        private PassiveNodeViewModel? _hoveredNode;

        private PassiveNodeViewModel? _lastHoveredNode;

        private bool _noAsyncTaskRunning = true;
        /// <summary>
        /// Specifies if there is a task running asynchronously in the background.
        /// Used to disable UI buttons that might interfere with the result of the task.
        /// </summary>
        public bool NoAsyncTaskRunning
        {
            get => _noAsyncTaskRunning;
            private set => SetProperty(ref _noAsyncTaskRunning, value);
        }

        private TreeGeneratorInteraction? _treeGeneratorInteraction;

        [DisallowNull]
        public TreeGeneratorInteraction? TreeGeneratorInteraction
        {
            get => _treeGeneratorInteraction;
            private set => SetProperty(ref _treeGeneratorInteraction, value);
        }

        /// <summary>
        /// Set to true when CurrentBuild.TreeUrl was set after direct SkillTree changes so the SkillTree
        /// doesn't need to be reloaded.
        /// </summary>
        private bool _skipLoadOnCurrentBuildTreeChange;

        private string? _inputTreeUrl;
        /// <summary>
        /// The tree url that is the current input of the tree text box. Can be different from
        /// CurrentBuild.TreeUrl if the user changes it (until the user presses "Load Tree" or Enter).
        /// </summary>
        [DisallowNull]
        public string? InputTreeUrl
        {
            get => _inputTreeUrl;
            set => SetProperty(ref _inputTreeUrl, value);
        }

        public ICommand UndoTreeUrlChangeCommand { get; }
        public ICommand RedoTreeUrlChangeCommand { get; }

        public static readonly DependencyProperty TitleBarProperty = DependencyProperty.Register(
            "TitleBar", typeof(RibbonTitleBar), typeof(MainWindow), new PropertyMetadata(default(RibbonTitleBar)));

        public RibbonTitleBar TitleBar
        {
            get => (RibbonTitleBar)GetValue(TitleBarProperty);
            private set => SetValue(TitleBarProperty, value);
        }

#pragma warning disable CS8618 // Initialized in Window_Loaded
        public MainWindow()
#pragma warning restore
        {
            InitializeComponent();

            UndoTreeUrlChangeCommand = new RelayCommand(UndoTreeUrlChange, CanUndoTreeUrlChange);
            RedoTreeUrlChangeCommand = new RelayCommand(RedoTreeUrlChange, CanRedoTreeUrlChange);
        }

        private void SetProperty<T>(
            ref T backingStore, T value, Action? onChanged = null, [CallerMemberName] string propertyName = "Unspecified")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;

            backingStore = value;

            onChanged?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RegisterPersistentDataHandlers()
        {
            // Register handlers
            PersistentData.CurrentBuild.PropertyChanged += CurrentBuildOnPropertyChanged;
            PersistentData.CurrentBuild.Bandits.PropertyChanged += CurrentBuildOnPropertyChanged;
            // Re-register handlers when PersistentData.CurrentBuild is set.
            PersistentData.PropertyChanged += async (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(PersistentData.CurrentBuild):
                        PersistentData.CurrentBuild.PropertyChanged += CurrentBuildOnPropertyChanged;
                        PersistentData.CurrentBuild.Bandits.PropertyChanged += CurrentBuildOnPropertyChanged;
                        await CurrentBuildChanged();
                        break;
                    case nameof(PersistentData.SelectedBuild):
                        UpdateTreeComparison();
                        break;
                }
            };
            // This makes sure CurrentBuildOnPropertyChanged is called only
            // on the PoEBuild instance currently stored in PersistentData.CurrentBuild.
            PersistentData.PropertyChanging += (sender, args) =>
            {
                if (args.PropertyName == nameof(PersistentData.CurrentBuild))
                {
                    TreeGeneratorInteraction?.SaveSettings();
                    PersistentData.CurrentBuild.PropertyChanged -= CurrentBuildOnPropertyChanged;
                    PersistentData.CurrentBuild.Bandits.PropertyChanged -= CurrentBuildOnPropertyChanged;
                }
            };
        }

        private async void CurrentBuildOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case nameof(PoEBuild.ItemData):
                    await LoadItemData();
                    _jewelSocketObserver.SetTreeJewelViewModels(InventoryViewModel.TreeJewels);
                    break;
                case nameof(PoEBuild.TreeUrl):
                    if (!_skipLoadOnCurrentBuildTreeChange)
                        await SetTreeUrl(PersistentData.CurrentBuild.TreeUrl);
                    InputTreeUrl = PersistentData.CurrentBuild.TreeUrl;
                    break;
                case nameof(PoEBuild.CheckedNodeIds):
                case nameof(PoEBuild.CrossedNodeIds):
                    Tree.ResetTaggedNodes();
                    break;
                case nameof(PoEBuild.AdditionalData):
                    TreeGeneratorInteraction?.LoadSettings();
                    break;
                case nameof(BanditSettings.Choice):
                    UpdateUI();
                    ComputationViewModel?.SharedConfiguration.SetBandit(PersistentData.CurrentBuild.Bandits.Choice);
                    break;
                case nameof(PoEBuild.Level):
                    ComputationViewModel?.SharedConfiguration.SetLevel(PersistentData.CurrentBuild.Level);
                    break;
            }
        }

        //This whole region, along with most of GroupStringConverter, makes up our user-defined attribute group functionality - Sectoidfodder 02/29/16
        #region Attribute grouping helpers

        //Necessary to update the summed numbers in group names before every refresh
        private void RefreshAttributeLists()
        {
            _attributeGroups.UpdateGroupNames(_attiblist);
            _attributeCollection.Refresh();
        }

        private void SetCustomGroups(IList<string[]> customgroups)
        {
            cmAddToGroup.Items.Clear();
            cmDeleteGroup.Items.Clear();

            var groupnames = new List<string>();

            foreach (var gp in customgroups)
            {
                if (!groupnames.Contains(gp[1]))
                {
                    groupnames.Add(gp[1]);
                }
            }

            cmAddToGroup.IsEnabled = false;
            cmDeleteGroup.IsEnabled = false;

            foreach (var name in groupnames)
            {
                var newSubMenu = new MenuItem { Header = name };
                newSubMenu.Click += AddToGroup;
                cmAddToGroup.Items.Add(newSubMenu);
                cmAddToGroup.IsEnabled = true;
                newSubMenu = new MenuItem { Header = name };
                newSubMenu.Click += DeleteGroup;
                cmDeleteGroup.Items.Add(newSubMenu);
                cmDeleteGroup.IsEnabled = true;
            }

            _attributeGroups.ResetGroups(customgroups);
            RefreshAttributeLists();
        }

        //Adds currently selected attributes to a new group
        private async void CreateGroup(object sender, RoutedEventArgs e)
        {
            var attributelist = new List<string>();
            foreach (var o in lbAttr.SelectedItems.Cast<Attribute>())
            {
                attributelist.Add(o.ToString());
            }

            //Build and show form to enter group name
            var name = await ExtendedDialogManager.ShowInputAsync(this, L10n.Message("Create New Attribute Group"), L10n.Message("Group name"));
            if (!string.IsNullOrEmpty(name))
            {
                if (_attributeGroups.AttributeGroups.ContainsKey(name))
                {
                    await this.ShowInfoAsync(L10n.Message("A group with that name already exists."));
                    return;
                }

                //Add submenus that add to and delete the new group
                var newSubMenu = new MenuItem { Header = name };
                newSubMenu.Click += AddToGroup;
                cmAddToGroup.Items.Add(newSubMenu);
                cmAddToGroup.IsEnabled = true;
                newSubMenu = new MenuItem { Header = name };
                newSubMenu.Click += DeleteGroup;
                cmDeleteGroup.Items.Add(newSubMenu);
                cmDeleteGroup.IsEnabled = true;

                //Back end - actually make the new group
                _attributeGroups.AddGroup(name, attributelist.ToArray());
                RefreshAttributeLists();
            }
        }

        //Removes currently selected attributes from their custom groups, restoring them to their default groups
        private void RemoveFromGroup(object sender, RoutedEventArgs e)
        {
            var attributelist = new List<string>();
            //string SelectedAttrName;
            //int index;
            foreach (var o in lbAttr.SelectedItems.Cast<Attribute>())//lb.SelectedItems
            {
/*
                SelectedAttrName = o.ToString();
                index = GlobalSettings.TrackedStats.IndexOf(SelectedAttrName);
                if (index>-1)//Functionality of removing individual tracked stat
                {
                    GlobalSettings.TrackedStats.RemoveAt(index);
                }
                else
                {
                    attributelist.Add(SelectedAttrName);
                }
*/
                attributelist.Add(o.ToString());
            }
            if (attributelist.Count > 0)
            {
                _attributeGroups.RemoveFromGroup(attributelist.ToArray());
                RefreshAttributeLists();
            }
        }

        //Adds currently selected attributes to an existing custom group named by sender.Header
        private void AddToGroup(object sender, RoutedEventArgs e)
        {
            var attributelist = new List<string>();
            foreach (var o in lbAttr.SelectedItems.Cast<Attribute>())
            {
                attributelist.Add(o.ToString());
            }
            if (attributelist.Count > 0)
            {
                _attributeGroups.AddGroup(((MenuItem)sender).Header.ToString()!, attributelist.ToArray());
                RefreshAttributeLists();
            }
        }

        //Deletes the entire custom group named by sender.Header, restoring all contained attributes to their default groups
        private void DeleteGroup(object sender, RoutedEventArgs e)
        {
            //Remove submenus that work with the group
            for (var i = 0; i < cmAddToGroup.Items.Count; i++)
            {
                if (((MenuItem)cmAddToGroup.Items[i]).Header.ToString()!.ToLower().Equals(((MenuItem)sender).Header.ToString()!.ToLower()))
                {
                    cmAddToGroup.Items.RemoveAt(i);
                    if (cmAddToGroup.Items.Count == 0)
                        cmAddToGroup.IsEnabled = false;
                    break;
                }
            }
            for (var i = 0; i < cmDeleteGroup.Items.Count; i++)
            {
                if (((MenuItem)cmDeleteGroup.Items[i]).Header.ToString()!.ToLower().Equals(((MenuItem)sender).Header.ToString()!.ToLower()))
                {
                    cmDeleteGroup.Items.RemoveAt(i);
                    if (cmDeleteGroup.Items.Count == 0)
                        cmDeleteGroup.IsEnabled = false;
                    break;
                }
            }

            _attributeGroups.DeleteGroup(((MenuItem)sender).Header.ToString()!);
            RefreshAttributeLists();
        }

        #endregion

        #region Window methods

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            var controller = await this.ShowProgressAsync(L10n.Message("Initialization"),
                        L10n.Message("Initializing window ..."));
            controller.Maximum = 1;
            controller.SetIndeterminate();

            var computationInitializer = ComputationInitializer.StartNew();
            _gameData = computationInitializer.GameData;
            var persistentDataTask = PersistentData.InitializeAsync(DialogCoordinator.Instance);

            InitializeIndependentUI();
            Log.Info($"Independent UI initialized after {stopwatch.ElapsedMilliseconds} ms");

            await persistentDataTask;
            InitializePersistentDataDependentUI();
            Log.Info($"PersistentData UI initialized after {stopwatch.ElapsedMilliseconds} ms");

            controller.SetMessage(L10n.Message("Loading skill tree assets ..."));
            Tree = await CreateSkillTreeAsync(controller);
            InitializeTreeDependentUI();
            Log.Info($"Tree UI initialized after {stopwatch.ElapsedMilliseconds} ms");

            controller.SetMessage(L10n.Message("Initializing window ..."));
            controller.SetIndeterminate();
            await Task.Delay(1); // Give the progress dialog a chance to update

            var initialComputationTask = computationInitializer.InitializeAsync(SkillTree.Skillnodes.Values);

            _justLoaded = true;
            // loading last build
            await CurrentBuildChanged();
            _justLoaded = false;
            InitializeBuildDependentUI();
            Log.Info($"Build UI initialized after {stopwatch.ElapsedMilliseconds} ms");

            await initialComputationTask;
            await InitializeComputationDependentAsync(computationInitializer);
            Log.Info($"Computation UI initialized after {stopwatch.ElapsedMilliseconds} ms");

            await controller.CloseAsync();

            stopwatch.Stop();
            Log.Info($"Window_Loaded took {stopwatch.ElapsedMilliseconds} ms");
        }

        private void InitializeIndependentUI()
        {
            TitleBar = this.FindChild<RibbonTitleBar>("RibbonTitleBar");
            TitleBar.InvalidateArrange();
            TitleBar.UpdateLayout();

            var cmHighlight = new MenuItem
            {
                Header = L10n.Message("Highlight nodes by attribute")
            };
            cmHighlight.Click += HighlightNodesByAttribute;
            var cmRemoveHighlight = new MenuItem
            {
                Header = L10n.Message("Remove highlights by attribute")
            };
            cmRemoveHighlight.Click += UnhighlightNodesByAttribute;
            var cmCreateGroup = new MenuItem { Header = L10n.Message("Create new group") };
            cmCreateGroup.Click += CreateGroup;
            cmAddToGroup = new MenuItem
            {
                Header = L10n.Message("Add to group..."),
                IsEnabled = false
            };
            cmDeleteGroup = new MenuItem
            {
                Header = L10n.Message("Delete group..."),
                IsEnabled = false
            };
            var cmRemoveFromGroup = new MenuItem { Header = L10n.Message("Remove from group") };
            cmRemoveFromGroup.Click += RemoveFromGroup;

            _attributeGroups = new GroupStringConverter();
            _attributeContextMenu = new ContextMenu();
            _attributeContextMenu.Items.Add(cmHighlight);
            _attributeContextMenu.Items.Add(cmRemoveHighlight);
            _attributeContextMenu.Items.Add(cmCreateGroup);
            _attributeContextMenu.Items.Add(cmAddToGroup);
            _attributeContextMenu.Items.Add(cmDeleteGroup);
            _attributeContextMenu.Items.Add(cmRemoveFromGroup);

            _attributeCollection = new ListCollectionView(_attiblist);
            _attributeCollection.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Attribute.Text), _attributeGroups));
            _attributeCollection.CustomSort = _attributeGroups;
            lbAttr.ItemsSource = _attributeCollection;
            lbAttr.SelectionMode = SelectionMode.Extended;
            lbAttr.ContextMenu = _attributeContextMenu;

            cbCharType.ItemsSource = Enums.GetValues<CharacterClass>();
            cbAscType.SelectedIndex = 0;
        }

        private void InitializePersistentDataDependentUI()
        {
            _dialogCoordinator = new ExtendedDialogCoordinator(_gameData, PersistentData);
            //GlobalSettings.dialogCoordinatorRef = _dialogCoordinator;
            RegisterPersistentDataHandlers();
            StashViewModel.Initialize(_dialogCoordinator, PersistentData);
            _importViewModels = new ImportViewModels(_dialogCoordinator, PersistentData, StashViewModel);
            InitializeTheme();
        }

        private void InitializeTreeDependentUI()
        {
            updateCanvasSize();
            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);
        }

        private void InitializeBuildDependentUI()
        {
            PersistentData.Options.PropertyChanged += Options_PropertyChanged;
            PopulateAscendancySelectionList();
            BuildsControlViewModel = new BuildsControlViewModel(_dialogCoordinator, PersistentData, Tree, _httpClient);
            UpdateTreeComparison();
            TreeGeneratorInteraction =
                new TreeGeneratorInteraction(SettingsDialogCoordinator.Instance, PersistentData, Tree);
            TreeGeneratorInteraction.RunFinished += (o, args) =>
            {
                UpdateUI();
                SetCurrentBuildUrlFromTree();
            };

            InitializeLoadTreeButtonViewModel();
        }

        private void InitializeLoadTreeButtonViewModel()
        {
            LoadTreeButtonViewModel.Add(L10n.Message("Load Tree"), async () =>
            {
                if (string.IsNullOrWhiteSpace(InputTreeUrl))
                    return;
                await LoadBuildFromUrlAsync(InputTreeUrl);
            }, () => NoAsyncTaskRunning);
            LoadTreeButtonViewModel.Add(L10n.Message("Load as new build"), async () =>
            {
                if (string.IsNullOrWhiteSpace(InputTreeUrl))
                    return;

                var url = InputTreeUrl;
                BuildsControlViewModel.NewBuild(BuildsControlViewModel.BuildRoot);
                await LoadBuildFromUrlAsync(url);
            }, () => NoAsyncTaskRunning);
            LoadTreeButtonViewModel.SelectedIndex = PersistentData.Options.LoadTreeButtonIndex;
            LoadTreeButtonViewModel.PropertyChanged += (o, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(LoadTreeButtonViewModel.SelectedItem):
                        PersistentData.Options.LoadTreeButtonIndex = LoadTreeButtonViewModel.SelectedIndex;
                        break;
                }
            };
        }

        private async Task InitializeComputationDependentAsync(ComputationInitializer computationInitializer)
        {
            _equipmentConverter = new ObservableItemCollectionConverter(computationInitializer.CreateAdditionalSkillStatApplier());
            _equipmentConverter.ConvertFrom(ItemAttributes);
            await computationInitializer.InitializeAfterBuildLoadAsync(
                Tree.SkilledNodes,
                _equipmentConverter.Equipment,
                _equipmentConverter.Jewels,
                _equipmentConverter.Gems,
                _equipmentConverter.Skills);
            computationInitializer.SetupPeriodicActions();
            ComputationViewModel = await computationInitializer.CreateComputationViewModelAsync(PersistentData);
            Tree.ItemConnectedNodesSelector = ComputationViewModel.PassiveTreeConnections.GetConnectedNodes;
            _abyssalSocketObserver = computationInitializer.CreateAbyssalSocketObserver(InventoryViewModel.ItemJewels);
            (await computationInitializer.CreateItemAllocatedPassiveNodesObservableAsync()).Subscribe(
                nodes => Tree.ItemAllocatedNodes = nodes,
                ex => Log.Error(ex, "Error in ItemAllocatedPassiveNodesObservable"));
        }

        private void Options_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Options.ShowAllAscendancyClasses):
                    Tree.ToggleAscendancyTree(PersistentData.Options.ShowAllAscendancyClasses);
                    break;
                case nameof(Options.TreeComparisonEnabled):
                    UpdateTreeComparison();
                    break;
                case nameof(Options.Theme):
                case nameof(Options.Accent):
                    SetTheme();
                    break;
            }
            SearchUpdate();
        }

        private void Tree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SkillTree.CharClass):
                    Tree.UpdateAscendancyClasses = true;
                    PopulateAscendancySelectionList();
                    ComputationViewModel?.SharedConfiguration.SetCharacterClass(Tree.CharClass);
                    break;
            }
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Q:
                        ToggleAttributes();
                        break;
                    case Key.B:
                        ToggleBuilds();
                        break;
                    case Key.R:
                        ResetTree();
                        break;
                    case Key.E:
                        await DownloadPoeUrlAsync();
                        break;
                    case Key.D1:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 0;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D2:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 1;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D3:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 2;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D4:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 3;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D5:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 4;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D6:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 5;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D7:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 6;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.OemPlus:
                    case Key.Add:
                        zbSkillTreeBackground.ZoomIn(Mouse.PrimaryDevice);
                        break;
                    case Key.OemMinus:
                    case Key.Subtract:
                        zbSkillTreeBackground.ZoomOut(Mouse.PrimaryDevice);
                        break;
                    case Key.Z:
                        UndoTreeUrlChange();
                        break;
                    case Key.Y:
                        RedoTreeUrlChange();
                        break;
                    case Key.G:
                        ToggleShowSummary();
                        if (_hoveredNode != null && !_hoveredNode.IsRootNode)
                        {
                            GenerateTooltipForNode(_hoveredNode, true);
                        }
                        break;
                    case Key.F:
                        tbSearch.Focus();
                        tbSearch.SelectAll();
                        break;
                }
            }

            if (HighlightByHoverKeys.Any(key => key == e.Key))
            {
                HighlightNodesByHover();
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (HighlightByHoverKeys.Any(key => key == e.Key))
            {
                HighlightNodesByHover();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (SkillTree.SkillTreeRect.Height == 0) // Not yet initialized
                return;

            updateCanvasSize();
        }

        private void updateCanvasSize()
        {
            double aspectRatio = SkillTree.SkillTreeRect.Width / SkillTree.SkillTreeRect.Height;
            if (zbSkillTreeBackground.ActualWidth / zbSkillTreeBackground.ActualHeight > aspectRatio)
            {
                recSkillTree.Height = zbSkillTreeBackground.ActualHeight;
                recSkillTree.Width = aspectRatio * recSkillTree.Height;
            }
            else
            {
                recSkillTree.Width = zbSkillTreeBackground.ActualWidth;
                recSkillTree.Height = recSkillTree.Width / aspectRatio;
            }
            recSkillTree.UpdateLayout();
            _multransform = SkillTree.SkillTreeRect.Size / new Vector2D(recSkillTree.RenderSize.Width, recSkillTree.RenderSize.Height);
            _addtransform = SkillTree.SkillTreeRect.TopLeft;
        }

        private bool? _canClose;

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_canClose.HasValue)
            {
                // We want to close later
                e.Cancel = true;
                // Stop close calls while async processing from closing
                _canClose = false;
                var message = L10n.Message("There are unsaved builds. Do you want to save them before closing?\n\n"
                                           + "Canceling stops the program from closing and does not save any builds.");
                // Might affect unsaved builds state, so needs to be done here.
                TreeGeneratorInteraction?.SaveSettings();
                if (await BuildsControlViewModel.HandleUnsavedBuilds(message, true))
                {
                    // User wants to close
                    _canClose = true;
                    // Calling Close() here again is not possible as the Closing event might still be handled
                    // (Close() is not allowed while a previous one is not completely processed)
                    Application.Current.Shutdown();
                }
                else
                {
                    // User doesn't want to close. Reset _canClose.
                    _canClose = null;
                }
            }
            else if (!_canClose.Value)
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region Menu

        public async Task ScreenShotAsync()
        {
            const int maxsize = 3000;
            Rect2D contentBounds = Tree.ActivePaths.ContentBounds;
            contentBounds *= 1.2;
            if (!double.IsNaN(contentBounds.Width) && !double.IsNaN(contentBounds.Height))
            {
                var aspect = contentBounds.Width / contentBounds.Height;
                var xmax = contentBounds.Width;
                var ymax = contentBounds.Height;
                if (aspect > 1 && xmax > maxsize)
                {
                    xmax = maxsize;
                    ymax = xmax / aspect;
                }
                if (aspect < 1 & ymax > maxsize)
                {
                    ymax = maxsize;
                    xmax = ymax * aspect;
                }

                var clipboardBmp = new RenderTargetBitmap((int)xmax, (int)ymax, 96, 96, PixelFormats.Pbgra32);
                var db = new VisualBrush(Tree.SkillTreeVisual)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = contentBounds
                };
                var dw = new DrawingVisual();

                using (var dc = dw.RenderOpen())
                {
                    dc.DrawRectangle(db, null, new Rect(0, 0, xmax, ymax));
                }
                clipboardBmp.Render(dw);
                clipboardBmp.Freeze();

                //Save image in clipboard
                Clipboard.SetImage(clipboardBmp);

                //Convert renderTargetBitmap to bitmap
                var stream = new MemoryStream();
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(clipboardBmp));
                encoder.Save(stream);

                var image = System.Drawing.Image.FromStream(stream);

                // Configure save file dialog box
                var dialog = new Microsoft.Win32.SaveFileDialog();

                // Default file name -- current build name ("buildname - xxx points used")
                //To-Do switch to ("buildname - xxx skillpoints used") //not counting AscendancyPoints
                var skilledNodes = (uint)Tree.GetPointCount()["NormalUsed"];
                dialog.FileName = PersistentData.CurrentBuild.Name + " - " + string.Format(L10n.Plural("{0} point", "{0} points", skilledNodes), skilledNodes);

                dialog.DefaultExt = ".jpg"; // Default file extension
                dialog.Filter = "JPEG (*.jpg, *.jpeg)|*.jpg;|PNG (*.png)|*.png"; // Filter files by extension
                dialog.OverwritePrompt = true;

                // Show save file dialog box
                var result = dialog.ShowDialog();

                // Continue if the user did select a path
                if (result.HasValue && result == true)
                {
                    System.Drawing.Imaging.ImageFormat format;
                    var fileExtension = Path.GetExtension(dialog.FileName);

                    //set the selected data type
                    switch (fileExtension)
                    {
                        case ".png":
                            format = System.Drawing.Imaging.ImageFormat.Png;
                            break;
                        default:
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                    }

                    //save the file
                    image.Save(dialog.FileName, format);
                }

                recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);
            }
            else
            {
                await this.ShowInfoAsync(L10n.Message("Your build must use at least one node to generate a screenshot"),
                    title: L10n.Message("Screenshot Generator"));
            }
        }

        public async Task ImportCharacterAsync()
        {
            await this.ShowDialogAsync(_importViewModels.ImportCharacter(ItemAttributes, Tree), new ImportCharacterWindow());
            UpdateUI();
            SetCurrentBuildUrlFromTree();
        }

        public async Task ImportStashAsync()
        {
            await this.ShowDialogAsync(_importViewModels.ImportStash, new ImportStashWindow());
        }

        public async Task RedownloadTreeAssetsAsync()
        {
            var sMessageBoxText = L10n.Message("The existing skill tree data will be deleted. The data will " +
                                               "be downloaded from the official online skill tree and " +
                                               "is from the latest released version of the game.")
                                     + "\n\n" + L10n.Message("Do you want to continue?");

            var rsltMessageBox = await this.ShowQuestionAsync(sMessageBoxText, image: MessageBoxImage.Warning);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    var controller = await ExtendedDialogManager.ShowProgressAsync(this, L10n.Message("Downloading skill tree assets ..."), null);
                    controller.Maximum = 1;
                    controller.SetProgress(0);
                    var assetLoader = new AssetLoader(_httpClient, AppData.GetFolder("Data", true), false);
                    try
                    {
                        assetLoader.MoveToBackup();

                        SkillTree.ClearAssets(); //enable recaching of assets
                        Tree = await CreateSkillTreeAsync(controller, assetLoader); //create new skilltree to reinitialize cache
                        recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);

                        await ResetTreeUrl();
                        _justLoaded = false;

                        assetLoader.DeleteBackup();
                    }
                    catch (Exception ex)
                    {
                        assetLoader.RestoreBackup();
                        Log.Error(ex, "Exception while downloading skill tree assets");
                        await this.ShowErrorAsync(L10n.Message("An error occurred while downloading assets."), ex.Message);
                    }
                    await controller.CloseAsync();
                    break;

                case MessageBoxResult.No:
                    //Do nothing
                    break;
            }
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                // No non-Task way without rewriting Updater to support/use await directly.
                var release =
                    await AwaitAsyncTask(L10n.Message("Checking for updates"),
                        Task.Run(() => Updater.CheckForUpdates()));

                if (release == null)
                {
                    await this.ShowInfoAsync(L10n.Message("You have the latest version!"));
                }
                else
                {
                    var message = release.IsUpdate
                        ? string.Format(L10n.Message("An update for {0} ({1}) is available!"),
                            AppData.ProductName, release.Version)
                          + "\n\n" +
                          L10n.Message("The application will be closed when download completes to proceed with the update.")
                        : string.Format(L10n.Message("A new version {0} is available!"), release.Version)
                          + "\n\n" +
                          L10n.Message(
                              "The new version of application will be installed side-by-side with earlier versions.");

                    if (release.IsPreRelease)
                        message += "\n\n" +
                                   L10n.Message("Warning: This is a pre-release, meaning there could be some bugs!");

                    message += "\n\n" +
                               (release.IsUpdate
                                   ? L10n.Message("Do you want to download and install the update?")
                                   : L10n.Message("Do you want to download and install the new version?"));

                    var download = await this.ShowQuestionAsync(message, title: L10n.Message("Continue installation?"),
                        image: release.IsPreRelease ? MessageBoxImage.Warning : MessageBoxImage.Question);
                    if (download == MessageBoxResult.Yes)
                        await InstallUpdateAsync();
                    else
                        Updater.Dispose();
                }
            }
            catch (UpdaterException ex)
            {
                await this.ShowErrorAsync(
                    L10n.Message("An error occurred while attempting to contact the update location."),
                    ex.Message);
            }
        }

        // Starts update process.
        private async Task InstallUpdateAsync()
        {
            var controller = await this.ShowProgressAsync(L10n.Message("Downloading latest version"), null, true);
            controller.Maximum = 100;
            controller.Canceled += (sender, args) => Updater.Cancel();
            try
            {
                var downloadCs = new TaskCompletionSource<AsyncCompletedEventArgs>();
                Updater.Download((sender, args) => downloadCs.SetResult(args),
                    (sender, args) => controller.SetProgress(args.ProgressPercentage));

                var result = await downloadCs.Task;
                await controller.CloseAsync();
                await UpdateDownloadCompleted(result);
            }
            catch (UpdaterException ex)
            {
                await this.ShowErrorAsync(L10n.Message("An error occurred during the download operation."),
                    ex.Message);
                await controller.CloseAsync();
            }
        }

        // Invoked when update download completes, aborts or fails.
        private async Task UpdateDownloadCompleted(AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) // Check whether download was canceled.
            {
                Updater.Dispose();
            }
            else if (e.Error != null) // Check whether error occurred.
            {
                await this.ShowErrorAsync(L10n.Message("An error occurred during the download operation."), e.Error.Message);
            }
            else // Download completed.
            {
                try
                {
                    Updater.Install();
                    // Release being installed is an update, we have to exit application.
                    if (Updater.LatestReleaseIsUpdate) Application.Current.Shutdown();
                }
                catch (UpdaterException ex)
                {
                    Updater.Dispose();
                    await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to start the installation."), ex.Message);
                }
            }
        }

        #endregion

        #region  Character Selection
        private void userInteraction_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _userInteraction = true;
        }

        private void cbCharType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tree == null)
                return;
            if (!_userInteraction)
                return;

            var charClass = (CharacterClass)cbCharType.SelectedItem;
            if (Tree.CharClass == charClass) return;

            Tree.SwitchClass(charClass);
            UpdateUI();
            SetCurrentBuildUrlFromTree();
            _userInteraction = false;
            cbAscType.SelectedIndex = 0;
        }

        private void cbAscType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_userInteraction)
                return;
            if (cbAscType.SelectedIndex < 0 || cbAscType.SelectedIndex > 3)
                return;

            Tree.AscType = cbAscType.SelectedIndex;

            UpdateUI();
            SetCurrentBuildUrlFromTree();
            _userInteraction = false;
        }

        private void PopulateAscendancySelectionList()
        {
            if (!Tree.UpdateAscendancyClasses) return;
            Tree.UpdateAscendancyClasses = false;
            var ascendancyItems = new List<string> { "None" };
            foreach (var name in Tree.AscendancyClasses.GetClasses(Tree.CharClass))
                ascendancyItems.Add(name.DisplayName);
            cbAscType.ItemsSource = ascendancyItems.Select(x => new ComboBoxItem { Name = x, Content = x });
        }

        private void Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> args)
        {
            if (Tree == null)
                return;
            UpdateUI();
        }

        public void ResetTree()
        {
            if (Tree == null)
                return;
            Tree.Reset();
            UpdateUI();
            SetCurrentBuildUrlFromTree();
        }

        #endregion

        #region Update Attribute lists

        public void UpdateUI()
        {
            UpdateAttributeList();
            RefreshAttributeLists();
            UpdateClass();
            UpdatePoints();
            //IntuitiveLeapCheckup();
        }

        public void UpdateClass()
        {
            cbCharType.SelectedItem = Tree.CharClass;
            cbAscType.SelectedIndex = Tree.AscType;
        }

        /// <summary>
        /// Updates the attribute list.
        /// </summary>
        public void UpdateAttributeList()
        {
            lbAttr.SelectedIndex = -1;
            _attiblist.Clear();
            var copy = Tree.HighlightedAttributes == null
                ? null
                : new Dictionary<string, List<float>>(Tree.HighlightedAttributes);

            if (GlobalSettings.JewelInfo.NotSetup)
            {
                GlobalSettings.JewelInfo.CategorizeJewelSlots(); GlobalSettings.JewelInfo.NotSetup = false;
            }

            foreach (var item in Tree.SelectedAttributes)
            {
                var a = new Attribute(InsertNumbersInAttributes(item));
                if (!CheckIfAttributeMatchesFilter(a)) continue;
                if (copy != null && copy.ContainsKey(item.Key))
                {
                    var citem = copy[item.Key];
                    a.Deltas = item.Value.Zip(citem, (s, h) => s - h).ToArray();
                    copy.Remove(item.Key);
                }
                else
                {
                    a.Deltas = copy != null ? item.Value.ToArray() : item.Value.Select(v => 0f).ToArray();
                }
                _attiblist.Add(a);
            }

/*
            if (GlobalSettings.TrackedStats.Count != 0)
            {
                TreeGenerator.Model.PseudoAttributes.PseudoAttribute Element;
                string AttrName;
                for (int Index = 0; Index < GlobalSettings.TrackedStats.Count; ++Index)
                {
                    Element = GlobalSettings.TrackedStats[Index];
                    AttrName = Element.Name;
                    if (Tree.SelectedAttributes.ContainsKey(AttrName))
                    {
                        Tree.SelectedAttributes[AttrName] = new List<float> { Element.CalculateValue(Tree.SelectedAttributes) };
                    }
                    else
                    {
                        Tree.SelectedAttributes.Add(AttrName, new List<float> { Element.CalculateValue(Tree.SelectedAttributes) });
                    }
                }
            }
*/

            if (copy != null)
            {
                foreach (var item in copy)
                {
                    var a = new Attribute(InsertNumbersInAttributes(new KeyValuePair<string, List<float>>(item.Key, item.Value.Select(v => 0f).ToList())));
                    if (!CheckIfAttributeMatchesFilter(a)) continue;
                    a.Deltas = item.Value.Select(h => 0 - h).ToArray();
                    a.Missing = true;
                    _attiblist.Add(a);
                }
            }
        }

/*
        /// <summary>
        /// Apply removal of Intuitive Leap supported jewels
        /// </summary>
        public void IntuitiveLeapCheckup()
        {
            if(GlobalSettings.RemovingIntLeapJewels!=0)
            {

            }
        }
*/

        public void UpdatePoints()
        {
            var points = Tree.GetPointCount();
            NormalUsedPoints.Text = points["NormalUsed"].ToString();
            NormalTotalPoints.Text = points["NormalTotal"].ToString();
            AscendancyUsedPoints.Text = points["AscendancyUsed"].ToString();
            AscendancyTotalPoints.Text = points["AscendancyTotal"].ToString();
        }

        private string InsertNumbersInAttributes(KeyValuePair<string, List<float>> attrib)
        {
            var s = attrib.Key;
            foreach (var f in attrib.Value)
            {
                s = _backreplace.Replace(s, f + "", 1);
            }
            return s;
        }

        private bool CheckIfAttributeMatchesFilter(Attribute a)
        {
            var filter = tbAttributesFilter.Text;
            if (cbAttributesFilterRegEx.IsChecked == true)
            {
                try
                {
                    var regex = new Regex(filter, RegexOptions.IgnoreCase);
                    if (!regex.IsMatch(a.Text)) return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (!a.Text.Contains(filter, StringComparison.InvariantCultureIgnoreCase)) return false;
            return true;
        }

        #endregion

        #region Attribute lists - Event Handlers

        private void ToggleAttributes()
        {
            PersistentData.Options.AttributesBarOpened = !PersistentData.Options.AttributesBarOpened;
        }

        private void ToggleShowSummary()
        {
            PersistentData.Options.ChangeSummaryEnabled = !PersistentData.Options.ChangeSummaryEnabled;
        }

        private void HighlightNodesByAttribute(object sender, RoutedEventArgs e)
        {
            var listBox = _attributeContextMenu.PlacementTarget as ListBox;
            if (listBox == null || !listBox.IsVisible) return;

            var newHighlightedAttribute =
                "^" + Regex.Replace(listBox.SelectedItem.ToString()!
                        .Replace(@"+", @"\+")
                        .Replace(@"-", @"\-")
                        .Replace(@"%", @"\%"), @"[0-9]*\.?[0-9]+", @"[0-9]*\.?[0-9]+") + "$";
            Tree.HighlightNodesBySearch(newHighlightedAttribute, true, NodeHighlighter.HighlightState.FromAttrib);
        }

        private void UnhighlightNodesByAttribute(object sender, RoutedEventArgs e)
        {
            Tree.HighlightNodesBySearch("", true, NodeHighlighter.HighlightState.FromAttrib);
        }

        private void ToggleBuilds()
        {
            PersistentData.Options.BuildsBarOpened = !PersistentData.Options.BuildsBarOpened;
        }

        private void tbAttributesFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAttributeLists();
        }

        private void cbAttributesFilterRegEx_Click(object sender, RoutedEventArgs e)
        {
            FilterAttributeLists();
        }

        private void FilterAttributeLists()
        {
            if (cbAttributesFilterRegEx.IsChecked == true && !RegexTools.IsValidRegex(tbAttributesFilter.Text)) return;
            UpdateAttributeList();
            RefreshAttributeLists();
        }

        #endregion

        #region zbSkillTreeBackground

        private void zbSkillTreeBackground_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _lastMouseButton = e.ChangedButton;
        }

/*
        static private void AddLeapTagToNode(PoESkillTree.SkillTreeFiles.SkillNode CurrentNode)
        {
            List<float> BlankList = new List<float>();
            int attributeSize = CurrentNode.Attributes.Count;
            if (!CurrentNode.Attributes.ContainsKey(GlobalSettings.LeapedNode) && attributeSize != 0)
            {
                CurrentNode.Attributes.Add(GlobalSettings.LeapedNode, BlankList);
            }
        }

        static private void RemoveLeapTagFromNode(PoESkillTree.SkillTreeFiles.SkillNode CurrentNode)
        {
            if (CurrentNode.Attributes.ContainsKey(GlobalSettings.LeapedNode))
            {
                CurrentNode.Attributes.Remove(GlobalSettings.LeapedNode);
            }
        }

        private void NormalRefund(PoESkillTree.SkillTreeFiles.SkillNode node)
        {
            Tree.ForceRefundNode(node);
            _prePath = Tree.GetShortestPathTo(node, Tree.SkilledNodes);
            Tree.DrawPath(_prePath);
        }

        private void NormalNodeClick(PoESkillTree.SkillTreeFiles.SkillNode node)
        {
            if (_prePath != null)
            {
                Tree.AllocateSkillNodes(_prePath);
                _toRemove = Tree.ForceRefundNodePreview(node);
                if (_toRemove != null)
                    Tree.DrawRefundPreview(_toRemove);
            }
        }
*/

        private async void zbSkillTreeBackground_Click(object sender, RoutedEventArgs e)
        {
            var p = ((MouseEventArgs)e.OriginalSource).GetPosition(zbSkillTreeBackground.Child);
            var v = new Vector2D(p.X, p.Y);
            v = v * _multransform + _addtransform;

            var node = Tree.FindNodeInRange(v);
            if (node != null && !node.IsRootNode)
            {
                if (node.IsAscendancyNode && !Tree.DrawAscendancy)
                    return;
                var ascendancyClassName = Tree.AscendancyClassName;
                if (!PersistentData.Options.ShowAllAscendancyClasses && node.IsAscendancyNode && node.AscendancyName != ascendancyClassName)
                    return;
                // Ignore clicks on character portraits and masteries
                if (node.StartingCharacterClass == null)
                {
                    if (_lastMouseButton == MouseButton.Right)
                    {
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        {
                            // Backward on shift+RMB
                            Tree.CycleNodeTagBackward(node);
                        }
                        else
                        {
                            // Forward on RMB
                            Tree.CycleNodeTagForward(node);
                        }
                        e.Handled = true;
                    }
                    else
                    {
                        // Toggle whether the node is included in the tree
                        if (Tree.SkilledNodes.Contains(node))
                        {
/*
                            if (node.Attributes.ContainsKey(GlobalSettings.LeapedNode))
                            {
                                if (NonLeapedNeighborIsConnected)
                                {
                                    NormalRefund(node);
                                }
                                else
                                {
                                    Tree.SkilledNodes.Remove(node);
                                }
                                RemoveLeapTagFromNode(node);
                            }
                            else
                            {
                                NormalRefund(node);
                            }
*/
                            Tree.ForceRefundNode(node);
                            _prePath = Tree.GetShortestPathTo(node);
                            Tree.DrawPath(_prePath);
                        }
                        else if (_prePath != null)
                        {
/*
                            if (NonLeapedNeighborIsConnected)
                            {
                                NormalNodeClick(node);
                                //Remove Leaping Tag from node if now connected in to tree
                                if (node.Attributes.ContainsKey(GlobalSettings.LeapedNode))
                                {
                                    foreach (var skillNode in node.Neighbor)//Checking for tree connection
                                    {
                                        if (Tree.SkilledNodes.Contains(skillNode) && skillNode.Attributes.ContainsKey(GlobalSettings.LeapedNode))
                                        {
                                            RemoveLeapTagFromNode(skillNode);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (node.Attributes.ContainsKey(GlobalSettings.FakeIntuitiveLeapSupportAttribute))
                                {
                                    Tree.SkilledNodes.Add(node);
                                    AddLeapTagToNode(node);
                                }
                                else
                                {
                                    NormalNodeClick(node);
                                }
                            }
*/

                            if (node.PassiveNodeType == PassiveNodeType.Mastery)
                            {
                                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                                {
                                    //var SkillBefore = node.Skill;
                                    var model = new MasteryEffectSelectionViewModel(node, Tree.SkilledNodes.Where(x => x.PassiveNodeType == PassiveNodeType.Mastery));
                                    await MasteryEffectSelectionAsync(model, new MasteryEffectSelectionView());
                                    //var SkillAfter = node.Skill;
                                    //if(SkillBefore!=SkillAfter)//Making sure remove(Needed when reapplying stat labels otherwise it fails to remove)
                                    //{
                                    //}
                                    //GlobalSettings.ApplyMasteryLabel(node);//Reapplying Mastery Label attribute(So don't lose ability to have generator find Mastery Node based on type)
                                }
                                if (_prePath == null)
                                    return;//Exception Prevention measure for if select no options
                            }

                            Tree.AllocateSkillNodes(_prePath);
                            _toRemove = Tree.ForceRefundNodePreview(node);
                            if (_toRemove != null)
                                Tree.DrawRefundPreview(_toRemove);
                        }
                    }
                }
                SetCurrentBuildUrlFromTree();
                UpdateUI();
            }
            else if (Tree.AscendancyButtonRect.Contains(v) && Tree.AscType != 0)
            {
                if (PersistentData.Options.ShowAllAscendancyClasses) return;
                Tree.DrawAscendancyButton("Pressed");
                Tree.ToggleAscendancyTree();
                SearchUpdate();
            }
            else
            {
                var size = zbSkillTreeBackground.Child.DesiredSize;
                if (p.X < 0 || p.Y < 0 || p.X > size.Width || p.Y > size.Height)
                {
                    if (_lastMouseButton == MouseButton.Right)
                        zbSkillTreeBackground.Reset();
                }
            }
        }

        private void zbSkillTreeBackground_MouseLeave(object sender, MouseEventArgs e)
        {
            // We might have popped up a tooltip while the window didn't have focus,
            // so we should close tooltips whenever the mouse leaves the canvas in addition to
            // whenever we lose focus.
            _sToolTip.IsOpen = false;
        }

        private void zbSkillTreeBackground_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(zbSkillTreeBackground.Child);
            var v = new Vector2D(p.X, p.Y);
            v = v * _multransform + _addtransform;

            var node = Tree.FindNodeInRange(v);
            _hoveredNode = node;
            if (node != null && !node.IsRootNode)
            {
                GenerateTooltipForNode(node);
            }
            else if (Tree.AscendancyButtonRect.Contains(v))
            {
                Tree.DrawAscendancyButton("Highlight");
            }
            else
            {
                _sToolTip.Tag = false;
                _sToolTip.IsOpen = false;
                _prePath = null;
                _toRemove = null;
                Tree?.ClearPath();
                Tree?.ClearJewelHighlight();
                Tree?.DrawAscendancyButton();
            }
        }

        private void zbSkillTreeBackground_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            if (e.SystemGesture == SystemGesture.TwoFingerTap)
                zbSkillTreeBackground.Reset();
        }

        private void GenerateTooltipForNode(PassiveNodeViewModel node, bool forcerefresh = false)
        {
            if (!Tree.DrawAscendancy && node.IsAscendancyNode && !forcerefresh)
                return;
            if (!PersistentData.Options.ShowAllAscendancyClasses && node.IsAscendancyNode &&
                node.AscendancyName != Tree.AscendancyClassName)
                return;

            var socketedJewel = GetSocketedJewel(node);
            if (node.PassiveNodeType == PassiveNodeType.JewelSocket || node.PassiveNodeType == PassiveNodeType.ExpansionJewelSocket)
            {
                Tree.DrawJewelHighlight(node, socketedJewel);
            }

            if (Tree.SkilledNodes.Contains(node))
            {
                _toRemove = Tree.ForceRefundNodePreview(node);
                if (_toRemove != null)
                    Tree.DrawRefundPreview(_toRemove);
            }
            else
            {
                _prePath = Tree.GetShortestPathTo(node);
                Tree.DrawPath(_prePath);
            }
            var tooltip = node.Name;
            if (node.Attributes.Count != 0)
                tooltip += "\n" + node.StatDescriptions.Aggregate((s1, s2) => s1 + "\n" + s2);
            if (!(_sToolTip.IsOpen && _lasttooltip == tooltip) | forcerefresh)
            {
                _sToolTip.Content = CreateTooltipForNode(node, tooltip, socketedJewel);
                if (!HighlightByHoverKeys.Any(Keyboard.IsKeyDown))
                {
                    _sToolTip.IsOpen = true;
                }
                _lasttooltip = tooltip;
            }
        }

        private Item? GetSocketedJewel(PassiveNodeViewModel node) =>
            node.PassiveNodeType == PassiveNodeType.JewelSocket || node.PassiveNodeType == PassiveNodeType.ExpansionJewelSocket
            ? ItemAttributes.GetItemInSlot(ItemSlot.SkillTree, node.Id)
            : null;

        private object CreateTooltipForNode(PassiveNodeViewModel node, string tooltip, Item? socketedJewel)
        {
            var sp = new StackPanel();

            if (node.PassiveNodeType == PassiveNodeType.Mastery)//Add extra Details for Potential Mastery Choices
            {
                tooltip += "\n\nMastery Node Options are(Ctrl+Alt+LClick to choose):";
                switch (node.Name)
                {//Description and number of counts listed on https://poedb.tw/us/Passive_mastery
                    case "Life Mastery":
                        tooltip += "\n-||[10% increased maximum Life\n-||10% reduced Life Recovery rate]";
                        tooltip += "\n-||[Regenerate 2% of Life per second while moving]";
                        tooltip += "\n-||[+50 to maximum Life]";
                        tooltip += "\n-||[15% reduced Life Cost of Skills]";
                        tooltip += "\n-||[50% increased Life Recovery from Flasks used when on Low Life]";
                        tooltip += "\n-||[Vitality has 100% increased Mana Reservation Efficiency]";
                        break;
                    case "Mana Mastery":
                        tooltip += "\n-||[Regenerate 5 Mana per second while you have Arcane Surge]";
                        tooltip += "\n-||[Recover 10% of Mana over 1 second when you use a Guard Skill]";
                        tooltip += "\n-||[10% of Damage taken Recouped as Mana]";
                        tooltip += "\n-||[10% reduced Mana Cost of Skills]";
                        tooltip += "\n-||[10% chance to Recover 10% of Mana when you use a Skill]";
                        tooltip += "\n-||[Clarity has 100% increased Mana Reservation Efficiency]";
                        break;
                    case "Attack Mastery":
                        tooltip += "\n-||[+3 to Melee Strike Range]";
                        tooltip += "\n-||[Strike Skills which target additional Enemies can do so from 30% further away]";
                        tooltip += "\n-||[50% increased Mana Reservation Efficiency of Stance Skills]";
                        tooltip += "\n-||[40% increased Melee Damage with Hits at Close Range]";
                        tooltip += "\n-||[1% increased Attack Damage per 16 Strength]";
                        tooltip += "\n-||[Ruthless Hits Intimidate Enemies for 4 seconds]";
                        break;
                    case "Flask Mastery":
                        tooltip += "\n-||[Recover 10% of Life when you use a Life Flask while on Low Life]";
                        tooltip += "\n-||[Utility Flasks gain 1 Charge every 3 seconds]";
                        tooltip += "\n-||[Life Flasks gain 1 Charge every 3 seconds\n-||Mana Flasks gain 1 Charge every 3 seconds]";
                        tooltip += "\n-||[Flasks applied to you have 10% increased Effect]";
                        tooltip += "\n-||[Remove a random Elemental Ailment when you use a Mana Flask]";
                        tooltip += "\n-||[Remove a random non-Elemental Ailment when you use a Life Flask]";
                        break;
                    case "Leech Mastery":
                        tooltip += "\n-||[20% increased Maximum total Life Recovery per second from Leech]";
                        tooltip += "\n-||[30% increased Maximum total Mana Recovery per second from Leech]";
                        tooltip += "\n-||[30% increased Maximum total Energy Shield Recovery per second from Leech]";
                        tooltip += "\n-||[30% increased Damage while Leeching]";
                        tooltip += "\n-||[100% increased total Recovery per second from Life, Mana, or Energy Shield Leech]";
                        break;
                    case "Caster Mastery":
                        tooltip += "\n-||[Final Repeat of Spells has 30% increased Area of Effect]";
                        tooltip += "\n-||[Spells cause you to gain Mana equal to their Upfront Cost every fifth time you Pay it]";
                        tooltip += "\n-||[Cannot be Chilled or Frozen while Casting a Spell]";
                        tooltip += "\n-||[Spells which can gain Intensity have +1 to maximum Intensity]";
                        tooltip += "\n-||[Skills supported by Unleash have +1 to maximum number of Seals]";
                        tooltip += "\n-||[1% increased Spell Damage per 16 Intelligence]";
                        break;
                    case "Minion Offence Mastery":
                        tooltip += "\n-||[Minions Attacks Overwhelm 20% Physical Damage Reduction]";
                        tooltip += "\n-||[Minions Penetrate 8% of Cursed Enemies' Elemental Resistances]";
                        tooltip += "\n-||[Minions have 25% chance to gain Unholy Might for 4 seconds on Kill]";
                        tooltip += "\n-||[Minions have 100% increased Critical Strike Chance]";
                        tooltip += "\n-||[Minions have +250 to Accuracy Rating]";
                        tooltip += "\n-||[20% increased effect of Offerings]";
                        break;
                    case "Critical Mastery":
                        tooltip += "\n-||[50% increased Effect of non-Damaging Ailments you inflict with Critical Strikes]";
                        tooltip += "\n-||[+25% to Critical Strike Multiplier against Unique Enemies]";
                        tooltip += "\n-||[Stuns from Critical Strikes have 100% increased Duration]";
                        tooltip += "\n-||[+3 to Level of all Critical Support Gems]";
                        tooltip += "\n-||[You take 30% reduced Extra Damage from Critical Strikes]";
                        tooltip += "\n-||[150% increased Critical Strike Chance against Enemies on Full Life]";
                        break;
                    case "Fire Mastery":
                        tooltip += "\n-||[Fire Exposure you inflict applies an extra -5% to Fire Resistance]";
                        tooltip += "\n-||[Recover 2% of Life when you Ignite a non-Ignited Enemy]";
                        tooltip += "\n-||[40% of Physical Damage Converted to Fire Damage]";
                        tooltip += "\n-||[20% chance to cover Enemies in Ash when they Hit you]";
                        tooltip += "\n-||[1% increased Fire Damage per 20 Strength]";
                        tooltip += "\n-||[+20% to Fire Damage over Time Multiplier\n-||-30% to Fire Resistance]";
                        break;
                    case "Elemental Mastery":
                        tooltip += "\n-||[Exposure you inflict applies at least -18% to the affected Resistance]";
                        tooltip += "\n-||[60% reduced Reflected Elemental Damage taken]";
                        tooltip += "\n-||[Gain 10% of Physical Damage as Extra Damage of a random Element]";
                        tooltip += "\n-||[40% increased Effect of Non-Damaging Ailments]";
                        tooltip += "\n-||[+15% to all Elemental Resistances]";
                        tooltip += "\n-||[1% of Elemental Damage Leeched as Energy Shield]";
                        break;
                    case "Energy Shield Mastery":
                        tooltip += "\n-||[30% increased Light Radius\n-||Light Radius is based on Energy Shield instead of Life]";
                        tooltip += "\n-||[Cannot be Frozen if Energy Shield Recharge has started Recently]";
                        tooltip += "\n-||[Stun Threshold is based on 50% of your Energy Shield instead of Life]";
                        tooltip += "\n-||[Regenerate 2% of Energy Shield per second]";
                        tooltip += "\n-||[Gain 3% of Maximum Mana as Extra Maximum Energy Shield]";
                        tooltip += "\n-||[Discipline has 25% increased Mana Reservation Efficiency]";
                        break;
                    case "Physical Mastery":
                        tooltip += "\n-||[War Banner has 100% increased Mana Reservation Efficiency]";
                        tooltip += "\n-||[20% chance to Impale Enemies on Hit]";
                        tooltip += "\n-||[60% reduced Reflected Physical Damage taken]";
                        tooltip += "\n-||[Overwhelm 15% Physical Damage Reduction]";
                        tooltip += "\n-||[40% increased Physical Damage with Skills that Cost Life]";
                        tooltip += "\n-||[Skills that leave Lingering Blades have 20% chance to leave two Lingering Blades instead of one\n-||Skills that leave Lingering Blades have +10 to Maximum Lingering Blades]";
                        break;
                    case "Mine Mastery":
                        tooltip += "\n-||[Each Mine applies 2% increased Damage taken to Enemies near it, up to 10%]";
                        tooltip += "\n-||[Each Mine applies 2% reduced Damage dealt to Enemies near it, up to 10%]";
                        tooltip += "\n-||[30% increased Effect of Auras from Mines]";
                        tooltip += "\n-||[Summoned Skitterbots have 100% increased Cooldown Recovery]";
                        tooltip += "\n-||[Mines cannot be Damaged]";
                        tooltip += "\n-||[Regenerate 2.5% of Life per Second if you've Detonated a Mine Recently]";
                        break;
                    case "Reservation Mastery":
                        tooltip += "\n-||[8% increased Damage for each of your Aura or Herald Skills affecting you]";
                        tooltip += "\n-||[15% increased Mana Reservation Efficiency of Skills]";
                        tooltip += "\n-||[30% increased Life Reservation Efficiency of Skills]";
                        tooltip += "\n-||[30% increased Area of Effect of Aura Skills]";
                        tooltip += "\n-||[Auras from your Skills have 15% increased Effect on you]";
                        tooltip += "\n-||[Non-Curse Aura Skills have 50% increased Duration]";
                        break;
                    case "Resistance and Ailment Protection Mastery":
                        tooltip += "\n-||[Corrupted Blood cannot be inflicted on you]";
                        tooltip += "\n-||[You cannot be Maimed\n-||You cannot be Hindered]";
                        tooltip += "\n-||[You cannot be Impaled]";
                        tooltip += "\n-||[20% chance to Avoid being Stunned]";
                        tooltip += "\n-||[20% reduced Effect of Curses on you]";
                        tooltip += "\n-||[+12% to all Elemental Resistances\n-||+7% to Chaos Resistance]";
                        break;
                    case "Armour Mastery":
                        tooltip += "\n-||[+600 to Armour while affected by a Guard Skill Buff]";
                        tooltip += "\n-||[You take 30% reduced Extra Damage from Critical Strikes]";
                        tooltip += "\n-||[20% chance to Defend with 200% of Armour]";
                        tooltip += "\n-||[50% increased Stun Threshold]";
                        tooltip += "\n-||[Determination has 25% increased Mana Reservation Efficiency]";
                        tooltip += "\n-||[100% increased Armour from Equipped Shield]";
                        break;
                    case "Bow Mastery":
                        tooltip += "\n-||[Blink Arrow and Mirror Arrow have 60% increased Cooldown Recovery Rate]";
                        tooltip += "\n-||[20% increased Area of Effect while wielding a Bow]";
                        tooltip += "\n-||[Arrows gain Critical Strike Chance as they travel farther, up to 100% increased Critical Strike Chance]";
                        tooltip += "\n-||[50% increased Mirage Archer Duration]";
                        tooltip += "\n-||[8% increased Movement Speed while Phasing\n-||20% chance to gain Phasing for 4 seconds on Kill]";
                        tooltip += "\n-||[35% increased Damage while you are wielding a Bow and have a Totem]";
                        break;
                    case "Cold Mastery":
                        tooltip += "\n-||[Enemies Become Chilled as they Unfreeze, causing 30% reduced Action Speed]";
                        tooltip += "\n-||[40% of Physical Damage Converted to Cold Damage]";
                        tooltip += "\n-||[60% increased Freeze Duration on Enemies]";
                        tooltip += "\n-||[25% chance to gain a Frenzy Charge when you Shatter an Enemy]";
                        tooltip += "\n-||[Cold Exposure you inflict applies an extra -5% to Cold Resistance]";
                        tooltip += "\n-||[Curses on Enemies in your Chilling Areas have 15% increased Effect]";
                        break;
                    case "Curse Mastery":
                        tooltip += "\n-||[+20% chance to Ignite, Freeze, Shock, and Poison Cursed Enemies]";
                        tooltip += "\n-||[20% chance for Hexes you Cast which can gain Doom to be applied with Maximum Doom]";
                        tooltip += "\n-||[Non-Cursed Enemies you inflict Non-Aura Curses on are Blinded for 4 seconds]";
                        tooltip += "\n-||[Your Curses have 20% increased Effect if 50% of Curse Duration expired]";
                        tooltip += "\n-||[Enemies you Curse are Hindered, with 15% reduced Movement Speed]";
                        tooltip += "\n-||[30% increased Mana Reservation Efficiency of Curse Aura Skills]";
                        break;
                    case "Evasion Mastery":
                        tooltip += "\n-||[Cannot be Stunned if you haven't been Hit Recently]";
                        tooltip += "\n-||[40% increased Evasion Rating if you have been Hit Recently]";
                        tooltip += "\n-||[10% increased Movement Speed if you haven't taken Damage Recently]";
                        tooltip += "\n-||[30% chance to Avoid being Poisoned\n-||30% chance to Avoid Bleeding\n-||30% chance to Avoid being Impaled]";
                        tooltip += "\n-||[Grace has 25% increased Mana Reservation Efficiency]";
                        tooltip += "\n-||[100% increased Evasion Rating from your Body Armour]";
                        break;
                    case "Mace Mastery":
                        tooltip += "\n-||[All Damage with Maces and Sceptres inflicts Chill]";
                        tooltip += "\n-||[20% increased Area of Effect if you've dealt a Critical Strike Recently]";
                        tooltip += "\n-||[Crush Enemies on hit with Maces and Sceptres]";
                        tooltip += "\n-||[12% chance to deal Double Damage with Attacks if Attack Time is longer than 1 second]";
                        tooltip += "\n-||[50% increased Stun Duration on Enemies]";
                        tooltip += "\n-||[Hits that Stun Enemies have Culling Strike]";
                        break;
                    case "Minion Defence Mastery":
                        tooltip += "\n-||[Minions have +8% to all maximum Elemental Resistances]";
                        tooltip += "\n-||[Link Skills can target Damageable Minions]";
                        tooltip += "\n-||[Minions Leech 1% of Damage as Life]";
                        tooltip += "\n-||[Convocation has 40% increased Cooldown Recovery Rate]";
                        tooltip += "\n-||[Minions have 15% reduced Life Recovery rate\n-||Minions have 30% increased maximum Life]";
                        tooltip += "\n-||[Minions Recover 5% of Life on Minion Death]";
                        break;
                    case "Shield Mastery":
                        tooltip += "\n-||[Counterattacks have a 50% chance to Debilitate on Hit for 1 second]";
                        tooltip += "\n-||[+1% Chance to Block Attack Damage per 5% Chance to Block on Equipped Shield]";
                        tooltip += "\n-||[Intimidate Enemies for 4 seconds on Block while holding a Shield]";
                        tooltip += "\n-||[20% chance to Avoid Elemental Ailments while holding a Shield]";
                        tooltip += "\n-||[2% increased Attack Damage per 75 Armour or Evasion Rating on Shield]";
                        tooltip += "\n-||[+1% to Critical Strike Multiplier per 10 Maximum Energy Shield on Shield]";
                        break;
                    case "Staff Mastery":
                        tooltip += "\n-||[Recover 2% of Energy Shield when you Block Spell Damage while wielding a Staff\n-||Recover 2% of Life when you Block Attack Damage while wielding a Staff]";
                        tooltip += "\n-||[30% increased Defences while wielding a Staff]";
                        tooltip += "\n-||[+8% Chance to Block Attack Damage if you've Stunned an Enemy Recently]";
                        tooltip += "\n-||[20% chance to double Stun Duration]";
                        tooltip += "\n-||[Gain Unholy Might on block for 3 seconds]";
                        tooltip += "\n-||[+60% to Critical Strike Multiplier if you haven't dealt a Critical Strike Recently]";
                        break;
                    case "Trap Mastery":
                        tooltip += "\n-||[5% chance to throw up to 4 additional Traps]";
                        tooltip += "\n-||[Summon Skitterbots has 50% increased Mana Reservation Efficiency]";
                        tooltip += "\n-||[Can have up to 5 additional Traps placed at a time]";
                        tooltip += "\n-||[60% increased Trap Trigger Area of Effect]";
                        tooltip += "\n-||[Recover 30 Life when your Trap is triggered by an Enemy]";
                        tooltip += "\n-||[Traps cannot be Damaged]";
                        break;
                    case "Accuracy Mastery":
                        tooltip += "\n-||[30% increased Accuracy Rating if you haven't Killed Recently]";
                        tooltip += "\n-||[Precision has 100% increased Mana Reservation Efficiency]";
                        tooltip += "\n-||[Dexterity's Accuracy Bonus instead grants +3 to Accuracy Rating per Dexterity]";
                        tooltip += "\n-||[40% increased Accuracy Rating against Unique Enemies]";
                        break;
                    case "Axe Mastery":
                        tooltip += "\n-||[Enemies you hit are destroyed on Kill]";
                        tooltip += "\n-||[Bleeding you inflict deals Damage 15% faster]";
                        tooltip += "\n-||[40% increased Onslaught Effect]";
                        tooltip += "\n-||[30% increased Damage while in Blood Stance\n-||15% increased Area of Effect while in Sand Stance]";
                        tooltip += "\n-||[10% more Damage with Hits and Ailments against Enemies that are on Low Life]";
                        tooltip += "\n-||[Attacks with Axes or Swords grant 1 Rage on Hit, no more than once every second]";
                        break;
                    case "Charge Mastery":
                        tooltip += "\n-||[Cannot be Ignited while at maximum Endurance Charges]";
                        tooltip += "\n-||[Cannot be Chilled while at maximum Frenzy Charges]";
                        tooltip += "\n-||[Cannot be Shocked while at maximum Power Charges]";
                        tooltip += "\n-||[100% increased Charge Duration]";
                        tooltip += "\n-||[3% increased Damage per Endurance, Frenzy or Power Charge]";
                        break;
                    case "Claw Mastery":
                        tooltip += "\n-||[1% of Attack Damage Leeched as Life\n-||1% of Attack Damage Leeched as Mana]";
                        tooltip += "\n-||[20% chance to Blind Enemies on Critical Strike]";
                        tooltip += "\n-||[+10 Life gained for each Enemy hit by your Attacks\n-||+5 Mana gained for each Enemy hit by your Attacks]";
                        tooltip += "\n-||[6% chance to gain a Frenzy Charge and a Power Charge on Kill]";
                        tooltip += "\n-||[Enemies Poisoned by you cannot deal Critical Strikes]";
                        tooltip += "\n-||[Skills Supported by Nightblade have 40% increased Effect of Elusive]";
                        break;
                    case "Dagger Mastery":
                        tooltip += "\n-||[+100% to Critical Strike Multiplier against Enemies that are on Full Life]";
                        tooltip += "\n-||[Critical Strikes have Culling Strike]";
                        tooltip += "\n-||[+8% chance to Suppress Spell Damage for each Dagger you're Wielding]";
                        tooltip += "\n-||[8% more Damage with Hits and Ailments against Enemies affected by at least 5 Poisons]";
                        tooltip += "\n-||[15% more Maximum Physical Attack Damage with Daggers]";
                        tooltip += "\n-||[Elusive also grants +40% to Critical Strike Multiplier for Skills Supported by Nightblade]";
                        break;
                    case "Damage Over Time Mastery":
                        tooltip += "\n-||[30% increased Effect of Cruelty]";
                        tooltip += "\n-||[+10% to Damage over Time Multiplier if you've Killed Recently]";
                        tooltip += "\n-||[15% increased Duration of Ailments on Enemies\n-||15% increased Skill Effect Duration]";
                        tooltip += "\n-||[10% less Damage Taken from Damage over Time]";
                        break;
                    case "Lightning Mastery":
                        tooltip += "\n-||[40% of Physical Damage Converted to Lightning Damage]";
                        tooltip += "\n-||[Lightning Damage with Non-Critical Strikes is Lucky]";
                        tooltip += "\n-||[Your Shocks can increase Damage taken by up to a maximum of 60%]";
                        tooltip += "\n-||[80% increased Critical Strike Chance against Shocked Enemies]";
                        tooltip += "\n-||[Non-Projectile Chaining Lightning Skills Chain +1 times]";
                        tooltip += "\n-||[Increases and reductions to Maximum Mana also apply to Shock Effect at 30% of their value]";
                        break;
                    case "Poison Mastery":
                        tooltip += "\n-||[Poisons you inflict on non-Poisoned Enemies deal 300% increased Damage]";
                        tooltip += "\n-||[Poisons you inflict deal Damage 20% faster]";
                        tooltip += "\n-||[+12% to Damage over Time Multiplier for Poison you inflict on Bleeding Enemies]";
                        tooltip += "\n-||[20% increased Poison Duration]";
                        tooltip += "\n-||[Recover 3% of Life on Killing a Poisoned Enemy]";
                        tooltip += "\n-||[Plague Bearer has 20% increased Maximum Plague Value]";
                        break;
                    case "Sword Mastery":
                        tooltip += "\n-||[+3 to Melee Strike Range with Swords]";
                        tooltip += "\n-||[20% chance to Impale Enemies on Hit with Attacks]";
                        tooltip += "\n-||[8% chance to gain a Frenzy Charge when you Hit a Unique Enemy]";
                        tooltip += "\n-||[Off Hand Accuracy is equal to Main Hand Accuracy while wielding a Sword]";
                        tooltip += "\n-||[120% increased Critical Strike Chance with Swords\n-||-20% to Critical Strike Multiplier with Swords]";
                        tooltip += "\n-||[50% reduced Enemy Chance to Block Sword Attacks]";
                        break;
                    case "Two Hand Mastery":
                        tooltip += "\n-||[3% chance to deal Triple Damage]";
                        tooltip += "\n-||[40% increased Damage with Hits against Rare and Unique Enemies]";
                        tooltip += "\n-||[10% increased Armour per Red Socket on Main Hand Weapon\n-||10% increased Evasion Rating per Green Socket on Main Hand Weapon]";
                        tooltip += "\n-||[15% more Stun Duration with Two Handed Weapons]";
                        tooltip += "\n-||[Attacks with Two Handed Weapons deal 60% increased Damage with Hits and Ailments\n-||10% reduced Attack Speed]";
                        break;
                    case "Wand Mastery":
                        tooltip += "\n-||[10% chance to gain a Power Charge on Critical Strike with Wands]";
                        tooltip += "\n-||[Unnerve Enemies for 4 seconds on Hit with Wands]";
                        tooltip += "\n-||[Wand Attacks fire an additional Projectile]";
                        tooltip += "\n-||[Increases and Reductions to Spell Damage also apply to Attacks while wielding a Wand]";
                        tooltip += "\n-||[0.5% of Attack Damage Leeched as Life\n-||0.5% of Attack Damage Leeched as Mana]";
                        tooltip += "\n-||[Intelligence is added to Accuracy Rating with Wands]";
                        break;
                    case "Warcry Mastery":
                        tooltip += "\n-||[Exerted Attacks deal 25% increased Damage\n-||Warcries cannot Exert Travel Skills]";
                        tooltip += "\n-||[Remove an Ailment when you Warcry]";
                        tooltip += "\n-||[Recover 15% of Life when you use a Warcry]";
                        tooltip += "\n-||[20% increased Damage for each time you've Warcried Recently]";
                        tooltip += "\n-||[Warcries Debilitate Enemies for 1 second]";
                        tooltip += "\n-||[Warcries have a minimum of 10 Power]";
                        break;
                    case "Spell Suppression Mastery":
                        tooltip += "\n-||[Prevent +2% of Suppressed Spell Damage]";
                        tooltip += "\n-||[Debilitate Enemies for 1 second when you Suppress their Spell Damage]";
                        tooltip += "\n-||[Critical Strike Chance is increased by chance to Suppress Spell Damage]";
                        tooltip += "\n-||[You take 50% reduced Extra Damage from Suppressed Critical Strikes]";
                        tooltip += "\n-||[+10% chance to Suppress Spell Damage if your Boots, Helmet and Gloves have Evasion]";
                        break;
                    case "Impale Mastery":
                        tooltip += "\n-||[Dread Banner has 100% increased Mana Reservation Efficiency]";
                        tooltip += "\n-||[Impale Damage dealt to Enemies Impaled by you Overwhelms 20% Physical Damage Reduction]";
                        tooltip += "\n-||[Call of Steel deals Reflected Damage with 40% increased Area of Effect\n-||Call of Steel has 40% increased Use Speed]";
                        tooltip += "\n-||[Call of Steel has +4 to maximum Steel Shards\n-||Call of Steel causes 10% increased Reflected Damage]";
                        tooltip += "\n-||[20% increased Effect of Impales you inflict on non-Impaled Enemies]";
                        break;
                    case "Block Mastery":
                        tooltip += "\n-||[+2% to maximum Chance to Block Attack Damage]";
                        tooltip += "\n-||[+2% to maximum Chance to Block Spell Damage]";
                        tooltip += "\n-||[+20 Life and Mana gained when you Block]";
                        tooltip += "\n-||[3% increased Spell Damage per 5% Chance to Block Spell Damage]";
                        tooltip += "\n-||[3% increased Attack Damage per 5% Chance to Block Attack Damage]";
                        break;
                    case "Projectile Mastery":
                        tooltip += "\n-||[Projectiles deal 20% increased Damage for each Enemy Pierced]";
                        tooltip += "\n-||[Projectiles deal 20% increased Damage for each time they have Chained]";
                        tooltip += "\n-||[1% increased Projectile Damage per 16 Dexterity]";
                        tooltip += "\n-||[Knock Back Enemies if you get a Critical Strike with Projectile Damage]";
                        tooltip += "\n-||[15% more Projectile Speed]";
                        tooltip += "\n-||[15% less Projectile Speed]";
                        break;
                    case "Brand Mastery":
                        tooltip += "\n-||[Recover 10% of Mana when a Brand expires while Attached]";
                        tooltip += "\n-||[Brands have 30% increased Area of Effect if 50% of Attached Duration expired]";
                        tooltip += "\n-||[Brands Attach to a new Enemy each time they Activate, no more than once every 0.3 seconds]";
                        tooltip += "\n-||[Brand Recall has 50% increased Cooldown Recovery Rate]";
                        tooltip += "\n-||[You can Cast 2 additional Brands]";
                        tooltip += "\n-||[40% increased Brand Attachment range]";
                        break;
                    case "Chaos Mastery":
                        tooltip += "\n-||[+1% to Chaos Damage over Time Multiplier per 4% Chaos Resistance]";
                        tooltip += "\n-||[+1 to Level of all Chaos Skill Gems\n-||Lose 10% of Life and Energy Shield when you use a Chaos Skill]";
                        tooltip += "\n-||[20% increased Effect of Withered]";
                        tooltip += "\n-||[0.5% of Chaos Damage Leeched as Energy Shield]";
                        tooltip += "\n-||[+17% to Chaos Resistance]";
                        tooltip += "\n-||[40% of Physical Damage Converted to Chaos Damage]";
                        break;
                    case "Dual Wielding Mastery":
                        tooltip += "\n-||[+15% Chance to Block Spell Damage while Dual Wielding\n-||Dual Wielding does not inherently grant chance to Block Attack Damage]";
                        tooltip += "\n-||[+1% to Off Hand Critical Strike Chance while Dual Wielding]";
                        tooltip += "\n-||[60% increased Damage while wielding two different Weapon Types]";
                        tooltip += "\n-||[20% chance to gain Elusive when you Block while Dual Wielding]";
                        tooltip += "\n-||[+15% Chance to Block Attack Damage if you have not Blocked Recently]";
                        tooltip += "\n-||[20% chance to Maim Enemies with Main Hand Hits\n-||20% chance to Blind Enemies with Off Hand Hits]";
                        break;
                    case "Attributes Mastery":
                        tooltip += "\n-||[5% increased Attributes]";
                        tooltip += "\n-||[1% increased Damage per 5 of your lowest Attribute]";
                        tooltip += "\n-||[+5 to Strength per Allocated Mastery Passive Skill]";
                        tooltip += "\n-||[+5 to Intelligence per Allocated Mastery Passive Skill]";
                        tooltip += "\n-||[+5 to Dexterity per Allocated Mastery Passive Skill]";
                        break;
                    case "Fortify Mastery":
                        tooltip += "\n-||[Melee Hits Fortify\n-||-3 to maximum Fortification]";
                        tooltip += "\n-||[+3 to maximum Fortification]";
                        tooltip += "\n-||[100% increased Fortification Duration]";
                        tooltip += "\n-||[10% reduced Damage over Time Taken while you have at least 20 Fortification]";
                        break;
                    case "Armour and Energy Shield Mastery":
                        tooltip += "\n-||[+1 to Armour per 1 Maximum Energy Shield on Helmet]";
                        tooltip += "\n-||[Defend with 120% of Armour while not on Low Energy Shield]";
                        tooltip += "\n-||[Increases and Reductions to Armour also apply to Energy Shield Recharge rate at 20% of their value]";
                        tooltip += "\n-||[20% reduced Effect of Curses on you]";
                        break;
                    case "Blind Mastery":
                        tooltip += "\n-||[30% increased Blind Effect]";
                        tooltip += "\n-||[100% increased Blind duration]";
                        tooltip += "\n-||[60% increased Critical Strike Chance against Blinded Enemies]";
                        tooltip += "\n-||[Cannot be Blinded]";
                        break;
                    case "Bleeding Mastery":
                        tooltip += "\n-||[Moving while Bleeding doesn't cause you to take extra Damage]";
                        tooltip += "\n-||[60% increased Damage with Bleeding inflicted on Poisoned Enemies]";
                        tooltip += "\n-||[60% increased Bleeding Duration]";
                        tooltip += "\n-||[80% increased Critical Strike Chance against Bleeding Enemies]";
                        tooltip += "\n-||[+3% to Damage over Time Multiplier for Bleeding per Endurance Charge]";
                        break;
                    case "Mark Mastery":
                        tooltip += "\n-||[25% increased Effect of your Marks]";
                        tooltip += "\n-||[Enemies near your Marked Enemy are Blinded]";
                        tooltip += "\n-||[10% increased Movement Speed if you've cast a Mark Spell Recently]";
                        tooltip += "\n-||[Life Flasks gain a Charge when you hit your Marked Enemy, no more than once every 0.5 seconds]";
                        break;
                    case "Link Mastery":
                        tooltip += "\n-||[Enemies inflict Elemental Ailments on you instead of Linked targets]";
                        tooltip += "\n-||[20% increased Damage per Linked target]";
                        tooltip += "\n-||[Your Linked Targets take 5% reduced Damage]";
                        tooltip += "\n-||[Enemies near your Linked targets have Fire, Cold and Lightning Exposure]";
                        break;
                    case "Duration Mastery":
                        tooltip += "\n-||[10% more Skill Effect Duration]";
                        tooltip += "\n-||[10% less Skill Effect Duration]";
                        tooltip += "\n-||[Debuffs on you expire 15% faster]";
                        tooltip += "\n-||[20% reduced Elemental Ailment Duration on you]";
                        break;
                    case "Evasion and Energy Shield Mastery":
                        tooltip += "\n-||[30% increased Evasion Rating while you have Energy Shield]";
                        tooltip += "\n-||[20% increased Energy Shield Recovery Rate if you haven't been Hit Recently]";
                        tooltip += "\n-||[30% of Chaos Damage does not bypass Energy Shield]";
                        tooltip += "\n-||[+1 to Energy Shield per 8 Evasion on Boots]";
                        break;
                    case "Armour and Evasion Mastery":
                        tooltip += "\n-||[Gain 5% of Evasion Rating as Extra Armour]";
                        tooltip += "\n-||[8% increased Evasion Rating per Frenzy Charge\n-||8% increased Armour per Endurance Charge]";
                        tooltip += "\n-||[Defiance Banner has 100% increased Mana Reservation Efficiency]";
                        tooltip += "\n-||[+1 to Evasion Rating per 1 Armour on Gloves]";
                        break;
                    default:
                        tooltip += "\n-||(Not Listed Yet)";
                        break;
                }
            }

            if (socketedJewel is null)
            {
                sp.Children.Add(new TextBlock { Text = tooltip });
            }
            else
            {
                sp.Children.Add(new ItemTooltip { DataContext = socketedJewel });
                ComputationViewModel!.AttributesInJewelRadius.Calculate(
                    node.Id, socketedJewel.JewelRadius, socketedJewel.ExplicitMods.Select(m => m.Attribute).ToList());
                sp.Children.Add(new AttributesInJewelRadiusView { DataContext = ComputationViewModel.AttributesInJewelRadius });
            }

            if (node.ReminderText != null && node.ReminderText.Any())
            {
                sp.Children.Add(new Separator());
                sp.Children.Add(new TextBlock { Text = node.ReminderText.Aggregate((s1, s2) => s1 + '\n' + s2) });
            }

            if (_prePath != null)
            {
                var points = _prePath.Count(n => !n.IsAscendancyStart && !Tree.SkilledNodes.Contains(n));
                sp.Children.Add(new Separator());
                sp.Children.Add(new TextBlock { Text = "Points to skill node: " + points });
            }

            //Change summary, activated with ctrl
            if (PersistentData.Options.ChangeSummaryEnabled)
            {
                //Sum up the total change to attributes and add it to the tooltip
                if (_prePath != null | _toRemove != null)
                {

                    var attributechanges = new Dictionary<string, List<float>>();

                    int changedNodes;

                    if (_prePath != null)
                    {
                        var nodesToAdd = _prePath.Where(n => !Tree.SkilledNodes.Contains(n)).ToList();
                        attributechanges = SkillTree.GetAttributesWithoutImplicitNodesOnly(nodesToAdd);
                        tooltip = "Total gain:";
                        changedNodes = nodesToAdd.Count;
                    }
                    else if (_toRemove != null)
                    {
                        attributechanges = SkillTree.GetAttributesWithoutImplicitNodesOnly(_toRemove);
                        tooltip = "Total loss:";
                        changedNodes = _toRemove.Count;
                    }
                    else
                    {
                        changedNodes = 0;
                    }

                    if (changedNodes > 1)
                    {
                        foreach (var attrchange in attributechanges)
                        {
                            if (attrchange.Value.Count != 0)
                            {
                                var regex = new Regex(Regex.Escape("#"));
                                var attr = attrchange.Key;
                                foreach (var val in attrchange.Value)
                                    attr = regex.Replace(attr, val.ToString(), 1);
                                tooltip += "\n" + attr;
                            }
                        }

                        sp.Children.Add(new Separator());
                        sp.Children.Add(new TextBlock { Text = tooltip });
                    }
                }
            }

            return sp;
        }

        private void HighlightNodesByHover()
        {
            if (Tree == null)
            {
                return;
            }

            if (_hoveredNode == null || _hoveredNode.Attributes.Count == 0 ||
                !HighlightByHoverKeys.Any(Keyboard.IsKeyDown))
            {
                if (_hoveredNode != null && _hoveredNode.Attributes.Count > 0)
                {
                    _sToolTip.IsOpen = true;
                }

                Tree.HighlightNodesBySearch("", true, NodeHighlighter.HighlightState.FromHover);

                _lastHoveredNode = null;
            }
            else
            {
                _sToolTip.IsOpen = false;

                if (_lastHoveredNode == _hoveredNode)
                {
                    // Not necessary, but stops it from continuously searching when holding down shift.
                    return;
                }

                var search = _hoveredNode.Attributes.Aggregate("^(", (current, attr) => current + (attr.Key + "|"));
                search = search.Substring(0, search.Length - 1);
                search += ")$";
                search = Regex.Replace(search, @"(\+|\-|\%)", @"\$1");
                search = Regex.Replace(search, @"\#", @"[0-9]*\.?[0-9]+");

                Tree.HighlightNodesBySearch(search, true, NodeHighlighter.HighlightState.FromHover,
                    _hoveredNode.Attributes.Count); // Remove last parameter to highlight nodes with any of the attributes.

                _lastHoveredNode = _hoveredNode;
            }
        }

        #endregion

        #region Items

        private bool _pauseLoadItemData;

        private async Task LoadItemData()
        {
            if (_pauseLoadItemData)
                return;

            _jewelSocketObserver.ResetTreeJewelViewModels();
            _abyssalSocketObserver?.ResetItemJewelViewModels();
            if (ItemAttributes != null)
            {
                ItemAttributes.ItemDataChanged -= ItemAttributesOnItemDataChanged;
                ItemAttributes.Dispose();
            }
            SkillsEditingViewModel?.Dispose();
            SkillTreeAreaViewModel?.Dispose();
            InventoryViewModel?.Dispose();

            var equipmentData = PersistentData.EquipmentData;
            var itemData = PersistentData.CurrentBuild.ItemData;
            var skillDefinitions = await _gameData.Skills;
            ItemAttributes itemAttributes;
            try
            {
                itemAttributes = new ItemAttributes(equipmentData, skillDefinitions, itemData);
            }
            catch (Exception ex)
            {
                itemAttributes = new ItemAttributes(equipmentData, skillDefinitions);
                await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to load item data."),
                    ex.Message);
            }

            itemAttributes.ItemDataChanged += ItemAttributesOnItemDataChanged;
            _equipmentConverter?.ConvertFrom(itemAttributes);
            ItemAttributes = itemAttributes;
            InventoryViewModel =
                new InventoryViewModel(_dialogCoordinator, itemAttributes, await GetJewelPassiveNodesAsync());
            SkillTreeAreaViewModel = new SkillTreeAreaViewModel(SkillTree.Skillnodes, InventoryViewModel.TreeJewels);
            SkillsEditingViewModel = new SkillsEditingViewModel(skillDefinitions, equipmentData.ItemImageService, itemAttributes);
            _abyssalSocketObserver?.SetItemJewelViewModels(InventoryViewModel.ItemJewels);
            Tree.JewelViewModels = InventoryViewModel.TreeJewels;
            UpdateUI();
        }

        private void ItemAttributesOnItemDataChanged(object? sender, EventArgs args)
        {
            _pauseLoadItemData = true;
            PersistentData.CurrentBuild.ItemData = ItemAttributes.ToJsonString();
            _pauseLoadItemData = false;
        }

        private async Task<IEnumerable<ushort>> GetJewelPassiveNodesAsync()
        {
            var treeDefinition = await _gameData.PassiveTree;
            return treeDefinition.Nodes
                .Where(d => d.Type == PassiveNodeType.JewelSocket || d.Type == PassiveNodeType.ExpansionJewelSocket)
                .Where(d => !d.Name.StartsWith("Small") && !d.Name.StartsWith("Medium"))
                .Select(d => d.Id);
        }

        #endregion

        #region Builds - Services

        private async Task CurrentBuildChanged()
        {
            var build = PersistentData.CurrentBuild;
            InputTreeUrl = PersistentData.CurrentBuild.TreeUrl;
            Tree.ResetTaggedNodes();
            TreeGeneratorInteraction?.LoadSettings();
            await LoadItemData();
            SetCustomGroups(build.CustomGroups);
            await ResetTreeUrl();
            _jewelSocketObserver.SetTreeJewelViewModels(InventoryViewModel.TreeJewels);
            ComputationViewModel?.SharedConfiguration.SetBandit(build.Bandits.Choice);
        }

        /// <summary>
        /// Call this to set CurrentBuild.TreeUrl when there were direct SkillTree changes.
        /// </summary>
        private void SetCurrentBuildUrlFromTree()
        {
            _skipLoadOnCurrentBuildTreeChange = true;
            PersistentData.CurrentBuild.TreeUrl = Tree.EncodeUrl();
            _skipLoadOnCurrentBuildTreeChange = false;
        }

        private Task ResetTreeUrl()
        {
            return SetTreeUrl(PersistentData.CurrentBuild.TreeUrl);
        }

        private async Task SetTreeUrl(string treeUrl)
        {
            try
            {
                // If the url did change, it'll run through this method again anyway.
                // So no need to call Tree.LoadFromUrl in that case.
                if (PersistentData.CurrentBuild.TreeUrl == treeUrl)
                    Tree.LoadFromUrl(treeUrl);
                else
                    PersistentData.CurrentBuild.TreeUrl = treeUrl;

                if (_justLoaded)
                {
                    if (_undoList.Count > 1)
                    {
                        var holder = _undoList.Pop();
                        _undoList.Clear();
                        _undoList.Push(holder);
                    }
                }
                else
                {
                    UpdateClass();
                    Tree.UpdateAscendancyClasses = true;
                    PopulateAscendancySelectionList();
                }
                UpdateUI();
                _justLoaded = false;
            }
            catch (Exception ex)
            {
                PersistentData.CurrentBuild.TreeUrl = Tree.EncodeUrl();
                await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to load Skill tree from URL."), ex.Message);
            }
        }

        private async Task LoadBuildFromUrlAsync(string treeUrl)
        {
            try
            {
                var currentUrl = Tree.EncodeUrl();
                var normalizedUrl = await _buildUrlNormalizer.NormalizeAsync(treeUrl, AwaitAsyncTask);
                var data = Tree.DecodeUrl(normalizedUrl);

                if (!data.IsValid && data.CompatibilityIssues.Any())
                {
                    await this.ShowWarningAsync(string.Join(Environment.NewLine, data.CompatibilityIssues));
                    normalizedUrl = currentUrl;
                }

                PersistentData.CurrentBuild.TreeUrl = normalizedUrl;
                InputTreeUrl = normalizedUrl;
            }
            catch (Exception ex)
            {
                PersistentData.CurrentBuild.TreeUrl = Tree.EncodeUrl();
                await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to load Skill tree from URL."), ex.Message);
            }
        }
        #endregion

        #region Bottom Bar (Build URL etc)

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchUpdate();
        }

        private void cbRegEx_Click(object sender, RoutedEventArgs e)
        {
            SearchUpdate();
        }

        private void SearchUpdate()
        {
            Tree.HighlightNodesBySearch(tbSearch.Text, cbRegEx.IsChecked != null && cbRegEx.IsChecked.Value, NodeHighlighter.HighlightState.FromSearch);
        }

        public void ClearSearch()
        {
            tbSearch.Text = "";
            SearchUpdate();
        }

        private void tbSkillURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            _undoList.Push(PersistentData.CurrentBuild.TreeUrl);
        }

        private bool CanUndoTreeUrlChange() =>
            _undoList.Any(s => s != PersistentData.CurrentBuild.TreeUrl);

        private void UndoTreeUrlChange()
        {
            if (_undoList.Count <= 0) return;
            if (_undoList.Peek() == PersistentData.CurrentBuild.TreeUrl && _undoList.Count > 1)
            {
                _undoList.Pop();
                UndoTreeUrlChange();
            }
            else if (_undoList.Peek() != PersistentData.CurrentBuild.TreeUrl)
            {
                _redoList.Push(PersistentData.CurrentBuild.TreeUrl);
                PersistentData.CurrentBuild.TreeUrl = _undoList.Pop();
                UpdateUI();
            }
        }

        private bool CanRedoTreeUrlChange() =>
            _redoList.Any(s => s != PersistentData.CurrentBuild.TreeUrl);

        private void RedoTreeUrlChange()
        {
            if (_redoList.Count <= 0) return;
            if (_redoList.Peek() == PersistentData.CurrentBuild.TreeUrl && _redoList.Count > 1)
            {
                _redoList.Pop();
                RedoTreeUrlChange();
            }
            else if (_redoList.Peek() != PersistentData.CurrentBuild.TreeUrl)
            {
                PersistentData.CurrentBuild.TreeUrl = _redoList.Pop();
                UpdateUI();
            }
        }

        public async Task DownloadPoeUrlAsync()
        {
            var regx =
                new Regex(
                    @"https?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?",
                    RegexOptions.IgnoreCase);

            var matches = regx.Matches(PersistentData.CurrentBuild.TreeUrl);

            if (matches.Count == 1)
            {
                try
                {
                    var url = matches[0].ToString();
                    if (!url.ToLower().StartsWith(Constants.TreeAddress))
                    {
                        return;
                    }
                    // PoEUrl can't handle https atm.
                    url = url.Replace("https://", "http://");

                    var result =
                        await AwaitAsyncTask(L10n.Message("Generating PoEUrl of Skill tree"),
                            _httpClient.GetStringAsync("http://poeurl.com/shrink.php?url=" + url));
                    await ShowPoeUrlMessageAndAddToClipboard("http://poeurl.com/" + result.Trim());
                }
                catch (Exception ex)
                {
                    await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to contact the PoEUrl location."), ex.Message);
                }
            }
        }

        private async Task ShowPoeUrlMessageAndAddToClipboard(string poeurl)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetDataObject(poeurl, true);
                await this.ShowInfoAsync(L10n.Message("The PoEUrl link has been copied to Clipboard.") + "\n\n" + poeurl);
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync(L10n.Message("An error occurred while copying to Clipboard."), ex.Message);
            }
        }

        #endregion

        #region Theme

        private void InitializeTheme()
        {
            SetTheme();
            SyncThemeManagers(null, null);
            ThemeManager.ThemeChanged += SyncThemeManagers;
        }

        private void SetTheme()
        {
            ThemeManager.ChangeTheme(Application.Current, PersistentData.Options.Theme, PersistentData.Options.Accent);
        }

        private void SyncThemeManagers(object? sender, ThemeChangedEventArgs? args)
        {
            var mahAppsTheme = args?.NewTheme ?? ThemeManager.DetectTheme();
            if (mahAppsTheme != null)
            {
                Fluent.ThemeManager.ChangeTheme(this, mahAppsTheme.Name);
            }
        }

        public IEnumerable<string> AvailableThemes => ThemeManager.BaseColors;

        public IEnumerable<AccentItemViewModel> AvailableAccents =>
            ThemeManager.Themes
                .GroupBy(x => x.ColorScheme)
                .OrderBy(x => x.Key)
                .Select(x => new AccentItemViewModel(x.Key, x.First().ShowcaseBrush));
        #endregion

        private void UpdateTreeComparison()
        {
            if (Tree == null)
                return;

            var build = PersistentData.SelectedBuild as PoEBuild;
            if (build != null && PersistentData.Options.TreeComparisonEnabled)
            {
                SkillTree.DecodeUrl(build.TreeUrl, out var nodes, out var charClass, Tree);

                Tree.HighlightedNodes.Clear();
                Tree.HighlightedNodes.UnionWith(nodes);
                Tree.HighlightedAttributes = SkillTree.GetAttributes(nodes, charClass, build.Level, build.Bandits);
            }
            else
            {
                Tree.HighlightedNodes.Clear();
                Tree.HighlightedAttributes = null;
            }
            UpdateUI();
        }

        private async Task MasteryEffectSelectionAsync(MasteryEffectSelectionViewModel viewModel, BaseDialog view)
        {
            if (!await this.ShowDialogAsync(viewModel, view))
            {
                return;
            }
        }

        public async Task CraftItemAsync()
        {
            await CraftItemAsync(new CraftingViewModel(PersistentData.EquipmentData), new CraftingView());
        }

        public async Task CraftUniqueAsync()
        {
            await CraftItemAsync(new UniqueCraftingViewModel(PersistentData.EquipmentData), new UniqueCraftingView());
        }

        private async Task CraftItemAsync<TBase>(AbstractCraftingViewModel<TBase> viewModel, BaseDialog view)
            where TBase : class, IItemBase
        {
            if (!await this.ShowDialogAsync(viewModel, view))
            {
                return;
            }

            var item = viewModel.Item;
            if (StashViewModel.Items.Count > 0)
            {
                item.Y = StashViewModel.LastOccupiedRow + 1;
            }

            StashViewModel.AddItem(item, true);
        }

        #region Async task helpers

        private void AsyncTaskStarted(string infoText)
        {
            NoAsyncTaskRunning = false;
            TitleStatusTextBlock.Text = infoText;
            TitleStatusButton.Visibility = Visibility.Visible;
        }

        private void AsyncTaskCompleted()
        {
            TitleStatusButton.Visibility = Visibility.Hidden;

            NoAsyncTaskRunning = true;
        }

        private async Task<TResult> AwaitAsyncTask<TResult>(string infoText, Task<TResult> task)
        {
            AsyncTaskStarted(infoText);
            try
            {
                return await task;
            }
            finally
            {
                AsyncTaskCompleted();
            }
        }

        #endregion

        public override IWindowPlacementSettings GetWindowPlacementSettings()
        {
            var settings = base.GetWindowPlacementSettings();
            if (WindowPlacementSettings != null) return settings;

            // Settings just got created, give them a proper SettingsProvider.
            var appSettings = settings as ApplicationSettingsBase;
            if (appSettings == null)
            {
                // Nothing we can do here.
                return settings;
            }
            var provider = new CustomSettingsProvider(appSettings.SettingsKey);
            // This may look ugly, but it is needed and nulls are the only parameter
            // Initialize is ever called with by anything.
            provider.Initialize(null, null);
            appSettings.Providers.Add(provider);
            // Change the provider for each SettingsProperty.
            foreach (var property in appSettings.Properties.Cast<SettingsProperty>())
            {
                property.Provider = provider;
            }
            appSettings.Reload();
            return settings;
        }
    }
}