using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace PoESkillTree.ViewModels
{
    class TrackedStatToTextblockConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var attr = value as PseudoTotal;
            if (attr == null)
                return new TextBlock();

            var tb = new TextBlock { TextWrapping = TextWrapping.Wrap };
            var txt = new Run(attr.Text);
            if (attr.Missing)
                txt.Foreground = Brushes.Red;

            tb.Inlines.Add(txt);

            if (attr.Total != 0)
            {
                tb.Inlines.Add(" ");
                txt = new Run(attr.Total.ToString("+#;-#;0"));
                txt.Foreground = (attr.Total < 0) ? Brushes.Red : Brushes.Green;
                tb.Inlines.Add(txt);
            }

            return tb;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}