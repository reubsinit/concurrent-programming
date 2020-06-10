using System;
using System.Threading;
using Reubs.Concurrent.Utils;

public class RekingChannelBasedObject: ChannelBasedActiveObject<String>
{

	public RekingChannelBasedObject (String name, Channel<String> channel = default(Channel<String>)) : base (name, channel)
	{

	}

	protected override void Process(String data)
	{
		Console.WriteLine (Thread.CurrentThread.Name + " I started with Rekt, and I'm now " + data);
		Channel.Enqueue (data = "Rekt");
		Thread.Sleep (500);
	}
}

public class WreckingChannelBasedObject: ChannelBasedActiveObject<String>
{

	public WreckingChannelBasedObject (String name, Channel<String> channel = default(Channel<String>)) : base (name, channel)
	{

	}

	protected override void Process(String data)
	{
		Console.WriteLine (Thread.CurrentThread.Name + " I started with Wrecked, and I'm now " + data);
		Channel.Enqueue (data = "Wrecked");
		Thread.Sleep (500);
	}
}

class MainClass
{

	public static void Main ()
	{
		Channel<String> aChannel = new Channel<String> ();
		aChannel.Enqueue ("lol rekt");

		RekingChannelBasedObject _mCBO1 = new RekingChannelBasedObject("MCBO 1:", aChannel);
		WreckingChannelBasedObject _mCBO2 = new WreckingChannelBasedObject("MCBO 2:", aChannel);
		_mCBO1.Start ();
		_mCBO2.Start ();
	}
}
