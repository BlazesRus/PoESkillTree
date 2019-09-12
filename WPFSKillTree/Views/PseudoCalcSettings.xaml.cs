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

namespace PoESkillTree.PseudoCalcSettings
{
    //    [XamlCompilation(XamlCompilationOptions.Compile)]
    //    public partial class PseudoCalcSettings : ContentPage
    /// <summary>
    /// Interaction logic for PseudoCalcSettings.xaml
    /// </summary>
    public partial class PseudoCalcSettings : INotifyPropertyChanged
    {
        public PseudoCalcSettings()
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
    }
}