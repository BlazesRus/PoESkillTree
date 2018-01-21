using MoreLinq;
using Newtonsoft.Json.Linq;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.JsonSettings;
using POESKillTree.Model.Serialization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TrackedStatViews;
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

        /// <summary>
        /// Gets or sets the stat tracking save path.
        /// </summary>
        /// <value>
        /// The stat tracking save path.
        /// </value>
        public string StatTrackingSavePath
        {
            get
            {
                if (GlobalSettings.StatTrackingSavePath == null)
                {
                    return GlobalSettings.DefaultTrackingDir;
                }
                else
                {
                    return GlobalSettings.StatTrackingSavePath;
                }
            }
            set
            {
                SetProperty(ref GlobalSettings.StatTrackingSavePath, value, () =>
                {
                    if (value != null && value != "" && StatTrackingSavePath != value)
                    {
                        SetProperty(ref GlobalSettings.StatTrackingSavePath, value);
                    }
                });
            }
        }

        #region Menu Binding Properties

        public ObservableCollection<StringData> SourceList
        {
            get;
            set;
        }

        #endregion Menu Binding Properties

        #region ICommand Initialization

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
        public TrackedStatsMenuModel(IPersistentData persistentData, IDialogCoordinator dialogCoordinator, BuildsControlViewModel buildsControlViewModel)
        {
            _persistentData = persistentData;
            _dialogCoordinator = dialogCoordinator;
            _buildsControlViewModel = buildsControlViewModel;
            Options = persistentData.Options;
            StatTrackingSavePath = persistentData.StatTrackingSavePath;
            DisplayName = L10n.Message("Tracked Stat Settings");

            ChangeStatTrackingPathCommand = new AsyncRelayCommand(ChangeStatTrackingPath);
            LoadTrackedStatsCommand = new AsyncRelayCommand(LoadTrackedStats);

            Options.PropertyChanged += OptionsOnPropertyChanged;
        }

        protected override void OnClose()
        {
            Options.PropertyChanged -= OptionsOnPropertyChanged;
            _persistentData.StatTrackingSavePath = StatTrackingSavePath;
            _persistentData.Save();
        }

        private void OptionsOnPropertyChanged(object sender, PropertyChangedEventArgs args){}

        #region TrackingCommand Code

        /// <summary>
        /// Changes the stat tracking path.
        /// </summary>
        /// <returns></returns>
        private async Task ChangeStatTrackingPath()
        {
            var dialogSettings = new FileSelectorDialogSettings
            {
                DefaultPath = StatTrackingSavePath,
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

            StatTrackingSavePath = path;
        }

        /// <summary>
        /// Loads the tracked stats.
        /// </summary>
        /// <returns></returns>
        private async Task LoadTrackedStats()
        {
            if (GlobalSettings.CurrentTrackedFile != "")
            {
                string TargetFile;
                if(GlobalSettings.CurrentTrackedFile==GlobalSettings.FallbackValue)
                {
                    TargetFile = Path.Combine(StatTrackingSavePath, GlobalSettings.FallbackValue);
                }
                else
                {
                    TargetFile = GlobalSettings.CurrentTrackedFile;
                }
                string[] TrackedAttributeNames = await AsyncFileCommands.ReadAllLinesAsync(TargetFile);

                PseudoAttributeLoader Loader = new PseudoAttributeLoader();
                foreach (PseudoAttribute item in Loader.LoadPseudoAttributes())
                {
                    if (TrackedAttributeNames.Any(s => TargetFile.Contains(s)) && GlobalSettings.TrackedStats.GetIndexOfAttribute(item.Name) == -1)
                    {
                        GlobalSettings.TrackedStats.Add(item);
                    }
                }
            }
            else
            {
                await _dialogCoordinator.ShowInfoAsync(this, L10n.Message("Need to select Tracked File in ComboBox."), title: L10n.Message("No Tracked File Currently Selected"));
            }
        }

        #endregion TrackingCommand Code
    }
}