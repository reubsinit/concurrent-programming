using System;
using System.Threading;
using BitNak.Concurrent.Utils;

class LatchTest
{
	private static Latch _testLatch = new Latch();

	public static void TestLatchAcquire()
	{
		while (true) 
		{
			Thread.Sleep (1000);
			Console.WriteLine (Thread.CurrentThread.Name + " is going to attempt to test latch acquire!");
			_testLatch.Acquire();
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + " completed the test and a token has been immediately released!");
			Thread.Sleep (1000);
		}
	}

	public static void Main ()
	{
		Thread[] threads = new Thread[15];

		for (int i = 0; i < threads.Length; i++)
		{
			threads[i] = new Thread(TestLatchAcquire);
			threads[i].Name = "Test thread " + i;
			threads[i].IsBackground = false;
			threads[i].Start();
		}

		Thread.Sleep (2000);

		Console.WriteLine ("Latch acquire has not completed yet because there are no available tokens!");
		Console.WriteLine ("Let's see what happens when we release a token into the latch!\n");
		Thread.Sleep (2000);
		_testLatch.Release ();
		
	}
}

