﻿using PoESkillTree.Common.ViewModels;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Localization;
using PoESkillTree.Model;
using PoESkillTree.Model.Serialization;
using PoESkillTree.TrackedStatViews;
using PoESkillTree.TreeGenerator.Model;
using PoESkillTree.TreeGenerator.Model.PseudoAttributes;
using PoESkillTree.ViewModels.Builds;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PoESkillTree.ViewModels
{
    // Some aliases to make things clearer without the need of extra classes.

    public class TrackedStatsMenuModel : CloseableViewModel, INotifyPropertyChanged, INotifyPropertyChanging
    {
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
        string StatTrackingSavePath
        {
            get
            {
                if (GlobalSettings.StatTrackingSavePathVal == null)
                {
                    return GlobalSettings.DefaultTrackingDir;
                }
                else
                {
                    return GlobalSettings.StatTrackingSavePathVal;
                }
            }
            set
            {
                if (GlobalSettings.StatTrackingSavePathVal == value)
                    return;
                GlobalSettings.StatTrackingSavePathVal = value;
            }
        }

        #region ICommand Initialization

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

        #endregion TrackingCommand Code
    }
}