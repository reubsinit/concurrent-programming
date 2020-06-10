using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Reubs.Concurrent.Utils;

namespace dms
{
	/// <summary>
	/// Processes Message objects received on the channel. Message objects contain a string that will be parsed as a query.
	/// </summary>
	public class QueryManager: ChannelBasedActiveObject<Message>
	{
		//The TableManager that this interacts with
		private TableManager _querying;
		//DateTime for processing time
		private DateTime _queryProcessStartTime;
		//Regex for query character validation
		private static readonly Regex _queryCharValidator = new Regex(@"^[0-9\s,rdguc:-]*$");
		private static readonly Regex _queryWordValidator = new Regex(@"\b(crub|drop|comp|quit|clearcache|transrc|commit|rollback|transru|transrr|transrs)\b");

		/// <summary>
		/// Initializes a new instance of QueryManager, querying the TableManager <param name="querying">, with the name <param name="name"> and input channel of <param name="channel">.
		/// </summary>
		/// <param name="querying">
		/// The instance of TableManager that will be queried.
		/// </param>
		/// <param name="name">
		/// The name of the filemanager.
		/// </param>
		/// <param name="channel">
		/// The filemanager's input channel.
		/// </param>
		public QueryManager(TableManager querying, String name, Channel<Message> channel = default(Channel<Message>)) : base (name, channel)
		{
			//Assign the querying member
			_querying = querying;
		}

		/// <summary>
		/// Process the Message object <param name="data">.
		/// </summary>
		/// <param name="data">
		/// The message to process.
		/// </param>
		protected override void Process(Message data)
		{
			//If the message's connection state is null, assign it a new IOState
			if (data.Connection.State == null)
			{
				data.Connection.State = new IOState (data.Connection.Client);
			} 
			//If the state of the IOState is read, process the message as normal
			if (data.Connection.State.CurrentState == IOState.State.Read)
			{
				ProcessMessage (data);
			} 
			//If the state of the IOState is alter, process the alter request
			if (data.Connection.State.CurrentState == IOState.State.Alter)
			{
				ProcessAlterRequest(data);
			}
		}

