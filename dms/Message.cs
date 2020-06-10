using System;
using System.Net.Sockets;

namespace dms
{
	public class Message
	{
		private String _messageText;
		private Connection _connection;

		public Message (String messageText, Connection connection)
		{
			_messageText = messageText;
			_connection = connection;
		}

		public String MessageText
		{
			get 
			{
				return _messageText;
			}
			set 
			{
				_messageText = value;
			}
		}

		public Connection Connection
		{
			get 
			{
				return _connection;
			}
			set 
			{
				_connection = value;
			}
		}
	}
}