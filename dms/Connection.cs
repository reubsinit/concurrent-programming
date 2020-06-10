using System.IO;
using System.Net.Sockets;

namespace dms
{
	public class Connection
	{
		private StreamReader _reader;
		private StreamWriter _writer;
		private Socket _socket;
		public IOState State { get; set; }

		public Connection (StreamReader reader, StreamWriter writer, Socket socket)
		{
			_reader = reader;
			_writer = writer;
			_socket = socket;
			State = null;
		}

		public StreamReader Reader
		{
			get 
			{
				return _reader;
			}
			set 
			{
				_reader = value;
			}
		}

		public StreamWriter Writer
		{
			get 
			{
				return _writer;
			}
			set 
			{
				_writer = value;
			}
		}

		public Socket Client
		{
			get 
			{
				return _socket;
			}
			set 
			{
				_socket = value;
			}
		}
	}
}