		/// <summary>
		/// Process the database alter request for Message object <param name="data">.
		/// </summary>
		/// <param name="data">
		/// The message to process; contains alter request.
		/// </param>
		private void ProcessAlterRequest(Message data)
		{
			if (data.Connection.State.InputState == IOState.RequestState.InitialRowInputRequest)
			{
				//Initial row input request, so send the user the appropriate message and change the state to string input.
				data.Connection.State.InputState = IOState.RequestState.RowStringInput;
				data.Connection.State.EchoBackToClient (data.Connection.State.GetPrompt (data.Connection.State.QueryType, data.Connection.State.InputState));
			} 
			else
			{
				if (data.Connection.State.InputState == IOState.RequestState.RowStringInput)
				{
					//At string input for alter request - change state to number input, get the string input and validate it
					//The input will be converted to 20 characters I.e. padded or trimmed
					//Prompt the user for next stage of input
					data.Connection.State.InputState = IOState.RequestState.RowNumberInput;
					data.Connection.State.WorkingRow.NewTextData = data.Connection.State.Get20CharString (data.MessageText);
					data.Connection.State.EchoBackToClient (data.Connection.State.GetPrompt (data.Connection.State.QueryType, data.Connection.State.InputState));
				} 
				else
				{
					if (data.Connection.State.InputState == IOState.RequestState.RowNumberInput)
					{
						//At number input - vaidate the number input to a float, if it is, row input is complete
						float newNumberData;
						if (float.TryParse (data.MessageText, out newNumberData))
						{
							data.Connection.State.InputState = IOState.RequestState.RowInputComplete;
							data.Connection.State.WorkingRow.NewNumberData = newNumberData;
							data.Connection.State.EchoBackToClient (data.Connection.State.GetPrompt (data.Connection.State.QueryType, data.Connection.State.InputState));
						} 
						else
						{
							//Number input was invaid so change state to invalid input and prompt the user to let them know that their input has failed. Finally, change state back to number input.,
							data.Connection.State.InputState = IOState.RequestState.RowInputInValid;
							data.Connection.State.EchoBackToClient (data.Connection.State.GetPrompt (data.Connection.State.QueryType, data.Connection.State.InputState));
							data.Connection.State.InputState = IOState.RequestState.RowNumberInput;
						}
					} 
					if (data.Connection.State.InputState == IOState.RequestState.RowInputComplete)
					{
						//If we're at the final row to alter I.e. out of a list of 5 rows to alter, finish the input from the user
						if (data.Connection.State.CurrentTransaction.WorkingRows.IndexOf (data.Connection.State.WorkingRow) == data.Connection.State.CurrentTransaction.WorkingRows.Count - 1)
						{
							data.Connection.State.InputState = IOState.RequestState.AllInputFinalised;
						}
						if (data.Connection.State.InputState != IOState.RequestState.AllInputFinalised)
						{
							//The user is now required to provide input for the next row that they've flagged for alter, so assign the IOState object's working row to the next row to alter
							data.Connection.State.WorkingRow = data.Connection.State.CurrentTransaction.WorkingRows[data.Connection.State.CurrentTransaction.WorkingRows.IndexOf(data.Connection.State.WorkingRow) + 1];
							//Set the input state to string input and prompt the user
							data.Connection.State.InputState = IOState.RequestState.RowStringInput;
							data.Connection.State.EchoBackToClient (data.Connection.State.GetPrompt (data.Connection.State.QueryType, data.Connection.State.InputState));
						} 
						else
						{
							//If the transaction type was default then commit it and set the IOState object to a new instance
							if (data.Connection.State.CurrentTransaction.Type == Transaction.TransactionType.Default)
							{
								_queryProcessStartTime = DateTime.Now;
								data.Connection.State.EchoBackToClient (DateTime.Now
								+ ": "
								+ data.Connection.State.CurrentTransaction.Commit ()
								+ " rows affected in database. Processed in "
								+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds
								+ " milliseconds.\n");
								data.Connection.State = new IOState (data.Connection.Client);
							} 
							else
							{
								//Otherwise, if it is a specified user tranasaction, reset the IOState instance
								data.Connection.State.Reset(data.Connection.Client);
							}
						}
					} 
				}
			}
		}

		/// <summary>
		/// Process the database query for Message object <param name="toProcess">.
		/// </summary>
		/// <param name="toProcess">
		/// The message to process; contains the query.
		/// </param>
		private void ProcessMessage(Message toProcess)
		{
			//Validate query characters
			if (IsValidQuery (toProcess.MessageText))
			{
				String command = "";
				List<int> workingNumbers;
				String[] commandParameters;

				//Get the command!
				foreach (char c in toProcess.MessageText)
				{
					if (!char.IsNumber (c) && c != ' ')
					{
						command += c;
					} 
					else
					{
						//We're breaking here because we've found a space or a number (parameter)
						break;
					}
				}

				//If the current transaction is null
				if (toProcess.Connection.State.CurrentTransaction == null)
				{
					//If a transaction has been specified, return from this method, otherwise, a default has been assigned.
					//We return because the user provides queries after they've specified their transaction of choice. I.e. seperate Message object
					if (TranasactionSpecified (toProcess.Connection, command))
					{
						return;
					}
				}

				//If the transaction requires a commit
				if (toProcess.Connection.State.CurrentTransaction.TransactionState == Transaction.State.Commit)
				{
					//If the user has specified a commit or rollback, we will return. There is no need to further process the Message object
					if (TransactionCloseRequested (toProcess.Connection, command))
					{
						return;
					}
				}

				//Get the command parameters. Paraneters are seperated with :
				commandParameters = toProcess.MessageText.Remove (0, command.Length).Split (':');

				//If the command parameters were in correct format
				//Get the desired rows primary key's from the command parameters and process the request for such rows
				if (TryProcessCommandData(commandParameters, out workingNumbers))
				{
					ProcessDatabaseRequest (toProcess.Connection, command, workingNumbers);
				} 
				//Otherwise, the user has provided an incomplete query I.e. r 0-10:99-:1,2,
				else
				{
					toProcess.Connection.State.EchoBackToClient (DateTime.Now + ": Incomplete query.\n");	
				}
			} 
			//If the query contains any illegal characters/keywords, it is invalid.
			else
			{
				toProcess.Connection.State.EchoBackToClient (DateTime.Now + ": Invalid query.\n");	
			}
		}

