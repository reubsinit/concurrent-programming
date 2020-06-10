using System;

namespace Reubs.Concurrent.Utils
{
	/// <summary>
	/// Utility used to synchronize activity of a set number of threads.
	/// </summary>
	public class Barrier
	{
		/// Number of threads barrier synchronizes
		protected uint _barrierLimit;
		/// Number of threads that are waiting at the barrier
		protected uint _threadCount;
		/// Used to ensure only one thread may arrive at the barrier at a time
		protected Mutex _turnStile;
		/// Thread activity permission
		protected Semaphore _actPermission;
		/// Locking object
		private readonly Object _lock = new Object();


		/// <summary>
		/// Initializes a new instance of Barrier.
		/// </summary>
		/// <param name="barrierLimit">
		/// Specifies the number of threads that the Barrier will synchronize. Defaults to 2.
		/// </param>
		public Barrier (uint barrierLimit = 2)
		{
			_barrierLimit = barrierLimit;
			_threadCount = 0;
			_turnStile = new Mutex ();
			_actPermission = new Semaphore ();
		}

		/// <summary>
		/// Arrive at the Barrier.
		/// </summary>
		/// <returns>
		/// Returns Boolean, representing whether or not all threads have arrived at the Barrier and are now synchronized.
		/// </returns>
		public Boolean Arrive()
		{
			Boolean result = false;
			// Thread arrives
			_turnStile.Acquire ();
			lock (_lock)
			{
				_threadCount++;
				// If all threads have arrived, open the Barrier and let threads commence with activity
				if (_threadCount == _barrierLimit)
				{
					_actPermission.Release (_barrierLimit);
					result = true;
				}
				else
				{
					_turnStile.Release ();
				}
			}
			// Waiting threads acquire from Semaphore and wait for final thread to trip Barrier
			_actPermission.Acquire ();
			lock (_lock)
			{
				// Let the threads leave
				_threadCount--;
				// If the last thread to leave, free the turnstile
				if (_threadCount == 0)
				{
					_turnStile.Release ();
				}
			}
			return result;
		}
	}
}
