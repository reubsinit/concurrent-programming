using System;
using System.Threading;
using System.Collections.Generic;
using BitNak.Concurrent.Utils;
using Barrier = BitNak.Concurrent.Utils.Barrier;
using Mutex = BitNak.Concurrent.Utils.Mutex;

class Calculator: ActiveObject
{
	private static double _sum = 0;
	private int _startIdx;
	private int _finishIdx;
	public int _myMin;
	public int _myMax;
	public int _mySum = 0;
	private Barrier _barrier;
	private int[] _data;
	private List<Calculator> _calcs = new List<Calculator> ();

	public Calculator(string name, int startIdx, int finishIdx, int[] data, Barrier barrier): base(name)
	{
		_startIdx = startIdx;
		_finishIdx = finishIdx;
		_data = data;
		_myMin = _data [startIdx];
		_myMax = _data [startIdx];
		_mySum = 0;
		_barrier = barrier;
	}

	public List<Calculator> Calcs
	{
		get 
		{
			return _calcs;
		}
		set 
		{
			_calcs = value;
		}
	}

	public Barrier Barrier
	{
		set 
		{
			_barrier = value;
		}
	}

	public int[] Data
	{
		set 
		{
			_data = value;
		}
	}

	public void DoCalculations()
	{
		for (int i = _startIdx + 1; i < _finishIdx; i++)
		{
			if (_myMin > _data [i])
			{
				_myMin = _data [i];
			}
		}
		for (int i = _startIdx + 1; i < _finishIdx; i++)
		{
			if (_myMax < _data [i])
			{
				_myMax = _data [i];
			}
		}
		for (int i = _startIdx; i < _finishIdx; i++)
		{
			_mySum += _data [i];
		}
		if (_barrier.Arrive())
		{
			int overallMin = _myMin;
			int overallMax = _myMax;
			int overallSum = 0;
			Console.WriteLine (Thread.CurrentThread.Name + ": Everyone has finished. Time to crunch data.\n");
			foreach (Calculator calc in _calcs)
			{
				if (calc._myMin < overallMin)
				{
					overallMin = calc._myMin;
				}
				if (calc._myMax > overallMax)
				{
					overallMin = calc._myMax;
				}
				overallSum += calc._mySum;
			}
			Console.WriteLine (Thread.CurrentThread.Name + ": Min " + overallMin + " - Max " + overallMax + " - Sum " + overallSum + ".\n");
		}
	}

	protected override void Run ()
	{
		DoCalculations ();
	}
}

class MainClass
{
	private static Calculator[] _threads = new Calculator[10];
	private static Barrier _barrier = new Barrier ((uint)_threads.Length);
	private static int[] _theData = new int[100000000];
	private static List<Calculator> _calcs = new List<Calculator>();

	public static void Main()
	{
		Random rand = new Random ();
		int currentStartIdx = 0;
		int currentfinishIdx = (_theData.Length / _threads.Length) - 1;

		for (int i = 0; i < _theData.Length; i++)
		{
			_theData [i] = rand.Next (1, 101);
		}
			
		for (int i = 0; i < _threads.Length; i++)
		{
			_threads [i] = new Calculator (("Calculator " + (i + 1)), currentStartIdx, currentfinishIdx, _theData, _barrier);
			currentStartIdx = currentfinishIdx;
			currentfinishIdx += _theData.Length / _threads.Length;
			_threads [i].Calcs.Add (_threads [i]);
			_calcs.Add (_threads [i]);
		}

		for (int i = 0; i < _threads.Length; i++)
		{
			_threads [i].Calcs = _calcs;
		}

		for (int i = 0; i < _threads.Length; i++)
		{
			_threads [i].Start ();
		}
	}
}