using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace PoESkillTree.EnumToComboBox
{//Code from(or at least mostly based on) https://stackoverflow.com/questions/6145888/how-to-bind-an-enum-to-a-combobox-control-in-wpf/28173189 from Nick's Answer
    public class EnumValueDescription
    {
        public Enum Value { get; internal set; }
        public string Description { get; internal set; }

        //public string ToString(){ }
    }

    public static class EnumHelper
    {
        public static string Description(this Enum value)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (attributes.Any())
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                return (attributes.First() as DescriptionAttribute).Description;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                // If no description is found, the least we can do is replace underscores with spaces
                // You can add your own custom default formatting logic here
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
        }

        public static IEnumerable<EnumValueDescription> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");

            return Enum.GetValues(t).Cast<Enum>().Select((e) => new EnumValueDescription() { Value = e, Description = e.Description() }).ToList();
        }
    }

    /// <summary> Converts Enum to description fields for combo-box usage
    /// Use variation(in .xaml file) of
    /// <ComboBox ItemsSource = "{Binding Path=TargetValue, Converter={x:EnumToCollectionConverter}, Mode=OneTime}" SelectedValue="{Binding Path=TargetValue}"
    //  SelectedValuePath="Value" DisplayMemberPath="Description" />
    /// Implements the <see cref="System.Windows.Markup.MarkupExtension"/>
    /// Implements the <see cref="System.Windows.Data.IValueConverter"/></summary>
    [ValueConversion(typeof(Enum), typeof(IEnumerable<KeyValuePair<string, string>>))]
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return EnumHelper.GetAllValuesAndDescriptions(value.GetType());
        }
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
