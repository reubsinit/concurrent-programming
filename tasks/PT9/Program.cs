using System;
using System.Threading;
using System.Collections.Generic;
using Reubs.Concurrent.Utils;
using Mutex = Reubs.Concurrent.Utils.Mutex;

public enum PusherType {Red, Green, Blue};

public class Participant : ActiveObject
{
	protected Mutex _actPermission = new Mutex(true);

	public Participant(String threadName) :base (threadName) {}

	public Mutex ActPermission
	{
		get 
		{
			return _actPermission;
		}
	}

	protected override void Run ()
	{
		throw new NotImplementedException ();
	}
}

public class Agent : Participant
{
	private Dictionary<String, Pusher> _resourcePushers;

	public Agent (String threadName) : base (threadName) {}

	public Dictionary<String, Pusher> ResourcePushers
	{
		set 
		{
			_resourcePushers = value;
		}
	}

	private void TriggerPushers()
	{
		switch (new Random ().Next (3)) 
		{
		case 0:
			{
				Console.WriteLine (Thread.CurrentThread.Name + ": Let's release a Red and Green gem!");
				_resourcePushers ["Red Pusher"].ActPermission.Release (); 
				_resourcePushers ["Green Pusher"].ActPermission.Release ();
				break;
			}
		case 1:
			{
				Console.WriteLine (Thread.CurrentThread.Name + ": Let's release a Red and Blue gem!");
				_resourcePushers ["Red Pusher"].ActPermission.Release ();
				_resourcePushers ["Blue Pusher"].ActPermission.Release ();
				break;
			}
		case 2:
			{
				Console.WriteLine (Thread.CurrentThread.Name + ": Let's release a Blue and Green gem!");
				_resourcePushers ["Blue Pusher"].ActPermission.Release ();
				_resourcePushers ["Green Pusher"].ActPermission.Release ();
				break;
			}
		}
	}

	protected override void Run ()
	{
		while (true) 
		{
			_actPermission.Acquire ();
			TriggerPushers ();
		}
	}
}

public class Pusher : Participant
{
	private PusherType _pusherType;
	private static Dictionary<string, bool> _resourcePushersAvailability = new Dictionary<string, bool>()
	{
		{"red", false},
		{"green", false},
		{"blue", false}
	};
	private Dictionary<String, Player> _thePlayers;

	private static Mutex _preventDeadLock = new Mutex();

	public Pusher (String threadName, Dictionary<String, Player> thePlayers, PusherType pusherType) : base(threadName)
	{
		_thePlayers = thePlayers;
		_pusherType = pusherType;
	}

	public Dictionary<String, Boolean> ResourcePushersAvailability
	{
		get 
		{
			return _resourcePushersAvailability;
		}
	}

	private void RedCheck()
	{
		if (_resourcePushersAvailability ["green"]) 
		{
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": The Red and Green gems are both available. Inform Player 1!");
			//Thread.Sleep (new Random ().Next (500, 1000));
			_thePlayers ["Player 1"].ActPermission.Release ();
			_resourcePushersAvailability ["green"] = false;
		} 
		else if (_resourcePushersAvailability ["blue"]) 
		{
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": The Red and Blue gems are both available. Inform Player 2!");
			//Thread.Sleep (new Random ().Next (500, 1000));
			_thePlayers ["Player 2"].ActPermission.Release ();
			_resourcePushersAvailability ["blue"] = false;
		} 
		else 
		{
			_resourcePushersAvailability ["red"] = true;
		}
	}

	private void GreenCheck ()
	{
		if (_resourcePushersAvailability ["blue"]) 
		{
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": The Green and Blue gems are both available. Inform Player 3!");
			//Thread.Sleep (new Random ().Next (500, 1000));
			_thePlayers ["Player 3"].ActPermission.Release ();
			_resourcePushersAvailability ["blue"] = false;
		} 
		else if (_resourcePushersAvailability ["red"]) 
		{
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": The Green and Red gems are both available. Inform Player 1!");
			//Thread.Sleep (new Random ().Next (500, 1000));
			_thePlayers ["Player 1"].ActPermission.Release ();
			_resourcePushersAvailability ["red"] = false;
		} 
		else 
		{
			_resourcePushersAvailability ["green"] = true;
		}
	}

	private void BlueCheck ()
	{
		if (_resourcePushersAvailability ["red"]) 
		{
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": The Blue and Red gems are both available. Inform Player 2!");
			//Thread.Sleep (new Random ().Next (500, 1000));
			_thePlayers ["Player 2"].ActPermission.Release ();
			_resourcePushersAvailability ["red"] = false;
		} 
		else if (_resourcePushersAvailability ["green"]) 
		{
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": The Blue and Green gems are both available. Inform Player 3!");
			//Thread.Sleep (new Random ().Next (500, 1000));
			_thePlayers ["Player 3"].ActPermission.Release ();
			_resourcePushersAvailability ["green"] = false;
		} 
		else 
		{
			_resourcePushersAvailability ["blue"] = true;
		}
	}

	private void ActOnAvailabilities()
	{
		_preventDeadLock.Acquire ();
		switch (_pusherType) 
		{
		case PusherType.Red:
			{
				RedCheck ();
				break;
			}
		case PusherType.Green:
			{
				GreenCheck ();
				break;
			}
		case PusherType.Blue:
			{
				BlueCheck ();
				break;
			}
		}
		_preventDeadLock.Release ();
	}

	protected override void Run ()
	{
		while (true) 
		{
			_actPermission.Acquire ();
			ActOnAvailabilities();
		}
	}
}

public class Player : Participant
{
	private Agent _theGameAgent;
	private uint _playCount = 0;

	public Player (String threadName, Agent theGameAgent) : base(threadName)
	{
		_theGameAgent = theGameAgent;
	}

	protected override void Run ()
	{
		while (true) 
		{
			_actPermission.Acquire ();
			_playCount++;
			Console.WriteLine ("\t\t" + Thread.CurrentThread.Name + ": I'm playing with gems!");
			//Thread.Sleep (new Random ().Next (500, 1000));
			Console.WriteLine ("\t\t\t" + Thread.CurrentThread.Name + ": I'm finished playing. I've played " + _playCount + " times!");
			//Thread.Sleep (new Random ().Next (500, 1000));
			_theGameAgent.ActPermission.Release ();
		}
	}
}

public class MainClass
{
	private static Agent theAgent = new Agent ("The Agent");

	private static void SetUpGemParty()
	{
		Dictionary<String, Player> players = new Dictionary<String, Player>
		{
			{"Player 1", new Player("Player 1", theAgent)},
			{"Player 2", new Player("Player 2", theAgent)},
			{"Player 3", new Player("Player 3", theAgent)}
		};

		Dictionary<String, Pusher> resourcePushers = new Dictionary<String, Pusher>
		{
			{"Red Pusher", new Pusher("Red Pusher", players, PusherType.Red)},
			{"Green Pusher", new Pusher("Green Pusher", players, PusherType.Green)},
			{"Blue Pusher", new Pusher("Blue Pusher", players, PusherType.Blue)}
		};	


		theAgent.ResourcePushers = resourcePushers;
		theAgent.ActPermission.Release ();

		foreach (KeyValuePair<string, Player> player in players)
		{
			player.Value.Start ();
		}

		foreach (KeyValuePair<string, Pusher> pusher in resourcePushers)
		{
			pusher.Value.Start ();
		}
	}

	public static void Main ()
	{
		SetUpGemParty ();
		theAgent.Start ();
	}
}
