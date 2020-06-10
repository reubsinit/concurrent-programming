using System;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Utility used to allow exclusive access to activity permission.
	/// </summary>
	public class Switch
	{
		// Exclusivity controller
		private Semaphore _controlling;
		// Number of threads acting exclusivley through Switch
		private uint _arrived = 0;
		// Locking object
		private readonly Object _lock = new Object();

		/// <summary>
		/// Initializes a new instance of Switch.
		/// </summary>
		/// <param name="controlling">
		/// Specifies the Semaphore object that the Switch uses to control exlcusiv access.
		/// </param>
		public Switch (Semaphore controlling)
		{
			_controlling = controlling;
		}

		/// <summary>
		/// Acquire exclusive access.
		/// </summary>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when an acquiring thread is interrupted.
		/// </exception>
		public void Acquire()
		{
			lock (_lock)
			{
				// Increment the number of threads that have entered the Switch
				_arrived++;
				// If the first thread has arrived, then acquire the exclusive access
				if (_arrived == 1)
				{
					_controlling.Acquire ();
				}
			}
		}

		/// <summary>
		/// Release exlusive access.
		/// </summary>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when a releasing thread is interrupted.
		/// </exception>
		public void Release()
		{
			lock (_lock)
			{
				// Decrement the number of threads that are using the Switch
				_arrived--;
				// If the last thread is about to leave, release the exclusive access
				if (_arrived == 0)
				{
					_controlling.Release ();
				}
			}
		}
	}
}
