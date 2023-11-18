using System.Collections.Generic;

namespace ECS
{
	public class ComponentInfo
	{
		public readonly Dictionary<TableId, int> ColumnInTables;
		public readonly int SizeOfElement;

		public ComponentInfo(int sizeOfElement)
		{
			SizeOfElement = sizeOfElement;
			ColumnInTables = new Dictionary<TableId, int>();
		}
	}
}