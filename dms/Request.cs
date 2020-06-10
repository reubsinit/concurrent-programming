using System;
using System.Collections.Generic;

namespace dms
{
	public class Request
	{
		public Request (List<int> rowsToGet, List<Row> rowsToUpdate, ResultSet resultSet)
		{
			RowsToGet = rowsToGet;
			RowsToUpdate = rowsToUpdate;
			ResultSet = resultSet;
		}

		public Request (List<int> rowsToGet, ResultSet resultSet)
		{
			RowsToGet = rowsToGet;
			ResultSet = resultSet;
		}

		public Request (List<Row> rowsToUpdate, ResultSet resultSet = null)
		{
			RowsToUpdate = rowsToUpdate;
			ResultSet = resultSet;
		}

		public Request (List<Row> rowsToUpdate)
		{
			RowsToUpdate = rowsToUpdate;
		}

		public List<int> RowsToGet { get; set; }
		public List<Row> RowsToUpdate { get; set; }
		public ResultSet ResultSet { get; set; }
	}
}

