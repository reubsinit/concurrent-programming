using System;
using System.Threading;
using BitNak.Concurrent.Utils;

class ReadWriteLockSwitchTest
{
	private static ReadWriteLock _testReadWriteLock = new ReadWriteLock ();

	private static void DoSomeReading()
	{
		while (true)
		{
			_testReadWriteLock.AcquireReader ();
			Console.WriteLine (Thread.CurrentThread.Name + ": Yeah! I'm reading!");
			Thread.Sleep (new Random ().Next (1000, 1500));
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": Well, I'm done reading.");
			_testReadWriteLock.ReleaseReader ();
		}
	}

	private static void DoSomeWriting()
	{
		while (true)
		{
			_testReadWriteLock.AcquireWriter ();
			Console.WriteLine ("\t\t" + Thread.CurrentThread.Name + ": Yeah! I'm writing some stuff!");
			Thread.Sleep (new Random ().Next (1000, 1500));
			Console.WriteLine ("\t\t\t" + Thread.CurrentThread.Name + ": Well, I'm done writing.");
			_testReadWriteLock.ReleaseWriter ();
		}
	}

	public static void Main()
	{
		Console.Title = "Read Write Lock Switch Test";

		Thread[] threads = new Thread[15];

		for (int i = 0; i < threads.Length; i++)
		{
			if (i < 9) 
			{
				threads [i] = new Thread (DoSomeReading);
				threads [i].Name = "Reader " + i;
				threads [i].IsBackground = false;
				threads [i].Start ();
			} 
			else 
			{
				threads[i] = new Thread(DoSomeWriting);
				threads[i].Name = "Writer " + (14 - i);
				threads[i].IsBackground = false;
				threads[i].Start();
			}
		}
	}
}