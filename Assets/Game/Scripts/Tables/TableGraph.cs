using System.Collections.Generic;

namespace ECS
{
	/// <summary>
	/// Container to find linked tables.
	/// </summary>
	public class TableGraph
	{
		private readonly TablesStorage _tablesStorage;
		private readonly Dictionary<Table, Dictionary<EcsId, TablesLink>> _graph;

		public TableGraph(TablesStorage tablesStorage)
		{
			_tablesStorage = tablesStorage;
			_graph = new Dictionary<Table, Dictionary<EcsId, TablesLink>>();
		}

		public Table TableAfterAdd(Table table, EcsId id)
		{
			var link = GetOrCreateLink(table, id);

			if (link.OnAdd == null)
			{
				link.OnAdd = _tablesStorage.GetOrCreateArchetypeFor(table.Type.CloneAdd(id));
			}

			return link.OnAdd;
		}

		public Table ArchetypeAfterRemove(Table table, EcsId id)
		{
			var link = GetOrCreateLink(table, id);

			if (link.OnRemove == null)
			{
				link.OnRemove = _tablesStorage.GetOrCreateArchetypeFor(table.Type.CloneRemove(id));
			}

			return link.OnRemove;
		}

		private TablesLink GetOrCreateLink(Table table, EcsId component)
		{
			TablesLink link = null;
			bool hasTable = _graph.TryGetValue(table, out var connections);
			bool hasComponentLink = hasTable && connections.TryGetValue(component, out link);

			if (hasComponentLink)
			{
				return link;
			}

			if (!hasTable)
			{
				connections = new Dictionary<EcsId, TablesLink>();
				_graph.Add(table, connections);
			}

			link = new TablesLink();
			connections.Add(component, link);

			return link;
		}

		private class TablesLink
		{
			public Table OnAdd;
			public Table OnRemove;
		}
	}
}