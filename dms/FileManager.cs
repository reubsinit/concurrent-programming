
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Reubs.Concurrent.Utils;

namespace dms
{
	/// <summary>
	/// Manages the binary files that represent a single table and index.
	/// </summary>
	public class FileManager: ChannelBasedActiveObject<Request>
	{
		//Enum used to assist with the manner in which the method WriteRow should be used base on the type of writing required.
		private enum WriteType {NewRowWrite, RowMove, RowUpdate, RowDelete, IndexNewRowWrite, IndexRowMove}
		//Maximum number of characters for a string (row text data).
		private const uint MAX_STRING_DATA_BYTE_SIZE = 20;
		//The size of the table header in bytes.
		private const uint HEADER_BYTE_SIZE = MAX_STRING_DATA_BYTE_SIZE + sizeof(int) + sizeof(int) + sizeof(int);
		//The size of a single row in the table in bytes.
		private const uint ROW_BYTE_SIZE = sizeof(bool) + sizeof(int) + MAX_STRING_DATA_BYTE_SIZE + sizeof(float);
		//The position in bytes within a row where the string and float data is.
		private const uint ROW_DATA_LOCATION = sizeof(bool) + sizeof(int);
		//The position in bytes of the next primary key value in the table header.
		private const uint HEADER_INFO_NEXT_PRIMARY_KEY = MAX_STRING_DATA_BYTE_SIZE;
		//The postion in bytes of the total number of rows in the table.
		private const uint HEADER_INFO_NUM_ROWS = MAX_STRING_DATA_BYTE_SIZE + sizeof(int);
		//The position in bytes of the total number of rows that have been deleted.
		private const uint HEADER_INFO_NUM_ROWS_DELETED = MAX_STRING_DATA_BYTE_SIZE + sizeof(int) + sizeof(int);
		//The size of a row in bytes within the index file.
		private const uint INDEX_ROW_BYTE_SIZE = sizeof(int) + sizeof(int);
		//The name of the database file.
		private const String DATABASE_FILE_NAME = "database.dat";
		//The name of the database index file.
		private const String DATABASE_INDEX_FILE_NAME = "dbi.dat";

		//Track the next primary key to use.
		public int NextPrimaryKey { get; set; }
		//Track the total number of rows.
		private int RowCount { get; set; }
		//Track the total number of deleted rows.
		private int DeletedRowCount { get; set; }

		//Database stream.
		private FileStream DataBaseStream { get; set; }
		//Database index stream.
		private FileStream IndexStream { get; set; }
		//Database writer.
		private BinaryWriter DataBaseWriter { get; set; }
		//Database index writer.
		private BinaryWriter IndexWriter { get; set; }
		//Database reader.
		private BinaryReader DataBaseReader { get; set; }
		//Database index reader.
		private BinaryReader IndexReader { get; set; }
		//Database index (object representation).
		private Dictionary<int, int> Index { get; set; }
		//Database table (object representation).
		public Table Table { get; set;}

		/// <summary>
		/// Initializes a new instance of FileManager with the name <param name="name"> and input channel of <param name="channel">.
		/// </summary>
		/// <param name="name">
		/// The name of the filemanager.
		/// </param>
		/// <param name="channel">
		/// The filemanager's input channel.
		/// </param>
		public FileManager (string name, Channel<Request> channel = default(Channel<Request>)) : base(name, channel)
		{
			//Act on the status of the database.
			CheckDatabaseStatus ();
			//Initialise the table.
			Table = new Table ();
			Table.WorkingPrimarykey = NextPrimaryKey;
		}

		/// <summary>
		/// Act on the current status of the database. I.e. if it doesn't exist, create it.
		/// </summary>
		private void CheckDatabaseStatus()
		{
			// If it doesn't exist, create it, setup the header and the streams.
			if (!File.Exists (DATABASE_FILE_NAME))
			{
				SetUpStreams (FileMode.Create);
				WriteNewFileHeader ();
			} 
			else
			{
				// Otherwise, set up the streams
				SetUpStreams (FileMode.Open);
			}
			//Assign the header variables and build the object index.
			AssignHeaderVariables ();
			BuildIndex ();
		}

		/// <summary>
		/// Process the request <param name="data"> that has been dequeued from the filemanager's input channel.
		/// </summary>
		/// <param name="data">
		/// The request that the filemanger is obligated to complete.
		/// </param>
		protected override void Process(Request data)
		{
			if ((data.RowsToGet != null) && (data.RowsToGet.Count > 0))
			{
				//Get rows if we need to.
				GetRows (data);
			}
			if (data.RowsToUpdate != null)
			{
				//Modify the database if we need to.
				ModifyDataBase (data);
			}
		}

