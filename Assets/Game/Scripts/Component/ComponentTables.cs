using System.Collections.Generic;

namespace ECS
{
	public class ComponentTables
	{
		public readonly Dictionary<TableId, int> ColumnInTables;

		public ComponentTables()
		{
			ColumnInTables = new Dictionary<TableId, int>();
		}
	}
}