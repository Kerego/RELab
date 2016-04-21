using System.Windows;
using RELab.ViewModels;

namespace RELab
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		MainWindowViewModel _vm = new MainWindowViewModel();
		public MainWindow()
		{
			InitializeComponent();
			DataContext = _vm;
			this.Loaded += WindowLoaded;
			this.Closed += async (s,e) => await _vm.Close();
		}

		private async void WindowLoaded(object sender, RoutedEventArgs e)
		{
			_vm.MessageReceivedCallback = MessageReceivedCallback;
			await _vm.Initialize();
			await _vm.StartReceiveMessages();
		}

		private void MessageReceivedCallback()
		{
			if(ChatPart.Items.Count > 0)
				ChatPart.ScrollIntoView(ChatPart.Items[ChatPart.Items.Count - 1]);
		}
	}

}
