using System;

namespace BitNak.Collections.Generic
{
	/// <summary>
	/// A thread safe collection entity that can house data in a queue.
	/// </summary>
	public class Queue<T>
	{
		// Head node
		private SinglyLinkedNode<T> _headNode = new SinglyLinkedNode<T>(default(T));
		// Tail node
		private SinglyLinkedNode<T> _tailNode;
		// Locking object for placing data onto the queue
		private Object _putLock = new Object ();
		// Locking object for taking data from the queue
		private Object _takeLock = new Object();

		/// <summary>
		/// Initializes a new instance of Queue.
		/// </summary>
		public Queue()
		{
			_tailNode = _headNode;
		}

		/// <summary>
		/// De-queue.
		/// </summary>
		/// <returns>
		/// Returns the top element in the queue.
		/// </returns>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when de-queueing is interrupted.
		/// </exception>
		public T Dequeue()
		{
			// Lock with the take lock. May throw TIE here
			lock (_takeLock)
			{
				lock(_headNode)
				{
					T data = default(T);
					SinglyLinkedNode<T> firstNode = _headNode.Next;

					if (firstNode != null)
					{
						data = firstNode.Data;
						firstNode.Data = default(T);
						_headNode = firstNode;
					}
					return data;
				}
			}
		}

		/// <summary>
		/// En-queue data.
		/// </summary>
		/// <param name="data">
		/// Specifies the data to be en-queued.
		/// </param>
		/// <exception cref="System.Threading.ThreadInterruptedException">
		/// Thrown when en-queueing is interrupted.
		/// </exception>
		public void Enqueue(T data)
		{
			SinglyLinkedNode<T> newNode = new SinglyLinkedNode<T>(data);
			// Lock the put lock. May throw TIE here
			lock (_putLock)
			{
				lock (_tailNode)
				{
					_tailNode.Next = newNode;
					_tailNode = newNode;
				}
			}
		}
	}
}