		/// <summary>
		/// Print the table header information to the console.
		/// </summary>
		public void PrintHeader()
		{
			DataBaseStream.Position = 0;
			Console.WriteLine (DataBaseReader.ReadChars(20));
			Console.WriteLine (DataBaseReader.ReadInt32 ());
			Console.WriteLine (DataBaseReader.ReadInt32 ());
			Console.WriteLine (DataBaseReader.ReadInt32 ());
		}

		/// <summary>
		/// Generates <paramref name="numRows"/>number of random rows.
		/// </summary>
		/// <param name="numRows">
		/// The number of random rows to generate.
		/// </param>
		public List<Row> GetRubbishRows(int numRows)
		{
			List<Row> result = new List<Row> ();
			// The character pool to generate random string data.
			String chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
			// Random for random generation.
			Random random = new Random();

			for (int i = 0; i < numRows; i++)
			{
				Row toReturn = new Row ();
				// Assign the new random row appropriate values. I.e. status, random string data and random float data.
				toReturn.Status = true;
				toReturn.NewTextData = new string((Enumerable.Repeat (chars, 19).Select (s => s [random.Next (s.Length)]).ToArray ()));
				toReturn.NewNumberData = (float)((random.NextDouble () * 2.0) - 1.0 * Math.Pow (2.0, random.Next (-126, 128)));
				// Add it to the list of rows to return.
				result.Add (toReturn);
			}
			return result;
		}

		/// <summary>
		/// Drop and rebuild the database.
		/// </summary>
		public void DropAndReBuild()
		{
			// Delete the database and the index.
			File.Delete (DATABASE_FILE_NAME);
			File.Delete (DATABASE_INDEX_FILE_NAME);
			// Setup the appropriate streams.
			SetUpStreams (FileMode.Create);
			// Write the new header for the database.
			WriteNewFileHeader ();
			// Assign working variables. I.e Next primary key, number of rows etc.
			AssignHeaderVariables ();
			// Reset the index.
			Index = new Dictionary<int, int>();
			Table = new Table ();
		}

		/// <summary>
		/// Get the rows that match the primary keys in <param name="primaryKeys">.
		/// </summary>
		/// <param name="primaryKeys">
		/// A list of <see cref="int"/> representing the primary keys of the rows to get.
		/// </param>
		public List<Row> GetRows(List<int> primaryKeys)
		{
			List<Row> result = new List<Row> ();
			foreach (int i in primaryKeys)
			{
				// If the index contains the primary key
				if (Index.ContainsKey (i))
				{
					// Set the database stream to the location of the row
					DataBaseStream.Position = (Index [i] - 1) * ROW_BYTE_SIZE + HEADER_BYTE_SIZE;
					Row toReturn = new Row ();
					// Assign the row values to a row.
					toReturn.Status = DataBaseReader.ReadBoolean ();
					toReturn.PrimaryKey = DataBaseReader.ReadInt32 ();
					toReturn.TextData = new string (DataBaseReader.ReadChars(20));
					toReturn.NumberData = DataBaseReader.ReadSingle ();
					toReturn.NewTextData = toReturn.TextData;
					toReturn.NewNumberData = toReturn.NumberData;
					toReturn.Action = Row.RowAction.Stable;
					// If the row hasn't been deleted, add it to the list of rows to return.
					if (toReturn.Status)
					{
						result.Add (toReturn);
					}

				}
			}
			return result;
		}

		/// <summary>
		/// Write the news rows to the database as specified in <param name="rows">.
		/// </summary>
		/// <param name="rows">
		/// The new rows to write to the database.
		/// </param>
		public void WriteNewRows(List<Row> rows)
		{
			// A dictionary for the new values that need to be written to the index file.
			Dictionary<int, int> toAddToIndex = new Dictionary<int, int> ();

			DataBaseStream.Position = (RowCount * ROW_BYTE_SIZE) + HEADER_BYTE_SIZE;
			foreach (Row r in rows)
			{
				// Write the row with the next primary key.
				WriteRow (r, NextPrimaryKey, WriteType.NewRowWrite);
				// Bump up the row count.
				RowCount++;
				// Add the new index file data to dictionary.
				toAddToIndex.Add (NextPrimaryKey, RowCount);
				// Add the new index data to the dictionary that references the index file.
				Index.Add (NextPrimaryKey, RowCount);
				// Bump up the primary key.
				NextPrimaryKey++;
			}
			// Set the database stream position to the header and update the header with the next primary key and the new row count.
			DataBaseStream.Position = HEADER_INFO_NEXT_PRIMARY_KEY;
			DataBaseWriter.Write (NextPrimaryKey);
			DataBaseWriter.Write (RowCount);

			//Set the index stream to the end of the file and write the new index values to tthe index file.
			IndexStream.Position = (RowCount - toAddToIndex.Count) * INDEX_ROW_BYTE_SIZE;
			foreach (KeyValuePair<int, int> primaryKeyRowLocation in toAddToIndex)
			{
				WriteIndexRow (primaryKeyRowLocation.Key, primaryKeyRowLocation.Value, WriteType.IndexNewRowWrite);
			}

		}

