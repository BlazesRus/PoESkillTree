﻿using Newtonsoft.Json.Linq;
using NLog;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Localization;
using PoESkillTree.Model.Builds;
using PoESkillTree.Model.Items;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;
using PoESkillTree.ViewModels.PassiveTree;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PoESkillTree.ViewModels.Import
{
    public class ImportCharacterViewModel : CloseableViewModel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string ItemsEndpoint = "https://www.pathofexile.com/character-window/get-items?";
        private const string PassiveTreeEndpoint = "https://www.pathofexile.com/character-window/get-passive-skills?";

        private readonly HttpClient _httpClient;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly CurrentLeaguesViewModel _currentLeaguesViewModel;
        private readonly AccountCharactersViewModel _accountCharactersViewModel;
        private readonly ItemAttributes _itemAttributes;
        private readonly SkillTree _skillTree;

        public PoEBuild Build { get; }

        private bool _privateProfile;

        public bool PrivateProfile
        {
            get => _privateProfile;
            set => SetProperty(ref _privateProfile, value, () => OnPropertyChanged(nameof(PublicProfile)));
        }

        public bool PublicProfile => !PrivateProfile;

        private NotifyingTask<IReadOnlyList<string>> _currentLeagues;

        public NotifyingTask<IReadOnlyList<string>> CurrentLeagues
        {
            get => _currentLeagues;
            private set => SetProperty(ref _currentLeagues, value);
        }

        private NotifyingTask<IReadOnlyList<AccountCharacterViewModel>> _accountCharacters;

        public NotifyingTask<IReadOnlyList<AccountCharacterViewModel>> AccountCharacters
        {
            get => _accountCharacters;
            private set => SetProperty(ref _accountCharacters, value);
        }

        private AccountCharacterViewModel? _selectedAccountCharacter;

        public AccountCharacterViewModel? SelectedAccountCharacter
        {
            get => _selectedAccountCharacter;
            set => SetProperty(ref _selectedAccountCharacter, value);
        }

        private NotifyingTask<Unit> _importItemsSkillsAndLevelTask = NotifyingTask.WithDefaultResult<Unit>();

        public NotifyingTask<Unit> ImportItemsSkillsAndLevelTask
        {
            get => _importItemsSkillsAndLevelTask;
            private set => SetProperty(ref _importItemsSkillsAndLevelTask, value);
        }

        private NotifyingTask<Unit> _importPassiveTreeAndJewelsTask = NotifyingTask.WithDefaultResult<Unit>();

        public NotifyingTask<Unit> ImportPassiveTreeAndJewelsTask
        {
            get => _importPassiveTreeAndJewelsTask;
            private set => SetProperty(ref _importPassiveTreeAndJewelsTask, value);
        }

        private ICommand? _importItemsSkillsAndLevelCommand;
        public ICommand ImportItemsSkillsAndLevelCommand =>
            _importItemsSkillsAndLevelCommand ??= new RelayCommand(ImportItemsSkillsAndLevel, CanImportItemsSkillsAndLevel);

        private ICommand? _importItemsCommand;
        public ICommand ImportItemsCommand => _importItemsCommand ??= new RelayCommand(ImportItems, CanImportItemsSkillsAndLevel);

        private ICommand? _importSkillsCommand;
        public ICommand ImportSkillsCommand => _importSkillsCommand ??= new RelayCommand(ImportSkills, CanImportItemsSkillsAndLevel);

        private ICommand? _importLevelCommand;
        public ICommand ImportILevelCommand => _importLevelCommand ??= new RelayCommand(ImportLevel, CanImportItemsSkillsAndLevel);

        private ICommand? _importPassiveTreeAndJewelsCommand;
        public ICommand ImportPassiveTreeAndJewelsCommand =>
            _importPassiveTreeAndJewelsCommand ??= new RelayCommand(ImportPassiveTreeAndJewels, CanImportPassiveTreeAndJewels);

        private ICommand? _importPassiveTreeCommand;
        public ICommand ImportPassiveTreeCommand => _importPassiveTreeCommand ??= new RelayCommand(ImportPassiveTree, CanImportPassiveTreeAndJewels);

        private ICommand? _importJewelsCommand;
        public ICommand ImportJewelsCommand => _importJewelsCommand ??= new RelayCommand(ImportJewels, CanImportPassiveTreeAndJewels);

        public ImportCharacterViewModel(
            HttpClient httpClient, IDialogCoordinator dialogCoordinator,
            ItemAttributes itemAttributes, SkillTree skillTree,
            PoEBuild build, CurrentLeaguesViewModel currentLeagues, AccountCharactersViewModel accountCharacters)
        {
            _httpClient = httpClient;
            _dialogCoordinator = dialogCoordinator;
            _currentLeaguesViewModel = currentLeagues;
            _accountCharactersViewModel = accountCharacters;
            _itemAttributes = itemAttributes;
            _skillTree = skillTree;
            DisplayName = L10n.Message("Import Character");
            Build = build;
            Build.PropertyChanged += BuildOnPropertyChanged;
            _currentLeagues = _currentLeaguesViewModel[Build.Realm];
            _accountCharacters = GetAccountCharacters();
        }

        protected override void OnClose()
        {
            Build.PropertyChanged -= BuildOnPropertyChanged;
        }

        private void BuildOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var propertyName = propertyChangedEventArgs.PropertyName;
            if (propertyName == nameof(PoEBuild.Realm))
            {
                CurrentLeagues = _currentLeaguesViewModel[Build.Realm];
            }

            if (propertyName == nameof(PoEBuild.Realm) || propertyName == nameof(PoEBuild.AccountName) || propertyName == nameof(PoEBuild.League))
            {
                AccountCharacters = GetAccountCharacters();
            }
        }

        private NotifyingTask<IReadOnlyList<AccountCharacterViewModel>> GetAccountCharacters() =>
            _accountCharactersViewModel.Get(Build.Realm, Build.AccountName).Select(SelectAccountCharacters);

        private IReadOnlyList<AccountCharacterViewModel> SelectAccountCharacters(IEnumerable<AccountCharacterViewModel> characters)
        {
            var results = characters
                .Where(c => string.IsNullOrEmpty(Build.League) || c.League == Build.League)
                .OrderBy(c => c.Name)
                .ToList();
            SelectedAccountCharacter = results.FirstOrDefault(c => c.Name == Build.CharacterName);
            return results;
        }

        private bool CanImportItemsSkillsAndLevel() =>
            CanImport() && ImportItemsSkillsAndLevelTask.IsCompleted;

        private bool CanImportPassiveTreeAndJewels() =>
            CanImport() && ImportPassiveTreeAndJewelsTask.IsCompleted;

        private bool CanImport() =>
            !string.IsNullOrEmpty(Build.CharacterName) && (PrivateProfile || !string.IsNullOrEmpty(Build.AccountName));

        private void ImportItemsSkillsAndLevel()
        {
            StartItemSkillsAndLevelImport(L10n.Message("Import Items, Skills and Level"), true, true, true);
        }

        private void ImportItems()
        {
            StartItemSkillsAndLevelImport(L10n.Message("Import Items"), items: true);
        }

        private void ImportSkills()
        {
            StartItemSkillsAndLevelImport(L10n.Message("Import Skills"), skills: true);
        }

        private void ImportLevel()
        {
            StartItemSkillsAndLevelImport(L10n.Message("Import Level"), level: true);
        }

        private async void StartItemSkillsAndLevelImport(string title, bool items = false, bool skills = false, bool level = false)
        {
            ImportItemsSkillsAndLevelTask = NotifyingTask.Create(ImportItemSkillsAndLevelAsync(title, items, skills, level),
                e => Log.Error($"Could not retrieve {ItemsUrl}"));
            await ImportItemsSkillsAndLevelTask.TaskCompletion;
            CommandManager.InvalidateRequerySuggested();
        }

        private async Task<Unit> ImportItemSkillsAndLevelAsync(string title, bool importItems, bool importSkills, bool importLevel)
        {
            var importString = await RequestAsync(ItemsUrl, title);
            if (string.IsNullOrEmpty(importString))
                return Unit.Default;

            if (importItems)
            {
                var toRemove = _itemAttributes.Equip.Where(i => i.Slot != ItemSlot.SkillTree).ToList();
                foreach (var item in toRemove)
                {
                    _itemAttributes.RemoveItem(item);
                }
            }
            if (importSkills)
            {
                _itemAttributes.Gems.Clear();
            }

            var importJson = JObject.Parse(importString);
            if (importItems || importSkills)
            {
                _itemAttributes.DeserializeItemsWithGems(importJson, importItems, importSkills);
            }
            if (importLevel && importJson.TryGetValue("character", out var characterToken))
            {
                Build.Level = characterToken.Value<int>("level");
            }
            return Unit.Default;
        }

        private void ImportPassiveTreeAndJewels()
        {
            StartPassiveTreeAndJewelsImport(L10n.Message("Import Passive Tree and Jewels"), true, true);
        }

        private void ImportPassiveTree()
        {
            StartPassiveTreeAndJewelsImport(L10n.Message("Import Passive Tree"), passiveTree: true);
        }

        private void ImportJewels()
        {
            StartPassiveTreeAndJewelsImport(L10n.Message("Import Jewels"), jewels: true);
        }

        private async void StartPassiveTreeAndJewelsImport(string title, bool passiveTree = false, bool jewels = false)
        {
            ImportPassiveTreeAndJewelsTask = NotifyingTask.Create(ImportPassiveTreeAndJewelsAsync(title, passiveTree, jewels),
                e => Log.Error($"Could not retrieve {PassiveTreeUrl}"));
            await ImportPassiveTreeAndJewelsTask.TaskCompletion;
            CommandManager.InvalidateRequerySuggested();
        }

        private async Task<Unit> ImportPassiveTreeAndJewelsAsync(string title, bool importPassiveTree, bool importJewels)
        {
            var importString = await RequestAsync(PassiveTreeUrl, title);
            if (string.IsNullOrEmpty(importString))
                return Unit.Default;

            var importJson = JObject.Parse(importString);

            if (importPassiveTree && importJson.TryGetValue("hashes", out var nodeHashesJson))
            {
                var passiveNodes = nodeHashesJson.Values<ushort>()
                    .Where(n => SkillTree.Skillnodes.ContainsKey(n))
                    .Select(n => SkillTree.Skillnodes[n])
                    .ToList();
                var (characterClass, ascendancyClass) = GetCharacterAndAscendancyClass(passiveNodes);

                _skillTree.ResetSkilledNodesTo(Array.Empty<PassiveNodeViewModel>());
                _skillTree.SwitchClass(characterClass);
                _skillTree.AscType = ascendancyClass;
                _skillTree.AllocateSkillNodes(passiveNodes);
            }

            if (importJewels)
            {
                var toRemove = _itemAttributes.Equip.Where(i => i.Slot == ItemSlot.SkillTree).ToList();
                foreach (var item in toRemove)
                {
                    _itemAttributes.RemoveItem(item);
                }

                _itemAttributes.DeserializePassiveTreeJewels(importJson);
            }
            return Unit.Default;
        }

        private (CharacterClass characterClass, int ascendancyClass) GetCharacterAndAscendancyClass(IReadOnlyList<PassiveNodeViewModel> nodes)
        {
            if (SelectedAccountCharacter is null)
            {
                var ascendancyClassName = nodes.FirstOrDefault(n => n.IsAscendancyNode)?.AscendancyName;
                if (ascendancyClassName is null)
                {
                    var rootNode = nodes.SelectMany(n => n.NeighborPassiveNodes.Values).FirstOrDefault(n => n.IsRootNode);
                    return (rootNode?.StartingCharacterClass ?? 0, 0);
                }
                else
                {
                    return (_skillTree.AscendancyClasses.GetStartingClass(ascendancyClassName),
                        _skillTree.AscendancyClasses.GetAscendancyClassNumber(ascendancyClassName));
                }
            }
            else
            {
                return ((CharacterClass)SelectedAccountCharacter.ClassId, SelectedAccountCharacter.AscendancyClass);
            }
        }

        private async Task<string?> RequestAsync(string url, string title)
        {
            if (PrivateProfile)
            {
                var message = L10n.Message("A tab in your default web browser has been opened containing your character's data.") + "\n"
                    + L10n.Message("Copy its contents and paste them into the field below.") + "\n"
                    + L10n.Message("In case no browser has been opened, the URL was also copied to your clipboard.");
                var task = _dialogCoordinator.ShowInputAsync(this, title, message);
                Clipboard.SetText(url);
                await Task.Delay(TimeSpan.FromMilliseconds(200));
                Util.OpenInBrowser(url);
                return await task;
            }
            else
            {
                var result = await _httpClient.GetAsync(url);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            }
        }

        private string ItemsUrl => ItemsEndpoint + GetQueryString();
        private string PassiveTreeUrl => PassiveTreeEndpoint + GetQueryString();

        private string GetQueryString()
        {
            var query = $"realm={Build.Realm.ToGGGIdentifier()}&character={Build.CharacterName}";
            if (!string.IsNullOrEmpty(Build.AccountName))
                query += $"&accountName={Build.AccountName}";
            return query;
        }
    }
}