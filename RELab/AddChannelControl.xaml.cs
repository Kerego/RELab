using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RELab
{
	/// <summary>
	/// Interaction logic for AddChannelControl.xaml
	/// </summary>
	public partial class DialogWindow : Window
	{
		public DialogWindow(string details, string defaultAnswer = "")
		{
			InitializeComponent();
			DetailTextBlock.Content = details;
			InputBox.Text = defaultAnswer;
		}

		private void OkButtonClick(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			InputBox.SelectAll();
			InputBox.Focus();
		}

		public string Answer
		{
			get { return InputBox.Text; }
		}
	}
}
