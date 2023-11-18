using System.Collections.Generic;

namespace ECS
{
	public class ArchetypesStorage
	{
		private readonly TablesStorage _tablesStorage;
		private readonly Dictionary<ArchetypeId, Archetype> _archetypes;
		
		public ArchetypesStorage(TablesStorage tablesStorage)
		{
			_tablesStorage = tablesStorage;
			_archetypes = new Dictionary<ArchetypeId, Archetype>();
		}
		
		public Archetype GetOrCreateArchetypeFor(SortedSet<EcsId> components, SortedSet<EcsId> tags)
		{
			ArchetypeId archetypeId = EcsIdUtils.CalculateArchetypeId(components, tags);
			
			if (!_archetypes.TryGetValue(archetypeId, out var archetype))
			{
				Table table = components.Count == 0 ? null : _tablesStorage.GetOrCreateTableFor(components);
				archetype = new Archetype(components, tags, table);
				_archetypes.Add(archetypeId, archetype);
			}
			
			return archetype;
		}

		public Archetype GetArchetypeFor(ArchetypeId archetypeId)
		{
			return _archetypes[archetypeId];
		}
	}
}