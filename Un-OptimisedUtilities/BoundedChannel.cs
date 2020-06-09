using System;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Declaration of public class BoundedChannel - of generic type T - inherits from Barrier:
	/// Functions as a regular Channel when instantiated
	/// where the key difference is that there is a limit to the number
	/// of elements that the queue can contain at any given time.
	/// </summary>
	public class BoundedChannel<T>: Channel<T>
	{
		/// <summary>
		/// Declaration of private field:
		/// <see cref="_upperBoundary"/> as a Semaphore - Used to enfore the boundary.
		/// </summary>
		private Semaphore _upperBoundary;

		/// <summary>
		/// Declaration of BoundedChannel constructor: 
		/// Takes <see cref="uint"/><paramref name="boundary"/> as a parameter.
		/// <see cref="_upperBoundary"/> is instantiated as a new Semaphore object with n number of tokens
		/// as represented by the passed parameter boundary.
		/// </summary>
		/// <param name="boundary">
		/// Used to specify the number of tokens the instantiated Semaphore object will start with, enforcing the channel boundary.
		/// </param>
		public BoundedChannel (uint boundary)
		{
			_upperBoundary = new Semaphore (boundary);
		}

		/// <summary>
		/// Declaration of method, Enqueue:
		/// <see cref="_upperBoundary"/> Semaphore has a token acquired from it in order to decrement the number of available slots in the
		/// Queue <see cref="_queue"/>. Calls parent class Channel's Enqueue method.
		/// </summary>
		///<param name="data">
		/// Used to specify what will be enqueued onto the queue <see cref="_queue"/>.
		/// </param>
		public override void Enqueue (T data)
		{
			_upperBoundary.Acquire ();
			base.Enqueue (data);
		}

		/// <summary>
		/// Declaration of method, Dequeue - returns value of type T:
		/// Calls parent class Channel's Dequeue method and the value returned is assigned to local variable data of type T.
		/// <see cref="_upperBoundary"/> Semaphore then has a token release into it in order to increment the number of available slots in the
		/// Queue <see cref="_queue"/>.
		/// Local data is returned.
		/// </summary>
		public override T Dequeue ()
		{
			T data = base.Dequeue ();
			_upperBoundary.Release ();
			return data;
		}
	}
}

