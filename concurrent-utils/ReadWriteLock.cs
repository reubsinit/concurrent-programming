using System;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Utility used to control thread read and write access.
	/// </summary>
	/// <remarks>
	/// Ensures that multiple threads may read at any given time, while only one thread may write. No write permission will be granted if read permission has been acquired. No read permission will be granted if write permission has been acquired.
	/// </remarks>
	public class ReadWriteLock
	{
		// Allow only one thread to act at a time
		private Mutex _turnStile = new Mutex();
		// Allow only one thread at a time through to acquire write permission
		private readonly Mutex _writeTurnStile = new Mutex();
		// Allow only one thread to write
		private readonly Mutex _writePermission = new Mutex();
		// Allow for read permission
		private Switch _readPermission;

		/// <summary>
		/// Initializes a new instance of ReadWriteLock.
		/// </summary>
		public ReadWriteLock ()
		{
			_readPermission = new Switch(_writePermission);
		}

		/// <summary>
		/// Acquire read permission.
		/// </summary>
		public void AcquireReader()
		{
			// Only allow a single thread through at a time by using a turnstile system
			_turnStile.Acquire ();
			_turnStile.Release ();
			// Acquire the Switch object, which governs controls over the write permission Mutex
			_readPermission.Acquire ();
		}

		/// <summary>
		/// Release read permission.
		/// </summary>
		public void ReleaseReader()
		{
			//Release the Switch object, which governs controls over the write permission Mutex
			_readPermission.Release ();
		}

		/// <summary>
		/// Acquire write permission.
		/// </summary>
		public void AcquireWriter()
		{
			// Use a seperate turnstile for threads that seek write permission. Writing threads will queue here
			_writeTurnStile.Acquire ();
			// Enter the standard turnstile, acquire the write permission Mutex and release the standard turnstile to allow potential readers to queue
			_turnStile.Acquire ();
			_writePermission.Acquire ();
			_turnStile.Release ();
		}

		/// <summary>
		/// Release write permission.
		/// </summary>
		public void ReleaseWriter()
		{
			_writePermission.Release ();
			_writeTurnStile.Release ();
		}
	}
}
