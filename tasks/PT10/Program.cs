using System;
using Reubs.Concurrent.Utils;
using Thread = System.Threading.Thread;

public class RollerCoaster: ActiveObject
{
	private int _numPassengers = 0;
	private uint _maxPassengers;
	private Mutex _boardPermission = new Mutex(true);
	private Mutex _departPermission = new Mutex(true);
	private Semaphore _ticketing;
	private Mutex _permission = new Mutex(true);

	public RollerCoaster(string threadName, uint maxPassengers) : base(threadName)
	{
		_maxPassengers = maxPassengers;
		_ticketing = new Semaphore (_maxPassengers);
	}

	public int NumPassengers
	{
		get 
		{
			return _numPassengers;
		}
	}

	public uint MaxPassengers
	{
		get 
		{
			return _maxPassengers;
		}
	}

	public Mutex BoardPermission
	{
		get 
		{
			return _boardPermission;
		}
	}
		
	public Mutex DepartPermission
	{
		get 
		{
			return _departPermission;
		}
	}

	public Semaphore Ticketing
	{
		get 
		{
			return _ticketing;
		}
	}

	public Semaphore Permission
	{
		get 
		{
			return _permission;
		}
	}

	public void IncrementPassengerCount(int n)
	{
		_numPassengers += n;
	}

	public void RunRide()
	{
		// Thread.Sleep(new Random().Next(500, 1500));
		Console.WriteLine ("\t\t\t\t" + Thread.CurrentThread.Name + ": The ride is now running! Enjoy!");
	}

	public void Load()
	{
		Console.WriteLine (Thread.CurrentThread.Name + ": The ride is about to start! Time to load!");
		_boardPermission.Release ();
		_permission.Acquire ();

	}

	public void UnLoad()
	{
		// Thread.Sleep(new Random().Next(500, 1500));
		Console.WriteLine ("\t\t\t\t\t" + Thread.CurrentThread.Name + ": The ride has finished! Time to unload!");
		_departPermission.Release ();
	}

	protected override void Run ()
	{
		while (true) 
		{
			Load ();
			RunRide ();
			UnLoad ();
		}
	}
}

public class Passenger: ActiveObject
{
	private RollerCoaster _riding;
	private uint _rideCount = 0;

	public Passenger(string threadName, RollerCoaster riding) : base(threadName)
	{
		_riding = riding;
	}

	public void Board()
	{
		Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": I'm waiting to board the ride.");
		// Thread.Sleep(new Random().Next(500, 1500));
		_riding.BoardPermission.Acquire ();
		_riding.Ticketing.Acquire ();
		_rideCount++;
		Console.WriteLine ("\t\t" + Thread.CurrentThread.Name + ": I'm on the ride!");
		// Thread.Sleep(new Random().Next(500, 1500));
		_riding.IncrementPassengerCount (1);
		if (_riding.NumPassengers != _riding.MaxPassengers) 
		{
			_riding.BoardPermission.Release ();
		} 
		else 
		{
			_riding.Permission.Release ();
			Console.WriteLine ("\t\t\t" + Thread.CurrentThread.Name + ": The ride is now full! Let's get going!");
			// Thread.Sleep(new Random().Next(500, 1500));
		}

	}

	public void UnBoard()
	{
		_riding.DepartPermission.Acquire ();
		Console.WriteLine ("\t\t\t\t\t\t" + Thread.CurrentThread.Name + ": I've just got off the ride. I've riden " + _rideCount + " times.");
		// Thread.Sleep(new Random().Next(500, 1500));
		_riding.IncrementPassengerCount (-1);
		if (_riding.NumPassengers != 0) 
		{
			_riding.DepartPermission.Release ();
		} 
		else 
		{
			_riding.Ticketing.Release (_riding.MaxPassengers);
		}
	}

	protected override void Run ()
	{
		while (true) 
		{
			Board ();
			UnBoard ();
		}
	}
}


class MainClass
{
	private static Passenger[] _passengers = new Passenger[15];
	private static RollerCoaster _rollerCoaster = new RollerCoaster("The Roller Coaster", 7);

	private static void SetupThemePark()
	{
		for (int i = 0; i < _passengers.Length; i++) 
		{
			_passengers [i] = new Passenger ("Passenger " + (i + 1), _rollerCoaster);
		}
	}

	public static void Main ()
	{
		SetupThemePark ();
		_rollerCoaster.Start ();
		for (int i = 0; i < _passengers.Length; i++) 
		{
			_passengers [i].Start();
		}
	}
}

