using System.Collections.Generic;

namespace ECS
{
	public class ComponentInfo
	{
		public readonly Dictionary<TableId, int> ColumnInTable;
		public readonly int SizeOfElement;

		public ComponentInfo(int sizeOfElement)
		{
			SizeOfElement = sizeOfElement;
			ColumnInTable = new Dictionary<TableId, int>();
		}
	}
}