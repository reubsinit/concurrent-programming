using System;
using System.Threading;

public class HelloWorldMultiThreaded
{
	static void PrintThreadName()
	{
		while (true) 
		{
			Console.WriteLine ("Hello, brahhh! From {0}!", Thread.CurrentThread.Name);
		}
	}

	public static void Main (string[] args)
	{
		Thread[] t = new Thread[] 
		{
			new Thread (PrintThreadName),
			new Thread (PrintThreadName),
			new Thread (PrintThreadName)
		};

		t [0].Name = "Thread 1";
		t [1].Name = "\tThread 2";
		t [2].Name = "\t\tThread 3";

		foreach (Thread th in t) 
		{
			th.IsBackground = true;
		}

		foreach (Thread th in t) 
		{
			th.Start();
		}

		Console.ReadLine ();
	}
}
