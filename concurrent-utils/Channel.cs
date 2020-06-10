using System;
using System.Threading;
using BitNak.Collections.Generic;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// A Channel serves as a thread safe message passing pipeline.
	/// </summary>
	/// <remarks>
	/// <see cref="BitNak.Concurrent.Utils.BoundedChannel"/> for channel with a boundary.
	/// </remarks>
	public class Channel<T>
	{
		// Queue used for queue data
		protected Queue<T> _queue = new Queue<T>();
		// Queue access Semaphore
		protected Semaphore _access = new Semaphore(0);
		// Locking object
		private readonly Object _lock = new Object();

		/// <summary>
		/// Initializes a new instance of Channel.
		/// </summary>
		public Channel () {}

		/// <summary>
		/// Try to de-queue from the Channel with a timeout period.
		/// </summary>
		/// <returns>
		/// Returns Boolean, representing whether or not data was de-queued from the Channel.
		/// </returns>
		/// <param name="milliseconds">
		/// Specifies the timeout period for de-queueing.
		/// </param>
		/// <param name="result">
		/// Data that was de-queued from the Channel.
		/// </param>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when de-queueing is interrupted.
		/// </exception>
		public virtual bool TryDequeue (int milliseconds, out T result)
		{
			// Catch cases which mean infinite timeout period
			if (milliseconds < -1)
			{
				milliseconds = -1;
			}
			// Can de-queue happen
			if (_access.TryAcquire (milliseconds))
			{
				try
				{
					result = _queue.Dequeue ();
					return true;
				}
				catch (ThreadInterruptedException)
				{
					// Catch the exception if it is thrown and force release then throw
					_access.ForceRelease();
					throw;
				}
			}
			// Otherwise, assign the out variable the value of the default type, T, and return false
			else
			{
				result = default(T);
				return false;
			}
		}

		/// <summary>
		/// En-queue to the Channel.
		/// </summary>
		/// <param name="data">
		/// Specifies the data to be en-queued to the Channel.
		/// </param>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when en-queueing is interrupted.
		/// </exception>
		public virtual void Enqueue(T data)
		{
			_queue.Enqueue (data);
			//To avoid being interrupted while releasing, use ForceRelease.
			_access.ForceRelease ();
		}

		/// <summary>
		/// De-queue from the Channel.
		/// </summary>
		/// <returns>
		/// Returns data de-queued from the Channel.
		/// </returns>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when de-queueing is interrupted.
		/// </exception>
		public virtual T Dequeue()
		{
			//Calls TryDequeue with an infinite timeout period.
			T result;
			TryDequeue (-1, out result);
			return result;
		}
	}
}
