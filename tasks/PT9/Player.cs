//using System;
//using System.Threading;
//
//public class Player : Participant
//{
//	private Agent _theGameAgent;
//
//	public Player (String threadName, Agent theGameAgent) : base(threadName)
//	{
//		_theGameAgent = theGameAgent;
//	}
//
//	protected override void Run ()
//	{
//		while (true) 
//		{
//			_actPermission.Acquire ();
//			Console.WriteLine (Thread.CurrentThread.Name + ": I'm playing!");
//			Thread.Sleep (new Random ().Next (500, 1000));
//			Console.WriteLine (Thread.CurrentThread.Name + ": I'm finished playing!");
//			Thread.Sleep (new Random ().Next (500, 1000));
//			_theGameAgent.ActPermission.Release ();
//		}
//	}
//}