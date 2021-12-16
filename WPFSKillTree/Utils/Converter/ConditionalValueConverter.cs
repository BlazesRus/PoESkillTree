using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PoESkillTree.Utils.Converter
{
	//Uses a percentage of field if first value is true otherwise returns 0.0
	//Based https://newbedev.com/converter-with-multiple-parameters (for multiple parameters)
	//Use
	//<MultiBinding Converter="{StaticResource converter:ConditionalPercentageConverter}">
	//  <Binding Path="isEnabledValueVariable"/>
	//  <Binding Path="valueIfEnabledVariable"/>
	//</MultiBinding>
	public class ConditionalValueConverter : IMultiValueConverter {
	  public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	  {
		bool isEnabled = (bool)values[0];
		double valueIfEnabled = (double)values[1];
		return isEnabled==true?valueIfEnabled:0.0;
	  }

	  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
		throw new NotImplementedException("Going back to what you had isn't supported.");
	  }
	}
}