		/// <summary>
		/// Try process the command data. I.e. get the primary keys required for the specified query. Primary keys are returned in out parameter <param name="primaryKeys">.
		/// The range of primary keys to be resolved are contained within <param name="commandParameters">.
		/// </summary>
		/// <param name="commandParameters">
		/// The range of primary keys to be resolved.
		/// </param>
		/// <param name="primaryKeys">
		/// Out parameter; List containing all the specified primary keys.
		/// </param>
		private static bool TryProcessCommandData(String[] commandParameters, out List<int> primaryKeys)
		{
			primaryKeys = new List<int> ();
			String[] boundaries;
			String[] individualKeys;
			int lowerBound;
			int upperBound;
			int singlePrimaryKey;

			//Loop over the command parameters
			for (int i = 0; i < commandParameters.Length; i++)
			{
				//If the first parameter is a -
				if (commandParameters [i].Contains ("-"))
				{
					//We've found a boundary specification I.e. primary keys 1 to 10 1-10
					boundaries = commandParameters [i].Split ('-');
					//Try and get the boundaries. I.e lower boundary and upper boundary. If it fails, return false
					if (!(Int32.TryParse (boundaries [0], out lowerBound) && Int32.TryParse (boundaries [1], out upperBound)))
					{
						return false;
					} 
					else
					{
						//Otherwise, iterate from the lower boundary to the high and add each one to the list of primary keys to be returned
						for (int j = lowerBound; j <= upperBound; j++)
						{
							primaryKeys.Add (j);
						}
					}
				} 
				//If we've found the , a single primary key has been specified
				else if (commandParameters [i].Contains (","))
				{
					//Get the individual keys
					individualKeys = commandParameters [i].Split (',');
					for (int j = 0; j < individualKeys.Length; j++)
					{
						//If they're not ints, return false otherwise add each one to the list of primary keys to be returned
						if (!(Int32.TryParse (individualKeys [j], out singlePrimaryKey)))
						{
							return false;
						} 
						else
						{
							primaryKeys.Add (Int32.Parse (individualKeys [j]));
						}
					}
				} 
				//If only one primary key has been specified, add it to the list
				else if (Int32.TryParse(commandParameters [i], out singlePrimaryKey))
				{
					primaryKeys.Add(singlePrimaryKey);
				}
			}
			//All of the command parameters specified were valid, so return true. True means we were able to get the primary keys requested
			return true;
		}

		/// <summary>
		/// Process a request to create new rows where <param name="rowsToCreate"> specifies the number of rows to create and
		/// <param name="conn"> is the Connection that has sent the request.
		/// </summary>
		/// <param name="rowsToCreate">
		/// The number of new rows to create.
		/// </param>
		/// <param name="conn">
		/// The connection that sent the request.
		/// </param>
		private void ProcessCreateRequest(int rowsToCreate, Connection conn)
		{
			//Start the transaction with the appropriate request
			conn.State.CurrentTransaction.Start (_querying.GetNewRows(rowsToCreate), Transaction.Action.Create);
			//Set the IOState instance's working row to the current new row and set the IOState's state to DB modify
			conn.State.WorkingRow = conn.State.CurrentTransaction.WorkingRows[0];
			conn.State.CurrentState = IOState.State.Alter;
			conn.State.QueryType = IOState.RequestType.Create;
		}

