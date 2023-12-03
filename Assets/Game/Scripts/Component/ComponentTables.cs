using System.Collections.Generic;

namespace ECS
{
	public class ComponentTables
	{
		public readonly Dictionary<ArchetypeId, int> ColumnInTables;

		public ComponentTables()
		{
			ColumnInTables = new Dictionary<ArchetypeId, int>();
		}
	}
}