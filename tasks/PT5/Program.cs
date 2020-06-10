using System;
using System.Threading;
using Reubs.Concurrent.Utils;

class SemaphoreTest
{
	private static Rendezvous _testRendezvous = new Rendezvous (12);
	private static Thread[] _threads = new Thread[6];


	public static void TestRendezvousArrival()
	{
		while (true) 
		{
			Console.WriteLine(Thread.CurrentThread.Name + " is trying to arrive at Rendezvous.");
			_testRendezvous.Arrive ();
			Console.WriteLine("\t" + Thread.CurrentThread.Name + " has arrived at Rendezvous.");
			Thread.Sleep (2000);
		}
	}

	public static void Main()
	{
		Console.Title = "Rendezvous Test";

		for (int i = 0; i < _threads.Length; i++) 
		{
			_threads[i] = new Thread(TestRendezvousArrival);
			_threads[i].Name = "Test thread " + i;
			_threads[i].IsBackground = false;
			_threads[i].Start();
		}
	}
}