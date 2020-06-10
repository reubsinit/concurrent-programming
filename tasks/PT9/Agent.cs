//using System;
//using System.Collections.Generic;
//
//public class Agent : Participant
//{
//	private Dictionary<String, Pusher> _resourcePushers;
//
//	public Agent (String threadName) : base (threadName) {}
//
//	public Dictionary<String, Pusher> ResourcePushers
//	{
//		set 
//		{
//			_resourcePushers = value;
//		}
//	}
//
//	private void TriggerPushers()
//	{
//		switch (new Random ().Next (3)) 
//		{
//		case 0:
//			{
//				_resourcePushers ["Red Pusher"].ActPermission.Release (); 
//				_resourcePushers ["Green Pusher"].ActPermission.Release ();
//				break;
//			}
//		case 1:
//			{
//				_resourcePushers ["Red Pusher"].ActPermission.Release ();
//				_resourcePushers ["Blue Pusher"].ActPermission.Release ();
//				break;
//			}
//		case 2:
//			{
//				_resourcePushers ["Blue Pusher"].ActPermission.Release ();
//				_resourcePushers ["Green Pusher"].ActPermission.Release ();
//				break;
//			}
//		}
//	}
//
//	protected override void Run ()
//	{
//		while (true) 
//		{
//			_actPermission.Acquire ();
//			TriggerPushers ();
//		}
//	}
//}