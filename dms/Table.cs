using System;
using System.Collections.Generic;
using Reubs.Concurrent.Utils;

namespace dms
{
	public class Table
	{
		/// <summary>
		/// Initializes a new instance of Table.
		/// </summary>
		public Table ()
		{
			//Assign the Table's create access
			CreateAccess = new Mutex ();
			//Set the boolean flag for serialised access
			SerialisedAccess = false;
			//Dictionary of cached rows where the key is the primary key of the row
			CachedRows = new Dictionary<int, WeakReference> ();
			//Object for locing
			Lock = new object ();
		}

		//Object for locking
		public object Lock { get; set; }
		//The create access mutex
		public Mutex CreateAccess { get; set; }
		//The dictionary for the weak reference rows
		public Dictionary<int, WeakReference> CachedRows{ get; set; }
		//The working primary key of the table
		public int WorkingPrimarykey{ get; set; }
		//The boolean flag for serialised access
		private bool SerialisedAccess { get; set; }

		/// <summary>
		/// Is the table locked for a read serialised transaction.
		/// </summary>
		public bool IsSerialised()
		{
			//Return the boolean flag under lock
			lock (Lock)
			{
				return SerialisedAccess;
			}
		}

		/// <summary>
		/// Acquire the table's create access in the event that a read serialised transaction is in use.
		/// </summary>
		public void GetSerialisedAccess()
		{
			//Set the boolean flag under a lock
			lock (Lock)
			{
				SerialisedAccess = true;
			}
			//Acquire the mutex that represents the create access
			CreateAccess.Acquire ();
		}

		/// <summary>
		/// Release the table's create access in the event that a read serialised transaction has been finalised.
		/// </summary>
		public void ReleasedSerialisedAccess()
		{
			//Set the boolean flag under a lock
			lock (Lock)
			{
				SerialisedAccess = false;
			}
			//Release the mutex that represents the create access
			CreateAccess.Release ();
		}

		/// <summary>
		/// Check to see if the table contains a row with the primary key <param name="primaryKey">.
		/// </summary>
		/// <param name="primaryKey">
		/// The primary key of the row to check for.
		/// </param>
		public bool ContainsRow(int primaryKey)
		{
			Row theRow;
			lock (Lock)
			{
				//if the cached rows has the key
				if (CachedRows.ContainsKey (primaryKey))
				{
					//Get the weak reference that represents the row
					WeakReference weakRow = CachedRows [primaryKey];
					if (weakRow.IsAlive)
					{
						//If it's alive and not null, return true as the table has the row
						if (weakRow.Target is Row)
						{
							theRow = weakRow.Target as Row;
							if (theRow != null)
							{
								return true;
							}
						}
					} 
					else
					{
						//Otherwise, remove it from the cached rows
						CachedRows.Remove (primaryKey);
					}
				}
				// :( no, the row isn't here
				return false;
			}
		}

		/// <summary>
		/// Get new a number of new rows where the number of rows is specified by <param name="numberOfRows">.
		/// </summary>
		/// <param name="primaryKey">
		/// The primary key of the row to check for.
		/// </param>
		public List<Row> GetNewRows(int numberOfRows)
		{
			List<Row> result = new List<Row> ();
			for (int i = 0; i < numberOfRows; i++)
			{
				//Create the new row
				Row row = new Row ();
				//Set it to proposed as it is a new row
				row.Action = Row.RowAction.Proposed;
				//Under lock!
				lock (Lock)
				{
					//Assign it's primary key
					row.PrimaryKey = WorkingPrimarykey;
					//Add it to the cached rows
					CachedRows.Add (row.PrimaryKey, new WeakReference (row));
					//Increment the working primary key
					WorkingPrimarykey++;
				}
				//Add the new row to thje list of rows to return
				result.Add (row);
			}
			//Serve the new rows lol
			return result;
		}

		/// <summary>
		/// Get the rows from the table that match the primary keys in <param name="primaryKeys">.
		/// </summary>
		/// <param name="primaryKeys">
		/// The primary keuys of the rows to get.
		/// </param>
		public List<Row> GetRows(List<int> primaryKeys)
		{
			List<Row> result = new List<Row> ();
			Row toReturn;
			foreach (int primaryKey in primaryKeys)
			{	
				lock (Lock)
				{
					if (CachedRows.ContainsKey (primaryKey))
					{
						WeakReference weakRow = CachedRows [primaryKey];
						if (weakRow.IsAlive)
						{
							toReturn = weakRow.Target as Row;
							if (toReturn != null)
							{
								//If it's there and alive and not null, we want to return it
								result.Add (toReturn);
							}
						} 
						else
						{
							//If it is not alive, remove it from the cached rows
							CachedRows.Remove (primaryKey);
						}
					}
				}
			}
			//Yay! Return the request rows!
			return result;
		}
	}
}


