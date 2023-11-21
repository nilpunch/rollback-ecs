using System;
using System.Collections.Generic;
using System.Linq;

namespace ECS
{
	/// <summary>
	/// Container for all tables.
	/// </summary>
	public class TablesStorage
	{
		private readonly TypesStorage _typesStorage;
		private readonly ComponentsStorage _componentsStorage;
		private readonly Dictionary<TableId, Table> _tables;

		public TablesStorage(TypesStorage typesStorage, ComponentsStorage componentsStorage)
		{
			_componentsStorage = componentsStorage;
			_typesStorage = typesStorage;
			_tables = new Dictionary<TableId, Table>();
		}

		public Table GetOrCreateTableFor(SortedSet<EcsId> type)
		{
			SortedSet<EcsId> components = new SortedSet<EcsId>(_typesStorage.GetOnlyComponentIds(type));
			if (components.Count == 0)
				return null;

			TableId tableId = EcsIdUtils.CalculateTableId(components);

			if (!_tables.TryGetValue(tableId, out var table))
			{
				var columns = components.Select((componentId, columnIndex) =>
				{
					// Link component type to created table
					_componentsStorage.GetOrCreateInfo(componentId).ColumnInTables.Add(tableId, columnIndex);

					// TODO: put container creation into factory
					return ResizableDataContainer.Create(16, _typesStorage.GetTypeInfo(componentId).SizeOfElement);
				}).ToArray();

				table = new Table(tableId, columns);
				_tables.Add(tableId, table);
			}

			return table;
		}

		public Table GetTable(TableId tableId)
		{
			return _tables[tableId];
		}
	}
}