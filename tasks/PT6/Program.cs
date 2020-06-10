using System;
using System.Threading;
using Reubs.Concurrent.Utils;
using Mutex = Reubs.Concurrent.Utils.Mutex;

class SemaphoreTest
{
	private static Mutex _testMutex = new Mutex (true);

	private static void AcquireToken()
	{
		{
			_testMutex.Acquire();
			Console.WriteLine(Thread.CurrentThread.Name + " has acquired a token.\n");
		}
	}

	public static void Main()
	{
		Console.Title = "Mutex Test";

		try
		{
			Console.WriteLine ("Mutex will now break because we're trying to release more than 1 token!");
			Thread.Sleep (1000);
			_testMutex.Release(2);
		}
		catch (Exception e) 
		{
			Console.WriteLine (e.Message);
			Thread.Sleep (1000);
			Console.WriteLine ("See, it broke!\n");
			Thread.Sleep (1000);
		}

		_testMutex.Release ();

		try
		{
			Console.WriteLine ("Mutex will now break because we're trying to release a token into a full mutex!");
			Thread.Sleep (1000);
			_testMutex.Release(1);
		}
		catch (Exception e) 
		{
			Console.WriteLine (e.Message);
			Thread.Sleep (1000);
			Console.WriteLine ("See, it broke again!" +
				"\n");
			Thread.Sleep (1000);
		}

		Console.WriteLine ("Getting a token myself...");
		_testMutex.Acquire ();

		Thread[] threads = new Thread[2];
		for (int i = 0; i < threads.Length; i++)
		{
			threads[i] = new Thread(AcquireToken);
			threads[i].Name = "Test thread " + i;
			threads[i].IsBackground = false;
			threads[i].Start();
		}

		Console.WriteLine ("No tokens yet...");
		Console.WriteLine ("Mutex has been instantiated with no tokens\n");
		Thread.Sleep (3000);

		Console.WriteLine ("About to release 1 token");
		_testMutex.Release ();
		Thread.Sleep (3000);

		Console.WriteLine ("About to release another token");
		_testMutex.Release ();
			
	}
}