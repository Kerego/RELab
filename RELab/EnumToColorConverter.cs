using RELab.Core;
using RELab.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RELab
{
	public class EnumToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Status status = (Status)value;
			switch (status)
			{
				case Status.Away:
					return new SolidColorBrush(Colors.Yellow);
				case Status.Online:
					return new SolidColorBrush(Colors.Green);
				case Status.Offline:
					return new SolidColorBrush(Colors.Black);
				case Status.DoNotDisturb:
					return new SolidColorBrush(Colors.Red);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class EnumToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ChannelType status = (ChannelType)value;
			switch (status)
			{
				case ChannelType.Public:
				case ChannelType.Private:
					return Visibility.Visible;
				case ChannelType.Const:
					return Visibility.Collapsed;
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
