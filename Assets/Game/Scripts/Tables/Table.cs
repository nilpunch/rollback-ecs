using System.Collections.Generic;

namespace ECS
{
	public class Table
	{
		public readonly TableIndices Indices = new TableIndices();
		public readonly Column[] Columns;

		public Table(Column[] columns)
		{
			Columns = columns;
		}
		
		
	}
}