using System;
using System.Threading;
using BitNak.Concurrent.Utils;

public class MessageActiveObject : ActiveObject
{
	private String _message;

	public MessageActiveObject(String threadName, String message) : base(threadName)
	{
		_message = message;
	}

	protected override void Run()
	{
		while (true)
		{
			Console.WriteLine(_message);
		}
	}
}
