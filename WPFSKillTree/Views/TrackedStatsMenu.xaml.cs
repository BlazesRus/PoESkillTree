//using Xamarin.Forms;
//using Xamarin.Forms.Xaml;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using POESKillTree.Utils;

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
            //if (GlobalSettings.StatTrackingSavePath == null)
            //{
            //    string DefaultTrackingDir = Path.Combine(AppData.ProgramDirectory, "StatTracking" + Path.DirectorySeparatorChar);
            //    GlobalSettings.DefaultTrackingDir = DefaultTrackingDir;
            //    return string.IsNullOrEmpty(str) ? Path.Combine(DefaultTrackingDir, "CurrentTrackedAttributes.txt") : str;
            //}
            //else
            //{
            //    return string.IsNullOrEmpty(str) ? Path.Combine(GlobalSettings.StatTrackingSavePath, "CurrentTrackedAttributes.txt") : str;
            //}
            string ReturnVal = str ?? (GlobalSettings.StatTrackingSavePath != null? Path.Combine(GlobalSettings.StatTrackingSavePath, "CurrentTrackedAttributes.txt"): Path.Combine(Path.Combine(AppData.ProgramDirectory, "StatTracking" + Path.DirectorySeparatorChar), "CurrentTrackedAttributes.txt"));
            return ReturnVal;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public static class AsyncFileCommands
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


    //    [XamlCompilation(XamlCompilationOptions.Compile)]
    //    public partial class TrackedStatsMenu : ContentPage
    /// <summary>
    /// Interaction logic for TrackedStatsMenu.xaml
    /// </summary>
    public partial class TrackedStatsMenu : INotifyPropertyChanged
    {
        private string _TrackedStatSaveFile = "CurrentTrackedAttributes.txt";

        public string TrackedStatSaveFile
        {
            get
            {
                if (_TrackedStatSaveFile == null)
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

        /// <summary>
        /// The tracking list
        /// </summary>
        private static ObservableCollection<StringData> _TrackingList = new ObservableCollection<StringData>();

        /// <summary>
        /// The tracking list
        /// </summary>
        /// <value>
        /// The tracking list
        /// </value>
        public ObservableCollection<StringData> SourceList
        {
            get { return _TrackingList; }
            set
            {
                if (value != null && value != SourceList)
                {
                    _TrackingList = value; NotifyPropertyChanged("SourceList");
                }
            }
        }

        public static string FallbackValue = GlobalSettings.StatTrackingSavePath == null ? Path.Combine(AppData.ProgramDirectory, "StatTracking" + Path.DirectorySeparatorChar + "CurrentTrackedAttributes.txt") : Path.Combine(GlobalSettings.StatTrackingSavePath, "CurrentTrackedAttributes.txt");

        private static string _CurrentTrackedFile = FallbackValue;

        public static string CurrentTrackedFile
        {
            get { return _CurrentTrackedFile; }
            set
            {
                if (value != "" && value != null && value != CurrentTrackedFile)
                {
                    _CurrentTrackedFile = value;
                }
            }
        }

        public TrackedStatsMenu()
        {
            InitializeComponent();
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
            string FileToSaveTo;
            if (TrackedStatSaveFile.Contains(Path.DirectorySeparatorChar))
            {
                FileToSaveTo = TrackedStatSaveFile;
            }
            else if (!TrackedStatSaveFile.Contains("."))
            {
                FileToSaveTo = Path.Combine(GlobalSettings.StatTrackingSavePath, TrackedStatSaveFile + ".txt");
            }
            else
            {
                FileToSaveTo = Path.Combine(GlobalSettings.StatTrackingSavePath, TrackedStatSaveFile);
            }
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
            if (Directory.Exists(StatTrackingSavePath))
            {
                foreach (string file in Directory.EnumerateFiles(StatTrackingSavePath))
                {
                    SourceList.Add(file);
                }
            }
            else
            {//Create Directory if doesn't exist
                Directory.CreateDirectory(StatTrackingSavePath);
            }
            this.TrackingList.ItemsSource = SourceList;
        }

        /// <summary>
        /// Loads the tracked stats.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void LoadTrackedStats(object sender, RoutedEventArgs e)
        {
            if (CurrentTrackedFile != "")
            {
                string TargetFile = CurrentTrackedFile;
                if (File.Exists(TargetFile))
                {
                    string[] TrackedAttributeNames = await AsyncFileCommands.ReadAllLinesAsync(TargetFile);

                    PseudoAttributeLoader Loader = new PseudoAttributeLoader();
                    List<PseudoAttribute> PsList = Loader.LoadPseudoAttributes();
                    foreach (PseudoAttribute item in PsList)
                    {
                        if (GlobalSettings.TrackedStats.GetIndexOfAttribute(item.Name) == -1)//Check if Attribute name already tracked first
                        {
                            if (TrackedAttributeNames.Any(s => item.Name.Contains(s)))
                            {
                                GlobalSettings.TrackedStats.Add(item);
                            }
                        }
                    }
                }
                else
                {//Create Blank file if doesn't exist yet
                    using (var myFile = File.Create(TargetFile)){}//Creating new file and auto-disposing of FileStream
                }
            }
        }
    }
}