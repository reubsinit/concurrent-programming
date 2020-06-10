using System.Collections.Generic;
using Reubs.Concurrent.Utils;

namespace dms
{
	public class TableManager
	{
		public TableManager (Table table, FileManager fileManager, Channel<Request> outPutChannel)
		{
			Table = table;
			DataBaseFileManager = fileManager;
			OutPutChannel = outPutChannel;
		}

		public Table Table { get; set; }
		public FileManager DataBaseFileManager { get; set; }
		public Channel<Request> OutPutChannel { get; set; }

		public ResultSet GetRows(List<int> primaryKeys)
		{
			ResultSet result = new ResultSet();

			foreach (Row rowToReturn in Table.GetRows (primaryKeys))
			{ 
				if (rowToReturn.Action != Row.RowAction.Delete)
				{
					result.AddRow (rowToReturn);
				}
				if (primaryKeys.Contains(rowToReturn.PrimaryKey))
				{
					primaryKeys.Remove (rowToReturn.PrimaryKey);
				}
			}
			if (primaryKeys.Count > 0)
			{
				OutPutChannel.Enqueue (new Request (primaryKeys, result));
			} 
			else
			{
				result.Latch.Release ();
			}
			return result;
		}

		public List<Row> GetNewRows(int numberOfRows)
		{
			return Table.GetNewRows (numberOfRows);
		}

		public List<Row> GetRubbishRows(int numberOfRows)
		{
			return DataBaseFileManager.GetRubbishRows (numberOfRows);
		}

		public void CreateRubbishRows(int numberOfRows)
		{
			WriteRows (DataBaseFileManager.GetRubbishRows (numberOfRows));
			Table.WorkingPrimarykey = DataBaseFileManager.NextPrimaryKey;
		}

		public void DropAndReBuild()
		{
			DataBaseFileManager.DropAndReBuild();
			Table = DataBaseFileManager.Table;
		}

		public void CompactDataBase()
		{
			DataBaseFileManager.CompactDatabase ();
		}

		public int DeleteRows(List<Row> rows)
		{
			int rowsAffected = 0;
			foreach (Row row in rows)
			{
				if (Table.ContainsRow(row.PrimaryKey))
				{
					Table.CachedRows.Remove (row.PrimaryKey);
				}
				rowsAffected++;
			}
			Request result = new Request (rows, new ResultSet());
			OutPutChannel.Enqueue (result);
			result.ResultSet.Latch.Acquire ();

			return rowsAffected;
		}

		public void DeleteRowFromTable(Row row)
		{
			if (Table.ContainsRow(row.PrimaryKey))
			{
				Table.CachedRows.Remove (row.PrimaryKey);
				Table.WorkingPrimarykey--;
			}
		}

		public int UpdateRows(List<Row> rows)
		{
			int rowsAffected = rows.Count;
			foreach (Row row in rows)
			{
				row.TextData = row.NewTextData;
				row.NumberData = row.NewNumberData;
			}

			Request result = new Request (rows, new ResultSet());
			OutPutChannel.Enqueue (result);
			result.ResultSet.Latch.Acquire ();

			return rowsAffected;
		}

		public void WriteRows(List<Row> rows)
		{
			Request theRequest = new Request (rows, new ResultSet());
			OutPutChannel.Enqueue (theRequest);
			theRequest.ResultSet.Latch.Acquire ();
		}

		public void ClearCache()
		{
			Table.CachedRows.Clear();
		}
	}
}

