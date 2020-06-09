
using System;
using System.Threading;
using BitNak.Concurrent.Utils;
using Mutex = BitNak.Concurrent.Utils.Mutex;
using Semaphore = BitNak.Concurrent.Utils.Semaphore;

public class Philosopher : ActiveObject
{
	private enum Chopsticks {Left, Right};
	private uint _eatCount;
	private Mutex _leftChopStick;
	private Mutex _rightChopStick;
	private Chopsticks _firstChopStick;
	private static Semaphore _butler = new Semaphore(4);
	private Random _rand = new Random();

	public Philosopher (String threadName, Mutex leftChopstick, Mutex rightChopStick) : base(threadName)
	{
		_eatCount = 0;
		_leftChopStick = leftChopstick;
		_rightChopStick = rightChopStick;
	}

	private void Getbutler()
	{
		Console.WriteLine (Thread.CurrentThread.Name + ": butler, give me permission to eat!");
		_butler.Acquire ();
		Console.WriteLine ("\t" + Thread.CurrentThread.Name + ": Thank you, butler. I will now try to eat!");
		Thread.Sleep (_rand.Next(500, 1500));
	}

	private void Releasebutler()
	{
		Console.WriteLine ("\t\t\t" + Thread.CurrentThread.Name + ": Thank you, butler. You may serve another.");
		_butler.Release ();
		Thread.Sleep (_rand.Next(100, 1500));
	}

	private void GetChopSticks()
	{
		switch (_rand.Next (2)) 
		{
		case 0:
			{
				_leftChopStick.Acquire ();
				Thread.Sleep (_rand.Next (500, 1500));
				_rightChopStick.Acquire ();
				_firstChopStick = Chopsticks.Left;
				Console.WriteLine ("\t\t" + Thread.CurrentThread.Name + ": I've picked up a chopstick to my left and a chopstick to my right!");
				break;
			}
		default:
			{
				_rightChopStick.Acquire ();
				Thread.Sleep (_rand.Next (500, 1500));
				_leftChopStick.Acquire ();
				_firstChopStick = Chopsticks.Right;
				Console.WriteLine ("\t\t" + Thread.CurrentThread.Name + ": I've picked up a chopstick to my right and a chopstick to my left!");
				break;
			}
		}
		Thread.Sleep (_rand.Next (500, 1500));
	}

	private void ReleaseChopSticks()
	{
		Console.WriteLine ("\t\t\t\t\t" + Thread.CurrentThread.Name + ": I'm full. I will now release the chopsticks!");
		switch (_firstChopStick) 
		{
		case Chopsticks.Left:
			{
				_leftChopStick.Release ();
				_rightChopStick.Release ();
				break;
			}
		default:
			{
				_rightChopStick.Release ();
				_leftChopStick.Release ();
				break;
			}
		}
		Thread.Sleep (_rand.Next(500, 1500));
	}

	private void Eat()
	{
		_eatCount++;
		Console.WriteLine ("\t\t\t\t" + Thread.CurrentThread.Name + ": I will now enjoy eating! I've eaten " + _eatCount + " times.");
		Thread.Sleep (_rand.Next(500, 1500));
	}

	protected override void Run()
	{
		while (true) 
		{
			Getbutler ();
			GetChopSticks ();
			Releasebutler ();
			Eat ();
			ReleaseChopSticks ();
		}
	}
}

public class DiningPhilosophers
{

	private static Mutex[] _mutexs = new Mutex[5];
	private static Philosopher[] _philosophers = new Philosopher[5];

	private static void SetupDinnerTable()
	{
		for (int i = 0; i < _mutexs.Length; i++) 
		{
			_mutexs [i] = new Mutex ();
		}
		for (int i = 0; i < _philosophers.Length; i++) 
		{
			if (i != 0) 
			{
				_philosophers [i] = new Philosopher ("Philosopher " + (i + 1), _mutexs [i], _mutexs [i - 1]);
			} 
			else 
			{
				_philosophers [i] = new Philosopher ("Philosopher " + (i + 1), _mutexs [i], _mutexs [_mutexs.Length - 1]); 
			}
		}
	}

	public static void Main ()
	{
		SetupDinnerTable ();
		for (int i = 0; i < _philosophers.Length; i++) 
		{
			_philosophers [i].Start();
		}
	}
}