using RELab.Core;
using RELab.ViewModels;

namespace RELab.Models
{
	public class User : BindableBase
	{
		private string _name;

		public string Name
		{
			get { return _name; }
			set { _name = value; OnPropertyChanged(); }
		}

		private Status _status = Status.Online;

		public Status Status
		{
			get { return _status; }
			set { _status = value; OnPropertyChanged(); }
		}
	}
}