		/// <summary>
		/// Write the rows as specified in <param name="rows"> as deleted.
		/// </summary>
		/// <param name="rows">
		/// The rows that are to be marked as deleted.
		/// </param>
		public void MarkRowsDeleted(List<Row> rows)
		{
			foreach (Row row in rows)
			{
				// Write the row's status to false (deleted) and bump up the number of rows deleted.
				WriteRow(row, row.PrimaryKey, WriteType.RowDelete);
				DeletedRowCount++;
			}
			// Update the header information with the new number of rows deleted.
			DataBaseStream.Position = HEADER_INFO_NUM_ROWS_DELETED;
			DataBaseWriter.Write (DeletedRowCount);
		}

		/// <summary>
		/// Compact and resize both the database and database index file by overwriting rows that are marked as deleted
		/// with rows that are not marked as deleted and finally resizing the files appropriately.
		/// </summary>
		public void CompactDatabase()
		{
			// Location of the deleted row that will be overwritten.
			int locationToMoveTo;
			// The number of rows before compacting.
			int initialNumberOfRows = RowCount;
			// The location of the next row to check to see if it is not marked as deleted.
			int locationOfNextRowToCheck = RowCount;
			// Dictionary used to represent the new data in the database and the index file.
			Dictionary<Row, int> newIndex = new Dictionary<Row, int> ();

			// Search from the top to find a row that is marked as deleted.
			for (int i = 0; i < initialNumberOfRows; i++)
			{
				DataBaseStream.Position = (i * ROW_BYTE_SIZE) + HEADER_BYTE_SIZE;
				// If the row is marked as deleted
				if (!DataBaseReader.ReadBoolean ())
				{
					// Remove it from the dictionary index.
					Index.Remove (DataBaseReader.ReadInt32 ());
					// Set the location of the row that needs to be overwritten.
					locationToMoveTo = i + 1;
					// Search from the bottom up to the location of the current row to overwrite for a row that is not marked as deleted.
					for (int j = locationOfNextRowToCheck - 1; j >= locationToMoveTo; j--)
					{
						DataBaseStream.Position = (j * ROW_BYTE_SIZE) + HEADER_BYTE_SIZE;
						if (DataBaseReader.ReadBoolean ())
						{
							// We've got our row to move!
							Row rowToMove = new Row ();
							// Assign the location of the next row to check for as un-deleted.
							locationOfNextRowToCheck = j;
							// Read the row's details into a new row object
							rowToMove.Status = true;
							rowToMove.PrimaryKey = DataBaseReader.ReadInt32 ();
							rowToMove.TextData = new string(DataBaseReader.ReadChars (20));
							rowToMove.NumberData = DataBaseReader.ReadSingle ();
							// Add the row to the new index dictionary.
							newIndex.Add (rowToMove, locationToMoveTo);
							// Find the next row that is deletd.
							break;
						}
					}
				}
			}
			// Rearrange the database file and index file accordingly.
			RearrangeRowsAndIndex (newIndex);
		}

		/// <summary>
		/// Get the rows specified within the request <param name="data"> and update request accordingly.
		/// </summary>
		/// <param name="data">
		/// A get request.
		/// </param>
		private void GetRows(Request data)
		{
			// For each row returned from calling public GetRows
			foreach (Row rowToGet in GetRows(data.RowsToGet))
			{
				// Add the row to the request's result set.
				data.ResultSet.AddRow(rowToGet);
				// If the Table object that ressembles the data within the database file doesn't contain the row, add it.
				if (!Table.ContainsRow (rowToGet.PrimaryKey))
				{
					Table.CachedRows.Add(rowToGet.PrimaryKey, new WeakReference(rowToGet));
				}
			}
			//Release the latch locking the resultset's rows.
			data.ResultSet.Latch.Release ();
		}

