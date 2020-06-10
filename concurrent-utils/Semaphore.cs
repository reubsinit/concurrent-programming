using System;
using System.Threading;
using System.Diagnostics;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Utility used to control access to activity of a thread.
	/// </summary>
	/// <remarks>
	/// Access to activity is controlled by tokens. A thread may act if it can acquire a token.
	/// </remarks>
	public class Semaphore
	{
		// Number of tokens the Semaphore can offer
		protected uint _tokens;
		// Number of threads waiting to acquire a token
		private uint _threadsWaiting = 0;
		// Locking object
		private readonly Object _lock = new Object();


		/// <summary>
		/// Initializes a new instance of <see cref="Semaphore"/>.
		/// </summary>
		/// <param name="tokens">
		/// Specifies the number of tokens that the <see cref="Semaphore"/> will offer. Defaults to 0.
		/// </param>
		public Semaphore (uint tokens = 0)
		{
			_tokens = tokens;
		}

		/// <summary>
		/// Attempt to acquire a token with no timeout period.
		/// </summary>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when an acquiring thread is interrupted.
		/// </exception>
		public virtual void Acquire ()
		{
			TryAcquire (-1);
		}

		/// <summary>
		/// Make tokens available for offer.
		/// </summary>
		/// <param name="tokens">
		/// Specifies the number of tokens that will be made available for offer. Defaults to 1.
		/// </param>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when a releasing thread is interrupted.
		/// </exception>
		public virtual void Release (uint tokens = 1)
		{
			// Could throw ThreadInterruptedException here
			lock (_lock)
			{
				_tokens += tokens;
				//If the number of threads waiting on tokens is less than the number of tokens available, pulse all waiting threads
				if (_threadsWaiting < tokens)
				{
					Monitor.PulseAll (_lock);
				}
				//Otherwise, only a number of times that is equal to the number of tokens that is available
				else
				{
					for (int i = 0; i < tokens; i++)
					{
						Monitor.Pulse (_lock);
					}
				}
			}
		}

		/// <summary>
		/// Try to acquire a token with a timeout period.
		/// </summary>
		/// <param name="milliseconds">
		/// Specifies the timeout period for acquiring.
		/// </param>
		/// <returns>
		/// Returns Boolean, representing whether or not the acquirer was able to acquire a token.
		/// </returns>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when an acquiring thread is interrupted.
		/// </exception>
		public bool TryAcquire(int milliseconds)
		{
			// Watch to track of how long the acquirer has waited
			Stopwatch watch = new Stopwatch ();
			// Time left until timeout
			int timeLeft;
			// Catch cases which mean infinite timeout period
			if (milliseconds < -1)
			{
				milliseconds = -1;
			}
			watch.Start ();
			lock(_lock)
			{
				// While there are no tokens available, continuously try to acquire a token
				while (_tokens == 0)
				{
					// Calculate the time to wait until timeout occurs
					timeLeft = milliseconds == -1 ? -1 : milliseconds - (int)watch.ElapsedMilliseconds;
					// A thread is waiting for a token, so increment the number of threads waiting.
					_threadsWaiting++;
					try
					{
						// Thread has timed out this evaluates
						if (timeLeft < 0 && milliseconds != -1)
						{
							return false;
						}
						// Tell the thread to wait on the lock and the amount of time remaining
						if (!Monitor.Wait (_lock, timeLeft))
						{
							// If the thread has timed and upon doing so, there is a token available, break the loop and acquire a token. Otherwise return false
							if (_tokens > 0)
							{
								break;
							}
							return false;
						}
					}
					catch (ThreadInterruptedException)
					{
						// If a thread has been interrupted check to see if any tokens have been released and if so, interrupt the thread
						if (_tokens > 0)
						{
							Thread.CurrentThread.Interrupt ();
						}
						// Otherwsie, throw the exception
						else
						{
							throw;
						}
					}
					finally
					{
						// Once a thread has either acquired a token or has timed out, decrement the number of threads waiting
						_threadsWaiting--;
					}
				}
				_tokens--;
				return true;
			}
		}

		/// <summary>
		/// Make by force tokens available for offer.
		/// </summary>
		/// <param name="tokens">
		/// Specifies the number of tokens that will be made by force available for offer. Defaults to 1.
		/// </param>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when a releasing thread is interrupted.
		/// </exception>
		public void ForceRelease(uint tokens = 1)
		{
			// TIE variable set to null. Needed in case a thread is interrupted while attempting to force release a token in the Semaphore
			ThreadInterruptedException ex = null;
			// Infinite post test loop to ensure the designated number of tokens is released
			while (true)
			{
				try
				{
					// Release the tokens
					Release(tokens);
					// If the TIE variable references a valid TIE, interrupt the current thread
					if (ex != null)
					{
						Thread.CurrentThread.Interrupt();
					}
					// Tokens have been released so return
					return;
				}
				catch (ThreadInterruptedException e)
				{
					// A thread has been interrupted so assign our TIE variable to the thrown TIE
					ex = e;
				}
			}
		}
	}
}
