using System.Collections.Generic;
namespace dms
{
	public class Transaction
	{
		//Enum for transaction type
		public enum TransactionType {Default, ReadUnCommitted, ReadCommitted, RepeatableRead, SerialisedRead}
		//Enum for the type of action the transaction is commited to
		public enum Action {Read, Update, Create, Delete}
		//Enum for the state of the transaction
		public enum State {PreCommit, Commit}

		/// <summary>
		/// Initializes a new instance of Transaction, related to the TableManager <param name="manager">, and of type <param name="type">.
		/// </summary>
		/// <param name="manager">
		/// The instance of TableManager that the transaction is related to.
		/// </param>
		/// <param name="type">
		/// The type of the transaction.
		/// </param>
		public Transaction (TableManager manager, TransactionType type)
		{
			//Assign the TableManager and type
			Manager = manager;
			Type = type;
			//If the tpye is Serialised Read, then we want to get the Table's create access
			if (Type == TransactionType.SerialisedRead)
			{
				Manager.Table.GetSerialisedAccess();
			}
			//Instantiate the transactions working rows
			WorkingRows = new List<Row> ();
			//Set the state of the transaction to pre commit as the transaction has not been used for anything yet
			TransactionState = State.PreCommit;
		}

		//Transaction type 
		public TransactionType Type { get; set; }
		//The type of request the transaction is being used for. I.e. crub
		public Action Request { get; set; }
		//The state of the transaction. I.e. does it require a commit
		public State TransactionState { get; set; }
		//The rows that the transaction is working with
		public List<Row> WorkingRows { get; set; }
		//The TableManager the transaction is related to
		public TableManager Manager{ get; set; }

		/// <summary>
		/// Start the Transaction, with the rows <param name="transactionRows">, and the action <param name="action">.
		/// </summary>
		/// <param name="transactionRows">
		/// The rows the transaction will work with.
		/// </param>
		/// <param name="action">
		/// The type of action the transaction is being used for.
		/// </param>
		public void Start (List<Row> transactionRows, Action action)
		{
			//Assign the action
			Request = action;
			//Assign the transactions working rows
			WorkingRows = transactionRows;
			//If the transaction is not being used to read rows
			if (Request != Action.Read)
			{
				//If the action is create and the table has had it's create access acquired and the current transaction is not a Serialised Read, acquire the table's create access
				//This will happen when a Serialised Read is yet to be committed and some other form of transaction has been used to create rows, locking them out of the table
				if ((Request == Action.Create && Manager.Table.IsSerialised()) && Type != TransactionType.SerialisedRead)
				{
					Manager.Table.CreateAccess.Acquire ();
				}
				//Get the write access of the rows that are going to be created
				GetRowsWriteAccess (transactionRows);
			}
			//The transaction now requires a commit
			TransactionState = State.Commit;
		}

		/// <summary>
		/// Read the Transaction row <param name="row">.
		/// </summary>
		/// <param name="row">
		/// The transaction row that will be read.
		/// </param>
		public virtual void ReadRow(Row row)
		{
			//If it is read committed, we want to acquire the reader on the row
			if (Type == TransactionType.ReadCommitted)
			{
				row.Lock.AcquireReader ();
			} 
			//Otherwsie if it is repeatable read or serialised read, we want to acquire the writer lock of the row
			else if (Type == TransactionType.RepeatableRead || Type == TransactionType.SerialisedRead)
			{
				row.Lock.AcquireWriter ();
			}
		}

		/// <summary>
		/// Finish reading the Transaction row <param name="row">.
		/// </summary>
		/// <param name="row">
		/// The transaction row for which reading will finish.
		/// </param>
		public virtual void FinishReadRow(Row row)
		{
			//If the transaction is read committed, release the reader lock
			if (Type == TransactionType.ReadCommitted)
			{
				row.Lock.ReleaseReader ();
			}
		}

		/// <summary>
		/// Acquire the write access for every row in <param name="transactionRows">.
		/// </summary>
		/// <param name="transactionRows">
		/// The rows for which the write access will be acquire.
		/// </param>
		public void GetRowsWriteAccess(List<Row> transactionRows)
		{
			foreach (Row row in transactionRows)
			{
				row.Lock.AcquireWriter ();
			}
		}

		/// <summary>
		/// Release the write access for every row in <param name="transactionRows">.
		/// </summary>
		/// <param name="transactionRows">
		/// The rows for which the write access will be released.
		/// </param>
		public void ReleaseRowsWriteAccess(List<Row> transactionRows)
		{
			foreach (Row row in transactionRows)
			{
				row.Lock.ReleaseWriter ();
			}
		}

		/// <summary>
		/// Commit the transaction. Returns an integer representing the number of rows affected by the commit.
		/// </summary>
		public int Commit()
		{
			int result = 0;

			//Based on the transaction action
			switch (Request)
			{
				case Action.Read:
				{
					//Release the write access for all rows if the transaction is repeatable read or serialised read
					if (Type == TransactionType.RepeatableRead || Type == TransactionType.SerialisedRead)
					{
						ReleaseRowsWriteAccess (WorkingRows);
					}
					break;
				}
				case Action.Update:
				{
					//Pass the transaction rows to the TableManager for update and release the write access for all rows
					result += Manager.UpdateRows (WorkingRows);
					ReleaseRowsWriteAccess (WorkingRows);
					break;
				}
				case Action.Delete:
				{
					//Pass the transaction rows to the TableManager for delete and release the write access for all rows
					result += Manager.DeleteRows (WorkingRows);
					ReleaseRowsWriteAccess (WorkingRows);
					break;
				}
				case Action.Create:
				{
					result += WorkingRows.Count;
					//Pass the transaction rows to the TableManager for create and release the write access for all rows
					Manager.WriteRows (WorkingRows);
					ReleaseRowsWriteAccess (WorkingRows);
					break;
				}
			}
			//If the transaction is a serialised read, release the table's create access
			if (Type == TransactionType.SerialisedRead)
			{
				Manager.Table.ReleasedSerialisedAccess();
			}
			//Clear the working rows and return the number affected
			WorkingRows.Clear ();
			return result;

		}

		/// <summary>
		/// Rollback the transaction. Returns an integer representing the number of rows affected by the rollback.
		/// </summary>
		public int RollBack ()
		{
			int result = 0;

			foreach (Row row in WorkingRows)
			{
				//If the row is marked as deleted, return it to stable and release it's writer
				if (row.Action == Row.RowAction.Delete)
				{
					row.Action = Row.RowAction.Stable;
					result++;
					row.Lock.ReleaseWriter ();
				}
				//If the row is marked as update
				if (row.Action == Row.RowAction.Update)
				{
					//Revert it to stable
					row.Action = Row.RowAction.Stable;
					//Revert the data that reflects the update back to the state that it was before being flagged for update
					row.NewTextData = row.TextData;
					row.NewNumberData = row.NumberData;
					result++;
					//Release the writer
					row.Lock.ReleaseWriter ();
				}
				if (row.Action == Row.RowAction.Proposed)
				{
					//If it was proposed then remove the row from the table
					Manager.DeleteRowFromTable (row);
					result++;
				}
			}
			//If the transaction is a serialised read, release the table's create access
			if (Type == TransactionType.SerialisedRead)
			{
				Manager.Table.ReleasedSerialisedAccess();
			}
			//Clear the working rows and return the number affected
			WorkingRows.Clear ();
			return result;
		}
	}
}