		/// <summary>
		/// Handles request for writing, updating and deleting from the database as specified in the request <param name="data">.
		/// </summary>
		/// <param name="data">
		/// A request to alter the database in some way.
		/// </param>
		private void ModifyDataBase(Request data)
		{
			// Lists for rows to delete, write and updat.
			List<Row> rowsToDelete = new List<Row> ();
			List<Row> rowsToWrite = new List<Row> ();
			List<Row> rowsToUpdate = new List<Row> ();
			foreach (Row row in data.RowsToUpdate)
			{
				// Add the row to the appropriate list based on it's action.
				if (row.Action == Row.RowAction.Delete)
				{
					row.Status = false;
					rowsToDelete.Add (row);
				} 
				else if (row.Action == Row.RowAction.Proposed)
				{
					rowsToWrite.Add (row);
				} 
				else if (row.Action == Row.RowAction.Update)
				{
					rowsToUpdate.Add (row);
					row.Action = Row.RowAction.Stable;

				}
			}
			if (rowsToDelete.Count > 0)
			{
				// Mark the rows as deleted.
				MarkRowsDeleted (rowsToDelete);
				{
					//Release the latch for the result set
					data.ResultSet.Latch.Release ();
				}
			}
			if (rowsToWrite.Count > 0)
			{
				// Write the new rows to the database.
				WriteNewRows (rowsToWrite);
				if (data.ResultSet != null)
				{
					//Release the latch for the result set
					data.ResultSet.Latch.Release ();
				}
			}
			if (rowsToUpdate.Count > 0)
			{
				// Update the rows.
				UpdateRows (rowsToUpdate);
				//TODO: add rows to result set
				if (data.ResultSet != null)
				{
					//Release the latch for the result set
					data.ResultSet.Latch.Release ();
				}
			}
		}

		/// <summary>
		/// Build the database's object reference to the index file.
		/// </summary>
		private void BuildIndex()
		{
			Index = new Dictionary<int, int>();
			if (RowCount != 0)
			{
				IndexStream.Position = 0;
				for (int i = 0; i < RowCount; i++)
				{
					// Add each value from the index file to the index object.
					Index.Add (IndexReader.ReadInt32 (), IndexReader.ReadInt32 ());
				}
			}
		}

		/// <summary>
		/// Update the database row entries represented by the row objects in <param name="rows">, which contain the new values to be written.
		/// </summary>
		/// <param name="rows">
		/// The rows that contain the new information to be written to the database.
		/// </param>
		private void UpdateRows(List<Row> rows)
		{
			foreach (Row row in rows)
			{
				// Quick safety check to see if that row is actually in the index 
				if (Index.ContainsKey (row.PrimaryKey))
				{
					// If so, write that row.
					WriteRow (row, row.PrimaryKey, WriteType.RowUpdate);
					row.Action = Row.RowAction.Stable;
				}
			}
		}

		/// <summary>
		/// Write the row <param name="row"> with primary key <param name="primaryKey"> using the WriteType <param name="type">.
		/// </summary>
		/// <param name="row">
		/// Row to write.
		/// </param>
		/// <param name="primaryKey">
		/// Primary key of row to write.
		/// </param>
		/// <param name="type">
		/// The kind of row write to perform.
		/// </param>
		private void WriteRow(Row row, int primaryKey, WriteType type)
		{
			// If we're not udating an existing row. I.e. moving a row, writing a new row or deleting a row.
			if (type != WriteType.RowUpdate)
			{
				if (type != WriteType.NewRowWrite)
				{
					// Set the database stream to the location of the row
					DataBaseStream.Position = (Index [row.PrimaryKey] - 1) * ROW_BYTE_SIZE + HEADER_BYTE_SIZE;
				}
				// If the row is deleted, only write to it's status and leave the method.
				if (type == WriteType.RowDelete)
				{
					DataBaseWriter.Write (row.Status);
					return;
				}
				// Write the row's status and primary key.
				DataBaseWriter.Write (row.Status);
				DataBaseWriter.Write (primaryKey);
				// If it's a new row, write the new row's data and return from the method.
				if (type == WriteType.NewRowWrite)
				{
					DataBaseWriter.Write (row.NewTextData.PadRight(20, (char)0).ToCharArray());
					DataBaseWriter.Write (row.NewNumberData);
					return;
				}
			} 
			// If the row is being updated, set the database stream to the rows data locaton
			if (type == WriteType.RowUpdate)
			{
				DataBaseStream.Position = (Index [row.PrimaryKey] - 1) * ROW_BYTE_SIZE + HEADER_BYTE_SIZE + ROW_DATA_LOCATION;
			}
			// Write the row data. This will happen for rows being moved or updated.
			DataBaseWriter.Write (row.TextData.PadRight(20, (char)0).ToCharArray());
			DataBaseWriter.Write (row.NumberData);
		}
			
