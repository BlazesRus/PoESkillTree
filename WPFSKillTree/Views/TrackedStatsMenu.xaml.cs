//using Xamarin.Forms;
//using Xamarin.Forms.Xaml;
using PoESkillTree.TreeGenerator.Model.PseudoAttributes;
using PoESkillTree.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//using PoESkillTree.PoEMenuCommands;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.PoEMenuCommands;

namespace PoESkillTree.TrackedStatViews
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
            StringData ReturnVal = str ?? Path.Combine(GlobalSettings.StatTrackingSavePath, "CurrentTrackedAttributes.txt");
            return ReturnVal;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for TrackedStatsMenu.xaml
    /// </summary>
    public partial class TrackedStatsMenu : INotifyPropertyChanged
    {
        /// <summary>
        /// The tracking list
        /// </summary>
        private ObservableCollection<StringData> _TrackingList = new ObservableCollection<StringData>();

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

        public static string FallbackValue =  Path.Combine(GlobalSettings.StatTrackingSavePath, "StatTracking" + Path.DirectorySeparatorChar + "CurrentTrackedAttributes.txt");

        private string _CurrentTrackedFile = FallbackValue;

        public string CurrentTrackedFile
        {
            get { return _CurrentTrackedFile; }
            set
            {
                if (value != "" && value != null && value != CurrentTrackedFile)
                {
                    _CurrentTrackedFile = value;
                    NotifyPropertyChanged("CurrentTrackedFile");
                }
            }
        }

        public TrackedStatsMenu()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += new RoutedEventHandler(OnLoad);
        }

        /// <summary>
        /// Called when [load].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            this.TrackingList.DataContext = this.DataContext;
            //SourceList.Clear();
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
            this.TrackedFileText.DataContext = this.DataContext;
            if (this.TrackedFileText.Text == "" || this.TrackedFileText.Text == null)
                this.TrackedFileText.Text = CurrentTrackedFile;//Force Text to have value if fails to bind properly
        }

        /// <summary>
        /// INotifyPropertyChanged event that is called right after a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Clears file, then Writes the text asynchronous into file (Based from https://stackoverflow.com/questions/11774827/writing-to-a-file-asynchronously)
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static async Task WriteFileAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.ASCII.GetBytes(text);//ASCII instead of Unicode to prevent placing null after each character(https://stackoverflow.com/questions/14181866/converting-string-to-byte-creates-zero-character)

            using (FileStream sourceStream = new FileStream(filePath, FileMode.Truncate, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        /// <summary>
        /// Create new file, then writes the text asynchronous into file (Based from https://stackoverflow.com/questions/11774827/writing-to-a-file-asynchronously)
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static async Task WriteNewFileAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.ASCII.GetBytes(text);//ASCII instead of Unicode to prevent placing null after each character(https://stackoverflow.com/questions/14181866/converting-string-to-byte-creates-zero-character)

            using (FileStream sourceStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
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
                    StatsToSave = stat.Key;
                }
                else
                {
                    StatsToSave += "\n" + stat.Key;
                }
            }
            if (!Directory.Exists(StatTrackingSavePath)) { Directory.CreateDirectory(StatTrackingSavePath); }
            string FileToSaveTo;
            if (CurrentTrackedFile.Contains(Path.DirectorySeparatorChar))
            {
                FileToSaveTo = CurrentTrackedFile;
            }
            else if (!CurrentTrackedFile.Contains("."))
            {
                FileToSaveTo = Path.Combine(StatTrackingSavePath, CurrentTrackedFile + ".txt");
            }
            else//Local Path inside StatTracking Directory
            {
                FileToSaveTo = Path.Combine(StatTrackingSavePath, CurrentTrackedFile);
            }
            if (File.Exists(FileToSaveTo))
            {
                await WriteFileAsync(FileToSaveTo, StatsToSave);
            }
            else//Create New file if doesn't Exist
            {
                await WriteNewFileAsync(FileToSaveTo, StatsToSave);
            }
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
            //Slightly async version of Getting files from http://writeasync.net/?p=2621
            // Avoid blocking the caller for the initial enumerate call.
            await Task.Yield();
            SourceList.Clear();
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
                    string[] LoadedFileData = await AsyncFileCommands.ReadAllLinesAsync(TargetFile);
                    List<string> TrackedAttributeNames = new List<string>(LoadedFileData.Length) { };
                    string TempString;
                    bool PotentialLineComment;
                    bool IsComment;
                    foreach (var Line in LoadedFileData)//Filter out unneeded info from lines in file(enables to have C#/C++ style line comments and attributes inside parenthesis)
                    {
                        TempString = "";
                        PotentialLineComment = false;
                        IsComment = false;
                        foreach (var Elem in Line)
                        {
                            if (Elem == '/' && PotentialLineComment == false)
                            {
                                PotentialLineComment = true;
                            }
                            else if (PotentialLineComment)
                            {
                                if (Elem == '/')
                                {
                                    IsComment = true;
                                    continue;
                                }
                                else
                                {
                                    TempString += "/";
                                    PotentialLineComment = false;
                                }
                            }
                            if (Elem != '"')
                            {
                                TempString += Elem;
                            }
                        }
                        if (IsComment)
                            continue;
                        if (TempString.StartsWith('['))
                        {
                            //To-Do:Add loading of WeaponTypes and Tags later
                            //[MainWeaponType:
                            //[OffhandType:
                            //[SecondaryWeaponType:
                            //[TagFields:
                        }
                        else
                            TrackedAttributeNames.Add(TempString);
                    }

                    PseudoAttributeLoader Loader = new PseudoAttributeLoader();
                    List<PseudoAttribute> PsList = Loader.LoadPseudoAttributes();
                    foreach (PseudoAttribute item in PsList)
                    {
                        if (!GlobalSettings.TrackedStats.ContainsKey(item.Name))//Check if Attribute name already tracked first
                        {
                            if (TrackedAttributeNames.Any(s => item.Name.Contains(s)))
                            {
                                GlobalSettings.TrackedStats.Add(item.Name, new PseudoStat(item));
                            }
                        }
                    }
                }
                else
                {//Create Blank file if doesn't exist yet
                    if (!Directory.Exists(StatTrackingSavePath)) { Directory.CreateDirectory(StatTrackingSavePath); }
                    using (var myFile = File.Create(TargetFile)) { }//Creating new file and auto-disposing of FileStream
                }
            }
        }

        private void TrackingList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox self = (ComboBox)sender;
            StringData CurrentItem = (StringData)self.SelectedItem;
            CurrentTrackedFile = (string)CurrentItem;
            this.TrackedFileText.Text = CurrentTrackedFile; //Force Text to have value
        }

        private void TrackedFileText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox self = (TextBox)sender;
            if (self.Text != null && self.Text != "") { CurrentTrackedFile = self.Text; }
        }

        /// <summary>
        /// OffHand used for pseudo attribute calculations.
        /// </summary>
        public bool AutoTrackStats
        {
            get => GlobalSettings.AutoTrackStats;
            set => GlobalSettings.SetAutoTrackStats(value);
        }
    }
}