using System;
using System.Collections.Generic;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Utility used to control access to activity of a thread. Works on a first in first out principle.
	/// </summary>
	/// <remarks>
	/// Access to activity is controlled by tokens. A thread may act if it can acquire a token.
	/// </remarks>
	/// <remarks>
	/// <see cref="BitNak.Concurrent.Utils.Semaphore"/> for standard Sempahore that does not consider first in first out.
	/// </remarks>
	public class FiFoSemaphore: Semaphore
	{
		// Queueing for threads waiting on tokens
		private Queue<Mutex> _queue = new Queue<Mutex>();
		// Locking object
		private readonly Object _lock = new Object();

		/// <summary>
		/// Initializes a new instance of FiFoSemaphore.
		/// </summary>
		/// <param name="tokens">
		/// Specifies the number of tokens that the FiFoSemaphore will offer. Defaults to 1.
		/// </param>
		public FiFoSemaphore (uint tokens = 1): base (tokens)
		{
			_tokens = tokens;
		}

		/// <summary>
		/// Acquire a token.
		/// </summary>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when an acquiring thread is interrupted.
		/// </exception>
		public override void Acquire()
		{
			Mutex m = null;
			lock (_lock)
			{
				// If there are no tokens, queue a Mutex with no tokens
				if (_tokens == 0)
				{
					m = new Mutex (true);
					_queue.Enqueue (m);
				}
				// Otherwise call base.Acquire
				else
				{
					base.Acquire ();
				}
			}
			// If a Mutex was queued acquire from it, meaning a thread is waiting to acquire a token from the FiFoSemaphore
			if (m != null)
			{
				m.Acquire ();
			}
		}

		/// <summary>
		/// Make tokens available for offer.
		/// </summary>
		/// <param name="tokens">
		/// Specifies the number of tokens that the FiFoSemaphore will make available for offer. Defaults to 1.
		/// </param>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when a releasing thread is interrupted.
		/// </exception>
		public override void Release(uint tokens = 1)
		{
			lock (_lock)
			{
				uint numToDequeue;
				// If tokens to release is less than the the number of waiting threads, set the number of tokens to release
				if (tokens < _queue.Count)
				{
					numToDequeue = tokens;
				}
				// Otherwise, set it to the number of waiting threads
				else
				{
					numToDequeue = (uint)_queue.Count;
				}
				// Iterate for the number of times to dequeue and release into each Mutex dequeued from the queue allowing waiting threads to proceed with activity
				for (int i = 0; i < numToDequeue; i++)
				{
					_queue.Dequeue ().Release ();
				}
				// If the tokens is greater than the number to dequeue, call base.Release with the difference of the two
				if (tokens > numToDequeue)
				{
					base.Release ((tokens - numToDequeue));
				}
			}
		}
	}
}
