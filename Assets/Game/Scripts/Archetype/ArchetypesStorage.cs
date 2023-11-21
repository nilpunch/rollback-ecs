using System.Collections.Generic;

namespace ECS
{
	/// <summary>
	/// Container for all archetypes.
	/// </summary>
	public class ArchetypesStorage
	{
		private readonly TablesStorage _tablesStorage;
		private readonly Dictionary<ArchetypeId, Archetype> _archetypes;

		public ArchetypesStorage(TablesStorage tablesStorage)
		{
			_tablesStorage = tablesStorage;
			_archetypes = new Dictionary<ArchetypeId, Archetype>();
		}

		public Archetype GetOrCreateArchetypeFor(SortedSet<EcsId> type)
		{
			ArchetypeId archetypeId = EcsIdUtils.CalculateArchetypeId(type);

			if (!_archetypes.TryGetValue(archetypeId, out var archetype))
			{
				Table table = _tablesStorage.GetOrCreateTableFor(type);
				archetype = new Archetype(type, table);
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