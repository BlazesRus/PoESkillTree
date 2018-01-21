using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace POESKillTree.TrackedStatViews
{
    /// <summary>
    /// Converter class for use of fallback value in ComboBox etc when value is null or empty
    /// (based on https://social.msdn.microsoft.com/Forums/vstudio/en-US/04d501aa-baef-476e-911e-5e28b0c07ff4/wpf-combobox-bind-fallbackvalue-to-property?forum=wpf)
    /// </summary>
    /// <seealso cref="IValueConverter" />
    public sealed class EmptyStringToFallbackvalue : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;
            return string.IsNullOrEmpty(str) ? GlobalSettings.FallbackValue : str;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class StringData : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        /// <value>
        /// The current value selected in the ComboBox
        /// </value>
        public string CurrentValue { get; set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="TrackedListBox"/> class from being created.
        /// </summary>
        public StringData() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringData"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public StringData(string value) { CurrentValue = value; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        /// Need to implement this interface in order to get data binding
        /// to work properly.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged Members

        #region Convert to/from other types

        /// <summary>
        /// Performs an explicit conversion from <see cref="StringData"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator string(StringData self)
        {
            return self.CurrentValue;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="StringData"/>.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator StringData(string self)
        {
            return new StringData(self);
        }

        #endregion Convert to/from other types

        #region Operator Functionality
        //public static bool operator ==(StringData self, string Value)
        //{
        //    return self.CurrentValue == Value;
        //}

        //public static bool operator !=(StringData self, string Value)
        //{
        //    return self.CurrentValue != Value;
        //}

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="Value">The value.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static StringData operator +(StringData self, string Value)
        {
            string Total = self.CurrentValue + Value;
            return new StringData(Total);
        }

        #endregion Operator Functionality
    }

    /// <summary>
    /// Interaction logic for TrackedStatsMenu.xaml
    /// </summary>
    public partial class TrackedStatsMenu : INotifyPropertyChanged
    {
        private string _TrackedStatSaveFile;

        public string TrackedStatSaveFile
        {
            get
            {
                if(_TrackedStatSaveFile==null)
                {
                    return "CurrentTrackedAttributes.txt";
                }
                else
                {
                    return _TrackedStatSaveFile;
                }
            }
            set
            {
                if (value != "" && value != null && value != _TrackedStatSaveFile)
                {
                    _TrackedStatSaveFile = value;
                    NotifyPropertyChanged("TrackedStatSaveFile");
                }
            }
        }

        public ObservableCollection<StringData> SourceList
        {
            get { return GlobalSettings.TrackingList; }
            set
            {
                if (value != null && value != SourceList)
                {
                    GlobalSettings.TrackingList = value; NotifyPropertyChanged("SourceList");
                }
            }
        }

        public TrackedStatsMenu()
        {
            InitializeComponent();
            SourceList = new ObservableCollection<StringData>();
            SourceList.Add("CurrentTrackedAttributes.txt");
            this.DataContext = this;
            this.Loaded += new RoutedEventHandler(OnLoad);
        }

        //void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    //Force binding changes to TrackingList
        //    this.TrackingList.ItemsSource = SourceList;
        //}

        //void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    //Force binding changes to TrackingList
        //    this.TrackingList.ItemsSource = SourceList;
        //}

        /// <summary>
        /// Called when [load].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            this.TrackingList.DataContext = this.DataContext;
            this.TrackingList.ItemsSource = SourceList;
            //this.SourceList.CollectionChanged += this.OnCollectionChanged;
        }

        /// <summary>
        /// INotifyPropertyChanged event that is called right after a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Clears file, then Writes the text asynchronous into file (Based from https://stackoverflow.com/questions/11774827/writing-to-a-file-asynchronously)
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static async Task WriteFileAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.ASCII.GetBytes(text);//ASCII instead of Unicode to prevent placing null after each character(https://stackoverflow.com/questions/14181866/converting-string-to-byte-creates-zero-character)

            using (FileStream sourceStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResetTracking(object sender, RoutedEventArgs e)
        {
            GlobalSettings.TrackedStats.Clear();
        }

        /// <summary>
        /// Saves the tracked stats to File for loading later
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void SaveTrackedStats(object sender, RoutedEventArgs e)
        {
            string StatsToSave = "";
            foreach (var stat in GlobalSettings.TrackedStats)
            {
                if (StatsToSave == "")
                {
                    StatsToSave = stat.Name;
                }
                else
                {
                    StatsToSave += "\n" + stat.Name;
                }
            }
            string FileToSaveTo = Path.Combine(GlobalSettings.StatTrackingSavePath, TrackedStatSaveFile);
            await WriteFileAsync(FileToSaveTo, StatsToSave);
        }

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
                if (value != null && value != "" && StatTrackingSavePath != value)
                {
                    GlobalSettings.StatTrackingSavePath = value;
                    NotifyPropertyChanged("StatTrackingSavePath");
                }
            }
        }

        /// <summary>
        /// Loads the tracked stat file names.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void LoadTrackedStatFileNamesAsync(object sender, RoutedEventArgs e)
        { 
            //SLightly async version of Getting files from http://writeasync.net/?p=2621
            // Avoid blocking the caller for the initial enumerate call.
            await Task.Yield();
            SourceList.Clear();// = new ObservableCollection<string>();
            foreach (string file in Directory.EnumerateFiles(StatTrackingSavePath))
            {
                SourceList.Add(file);
            }
            this.TrackingList.ItemsSource = SourceList;
        }
    }
}