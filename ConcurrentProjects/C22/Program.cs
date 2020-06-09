using System;
using System.Diagnostics;
using System.Threading;
using BitNak.Concurrent.Utils;
using Semaphore = BitNak.Concurrent.Utils.Semaphore;
using Barrier = BitNak.Concurrent.Utils.Barrier;



class Test
{
	private static Semaphore _testSemaphore = new Semaphore (0);
	private static Channel<String> _testChannel = new Channel<String>();
	private static BoundedChannel<String> _testBoundedChannel = new BoundedChannel<String>(2);
	private static Barrier _barrier = new Barrier(2);
	private static bool _requiresInterrupt = false;
	private static bool _notGoingToRelease = false;
	private static bool _testsFinished = false;
	private static bool _finishedAcquiring = false;

	private static void ChannelTestEnqueue()
	{
		switch (new Random ().Next (2))
		{
		case 0:
			{
				Stopwatch watch = new Stopwatch ();
				int timeUntilRelease = new Random ().Next (8000);
				Console.WriteLine ("\t" + "\t" + "\t" + Thread.CurrentThread.Name + ": I'm going to wait " + timeUntilRelease + " milliseconds to enqueue some data.");
				watch.Start ();
				while (watch.ElapsedMilliseconds < timeUntilRelease)
				{

				}
				Console.WriteLine ("\t" + "\t" + "\t" + "\t" + Thread.CurrentThread.Name + ": I'm going to enqueue some data.");
				_testChannel.Enqueue ("Rekt\n");
				break;
			}
		case 1:
			{
				Console.WriteLine ("\t" + "\t" + "\t" + "\t" + "\t" + Thread.CurrentThread.Name + ": Haha! I'm not going to enqueue!");
				_notGoingToRelease = true;
				while (_notGoingToRelease)
				{

				}
				break;
			}
		}
	}

