using RELab.Core;
using RELab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RELab.ViewModels
{
	public enum ChannelType
	{
		Private,
		Public,
		Const
	}
	public class ChannelViewModel : BindableBase
	{
		public ChannelType ChannelType { get; set; } = ChannelType.Public;
		public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

		private string _id;

		public string Id
		{
			get { return _id; }
			set { _id = value; OnPropertyChanged(); }
		}


		public ChannelViewModel(string id, ChannelType channelType)
		{
			ChannelType = channelType;
			Id = id;
		}
	}

}
