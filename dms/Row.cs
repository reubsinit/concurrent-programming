using System;
using Reubs.Concurrent.Utils;

namespace dms
{
	public class Row
	{
		//Enum for the action of the row
		public enum RowAction {Proposed, Delete, Update, Stable}

		/// <summary>
		/// Initializes a new instance of Row.
		/// </summary>
		public Row ()
		{
			//The row's status is true
			Status = true;
			//Assign it's lock
			Lock = new ReadWriteLock ();
		}

		//The row action
		public RowAction Action { get; set; }
		//The row's primary key
		public int PrimaryKey{ get; set; }
		//The row status. I.e. deleted?
		public bool Status{ get; set; }
		//The row text data
		public String TextData{ get; set; }
		//The row new text data. I.e. used for updates
		public String NewTextData{ get; set; }
		//The row number data
		public float NumberData{ get; set; }
		//The row new number data. I.e. used for updates
		public float NewNumberData{ get; set; }
		//The row readwritelock
		public ReadWriteLock Lock{ get; set; }
	}
}