		/// <summary>
		/// Process a request to read rows where <param name="primaryKeys"> specifies the primary keys of the rows to read and
		/// <param name="conn"> is the Connection that has sent the request.
		/// </summary>
		/// <param name="primaryKeys">
		/// The primary keys of the rows to read.
		/// </param>
		/// <param name="conn">
		/// The connection that sent the request.
		/// </param>
		private void ProcessReadRequest(List<int> primaryKeys, Connection conn)
		{
			//Set the query process time to now
			_queryProcessStartTime = DateTime.Now;
			//Start the transaction with the appropriate action, giving it the specified rows as requested from the TableManager
			conn.State.CurrentTransaction.Start (_querying.GetRows (primaryKeys).Rows, Transaction.Action.Read);

			//Tell the user about the rows returned
			conn.State.EchoBackToClient (DateTime.Now + ": " 
				+ conn.State.CurrentTransaction.WorkingRows.Count 
				+ " row(s) returned. Processed in " 
				+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds 
				+ " milliseconds.\n");

			//Iterate over each row in the transaction's rows
			foreach (Row r in conn.State.CurrentTransaction.WorkingRows)
			{
					//Read the row
					conn.State.CurrentTransaction.ReadRow (r);
					conn.State.EchoBackToClient ("--->"
					+ r.Status + " "
					+ r.PrimaryKey
					+ " "
					+ r.NewTextData.TrimEnd ('\0').PadRight (20, ' ')
					+ " "
					+ r.NewNumberData
					+ "\n");
					//Finish reading the row
					conn.State.CurrentTransaction.FinishReadRow (r);
			}
		}

		/// <summary>
		/// Process a request to delete rows where <param name="primaryKeys"> specifies the primary keys of the rows to delete and
		/// <param name="conn"> is the Connection that has sent the request.
		/// </summary>
		/// <param name="primaryKeys">
		/// The primary keys of the rows to delete.
		/// </param>
		/// <param name="conn">
		/// The connection that sent the request.
		/// </param>
		private void ProcessDeleteRequest(List<int> primaryKeys, Connection conn)
		{
			//Set the query process time to now
			_queryProcessStartTime = DateTime.Now;

			//Start the transaction with the appropriate action, giving it the specified rows as requested from the TableManager
			conn.State.CurrentTransaction.Start (_querying.GetRows (primaryKeys).Rows, Transaction.Action.Delete);
			//Set each row's action to delete
			foreach (Row r in conn.State.CurrentTransaction.WorkingRows)
			{
				r.Action = Row.RowAction.Delete;
			}
			//Tell the user about what just happened
			conn.State.EchoBackToClient (DateTime.Now 
				+ ": " 
				+ conn.State.CurrentTransaction.WorkingRows.Count 
				+ " rows affected. Processed in " 
				+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds 
				+ " milliseconds.\n");
		}

		/// <summary>
		/// Process a request to create a number of rubbish rows where <param name="numberOfRows"> specifies the number of rubbish rows to create and
		/// <param name="conn"> is the Connection that has sent the request.
		/// </summary>
		/// <param name="numberOfRows">
		/// The number of rubbish rows to create.
		/// </param>
		/// <param name="conn">
		/// The connection that sent the request.
		/// </param>
		private void ProcessCrubRequest(int numberOfRows, Connection conn)
		{
			//Set the query process time to now
			_queryProcessStartTime = DateTime.Now;
			//Tell the TableManager about the number of rubbish rows to create
			_querying.CreateRubbishRows (numberOfRows);

			//Inform the user that the rubbish rows have been created
			conn.State.EchoBackToClient (DateTime.Now 
				+ ": " 
				+ numberOfRows 
				+ " rubbish rows written to Database. Processed in " 
				+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds 
				+ " milliseconds.\n");
		}

		/// <summary>
		/// Process a request to compact the database (remove deleted rows) where
		/// <param name="conn"> is the Connection that has sent the request.
		/// </summary>
		/// <param name="conn">
		/// The connection that sent the request.
		/// </param>
		private void ProcessCompactRequest(Connection conn)
		{
			//Set the query process time to now
			_queryProcessStartTime = DateTime.Now;
			//Tell the TableManager to compact the database
			_querying.CompactDataBase ();
			//Tell the user all is good!
			conn.State.EchoBackToClient (DateTime.Now 
				+ ": Database compaction completed. Processed in " 
				+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds 
				+ " milliseconds.\n");
		}

