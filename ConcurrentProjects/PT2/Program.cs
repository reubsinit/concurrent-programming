using System;
using System.Threading;
using BitNak.Concurrent.Utils;

public class Program
{
	public static void Main()
	{
		MessageActiveObject mac = new MessageActiveObject("The Only Thread", "Hello, Brah!");
		mac.Start();
	}
}