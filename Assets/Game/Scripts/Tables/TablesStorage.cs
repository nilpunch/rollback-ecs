using System.Collections.Generic;

namespace ECS
{
	public class TablesStorage
	{
		private readonly Dictionary<TypeId, Table> _tables = new Dictionary<TypeId, Table>();
		
		public Table GetOrCreateTableFor(SortedSet<EcsId> components)
		{
			TypeId type = EcsIdUtils.CalculateType(components);
			return GetOrCreateTableFor(type);
		}
		
		public Table GetOrCreateTableFor(TypeId type)
		{
			if (!_tables.TryGetValue(type, out var tableData))
			{
				tableData = new Table();
				_tables.Add(type, tableData);
			}

			return tableData;
		}
	}
}