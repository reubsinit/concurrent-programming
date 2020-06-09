using System;
using System.Threading;
using System.Diagnostics;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Declaration of public class Semaphore:
	/// When instantiated, a Semaphore can be used
	/// to control the activity of (a) thread(s).
	/// The semaphore can issue (a) token(s) to (a) thread(s)
	/// and only then can a thread proceed with actvity.
	/// Tokens are tracked by the <see cref="_tokens"/> field.
	/// </summary>
	public class Semaphore
	{
		/// <summary>
		/// Declaration of private field:
		/// <see cref="_tokens"/> as an unsigned int - Used to keep track of the number of tokens available.
		/// </summary>
		protected uint _tokens;
		private uint _threadsWaiting = 0;
		private readonly Object _lock = new Object(); 


		/// <summary>
		/// Declaration of Sempahore constructor: 
		/// Takes <see cref="uint"/><paramref name="tokens"/> as a parameter.
		/// If no value is provided to <paramref name="tokens"/>, the default value is set to 0.
		/// <see cref="_tokens"/> is assigned the value supplied to the <paramref name="tokens"/> parameter.
		/// </summary>
		/// <param name="tokens">
		/// Used to specify the number of tokens the instantiated Semaphore object will start with, stored in the <see cref="_tokens"/> field.
		/// </param>
		/// 
		public Semaphore (uint tokens = 0)
		{
			_tokens = tokens;
		}

		/// <summary>
		/// Declaration of method, Acquire:
		/// Attempt to acquire a token from the Semaphore object.
		/// If a token is available, the token will be acquired and the available number of tokens 
		/// will be decremented from the <see cref="_tokens"/> field.
		/// If a token is not available, wait until one is and attempt to acquire upon availability.
		/// </summary>
		//		public virtual void Acquire ()
		//		{
		//			lock (_lock) 
		//			{
		//				while (_tokens == 0) 
		//				{
		//					Monitor.Wait (this);
		//				}
		//				_tokens--;
		//			}
		//		}

		public virtual void Acquire ()
		{
			TryAcquire (Timeout.Infinite);
		}

		/// <summary>
		/// Declaration of virtual method, Release:
		/// This method may be overridden by child classes.
		/// Takes <see cref="uint"/><paramref name="tokens"/> as a parameter.
		/// If no value is provided to <paramref name="tokens"/>, the default value is set to 1.
		/// The Semaphore object will release, or make available n number of tokens
		/// based on the value provided to the <paramref name="tokens"/> parameter.
		/// Once made available, all threads waiting on a token are informed of token availability and attempt to acquire.
		/// </summary>
		/// <param name="tokens">
		/// Used to specify the number of tokens the Semaphore object will release, or make available.
		/// </param>
		public virtual void Release (uint tokens = 1)
		{
			lock (_lock)
			{
				_tokens += tokens;
				if (_threadsWaiting < tokens)
				{
					Monitor.PulseAll (_lock);
				} 
				else
				{
					for (int i = 0; i < tokens; i++)
					{
						Monitor.Pulse (_lock);
					}
				}
			}
		}



		public bool TryAcquire(int milliseconds)
		{
			Stopwatch watch = new Stopwatch ();
			int timeLeft;
			bool infiniteWait;
			bool tokenAcquired = false;

			if (milliseconds < -1)
			{
				milliseconds = -1;
			}

			infiniteWait = milliseconds == Timeout.Infinite;

			watch.Start ();

			do
			{
				if (infiniteWait)
				{
					timeLeft = Timeout.Infinite;
				}
				else
				{
					timeLeft = Math.Max (0, milliseconds - (int)watch.ElapsedMilliseconds);
				}
				//				timeLeft = infiniteWait ? Timeout.Infinite : Math.Max (0, milliseconds - (int)watch.ElapsedMilliseconds);

				lock (_lock)
				{
					try
					{
						_threadsWaiting++;
						if (_tokens <= 0)
						{
							Monitor.Wait (_lock, timeLeft);
						} 
					} 
					catch (ThreadInterruptedException)
					{
						if (_tokens > 0)
						{
							Thread.CurrentThread.Interrupt ();
						} 
						else
						{
							throw;
						}
					} 
					finally
					{
						_threadsWaiting--;
					}
					if (_tokens > 0)
					{
						_threadsWaiting--;
						tokenAcquired = true;
					}
				}
			} while (!tokenAcquired && (infiniteWait || watch.ElapsedMilliseconds < milliseconds));
			return tokenAcquired;
		}

		public void ForceRelease(uint tokens = 1)
		{
			bool didRelease = false;
			bool threadInterrupted = false;

			do
			{
				try
				{
					Release(tokens);
					didRelease = true;
				}
				catch (ThreadInterruptedException)
				{
					threadInterrupted = true;
				}
			} while (!didRelease);

			if (threadInterrupted)
			{
				Thread.CurrentThread.Interrupt ();
			}
		}
	}
}

