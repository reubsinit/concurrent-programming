using System;
using System.Threading;
using Reubs.Concurrent.Utils;

public class Program
{
	public static void Main()
	{
		MessageActiveObject mac = new MessageActiveObject("The Only Thread", "Hello, Brah!");
		mac.Start();
	}
}