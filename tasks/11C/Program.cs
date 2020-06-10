using System;
using Reubs.Concurrent.Utils;
using Thread = System.Threading.Thread;

public class Boat: ActiveObject
{
	private int _hackers = 0;
	private int _serfs = 0;
	private int _numPassengers = 0;
	private Barrier _barrier = new Barrier(4);
	private Mutex _boardPermission = new Mutex();
	private Mutex _deadLock = new Mutex();
	private Semaphore _hackerQueue = new Semaphore (0);
	private Semaphore _serfQueue = new Semaphore (0);
	private Mutex _permission = new Mutex(true);

	public Boat(string threadName) : base(threadName)
	{

	}

	public int Hackers
	{
		get 
		{
			return _hackers;
		}
	}

	public int Serfs
	{
		get 
		{
			return _serfs;
		}
	}

	public int Passengers
	{
		get 
		{
			return _numPassengers;
		}
	}

	public void IncrementHackers(int n)
	{
		_hackers += n;
	}

	public void IncrementSerfs(int n)
	{
		_serfs += n;
	}

	public void IncrementPassengerCount(int n)
	{
		_numPassengers += n;
	}

	public Mutex BoardPermission
	{
		get 
		{
			return _boardPermission;
		}
	}

	public Mutex DeadLock
	{
		get 
		{
			return _deadLock;
		}
	}

	public Semaphore HackerQueue
	{
		get 
		{
			return _hackerQueue;
		}
	}

	public Semaphore SerfQueue
	{
		get 
		{
			return _serfQueue;
		}
	}

	public Semaphore Permission
	{
		get 
		{
			return _permission;
		}
	}

	public Barrier Barrier
	{
		get 
		{
			return _barrier;
		}
	}

	public void SetSail()
	{
		_permission.Acquire ();
		//Thread.Sleep(new Random().Next(500, 1500));
		Console.WriteLine ("\t\t" + Thread.CurrentThread.Name + ": Boat full! Let's go for a boat ride!");
		//Thread.Sleep(new Random().Next(1500, 3000));
		Console.WriteLine ("\t\t\t" + Thread.CurrentThread.Name + ": Boat ride has finished! Let new patrons board.");
		_boardPermission.Release ();
	}

	protected override void Run ()
	{
		while (true) 
		{
			SetSail ();
		}
	}
}

public class Passenger: ActiveObject
{
	public enum Type {Hacker, Serf};
	private static Boat _boarding;
	private Type _passengerType;
	private uint _rideCount = 0;

	public Passenger(String threadName, Boat boarding, Passenger.Type passengerType) : base(threadName)
	{
		_boarding = boarding;
		_passengerType = passengerType;
	}

	public void HackerBoard()
	{
		_boarding.BoardPermission.Acquire ();
		_boarding.IncrementHackers (1);
		if (_boarding.Hackers == 4) 
		{
			_boarding.HackerQueue.Release (4);
			_boarding.IncrementHackers (-4);
		} 
		else if (_boarding.Serfs == 2 && _boarding.Hackers >= 2) 
		{
			_boarding.SerfQueue.Release (2);
			_boarding.HackerQueue.Release (2);
			_boarding.IncrementSerfs (-2);
			_boarding.IncrementHackers (-2);
		} 
		else 
		{
			_boarding.BoardPermission.Release ();
		}
		_boarding.HackerQueue.Acquire ();
		//		_boarding.DeadLock.Acquire ();
		//Thread.Sleep(new Random().Next(500, 1500));
		_rideCount++;
		Console.WriteLine (Thread.CurrentThread.Name + ": I've just boarded the boat! This will be my " + _rideCount + " ride.");
		_boarding.IncrementPassengerCount (1);
		if (_boarding.Barrier.Arrive()) 
		{
			//Thread.Sleep(new Random().Next(500, 1500));
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": It's time to sail!");
			_boarding.Permission.Release ();
			_boarding.IncrementPassengerCount (-4);
		}
		//		_boarding.DeadLock.Release ();
	}

	public void SerfBoard()
	{
		_boarding.BoardPermission.Acquire ();
		_boarding.IncrementSerfs (1);
		if (_boarding.Serfs == 4) 
		{
			_boarding.SerfQueue.Release (4);
			_boarding.IncrementSerfs (-4);
		} 
		else if (_boarding.Serfs == 2 && _boarding.Hackers >= 2) 
		{
			_boarding.SerfQueue.Release (2);
			_boarding.HackerQueue.Release (2);
			_boarding.IncrementSerfs (-2);
			_boarding.IncrementHackers (-2);
		} 
		else 
		{
			_boarding.BoardPermission.Release ();
		}
		_boarding.SerfQueue.Acquire ();
		//		_boarding.DeadLock.Acquire ();
		//Thread.Sleep(new Random().Next(500, 1500));
		_rideCount++;
		Console.WriteLine (Thread.CurrentThread.Name + ": I've just boarded the boat! This will be my " + _rideCount + " ride.");
		_boarding.IncrementPassengerCount (1);
		if (_boarding.Barrier.Arrive()) 
		{
			//Thread.Sleep(new Random().Next(500, 1500));
			Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": It's time to sail!");
			_boarding.Permission.Release ();
			_boarding.IncrementPassengerCount (-4);
		}
		//		_boarding.DeadLock.Release ();
	}

	public void Board()
	{
		switch (_passengerType) 
		{
		case Type.Hacker:
			{
				HackerBoard ();
				break;
			}
		case Type.Serf:
			{
				SerfBoard ();
				break;
			}
		}
	}

	protected override void Run ()
	{
		while (true) 
		{
			Board ();
		}
	}
}

class MainClass
{
	private static Passenger[] _passengers = new Passenger[8];
	private static Boat _boat = new Boat("The Boat");

	private static void SetupCruise()
	{
		for (int i = 0; i < 4; i++) 
		{
			_passengers [i] = new Passenger ("Passenger " + (i + 1) + " " + Passenger.Type.Hacker.ToString(), _boat, Passenger.Type.Hacker);
		}
		for (int i = 4; i < 8; i++) 
		{
			_passengers [i] = new Passenger ("Passenger " + (i + 1) + " " + Passenger.Type.Serf.ToString(), _boat, Passenger.Type.Serf);
		}

	}

	public static void Main ()
	{
		SetupCruise ();
		_boat.Start ();
		for (int i = 0; i < _passengers.Length; i++) 
		{
			_passengers [i].Start();
		}
	}
}
