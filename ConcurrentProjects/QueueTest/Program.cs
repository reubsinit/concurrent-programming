using System;
using BitNak.Collections.Generic;

namespace QueueTest
{
	class MainClass
	{
		private static DynamicQueue<String> _queue = new DynamicQueue<string> ();

		public static void Main ()
		{
			_queue.Enqueue ("one");
			_queue.Enqueue ("two");
			_queue.Enqueue ("three");
			_queue.Enqueue ("six");
			_queue.Enqueue ("four");
			_queue.Enqueue ("five");

			Console.WriteLine( "Dequeue elements:");

			while ( !_queue.IsEmpty() )
			{
				Console.WriteLine (_queue.Dequeue());
			} 
		}
	}
}