		/// <summary>
		/// Process a request to clear table cache where
		/// <param name="conn"> is the Connection that has sent the request.
		/// </summary>
		/// <param name="conn">
		/// The connection that sent the request.
		/// </param>
		private void ProcessClearCacheRequest(Connection conn)
		{
			//Set the query process time to now
			_queryProcessStartTime = DateTime.Now;
			//Tell the TableManager to cear the cache
			_querying.ClearCache ();
			//Tell the user all is good!
			conn.State.EchoBackToClient (DateTime.Now 
				+ ": Table cache clear completed. Processed in " 
				+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds 
				+ " milliseconds.\n");
		}

		/// <summary>
		/// Process a request to drop the database where
		/// <param name="conn"> is the Connection that has sent the request.
		/// </summary>
		/// <param name="conn">
		/// The connection that sent the request.
		/// </param>
		private void ProcessDropRequest(Connection conn)
		{
			//Set the query process time to now
			_queryProcessStartTime = DateTime.Now;
			//Tell the TableManager to drop and rebuild the database
			_querying.DropAndReBuild ();
			//Tell the user that such tasks have been completed
			conn.State.EchoBackToClient (DateTime.Now 
				+ ": Database drop and rebuild completed. Processed in " 
				+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds 
				+ " milliseconds.\n");
		}

		/// <summary>
		/// Process a request to update rows where <param name="primaryKeys"> specifies the primary keys of the rows to update and
		/// <param name="conn"> is the Connection that has sent the request.
		/// </summary>
		/// <param name="primaryKeys">
		/// The primary keys of the rows to update.
		/// </param>
		/// <param name="conn">
		/// The connection that sent the request.
		/// </param>
		private void FlagUpdate(List<int> primaryKeys, Connection conn)
		{
			//Get the rows that need to be updated from the TableManager
			List<Row> rows = _querying.GetRows (primaryKeys).Rows;

			//Set the IOState's working row to the appropriate row and set it's state to database alter
			conn.State.WorkingRow = rows[0];

			conn.State.CurrentState = IOState.State.Alter;
			conn.State.QueryType = IOState.RequestType.Update;
			//Start the transaction with the appropriate action, giving it the specified rows as requested from the TableManager
			conn.State.CurrentTransaction.Start(rows, Transaction.Action.Update);
			conn.State.EchoBackToClient ("The following rows are flagged for update:\n");

			//Print out each row that is going to be updated and set it's action to Update
			foreach (Row row in rows)
			{
				row.Action = Row.RowAction.Update;

				conn.State.EchoBackToClient ("--->"
					+ row.Status
					+ " "
					+ row.PrimaryKey
					+ " "
					+ row.TextData
					+ " "
					+ row.NumberData
					+ "\n");
			}
		}

