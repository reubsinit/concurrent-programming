using System;
using System.Threading;
using Reubs.Concurrent.Utils;

class Test
{
	private static FiFoSemaphore _testSemaphore = new FiFoSemaphore (2);

	private static void AcquireToken()
	{
		Console.WriteLine(Thread.CurrentThread.Name + " I'm going to try and acquire a token.");
		_testSemaphore.Acquire();
		Console.WriteLine("\t" + Thread.CurrentThread.Name + " has acquired a token.");
	}

	public static void Main()
	{
		Console.Title = "Semaphore Test";

		Thread[] threads = new Thread[4];

		for (int i = 0; i < threads.Length; i++)
		{
			threads[i] = new Thread(AcquireToken);
			threads[i].Name = "Test thread " + (i + 1);
			threads[i].IsBackground = false;
		}

		threads [0].Start ();
		Thread.Sleep (1500);
		threads [1].Start ();
		Thread.Sleep (1500);
		Console.WriteLine("Thread 3 will not be able to acquire because there are no tokens left!");
		Thread.Sleep (1500);
		threads [2].Start ();
		Thread.Sleep (1500);
		Console.WriteLine("See! Told you so.");
		Thread.Sleep (1500);
		Console.WriteLine("Thread 4 will not be able to acquire because there are no tokens left!");
		Thread.Sleep (1500);
		threads [3].Start ();
		Thread.Sleep (1500);
		Console.WriteLine("See! Told you so.");
		Thread.Sleep (1500);
		Console.WriteLine("If we release two tokens, Thread 3 should acquire before thread 4! That's how a priority Semaphore works!");
		Thread.Sleep (1500);

		_testSemaphore.Release (1);
		Thread.Sleep (15);
		_testSemaphore.Release (1);
	}
}

