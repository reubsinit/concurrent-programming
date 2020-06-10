using System;
using System.Diagnostics;
using System.Threading;
using Reubs.Concurrent.Utils;
using Semaphore = Reubs.Concurrent.Utils.Semaphore;
using Barrier = Reubs.Concurrent.Utils.Barrier;



class Test
{
	private static Semaphore _testSemaphore = new Semaphore (0);
	private static Channel<String> _testChannel = new Channel<String>();
	private static BoundedChannel<String> _testBoundedChannel = new BoundedChannel<String>(3);
	private static Barrier _barrier = new Barrier(2);
	private static bool _finishedAcquiring = false;

	private static void BoundedChannelTestEnqueueTimeOuts()
	{
		int timeUntilRelease = new Random ().Next (4000);
		Console.WriteLine ("\t" + "\t" + "\t" + Thread.CurrentThread.Name + ": I'm going to wait at most " + timeUntilRelease + " milliseconds to enqueue some data.");
		if (!_testBoundedChannel.TryEnqueue ("Rekt\n", timeUntilRelease))
		{
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": I've waited long enough.");
			Console.WriteLine ("\t" + "\t" + Thread.CurrentThread.Name + ": Now let's get back to enqueueing.");
		}
	}

	private static void BoundedChannelTestEnqueue()
	{
		Stopwatch watch = new Stopwatch ();
		int timeUntilRelease = new Random ().Next (8000);
		Console.WriteLine ("\t" + "\t" + "\t" + Thread.CurrentThread.Name + ": I'm going to wait " + timeUntilRelease + " milliseconds to enqueue some data.");
		watch.Start ();
		while (watch.ElapsedMilliseconds < timeUntilRelease)
		{

		}
		Console.WriteLine ("\t" + "\t" + "\t" + Thread.CurrentThread.Name + ": I'm going to enqueue some data.");
		_testBoundedChannel.Enqueue ("Rekt\n");
	}

	private static void BoundedChannelTestDequeue()
	{
		int maxWaitTime = new Random ().Next (4000);
		String outResult;
		Console.WriteLine (Thread.CurrentThread.Name + ": I'm going to wait for at most " + maxWaitTime + " milliseconds to dequeue.");

		if (_testBoundedChannel.TryDequeue (maxWaitTime, out outResult))
		{
			Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I managed to dequeue from the channel!\n");
		} 
		else
		{
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": I've waited long enough.");
			Console.WriteLine ("\t" + "\t" + Thread.CurrentThread.Name + ": Now let's get back to dequeueing.");
		}
	}

	private static void ChannelTestEnqueue()
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
	}

	private static void ChannelTestDequeue()
	{
		int maxWaitTime = new Random ().Next (4000);
		String outResult;
		Console.WriteLine (Thread.CurrentThread.Name + ": I'm going to wait for at most " + maxWaitTime + " milliseconds to dequeue.");

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

	private static void SemaphoreTestRelease()
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
	}

	private static void SemaphoreTestAcquire()
	{
		int maxWaitTime = new Random ().Next (4000);
		Console.WriteLine (Thread.CurrentThread.Name + ": I'm going to wait " + maxWaitTime + " milliseconds to acquire.");

		if (_testSemaphore.TryAcquire (maxWaitTime))
		{
			Console.WriteLine ("\n\t" + Thread.CurrentThread.Name + ": I managed to acquire!\n");
		} 
		else
		{
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": I've waited long enough and should be interrupted.");
			Console.WriteLine ("\t" + "\t" + Thread.CurrentThread.Name + ": Now let's get back to acquiring.");
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
			_finishedAcquiring = false;
			Console.WriteLine ("\n Bounded Channel Tests Commencing\n");
			Console.WriteLine ("**********************************************\n");
		}
		while (!_finishedAcquiring)
		{
			BoundedChannelTestEnqueue ();
		}
		if (_barrier.Arrive ())
		{
			Console.WriteLine ("\n More Bounded Channel Tests Commencing\n");
			Console.WriteLine ("**********************************************\n");
		}
		int bCTestCount = 0;
		while (bCTestCount < 5)
		{
			BoundedChannelTestEnqueueTimeOuts ();
			bCTestCount++;
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
			_finishedAcquiring = false;
			Console.WriteLine ("\n Bounded Channel Tests Commencing\n");
			Console.WriteLine ("**********************************************\n");
		}
		int bCTestCount = 0;
		while (bCTestCount < 5)
		{
			BoundedChannelTestDequeue ();
			bCTestCount++;
		}
		_finishedAcquiring = true;
		if (_barrier.Arrive ())
		{
			Console.WriteLine ("\n More Bounded Channel Tests Commencing\n");
			Console.WriteLine ("**********************************************\n");
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

	}
}