	private static void ChannelTestDequeue()
	{
		String outResult;
		switch (new Random ().Next (2))
		{
		case 0:
			{
				int maxWaitTime = new Random ().Next (4000);
				Console.WriteLine (Thread.CurrentThread.Name + ": I'm going to wait for at most " + maxWaitTime + " milliseconds to dequeue.");

				try
				{
					if (_testChannel.TryDequeue (maxWaitTime, out outResult))
					{
						Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I managed to dequeue from the channel!\n");
					} 
					else
					{
						Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": I've waited long enough.");
						Console.WriteLine ("\t" + "\t" + Thread.CurrentThread.Name + ": Now let's get back to dequeueing.");
					}
				}
				catch (ThreadInterruptedException)
				{
					Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I was interrupted!\n");
				}
				break;
			}
		case 1:
			{
				Console.WriteLine (Thread.CurrentThread.Name + ": I'm going to wait infinitely to dequeue.");
				_requiresInterrupt = true;
				try
				{
					if (_testChannel.TryDequeue (-1, out outResult))
					{
						Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I managed to dequeue!\n");
					}
				}
				catch (ThreadInterruptedException)
				{
					Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I was interrupted!\n");
				}
				break;
			}
		}

	}

	private static void SemaphoreTestRelease()
	{
		switch (new Random ().Next (2))
		{
		case 0:
			{
				Stopwatch watch = new Stopwatch ();
				int timeUntilRelease = new Random ().Next (8000);
				Console.WriteLine ("\t" + "\t" + "\t" + Thread.CurrentThread.Name + ": I'm going to wait " + timeUntilRelease + " milliseconds to release a token.");
				watch.Start ();
				while (watch.ElapsedMilliseconds < timeUntilRelease)
				{

				}
				Console.WriteLine ("\t" + "\t" + "\t" + "\t" + Thread.CurrentThread.Name + ": I'm releasing a token now!");
				_testSemaphore.Release ();
				break;
			}
		case 1:
			{
				Console.WriteLine ("\t" + "\t" + "\t" + "\t" + "\t" + Thread.CurrentThread.Name + ": Haha! I'm not going to release!");
				_notGoingToRelease = true;
				while (_notGoingToRelease)
				{

				}
				break;
			}
		}
	}

	private static void SemaphoreTestAcquire()
	{
		switch (new Random ().Next (2))
		{
		case 0:
			{
				int maxWaitTime = new Random ().Next (4000);
				Console.WriteLine (Thread.CurrentThread.Name + ": I'm going to wait " + maxWaitTime + " milliseconds to acquire.");
				try 
				{
					if (_testSemaphore.TryAcquire (maxWaitTime))
					{
						Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I managed to acquire!\n");
					} 
					else
					{
						Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": I've waited long enough.");
						Console.WriteLine ("\t" + "\t" + Thread.CurrentThread.Name + ": Now let's get back to acquiring.");
					}
				}
				catch (ThreadInterruptedException)
				{
					Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I was interrupted!\n");
				}
				break;
			}
		case 1:
			{
				Console.WriteLine (Thread.CurrentThread.Name + ": I'm going to wait infinitely to acquire.");
				_requiresInterrupt = true;
				try
				{
					if (_testSemaphore.TryAcquire (-1))
					{
						Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I managed to acquire!\n");
					}
				}
				catch (ThreadInterruptedException)
				{
					Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I was interrupted!\n");
				}
				break;
			}
		}
	}

	private static void ReleaseToken()
	{
		if (_barrier.Arrive ())
		{
			Console.WriteLine ("\n Semaphore Channel Tests Commencing\n");
			Console.WriteLine ("**********************************************\n");
		}
		while (!_finishedAcquiring)
		{
			SemaphoreTestRelease ();
		}
		if (_barrier.Arrive ())
		{
			_finishedAcquiring = false;
			Console.WriteLine ("\n Channel Tests Commencing\n");
			Console.WriteLine ("**********************************************\n");
		}
		while (!_finishedAcquiring)
		{
			ChannelTestEnqueue ();
		}
		if (_barrier.Arrive ())
		{
			_testsFinished = true;
		}
	}

	private static void AcquireToken()
	{
		if (_barrier.Arrive ())
		{
			Console.WriteLine ("\n Semaphore Channel Tests Commencing\n");
			Console.WriteLine ("**********************************************\n");
		}
		int semTestCount = 0;
		while (semTestCount < 5)
		{
			SemaphoreTestAcquire ();
			semTestCount++;
		}
		_finishedAcquiring = true;
		if (_barrier.Arrive ())
		{
			_finishedAcquiring = false;
			Console.WriteLine ("\n Channel Tests Commencing\n");
			Console.WriteLine ("**********************************************\n");
		}
		int cTestCount = 0;
		while (cTestCount < 5)
		{
			ChannelTestDequeue ();
			cTestCount++;
		}
		_finishedAcquiring = true;
		if (_barrier.Arrive ())
		{
			_testsFinished = true;
		}
	}

	public static void Main()
	{
		Console.Title = "Semaphore Test";

		Thread[] threads = new Thread[2];

		for (int i = 0; i < threads.Length - 1; i++)
		{
			threads[i] = new Thread(AcquireToken);
			threads[i].Name = "Acquiring thread " + (i + 1);
			threads[i].IsBackground = false;
			threads [i].Start ();
		}

		for (int i = 1; i < threads.Length; i++)
		{
			threads [i] = new Thread (ReleaseToken);
			threads[i].Name = "Releasing thread ";
			threads[i].IsBackground = false;
			threads [i].Start ();
		}

		while (!_testsFinished)
		{
			if (_requiresInterrupt && _notGoingToRelease)
			{
				Stopwatch watch = new Stopwatch ();
				watch.Start();
				while (watch.ElapsedMilliseconds < 4000)
				{

				}
				Console.WriteLine ("\n****Interrupt Happening Now****\n");
				threads [0].Interrupt ();
				_notGoingToRelease = false;
				_requiresInterrupt = false;
			}
		}

	}
}