		/// <summary>
		/// Process the validated query sent from <param name="connection"> where the specified query is represented by <param name="command">
		/// and the primary keys for the rows involved with the query are provided in <param name="workingNumbers">
		/// </summary>
		/// <param name="connection">
		/// The connection that sent the request.
		/// </param>
		/// <param name="command">
		/// The specified query.
		/// </param>
		/// <param name="workingNumbers">
		/// The primary keys of the rows involved with the query.
		/// </param>
		private void ProcessDatabaseRequest(Connection connection, String command, List<int> workingNumbers)
		{
			//If the current transaction requires a commit, tell the user they must do so before issuing more commands
			if (connection.State.CurrentTransaction.TransactionState == Transaction.State.Commit)
			{

				connection.State.EchoBackToClient (DateTime.Now 
					+ ": The current transaction requires a commit. Please commit before issuing further commands.\n");
				return;
			}

			switch (command)
			{
				//Handle a request to create rows
				case "c":
				{
					//If the list has more than one value, the user has used the create command incorrectly
					if (workingNumbers.Count > 1)
					{
						connection.State.EchoBackToClient (DateTime.Now + ": Incorrect usage of create command.\n");
						break;
					} 
					//No values in the list means create 1 new row. Command like this was issued 'c'
					else if (workingNumbers.Count == 0)
					{
						ProcessCreateRequest (1, connection);
					} 
					//Otheriwse, create the number of new rows as request. I.e 'c 5'
					else
					{
						ProcessCreateRequest (workingNumbers[0], connection);
					}
					break;
				}
				//Handle a request to read rows
				case "r":
				{
					ProcessReadRequest (workingNumbers, connection);
					break;
				}
				//Handle a request to update rows
				case "u":
				{
					FlagUpdate (workingNumbers, connection);
					break;
				}
				//Handle a request to delete rows
				case "d":
				{
					ProcessDeleteRequest (workingNumbers, connection);
					break;
				}
				//Handle a request to create rubbish rows
				case "crub":
				{
					if (workingNumbers.Count > 1)
					{
						connection.State.EchoBackToClient (DateTime.Now + ": Incorrect usage of crub command.\n");
						break;
					}
					ProcessCrubRequest (workingNumbers[0], connection);
					break;
				}
				//Handle a request to compact the database
				case "comp":
				{
					if (workingNumbers.Count > 0)
					{
						connection.State.EchoBackToClient (DateTime.Now + ": Incorrect usage of compact command.\n");
						break;
					}
					ProcessCompactRequest (connection);
					break;
				}
				//Handle a request to drop the database 
				case "drop":
				{
					if (workingNumbers.Count > 0)
					{
						connection.State.EchoBackToClient (DateTime.Now + ": Incorrect usage of drop command.\n");
						break;
					}
					ProcessDropRequest (connection);
					break;
				}
				//Handle a request to clear the table cache
				case "clearcache":
				{
					if (workingNumbers.Count > 0)
					{
						connection.State.EchoBackToClient (DateTime.Now + ": Incorrect usage of cache clear command.\n");
						break;
					}
					ProcessClearCacheRequest (connection);
					break;
				}
				//Handle a request to quit the application
				case "quit":
				{
					if (workingNumbers.Count > 0)
					{
						connection.State.EchoBackToClient (DateTime.Now + ": Incorrect usage of quit command.\n");
						break;
					}
					connection.State.EchoBackToClient (DateTime.Now + ": You have disconnected from the server.\n");
					connection.Client.Shutdown(SocketShutdown.Both);
					break;
				}
			}
			//Check to see if the current transaction is a default and it is currently not involved in an alter request
			if (connection.State.CurrentTransaction.Type == Transaction.TransactionType.Default && connection.State.CurrentState != IOState.State.Alter)
			{
				//Commit it and reassign the current transaction to null. Default's are commited as soon as the requested query is resolved
				connection.State.EchoBackToClient (DateTime.Now 
					+ ": " 
					+ connection.State.CurrentTransaction.Commit()
					+ " rows affected in database. Processed in " 
					+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds 
					+ " milliseconds.\n");
				connection.State.CurrentTransaction = null;
			}
		}

