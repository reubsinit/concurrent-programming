namespace BitNak.Concurrent.Utils
{
	/// <summary>
	/// A ChannelBasedActiveObject serves as a Channel managed by an ActiveObject.
	/// </summary>
	/// <remarks>
	/// <see cref="BitNak.Concurrent.Utils.Channel"/> for Channel utility.
	/// </remarks>
	/// <remarks>
	/// <see cref="BitNak.Concurrent.Utils.ActiveObject"/> for ActiveObject utility.
	/// </remarks>
	public abstract class ChannelBasedActiveObject<T>: ActiveObject
	{
		// Channel that ActiveObject will control
		protected Channel<T> _channel;

		/// <summary>
		/// Initializes a new instance of ChannelBasedActiveObject.
		/// </summary>
		/// <param name="name">
		/// Specifies the name of the ActiveObject managing the ChannelBasedActiveObject.
		/// </param>
		/// <param name="channel">
		/// Specifies the Channel that the ChannelBasedActiveObject manages. Defaults to a new instance of Channel.
		/// </param>
		protected ChannelBasedActiveObject (string name, Channel<T> channel = default(Channel<T>)) : base(name)
		{
			_channel = channel;
		}

		/// <summary>
		/// Get the Channel.
		/// </summary>
		/// <returns>
		/// Returns the Channel managed by the ChannelBasedActiveObject.
		/// </returns>
		public Channel<T> Channel
		{
			get
			{
				return _channel;
			}
		}

		/// <summary>
		/// Abstract method Process. Implement to specify how data on Channel should be processed.
		/// </summary>
		/// <param name="data">
		/// Specifies the data that the ChannelBasedActiveObject will process.
		/// </param>
		abstract protected void Process(T data);

		/// <summary>
		/// Run the ChannelBasedActiveObject.
		/// </summary>
		/// <remarks>
		/// Data will be de-queued from the Channel and processed.
		/// <see cref="BitNak.Concurrent.Utils.ChannelBasedActiveObject.Process"/> for how data will be processed.
		/// </remarks>
		protected override void Run()
		{
			// For as long as the ChannelBasedActiveObject is active, process data that is on the Channel
			while (true)
			{
				Process (_channel.Dequeue ());
			}
		}
	}
}
