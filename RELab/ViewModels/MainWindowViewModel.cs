using RELab.Core;
using RELab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RELab.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		#region Properties
		private string _publicMessageText = string.Empty;
		public string MessageText
		{
			get { return _publicMessageText; }
			set { _publicMessageText = value; OnPropertyChanged(); }
		}

		private string _nickname;

		public string Nickname
		{
			get { return _nickname; }
			set { _nickname = value; OnPropertyChanged(); }
		}

		private string _OldNickname;


		private string _topic;

		public string Topic
		{
			get { return _topic; }
			set { _topic = value; OnPropertyChanged(); }
		}


		private ChannelViewModel _selectedChannel;

		public ChannelViewModel SelectedChannel
		{
			get { return _selectedChannel; }
			set { _selectedChannel = value; OnPropertyChanged(); }
		}

		private string _newChannelName = "";

		public string NewChannelName
		{
			get { return _newChannelName; }
			set { _newChannelName = value; OnPropertyChanged(); }
		}

		private string _snoopName;

		public string SnoopName
		{
			get { return _snoopName; }
			set { _snoopName = value; }
		}


		private Status _currentStatus = Status.Online;

		public Status CurrentStatus
		{
			get { return _currentStatus; }
			set { _currentStatus = value; OnPropertyChanged(); _messenger.Send(new StatusMessage(Nickname, value)); }
		}

		public ObservableCollection<ChannelViewModel> Channels { get; set; } = new ObservableCollection<ChannelViewModel>();

		public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
		#endregion

		public ICommand SendMessageCommand { get; }
		public ICommand ChangeTopicCommand { get; }
		public ICommand ChangeNameCommand { get; }
		public ICommand AddChannelCommand { get; }
		public ICommand RemoveChannelCommand { get; }
		public ICommand AddPrivateChatCommand { get; }

		private QCMessenger _messenger;
		public Action MessageReceivedCallback;

		public MainWindowViewModel()
		{
			var listenEndPoint = new IPEndPoint(IPAddress.Any, 8167);
			var broadcastEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.255"), 8167);
			_messenger = new QCMessenger(listenEndPoint, broadcastEndPoint);

			Nickname = $"Guest_{new Random().Next()%1000}";
			_OldNickname = Nickname;
			Topic = "Unset";

			SendMessageCommand = new DelegateCommand(async () =>
			{
				if (MessageText == "")
					return;
				if(SelectedChannel.ChannelType == ChannelType.Public || SelectedChannel.ChannelType == ChannelType.Const)
					await _messenger.SendAsync(new PublicMessage(SelectedChannel.Id, String.IsNullOrWhiteSpace(SnoopName) ? Nickname : SnoopName, MessageText));
				else
					await _messenger.SendAsync(new PrivateMessage(String.IsNullOrWhiteSpace(SnoopName) ? Nickname : SnoopName, SelectedChannel.Id, MessageText));
				MessageText = string.Empty;
			});
			AddChannelCommand = new DelegateCommand(async () =>
			{
				if (NewChannelName == "" || Channels.Any(channel => channel.Id == NewChannelName))
					return;
				Channels.Add(new ChannelViewModel(NewChannelName, ChannelType.Public));
				SelectedChannel = Channels.Last();
				await _messenger.SendAsync(new ConnectMessage(NewChannelName, Nickname));
				NewChannelName = string.Empty;
			});
			RemoveChannelCommand = new DelegateCommand<ChannelViewModel>(async ch =>
			{
				if (ch.Id == "#Main")
					return;
				SelectedChannel = Channels.First();
				Channels.Remove(ch);
				await _messenger.SendAsync(new ExitMessage(ch.Id, Nickname));
			});
			ChangeTopicCommand = new DelegateCommand(async () => await _messenger.SendAsync(new TopicMessage(Topic, Nickname)));
			ChangeNameCommand = new DelegateCommand(async () =>
			{
				await _messenger.SendAsync(new RenameMessage(_OldNickname, Nickname));
				_OldNickname = Nickname;
			});
			AddPrivateChatCommand = new DelegateCommand<User>(u =>
			{
				if (Channels.Any(ch => ch.Id == u.Name))
					return;
				Channels.Add(new ChannelViewModel(u.Name, ChannelType.Private));
			});
		}

		public async Task Initialize()
		{
			Channels.Add(new ChannelViewModel("#Main", ChannelType.Const));
			Channels.Add(new ChannelViewModel("#noPrivacy", ChannelType.Const));
			SelectedChannel = Channels.First();
			await _messenger.SendAsync(new ConnectMessage(SelectedChannel.Id, Nickname));
		}

		public async Task Close()
		{
			foreach (var channel in Channels)
				await _messenger.SendAsync(new ExitMessage(channel.Id, Nickname));
		}

		public async Task StartReceiveMessages()
		{
			while (true)
			{
				var message = await _messenger.ListenAsync();
				switch (message)
				{
					case PublicMessage m:
						HandlePublicMessage(m);
						break;
					case PrivateMessage m:
						HandlePrivateMessage(m);
						Channels.ElementAt(1).Messages.Add(new SniffedMessage(m));
						break;
					case RenameMessage m:
						HandleRenameMessage(m);
						break;
					case ConnectMessage m:
						HandleConnectMessage(m);
						break;
					case TopicMessage m:
						Topic = m.Topic;
						Channels.SingleOrDefault(channel => channel.Id == "#Main")?.Messages.Add(m);
						break;
					case ExitMessage m:
						HandleExitMessage(m);
						break;
					case StatusMessage m:
						HandleStatusMessage(m);
						break;
					default:
						break;
				}
				MessageReceivedCallback();
			}

		}

		void HandlePublicMessage(PublicMessage m)
		{
			var channel = Channels.SingleOrDefault(ch => ch.Id == m.Channel);
			if (channel == null || channel.Id == "#noPrivacy")
				return;

			if (Users.SingleOrDefault(u => u.Name == m.Sender) == null)
				Users.Add(new User { Name = m.Sender });

			if (m.Sender != Nickname)
				channel?.Messages.Add(m);
			else
				channel?.Messages.Add(new MyMessage(m));
		}
		void HandlePrivateMessage(PrivateMessage m)
		{
			var channel = Channels.SingleOrDefault(ch => (ch.Id == m.Receiver && m.Sender == Nickname) || (ch.Id == m.Sender && m.Receiver == Nickname));
			if (channel == null && m.Receiver == Nickname)
			{
				channel = new ChannelViewModel(m.Sender, ChannelType.Private);
				Channels.Add(channel);
			}
			else if (channel == null)
				return;

			if (Users.SingleOrDefault(u => u.Name == m.Sender) == null)
				Users.Add(new User { Name = m.Sender });

			if (m.Sender != Nickname)
				channel?.Messages.Add(m);
			else
				channel?.Messages.Add(new MyMessage(m));
		}

		void HandleRenameMessage(RenameMessage m)
		{
			var user = Users.SingleOrDefault(u => u.Name == m.Sender);
			if (user == null)
				Users.Add(new User { Name = m.Nickname });
			else
			{
				var channel = Channels.FirstOrDefault(ch => ch.Id == user.Name);
				if(channel != null)
					channel.Id = m.Nickname;
				user.Name = m.Nickname;
			}

			Channels.First().Messages.Add(m);
		}

		void HandleConnectMessage(ConnectMessage m)
		{
			var channel = Channels.SingleOrDefault(ch => ch.Id == m.Channel);
			if (channel == null)
				return;
			channel.Messages.Add(m);

			if (Users.SingleOrDefault(u => u.Name == m.Sender) == null)
				Users.Add(new User { Name = m.Sender});
			if(channel == Channels.First())
				_messenger.SendAsync(new StatusMessage(m.Sender, Status.Online));
		}
		void HandleExitMessage(ExitMessage m)
		{
			var channel = Channels.SingleOrDefault(ch => ch.Id == m.Channel);
			if (channel == null)
				return;
			channel.Messages.Add(m);
			if(channel == Channels.First())
			{
				var user = Users.FirstOrDefault(us => us.Name == m.Sender);
				if (user == null)
					return;
				Users.Remove(user);
				Channels.Remove(Channels.FirstOrDefault(x => x.Id == user.Name && x.ChannelType == ChannelType.Private));
			}
		}

		void HandleStatusMessage(StatusMessage m)
		{
			var user = Users.SingleOrDefault(u => u.Name == m.Sender);
			if (user == null)
				Users.Add(new User { Name = m.Sender, Status = m.Status });
			else
				user.Status = m.Status;
			Channels.First().Messages.Add(m);
		}
	}
}
