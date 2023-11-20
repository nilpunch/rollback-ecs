using System.Collections.Generic;

namespace ECS
{
	/// <summary>
	/// Container for all components and their matched tables.
	/// </summary>
	public class ComponentsStorage
	{
		private readonly Dictionary<EcsId, ComponentTables> _componentInfos;

		public ComponentsStorage()
		{
			_componentInfos = new Dictionary<EcsId, ComponentTables>();
		}

		public int GetColumnInTable(EcsId componentId, TableId tableId)
		{
			return GetOrCreateInfo(componentId).ColumnInTables[tableId];
		}
		
		public bool HasColumnInTable(EcsId componentId, TableId tableId)
		{
			return GetOrCreateInfo(componentId).ColumnInTables.ContainsKey(tableId);
		}
		
		public ComponentTables GetOrCreateInfo(EcsId componentId)
		{
			if (!_componentInfos.TryGetValue(componentId, out var info))
			{
				info = new ComponentTables();
				_componentInfos.Add(componentId, info);
			}

			return info;
		}
	}
}