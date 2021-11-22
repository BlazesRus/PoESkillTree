using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Localization;
using PoESkillTree.Model;
using PoESkillTree.Model.Serialization;
using PoESkillTree.ViewModels.Builds;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels
{
    public class TrackedStatsMenuModel : CloseableViewModel//, INotifyPropertyChanged, INotifyPropertyChanging
    {
        private readonly IPersistentData _persistentData;
        private readonly IDialogCoordinator _dialogCoordinator;

        public Options Options { get; }

        /// <summary>
        /// Gets the change stat tracking path command.
        /// </summary>
        /// <value>
        /// The change stat tracking path command.
        /// </value>
        public ICommand ChangeStatTrackingPathCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedStatsMenuModel"/> class.
        /// </summary>
        /// <param name="persistentData">The persistent data.</param>
        /// <param name="dialogCoordinator">The dialog coordinator.</param>
        public TrackedStatsMenuModel(IPersistentData persistentData, IDialogCoordinator dialogCoordinator)
        {
            _persistentData = persistentData;
            _dialogCoordinator = dialogCoordinator;
            Options = persistentData.Options;
            StatTrackingSavePath = persistentData.StatTrackingSavePath;
            DisplayName = L10n.Message("Tracked Stat Settings");

            ChangeStatTrackingPathCommand = new AsyncRelayCommand(ChangeStatTrackingPath);

            Options.PropertyChanged += OptionsOnPropertyChanged;
        }

        protected override void OnClose()
        {
            Options.PropertyChanged -= OptionsOnPropertyChanged;
            _persistentData.StatTrackingSavePath = StatTrackingSavePath;
            _persistentData.Save();
        }

        private void OptionsOnPropertyChanged(object sender, PropertyChangedEventArgs args) { }

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
                if (GlobalSettings.StatTrackingSavePathVal == null || GlobalSettings.StatTrackingSavePathVal == "")
                    GlobalSettings.SetTrackedPathFolder(AppData.ProgramDirectory);
#pragma warning disable CS8603 // Possible null reference return.(Handled in previous line)
                return GlobalSettings.StatTrackingSavePathVal;
#pragma warning restore CS8603 // Possible null reference return.
            }
            set
            {
                if (value != null && value != "" && GlobalSettings.StatTrackingSavePathVal != value)
                {
                    GlobalSettings.StatTrackingSavePathVal = value;
                    GlobalSettings.NotifyStaticPropertyChanged("StatTrackingSavePath");
                }
            }
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

        #endregion TrackingCommand Code
    }
}