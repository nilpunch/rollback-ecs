using System.Collections.Generic;
using System.Linq;

namespace ECS
{
	/// <summary>
	/// Container for all archetypes.
	/// </summary>
	public class TablesStorage
	{
		private readonly TypesStorage _typesStorage;
		private readonly ComponentsStorage _componentsStorage;
		private readonly Dictionary<TableId, Table> _tables;

		public TablesStorage(TypesStorage typesStorage, ComponentsStorage componentsStorage)
		{
			_typesStorage = typesStorage;
			_componentsStorage = componentsStorage;
			_tables = new Dictionary<TableId, Table>();
		}

		public Table GetOrCreateArchetypeFor(SortedSet<EcsId> type)
		{
			TableId tableId = EcsIdUtils.CombineTableId(type);

			if (!_tables.TryGetValue(tableId, out var archetype))
			{
				List<Column> columns = new List<Column>();
				foreach (var componentId in type)
				{
					if (_typesStorage.GetTypeInfo(componentId).HasFields)
					{
						_componentsStorage.GetOrCreateInfo(componentId).ColumnInTables.Add(tableId, columns.Count);
						columns.Add(new Column(UnmanagedArray.Create(16, _typesStorage.GetTypeInfo(componentId).SizeOfElement), componentId));
					}
					else
					{
						_componentsStorage.GetOrCreateInfo(componentId).ColumnInTables.Add(tableId, -1);
					}
				}

				archetype = new Table(type, columns.ToArray());
				_tables.Add(tableId, archetype);
			}

			return archetype;
		}

		public Table GetTableFor(TableId tableId)
		{
			return _tables[tableId];
		}
	}
}