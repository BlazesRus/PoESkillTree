using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PoESkillTree.PoEMenuCommands
{
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

    public sealed class StringDataAsString : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as StringData;
            return (string)str;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}