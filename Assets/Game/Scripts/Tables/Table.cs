using System.Collections.Generic;

namespace ECS
{
	public class Table
	{
		public TableRows Rows = new TableRows();
		public List<Column> Columns = new List<Column>();
	}
}