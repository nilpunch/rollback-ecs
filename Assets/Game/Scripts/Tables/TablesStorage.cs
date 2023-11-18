using System.Collections.Generic;
using System.Linq;

namespace ECS
{
	public class TablesStorage
	{
		private readonly ComponentsStorage _componentsStorage;
		private readonly Dictionary<TableId, Table> _tables;

		public TablesStorage(ComponentsStorage componentsStorage)
		{
			_componentsStorage = componentsStorage;
			_tables = new Dictionary<TableId, Table>();
		}
		
		public Table GetOrCreateTableFor(SortedSet<EcsId> components)
		{
			TableId tableId = EcsIdUtils.CalculateTableId(components);
			
			if (!_tables.TryGetValue(tableId, out var table))
			{
				Column[] columns = components.Select((componentId, columnIndex) =>
				{
					ComponentInfo componentInfo = _componentsStorage.GetOrCreateInfo(componentId);
					componentInfo.ColumnInTables.Add(tableId, columnIndex);
					
					// TODO: put container creation into factory
					return new Column(ResizableDataContainer.Create(16, componentInfo.SizeOfElement));
				}).ToArray();
			
				table = new Table(columns);
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