		/// <summary>
		/// Check to see if the transaction has been finalised by <param name="connection"> where the specified finalisation command is <param name="command">.
		/// </summary>
		/// <param name="connection">
		/// The connection that sent the request to finalise the current transaction.
		/// </param>
		/// <param name="command">
		/// The finalisation command.
		/// </param>
		private bool TransactionCloseRequested(Connection connection, String command)
		{
			switch (command)
			{
				case "commit":
				{
					//Commit the transaction it is not null and not a default transaction. Set it to null after commiting
					if (connection.State.CurrentTransaction != null && connection.State.CurrentTransaction.Type != Transaction.TransactionType.Default)
					{
						_queryProcessStartTime = DateTime.Now;
						connection.State.EchoBackToClient (DateTime.Now 
							+ ": " 
							+ connection.State.CurrentTransaction.Commit()
							+ " rows affected in database. Processed in " 
							+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds 
							+ " milliseconds.\n");
						connection.State.CurrentTransaction = null;
					} 
					//Otherwise, there is nothing to commit
					else
					{
						connection.State.EchoBackToClient (DateTime.Now 
							+ ": There is nothing to commit.\n");
					}
					//Return true because a finalisation command has been issued
					return true;
				}
				case "rollback":
				{
					//Rollback the transaction it is not null and not a default transaction. Set it to null after commiting
					if (connection.State.CurrentTransaction != null && connection.State.CurrentTransaction.Type != Transaction.TransactionType.Default)
					{
						_queryProcessStartTime = DateTime.Now;
						connection.State.EchoBackToClient (DateTime.Now 
							+ ": " 
							+ connection.State.CurrentTransaction.RollBack()
							+ " rows rolled back. Processed in " 
							+ (DateTime.Now - _queryProcessStartTime).TotalMilliseconds 
							+ " milliseconds.\n");
						connection.State.CurrentTransaction = null;

					} 
					//Otherwise, there is nothing to rollback
					else
					{
						connection.State.EchoBackToClient (DateTime.Now 
							+ ": There is nothing to rollback.\n");
					}
					//Return true because a finalisation command has been issued
					return true;
				}
				default:
				{
					//If a commit or rollback has not been specified, return false.
					return false;
				}
			}
		}

		/// <summary>
		/// Check to see if a transaction has been specified by <param name="connection"> where the specified transaction is <param name="command">.
		/// Create a new transaction appropriately based on what the user has specified.
		/// </summary>
		/// <param name="connection">
		/// The connection that sent the request to create a transaction.
		/// </param>
		/// <param name="command">
		/// The transaction to create.
		/// </param>
		private bool TranasactionSpecified(Connection connection, String command)
		{
			switch (command)
			{
				//Create a read commited transaction and return true as a transaction has been specified
				case "transrc":
				{
					connection.State.CurrentTransaction = new Transaction (_querying, Transaction.TransactionType.ReadCommitted);
					connection.State.EchoBackToClient (DateTime.Now 
						+ ": Created new Read Committed transaction.\n");
					return true;
				}
				//Create a read uncommited transaction and return true as a transaction has been specified
				case "transru":
				{
					connection.State.CurrentTransaction = new Transaction (_querying, Transaction.TransactionType.ReadUnCommitted);
					connection.State.EchoBackToClient (DateTime.Now 
						+ ": Created new Read Uncommitted transaction.\n");
					return true;
				}
				//Create a repeatable read transaction and return true as a transaction has been specified
				case "transrr":
				{
					connection.State.CurrentTransaction = new Transaction (_querying, Transaction.TransactionType.RepeatableRead);
					connection.State.EchoBackToClient (DateTime.Now 
						+ ": Created new Repeatable Read transaction.\n");
					return true;
				}
				//Create a read serialised transaction and return true as a transaction has been specified
				case "transrs":
				{
					connection.State.CurrentTransaction = new Transaction (_querying, Transaction.TransactionType.SerialisedRead);
					connection.State.EchoBackToClient (DateTime.Now 
						+ ": Created new Read Serialised transaction.\n");
					return true;
				}
				//No transaction has been specified so create a default transaction and return false, indicating the user has not set a transaction
				default:
				{
					connection.State.CurrentTransaction = new Transaction (_querying, Transaction.TransactionType.Default);
					connection.State.EchoBackToClient (DateTime.Now 
						+ ": Created new Default transaction.\n");
					return false;
				}
			}
		}

		/// <summary>
		/// Validate the query <param name="queryTocheck">. Return true if it is a valid query.
		/// </summary>
		/// <param name="queryTocheck">
		/// The query to checl.
		/// </param>
		private static Boolean IsValidQuery(string queryTocheck)
		{
			//Use the regex to validate query
			return _queryCharValidator.IsMatch(queryTocheck) || _queryWordValidator.IsMatch(queryTocheck);
		}
	}
}