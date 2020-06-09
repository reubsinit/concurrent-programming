using System;
using System.Threading;
using System.Collections.Generic;
using BitNak.Concurrent.Utils;
using BitNak.Concurrent.TestTools;
using Semaphore = BitNak.Concurrent.Utils.Semaphore;

class SemaphoreTest
{

	private class ProgramTools
	{
		public Semaphore ConsoleSemaphore { get; set; }
		public Thread ConsoleThread { get; set; }

		public void Start()
		{
			ConsoleSemaphore = new Semaphore();
			ConsoleThread = new Thread(RenderConsole);
			ConsoleThread.IsBackground = false;
			ConsoleThread.Start ();
		}
	}

	private static SemaphoreTestAgent STT { get; set; }

	private static TerminalAgent TA { get; set; }

	public static void ColouredStringToConsole(ConsoleColor colour, string toWrite)
	{
		ConsoleColor currentColour = Console.ForegroundColor;
		Console.ForegroundColor = colour;
		Console.Write(toWrite);
		Console.ForegroundColor = currentColour;
	}

	private static void ReleaseToken()
	{
		Random rand = new Random();
		while (true)
		{
			STT.TokensToRelease = (uint)rand.Next (9);
			STT.TotalTokensReleased += STT.TokensToRelease;
			PT.ConsoleSemaphore.Release ();
			STT.TestSemaphore.Release (STT.TokensToRelease);
			Thread.Sleep (5000);
		}
	}

	private static void AcquireToken()
	{
		while (true)
		{
			STT.TestSemaphore.Acquire();
			STT.ThreadAcquisitionMap [Thread.CurrentThread].AcquiredCount++;
			STT.ThreadAcquisitionMap [Thread.CurrentThread].Acquired = true;
		}
	}

	private static void RenderConsole()
	{
		while (true)
		{
			//Get permission to print current state of semaphore test
			PT.ConsoleSemaphore.Acquire ();

			Console.Clear ();
			ColouredStringToConsole (ConsoleColor.Black, "Threads highlighted in ");
			ColouredStringToConsole (ConsoleColor.Green, "green ");
			ColouredStringToConsole (ConsoleColor.Black, "managed to acquire and those in ");
			ColouredStringToConsole (ConsoleColor.Red, "red ");
			ColouredStringToConsole (ConsoleColor.Black, "did not\n\n");
			ColouredStringToConsole (ConsoleColor.Blue, "Semaphore released " + STT.TokensToRelease + " token(s)\nTotal tokens released: " + STT.TotalTokensReleased + "\n\n");

			int cursorPos = (Console.WindowWidth - 13) / 10;
			int i = 0;
			foreach (KeyValuePair<Thread, ThreadData> entry in STT.ThreadAcquisitionMap)
			{
				Console.SetCursorPosition (cursorPos * i, Console.CursorTop);
				if (entry.Value.Acquired)
				{
					ColouredStringToConsole (ConsoleColor.Green, "Thread " + (i + 1) + ": " + entry.Value.AcquiredCount);	
					entry.Value.Acquired = false;
				} 
				else
				{
					ColouredStringToConsole (ConsoleColor.Red, "Thread " + (i + 1) + ": " + entry.Value.AcquiredCount);
				}
				i++;
			}
		}
	}

	public static void Main()
	{
		Console.Title = "Semaphore Test";

		STT = new SemaphoreTestAgent ();
		PT = new ProgramTools ();

		for (int i = 0; i < 10; i++)
		{
			Thread thread = new Thread(AcquireToken);
			thread.Name = String.Format ("{0}", i + 1);
			thread.IsBackground = true;
			thread.Start ();
			STT.ThreadAcquisitionMap.Add (thread, new ThreadData());
		}

		PT.Start ();
		STT.Start (ReleaseToken);
	}
}