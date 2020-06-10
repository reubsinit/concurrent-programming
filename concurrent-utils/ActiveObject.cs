using System;
using System.Threading;

namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// Utility to control life cycle of a single thread.
	/// </summary>
	public abstract class ActiveObject
	{
		/// Thread controlled by ActiveObject
		private Thread _thread;

		/// <summary>
		/// Initializes a new instance of the ActiveObject class.
		/// </summary>
		/// <param name="name">
		/// Specifies the name of the thread that the ActiveObject will control.
		/// </param>
		public ActiveObject(String name)
		{
			_thread = new Thread(Run);
			_thread.Name = name;
		}

		/// <summary>
		/// Start the ActiveObject's thread.
		/// </summary>
		public void Start()
		{
			_thread.Start();
		}

		/// <summary>
		/// Abstract method run. Implement for thread activity.
		/// </summary>
		protected abstract void Run();
	}
}