		/// <summary>
		/// Write an index row of key pair of <param name="rowPrimaryKey"> <param name="rowLocation"> using the WriteType <param name="type">.
		/// </summary>
		/// <param name="rowPrimaryKey">
		/// Primary key for row.
		/// </param>
		/// <param name="rowLocation">
		/// The location of the row with <param name="rowPrimaryKey"> in the database.
		/// </param>
		/// <param name="type">
		/// The kind of row write to perform.
		/// </param>
		private void WriteIndexRow(int rowPrimaryKey, int rowLocation, WriteType type)
		{
			// If we're moving an index within the index file (rellocating), set the stream position appropriately.
			if (type == WriteType.IndexRowMove)
			{
				IndexStream.Position = (rowLocation - 1) * INDEX_ROW_BYTE_SIZE;
			}
			// Write the index values.
			IndexWriter.Write (rowPrimaryKey);
			IndexWriter.Write (rowLocation);
		}

		/// <summary>
		/// Reorder the database and index rows with the dictionary of row - location in <param name="rowsToMove">.
		/// </summary>
		/// <param name="rowsToMove">
		/// Dictionary of row - location.
		/// </param>
		private void RearrangeRowsAndIndex(Dictionary<Row, int> rowsToMove)
		{
			// Set the new sizes of both the index and database file.
			DataBaseStream.SetLength(((RowCount - DeletedRowCount) * ROW_BYTE_SIZE) + HEADER_BYTE_SIZE);
			IndexStream.SetLength((RowCount - DeletedRowCount) * INDEX_ROW_BYTE_SIZE);
			// Write each row to it's new location. Write each index keyvalue pair of primary key location to the index file.
			// Update the object index.
			foreach (KeyValuePair<Row, int> data in rowsToMove)
			{
				Index [data.Key.PrimaryKey] = data.Value;
				WriteRow (data.Key, data.Key.PrimaryKey, WriteType.RowMove);
				WriteIndexRow (data.Key.PrimaryKey, data.Value, WriteType.IndexRowMove);
			}
			// Update the row count accordingly and reassign deleted rows the value of 0. We've just compacted!
			RowCount -= DeletedRowCount;
			DeletedRowCount = 0;
			DataBaseStream.Position = HEADER_INFO_NUM_ROWS;
			// Write that to the database header.
			DataBaseWriter.Write (RowCount);
			DataBaseWriter.Write (DeletedRowCount);
		}

		/// <summary>
		/// Setup the required streams (database and index) with the mode <param name="mode"> (Create or Open).
		/// </summary>
		/// <param name="mode">
		/// Mode to set the streams up with.
		/// </param>
		private void SetUpStreams(FileMode mode)
		{
			//Self explanatory really.
			DataBaseStream = new FileStream (DATABASE_FILE_NAME, mode);
			DataBaseWriter = new BinaryWriter (DataBaseStream);
			DataBaseReader = new BinaryReader (DataBaseStream);
			IndexStream = new FileStream (DATABASE_INDEX_FILE_NAME, mode);
			IndexWriter = new BinaryWriter (IndexStream);
			IndexReader = new BinaryReader (IndexStream);
		}

		/// <summary>
		/// Write the header details for the database file if it has just been created.
		/// </summary>
		private void WriteNewFileHeader()
		{
			DataBaseStream.Position = 0;
			DataBaseWriter.Write ("rubbishtable________".ToCharArray ());
			DataBaseWriter.Write (0);
			DataBaseWriter.Write (0);
			DataBaseWriter.Write (0);
		}

		/// <summary>
		/// Assign the private members the values read from the database file header. I.e. Next primary key, row count, number of rows deleted.
		/// </summary>
		private void AssignHeaderVariables()
		{
			DataBaseStream.Position = HEADER_INFO_NEXT_PRIMARY_KEY;
			NextPrimaryKey = DataBaseReader.ReadInt32 ();
			RowCount = DataBaseReader.ReadInt32 ();
			DeletedRowCount = DataBaseReader.ReadInt32 ();
		}
	}
}
