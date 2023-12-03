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

		public bool TryGetColumnInTable(EcsId componentId, ArchetypeId archetypeId, out int column)
		{
			return GetOrCreateInfo(componentId).ColumnInTables.TryGetValue(archetypeId, out column);
		}
		
		public int GetColumnInTable(EcsId componentId, ArchetypeId archetypeId)
		{
			return GetOrCreateInfo(componentId).ColumnInTables[archetypeId];
		}

		public bool HasColumnInTable(EcsId componentId, ArchetypeId archetypeId)
		{
			return GetOrCreateInfo(componentId).ColumnInTables.ContainsKey(archetypeId);
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