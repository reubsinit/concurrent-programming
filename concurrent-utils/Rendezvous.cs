using System;

namespace Reubs.Concurrent.Utils
{
	/// <summary>
	/// Utility used to synchronize activity of two threads.
	/// </summary>
	/// <remarks>
	/// <see cref="Reubs.Concurrent.Utils.Barrier"/> for utility to synchronize activity of more than two threads.
	/// </remarks>
	public class Rendezvous: Barrier
	{
		/// <summary>
		/// Initializes a new instance of Rendezvous.
		/// </summary>
		public Rendezvous ()
		{
			_barrierLimit = 2;
			_threadCount = 0;
			_turnStile = new Mutex ();
			_actPermission = new Semaphore ();
		}
	}
}
