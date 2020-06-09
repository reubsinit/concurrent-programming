using System;
using System.Collections.Generic;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Declaration of public class Channel - of generic type T:
	/// When instantiated, a Channel is used 
	/// to support the direct communication of threads through 
	/// synchronised, message passing, where messages are of Type T. A queueing system is used
	/// in order to implement a system of first in first out.
	/// </summary>
	public class Channel<T>
	{
		/// <summary>
		/// Declaration of private fields:
		/// <see cref="_queue"/> as a Queue of type T - Used to queue messages of type T.
		/// <see cref="_access"/> as a Semaphore - Used to determine persmission to the Queue in <see cref="_access"/>.
		/// </summary>
		protected Queue<T> _queue = new Queue<T>();
		protected Semaphore _access = new Semaphore(0);

		/// <summary>
		/// Declaration of Channel constructor:
		/// <see cref="_queue"/> instantiated in member declaration.
		/// <see cref="_access"/> instantiated in member declaration with no available tokens by default.
		/// </summary>
		public Channel ()
		{
			
		}

		/// <summary>
		/// Declaration of method, Enqueue:
		/// Takes <paramref name="data"/> as a parameter of type T.
		/// <see cref="_queue"/> is locked and passed parameter <paramref name="data"/> is placed on the queue.
		/// <see cref="_access"/> Semaphore then has a token released into it so that the data that has been queued is now available
		/// to any waiting Threads.
		/// </summary>
		///<param name="data">
		/// Used to specify what will be enqueued onto the queue <see cref="_queue"/>.
		/// </param>
		public virtual void Enqueue(T data)
		{
			lock (this) 
			{
				_queue.Enqueue (data);
			}
			_access.Release ();
		}

		/// <summary>
		/// Declaration of method, Dequeue - returns value of type T:
		/// <see cref="_access"/> Semaphore is acquired and then the queue <see cref="_queue"/>
		/// is dequeued and value of type T is returned under a lock.
		/// </summary>
		public virtual T Dequeue()
		{
			_access.Acquire ();
			lock (this)
			{
				return _queue.Dequeue ();
			}
		}
	}
}

