namespace Reubs.Collections.Generic
{
	/// <summary>
	/// An element that can be housed within a collection entity.
	/// </summary>
	public class SinglyLinkedNode<T>
	{
		/// <summary>
		/// Get/Set SinglyLinkedNode data.
		/// </summary>
		public T Data { get; set; }

		/// <summary>
		/// Get/Set SinglyLinkedNode's next SinglyLinkedNode.
		/// </summary>
		public SinglyLinkedNode<T> Next { get; set; }

		/// <summary>
		/// Initializes a new instance of SinglyLinkedNode.
		/// </summary>
		/// <param name="data">
		/// Used to specify the data that the node will represent.
		/// </param>
		public SinglyLinkedNode(T data)
		{
			Data = data;
		}
	}
}
