using Reubs.Concurrent.Utils;
using System.Collections.Generic;

namespace dms
{
	class MainClass
	{
		private static FileManager _fileManager;
		private static TableManager _tableManager;
		private static ConnectionManager _connectionManager;
		private static List<QueryManager> _qM = new List<QueryManager>();

		public static void Main ()
		{
			_fileManager = new FileManager("File Manager", new Channel<Request>());
			_tableManager = new TableManager (_fileManager.Table, _fileManager, _fileManager.Channel);
			_connectionManager = new ConnectionManager ("Connection Manager");
				
			_fileManager.Start ();

			_connectionManager.Start ();
			for (int i = 0; i < 99; i++)
			{
				QueryManager qM = new QueryManager (_tableManager, "Rekt", _connectionManager.OutputChannel);
				qM.Start ();
				_qM.Add (qM);
			}
			_fileManager.PrintHeader ();
		}
	}
}