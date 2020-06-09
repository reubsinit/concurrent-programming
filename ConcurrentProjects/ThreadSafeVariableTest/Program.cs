using System;
using BitNak.Concurrent.Utils;

namespace ThreadSafeVariableTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{



			ThreadSafeVariable<int> value = new ThreadSafeVariable<int> (1);

			Console.WriteLine (value.Value);
		}

	}
}
