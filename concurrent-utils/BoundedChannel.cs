using System.Threading;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// A BoundedChannel serves as a thread safe message passing pipeline with a maximum number of messages that may exist on the BoundedChannel at any given time.
	/// </summary>
	/// <remarks>
	/// <see cref="BitNak.Concurrent.Utils.Channel"/> for channel with no boundary.
	/// <see cref="BitNak.Concurrent.Utils.Semaphore"/> used to enforce channel boundary.
	/// </remarks>
	public class BoundedChannel<T>: Channel<T>
	{
		// Semaphore used to enforce BoundedChannel limit
		private readonly Semaphore _upperBoundary;

		/// <summary>
		/// Initializes a new instance of BoundedChannel.
		/// </summary>
		/// <param name="barrierLimit">
		/// Specifies the boundary of the BoundedChannel.
		/// </param>
		public BoundedChannel (uint boundary)
		{
			_upperBoundary = new Semaphore (boundary);
		}

		/// <summary>
		/// En-queue to the BoundedChannel.
		/// </summary>
		/// <param name="data">
		/// Specifies the data to be en-queued to the BoundedChannel.
		/// </param>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when en-queueing is interrupted.
		/// </exception>
		public override void Enqueue (T data)
		{
			_upperBoundary.Acquire ();
			base.Enqueue (data);
		}

		/// <summary>
		/// De-queue from the BoundedChannel.
		/// </summary>
		/// <returns>
		/// Returns data de-queued from the BoundedChannel.
		/// </returns>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when de-queueing is interrupted.
		/// </exception>
		public override T Dequeue ()
		{
			// Assign the value returned from the call to base.Enqueue
			T data = base.Dequeue ();
			// To avoid being interrupted while releasing, use ForceRelease
			_upperBoundary.ForceRelease ();
			// Return the de-queued value
			return data;
		}

		/// <summary>
		/// Try to en-queue to the BoundedChannel with a timeout period.
		/// </summary>
		/// <returns>
		/// Returns Boolean, representing whether or not data was en-queued to the BoundedChannel.
		/// </returns>
		/// <param name="data">
		/// Specifies the data to be en-queued to the BoundedChannel.
		/// </param>
		/// <param name="milliseconds">
		/// Specifies the timeout period for en-queueing.
		/// </param>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when en-queueing is interrupted.
		/// </exception>
		public bool TryEnqueue (T data, int milliseconds)
		{
			// Catch cases which mean infinite timeout period
			if (milliseconds < -1)
			{
				milliseconds = -1;
			}

			// If a token has successfully been acquired from the BoundedChannel's boundary Semaphore, en-queue the data with base.Enqueue and assign bool true
			if (_upperBoundary.TryAcquire (milliseconds))
			{
				try
				{
					// An interrupt may occure here
					base.Enqueue (data);
				}
				catch (ThreadInterruptedException)
				{
					// Catch the interrupt, force release into the boundary and throw the caught exception
					_upperBoundary.ForceRelease();
					throw;
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Try to de-queue from the BoundedChannel with a timeout period.
		/// </summary>
		/// <returns>
		/// Returns Boolean, representing whether or not data was de-queued from the BoundedChannel.
		/// </returns>
		/// <param name="milliseconds">
		/// Specifies the timeout period for de-queueing.
		/// </param>
		/// <param name="result">
		/// Data that was de-queued from the BoundedChannel.
		/// </param>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when de-queueing is interrupted.
		/// </exception>
		public override bool TryDequeue (int milliseconds, out T result)
		{
			//If the call to base.TryDequeue succeeds, release a token into the boundary Semaphore and return true
			if (base.TryDequeue(milliseconds, out result))
			{
				// Could be interrupted here so use force release then return true
				_upperBoundary.ForceRelease ();
				return true;
			}
			// Otherwise, return false
			return false;
		}
	}
}
