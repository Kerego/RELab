using RELab.Core;
namespace RELab.Models
{
	public class SniffedMessage : PrivateMessage
	{
		public SniffedMessage(PrivateMessage message) : base(message.Sender, message.Receiver, message.Content)
		{
		}
	}

	public class MyMessage : PublicMessage
	{
		public MyMessage(PublicMessage message) : base(message.Channel, message.Sender, message.Content)
		{

		}
		public MyMessage(PrivateMessage message) : base(message.Receiver, message.Sender, message.Content)
		{
		}
	}
}
