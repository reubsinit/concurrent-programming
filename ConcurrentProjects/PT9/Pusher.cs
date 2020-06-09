//using System;
//using System.Collections.Generic;
//
//public enum PusherType {Red, Green, Blue};
//
//public class Pusher : Participant
//{
//	private PusherType _pusherType;
//	private static Dictionary<string, bool> _resourcePushersAvailability = new Dictionary<string, bool>()
//	{
//		{"red", false},
//		{"green", false},
//		{"blue", false}
//	};
//
//	private Dictionary<String, Player> _thePlayers;
//
//	public Pusher (String threadName, Dictionary<String, Player> thePlayers, PusherType pusherType) : base(threadName)
//	{
//		_thePlayers = thePlayers;
//		_pusherType = pusherType;
//	}
//
//	public Dictionary<String, Boolean> ResourcePushersAvailability
//	{
//		get 
//		{
//			return _resourcePushersAvailability;
//		}
//	}
//
//	private void RedCheck()
//	{
//		if (_resourcePushersAvailability ["green"]) 
//		{
//			_thePlayers ["Player 1"].ActPermission.Release ();
//			_resourcePushersAvailability ["green"] = false;
//		} 
//		else if (_resourcePushersAvailability ["blue"]) 
//		{
//			_thePlayers ["Player 2"].ActPermission.Release ();
//			_resourcePushersAvailability ["blue"] = false;
//		} 
//		else 
//		{
//			_resourcePushersAvailability ["red"] = true;
//		}
//	}
//
//	private void GreenCheck ()
//	{
//		if (_resourcePushersAvailability ["blue"]) 
//		{
//			_thePlayers ["Player 3"].ActPermission.Release ();
//			_resourcePushersAvailability ["blue"] = false;
//		} 
//		else if (_resourcePushersAvailability ["red"]) 
//		{
//			_thePlayers ["Player 1"].ActPermission.Release ();
//			_resourcePushersAvailability ["red"] = false;
//		} 
//		else 
//		{
//			_resourcePushersAvailability ["green"] = true;
//		}
//	}
//
//	private void BlueCheck ()
//	{
//		if (_resourcePushersAvailability ["red"]) 
//		{
//			_thePlayers ["Player 2"].ActPermission.Release ();
//			_resourcePushersAvailability ["red"] = false;
//		} 
//		else if (_resourcePushersAvailability ["green"]) 
//		{
//			_thePlayers ["Player 3"].ActPermission.Release ();
//			_resourcePushersAvailability ["green"] = false;
//		} 
//		else 
//		{
//			_resourcePushersAvailability ["blue"] = true;
//		}
//	}
//
//	private void ActOnAvailabilities()
//	{
//		switch (_pusherType) 
//		{
//		case PusherType.Red:
//			{
//				RedCheck ();
//				break;
//			}
//		case PusherType.Green:
//			{
//				GreenCheck ();
//				break;
//			}
//		case PusherType.Blue:
//			{
//				BlueCheck ();
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
//			ActOnAvailabilities();
//		}
//	}
//}