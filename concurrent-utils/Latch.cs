using System;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Utility used to synchronize activity of an undefined number of threads.
	/// </summary>
	/// <remarks>
	/// All threads waiting on the Latch will only continue with activity when the latch is released.
	/// <see cref="BitNak.Concurrent.Utils.Barrier"/> utility that will synchronize a set number of threads which automatically releases.
	/// </remarks>
	public class Latch
	{
		// Controls the state of the Latch
		private readonly Mutex _mutex;

		/// <summary>
		/// Initializes a new instance of Latch.
		/// </summary>
		public Latch ()
		{
			_mutex = new Mutex (true);
		}

		/// <summary>
		/// Acquire the Latch.
		/// </summary>
		public void Acquire ()
		{
			//Acquire and immediately release the Mutex.
			_mutex.Acquire ();
			_mutex.Release ();
		}

		/// <summary>
		/// Release the Latch.
		/// </summary>
		public void Release ()
		{
			//Release the Mutex.
			_mutex.Release ();
		}
	}
}
