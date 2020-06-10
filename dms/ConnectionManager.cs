using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Reubs.Concurrent.Utils;

namespace dms
{
	/// <summary>
	/// Declaration of public class ConnectionManager - inherits from ActiveObject:
	/// When instantiated, a ConnectionManager can be used
	/// manage a number of simultaneous connections to a specific address and port number
	/// and deal with any data communicated to the ConnectionManager from any connected clients.
	/// </summary>
	public class ConnectionManager: ActiveObject
	{	
		/// <summary>
		/// Declaration of private fields:
		/// <see cref="_serverSocket"/> as a Socket - Represents the ConnectionManger's socket.
		/// <see cref="_outputChannel"/> as a Channel of type Message - Used to queue data communicated to the ConnectionManager from clients.
		/// <see cref="_connections"/> as a Dictionary - Used to keep track of all active client connections to the ConnectionManager.
		/// <see cref="_clients"/> as a List of Socket - Used to keep track of the clients.
		/// <see cref="_readableSockets"/> as a List of Socket - Used to keep track of the clients that are active.
		/// </summary>	
		private Socket _serverSocket;
		Channel<Message> _outputChannel = new Channel<Message>();
		private Dictionary<Socket, Connection> _connections = new Dictionary<Socket, Connection>();
		private List<Socket> _clients = new List<Socket>();
		private List<Socket> _readableSockets = new List<Socket>();

		/// <summary>
		/// Declaration of ConnectionManager constructor: 
		/// Takes <see cref="String"/><paramref name="name"/> as a parameter.
		/// Value of <paramref name="name"/> is passed to base.
		/// <see cref="_serverSocket"/> socket is then instantiated to be able to work within any 
		/// IP address range on port 64125 with a connection queue of 42.
		/// </summary>
		/// <param name="name">
		/// Used to specify the name of the thread in base class ActiveObject.
		/// </param>
		public ConnectionManager (String name): base(name)
		{
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 64125);
			_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_serverSocket.Bind(endPoint);
			_serverSocket.Listen(42);
		}

		/// <summary>
		/// Declaration of getter OutputChannel: 
		/// Returns value stored in field <see cref="_outputChannel"/>
		/// </summary>
		public Channel<Message> OutputChannel
		{
			get 
			{
				return _outputChannel;
			}
		}

		/// <summary>
		/// Declaration of overriden method, Run:
		/// Infinitely checks for active connections to the ConnectionManager's server socket <see cref="_serverSocket"/>.
		/// Clients are handle appropriately and any messages that a single connected client sends to the ConnectionManager
		/// is then queued onto the ConnectionManager's output channel <see cref="_outputChannel"/>. Data that is queued onto the output
		/// channel can be handled by a means external to the ConnectionManager.
		/// </summary>
		protected override void Run()
		{
			while (true)
			{
				_readableSockets.Clear ();
				_readableSockets.Add (_serverSocket);
				_readableSockets.AddRange (_clients);
				Socket.Select(_readableSockets, null, null, -1);

				foreach (Socket s in _readableSockets)
				{
					if (s == _serverSocket)
					{
						Socket client = _serverSocket.Accept();
						_clients.Add(client);
						NetworkStream ns = new NetworkStream(client);
						Connection aConnection = new Connection(new StreamReader(ns), new StreamWriter(ns), client);
						_connections[client] = aConnection;
						Console.WriteLine (DateTime.Now + ": " + client.RemoteEndPoint + " has connected.\n");
					}
					else
					{
						if (s.Available == 0 ) 
						{
							_clients.Remove(s);
							_connections.Remove(s);
							continue;
						}
						Message toQueue = new Message (_connections[s].Reader.ReadLine(), _connections[s]);
						Console.WriteLine (DateTime.Now + ": " + s.RemoteEndPoint + " has sent the following message\n\t---" +
							"" +
							" " + toQueue.MessageText + "\n");
						_outputChannel.Enqueue (toQueue);
					}
				}

			}
		}
	}
}

