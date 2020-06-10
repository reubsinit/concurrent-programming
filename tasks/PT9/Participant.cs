//using System;
//using Reubs.Concurrent.Utils;
//using Mutex = Reubs.Concurrent.Utils.Mutex;
//
//public class Participant : ActiveObject
//{
//	protected Mutex _actPermission = new Mutex(true);
//
//	public Participant(String threadName) :base (threadName) {}
//
//	public Mutex ActPermission
//	{
//		get 
//		{
//			return _actPermission;
//		}
//	}
//
//	protected override void Run ()
//	{
//
//	}
//}