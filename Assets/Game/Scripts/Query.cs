using System;
using System.Collections.Generic;
using System.Linq;

namespace ECS
{
	public delegate void ActionRef<T>(ref T value);
	public delegate void ActionSpan<T>(Span<T> value);

	public class Query<T> where T : unmanaged
	{
		private readonly World _world;
		private readonly SortedSet<EcsId> _include;
		private readonly SortedSet<EcsId> _exclude;

		private readonly ComponentTables _componentTables; 
		
		private int _componentTablesCount;
		private readonly List<Table> _matchedTables;
		private readonly List<IUnmanagedArray> _matchedColumns;
		
		public Query(World world, SortedSet<EcsId> include, SortedSet<EcsId> exclude)
		{
			_world = world;
			_include = include;
			_exclude = exclude;
			_matchedColumns = new List<IUnmanagedArray>();
			_matchedTables = new List<Table>();
			
			var selectedComponentId = _world.EnsureTypeRegistered<T>().Id;
			_componentTables = _world.ComponentsStorage.GetOrCreateInfo(selectedComponentId);
		}

		public int EntitiesCount
		{
			get
			{
				if (_componentTables.ColumnInTables.Count != _componentTablesCount)
				{
					MatchTables();
				}
				
				return _matchedTables.Sum(table => table.Entities.Count);
			}
		}

		public void ForEach(ActionRef<T> entityAction)
		{
			if (_componentTables.ColumnInTables.Count != _componentTablesCount)
			{
				MatchTables();
			}

			foreach (var matchedColumn in _matchedColumns)
			{
				var span = matchedColumn.AsSpan<T>();
				for (int i = 0; i < span.Length; i++)
				{
					entityAction.Invoke(ref span[i]);
				}
			}
		}
		
		public void ForEachTable(ActionSpan<T> spanAction)
		{
			if (_componentTables.ColumnInTables.Count != _componentTablesCount)
			{
				MatchTables();
			}

			foreach (var matchedColumn in _matchedColumns)
			{
				spanAction.Invoke(matchedColumn.AsSpan<T>());
			}
		}

		private void MatchTables()
		{
			_matchedColumns.Clear();

			_componentTablesCount = _componentTables.ColumnInTables.Count;
			
			foreach (var (tableId, columnInTable) in _componentTables.ColumnInTables)
			{
				Table table = _world.TablesStorage.GetTableFor(tableId);

				if (_include.IsSubsetOf(table.Type) && !_exclude.Overlaps(table.Type))
				{
					_matchedColumns.Add(table.Columns[columnInTable].Data);
					_matchedTables.Add(table);
				}
			}
		}
	}
}