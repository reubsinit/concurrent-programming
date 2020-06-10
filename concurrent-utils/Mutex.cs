using System;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Utility used to control access to activity of a thread.
	/// </summary>
	/// <remarks>
	/// Access to activity is controlled by a single token. A thread may act if it can acquire the token.
	/// <see cref="BitNak.Concurrent.Utils.Semaphore"/> utility if more than a single token is required to control thread activity permission.
	/// </remarks>
	public class Mutex : Semaphore
	{
		// Locking object
		private readonly Object _lock = new Object();

		/// <summary>
		/// Initializes a new instance of Mutex.
		/// </summary>
		/// <param name="toBeAcquired">
		/// Specifies if the Mutex will be instantiated without the single token being available. Defaults to false.
		/// </param>
		public Mutex (bool toBeAcquired = false) : base(toBeAcquired ? (uint)0 : (uint)1) {}


		/// <summary>
		/// Make tokens available for offer.
		/// </summary>
		/// <param name="tokens">
		/// Specifies the number of tokens that will be made available for offer. Defaults to 1.
		/// </param>
		/// <exception cref="System.Exception">
		/// Thrown when more than a single token is released into Mutex or when Mutex token is already available and a release is issued.
		/// </exception>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when a releasing thread is interrupted.
		/// </exception>
		public override void Release (uint tokens = 1)
		{
			if (tokens != 1)
			{
				throw new Exception("Mutex is trying to release too many tokens.");
			}
			lock (_lock)
			{
				if (_tokens == 1)
				{
					throw new Exception("Mutex already contains maximum token count of 1");
				}
				//Call base.Release.
				base.Release (tokens);
			}
		}
	}
}
