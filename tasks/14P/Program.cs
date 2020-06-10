using System;
using System.Threading;
using Reubs.Concurrent.Utils;

class ChannelTests
{
	private static Channel<String> _channel = new Channel<String>();
	private static BoundedChannel<String> _boundedChannel = new BoundedChannel<String>(3);

	public static void TestBCUpperLimit()
	{
		Console.WriteLine ("\n The bounded channel we're using has an upper limit of 3 so let's test that!\n");
		Thread.Sleep (1000);
		Console.WriteLine ("One push!");
		_boundedChannel.Enqueue ("Some nonsense!");
		Console.WriteLine ("First push worked!");
		Thread.Sleep (1000);
		Console.WriteLine ("Two push!");
		_boundedChannel.Enqueue ("Some nonsense!");
		Console.WriteLine ("Second push worked!");
		Thread.Sleep (1000);
		Console.WriteLine ("Three push!");
		_boundedChannel.Enqueue ("Some nonsense!");
		Console.WriteLine ("Third push worked!");
		Thread.Sleep (1000);
		Console.WriteLine ("Four push!");
		_boundedChannel.Enqueue ("Some nonsense!");
		Console.WriteLine ("Fourth push worked!");
	}

	public static void EnqueueChannel()
	{
		Thread.Sleep (3500);
		Console.WriteLine ("\nTime to put something on the channel so the threads waiting to dequeue may do so!\n");
		Thread.Sleep (1000);
		_channel.Enqueue ("Some nonsense!");
		Thread.Sleep (3500);
		Console.WriteLine ("\nA second something on the channel!\n");
		Thread.Sleep (1000);
		_channel.Enqueue ("Some nonsense!");
		Thread.Sleep (3500);
		Console.WriteLine ("\nAnd a third something!\n");
		Thread.Sleep (1000);
		_channel.Enqueue ("Some nonsense!");
		Thread.Sleep (3500);
	}

	public static void EnqueueBoundedChannel()
	{
		Thread.Sleep (3500);
		Console.WriteLine ("\nTime to put something on the channel so the threads waiting to dequeue may do so!\n");
		Thread.Sleep (1000);
		_boundedChannel.Enqueue ("Some nonsense!");
		Thread.Sleep (3500);
		Console.WriteLine ("\nA second something on the channel!\n");
		Thread.Sleep (1000);
		_boundedChannel.Enqueue ("Some nonsense!");
		Thread.Sleep (3500);
		Console.WriteLine ("\nAnd a third something!\n");
		Thread.Sleep (1000);
		_boundedChannel.Enqueue ("Some nonsense!");
		Thread.Sleep (3500);
	}

	public static void DequeueChannel()
	{
		Console.WriteLine (Thread.CurrentThread.Name + ": I'm waiting to be able to dequeue some nonsense off the channel!" +
			"");
		Thread.Sleep (1000);
		Console.WriteLine(_channel.Dequeue ());
		Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": I managed to dequeue some nonsense off the channel!");
	}

	public static void DequeueBoundedChannel()
	{
		Console.WriteLine (Thread.CurrentThread.Name + ": I'm waiting to be able to dequeue some nonsense off the bounded channel!" +
			"");
		Thread.Sleep (1000);
		Console.WriteLine(_boundedChannel.Dequeue ());
		Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": I managed to dequeue some nonsense off the channel!");
	}

	public static void Main()
	{
		Console.Title = "Channel Tests";

		Thread[] threads = new Thread[3];

		for (int i = 0; i < threads.Length; i++)
		{
				threads [i] = new Thread (DequeueChannel);
				threads [i].Name = "Channel Dequeue Over Lord " + i;
				threads [i].IsBackground = false;
				threads [i].Start ();
		}

		EnqueueChannel ();

		Console.WriteLine ("\n Let's do some bounded channel tests now!\n");

		for (int i = 0; i < threads.Length; i++)
		{
			threads [i] = new Thread (DequeueBoundedChannel);
			threads [i].Name = "Bounded Channel Dequeue Over Lord " + i;
			threads [i].IsBackground = false;
			threads [i].Start ();
		}
		Thread.Sleep (3500);

		EnqueueBoundedChannel ();

		threads = new Thread[2];

		for (int i = 0; i < threads.Length; i++)
		{
			if (i == 0) 
			{
				threads [i] = new Thread (TestBCUpperLimit);
				threads [i].Name = "Bounded Channel Dequeue Over Lord " + i;
				threads [i].IsBackground = false;
				threads [i].Start ();
			} 
			else 
			{
				threads [i] = new Thread (DequeueBoundedChannel);
				threads [i].Name = "Bounded Channel Dequeue Over Lord " + i;
				threads [i].IsBackground = false;
			}
		}

		Thread.Sleep (6000);
		threads [1].Start ();
	}
}