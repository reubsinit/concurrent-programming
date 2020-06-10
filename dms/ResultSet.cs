using System;
using System.Collections.Generic;
using Reubs.Concurrent.Utils;

namespace dms
{
	public class ResultSet
	{
		public ResultSet ()
		{
			Latch = new Latch ();
		}

		private List<Row> _rows = new List<Row> ();

		public Latch Latch{ get; set; }
	
		public void AddRow(Row row)
		{
			_rows.Add (row);
		}

		public List<Row> Rows 
		{ 
			get 
			{
				Latch.Acquire ();
				return _rows;
			}
			set 
			{
				_rows = value;
			}
		}
	}
}

