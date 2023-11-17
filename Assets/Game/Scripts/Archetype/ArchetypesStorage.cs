using System.Collections.Generic;

namespace ECS
{
	public class ArchetypesStorage
	{
		private readonly TablesStorage _tablesStorage;
		private readonly Dictionary<TypeId, Archetype> _archetypes;
		
		public ArchetypesStorage(TablesStorage tablesStorage)
		{
			_tablesStorage = tablesStorage;
			_archetypes = new Dictionary<TypeId, Archetype>();
		}
		
		public Archetype GetOrCreateArchetypeFor(SortedSet<EcsId> components, SortedSet<EcsId> tags)
		{
			TypeId archetype = EcsIdUtils.CalculateArchetype(components, tags);
			
			if (!_archetypes.TryGetValue(archetype, out var tableData))
			{
				tableData = new Archetype(components, tags, _tablesStorage.GetOrCreateTableFor(components));
				_archetypes.Add(archetype, tableData);
			}
			
			return tableData;
		}

		public Archetype GetArchetypeFor(TypeId archetypeId)
		{
			return _archetypes[archetypeId];
		}
	}
}