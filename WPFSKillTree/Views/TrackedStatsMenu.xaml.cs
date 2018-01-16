using System.ComponentModel;
using System.Windows;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for TrackedStatsMenu.xaml
    /// </summary>
    public partial class TrackedStatsMenu: INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// INotifyPropertyChanged event that is called right before a property is changed.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// INotifyPropertyChanged event that is called right after a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        ///// <summary>
        ///// Called when [value property changed].
        ///// </summary>
        ///// <param name="d">The DependencyObject</param>
        ///// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        //private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{

        //}

        public TrackedStatsMenu()
        {
            InitializeComponent();
        }

        private void ResetTracking(object sender, RoutedEventArgs e)
        {
            GlobalSettings.TrackedStats.Clear();
        }
    }
}