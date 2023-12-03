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
		private readonly Dictionary<ArchetypeId, Table> _archetypes;

		public TablesStorage(TypesStorage typesStorage, ComponentsStorage componentsStorage)
		{
			_typesStorage = typesStorage;
			_componentsStorage = componentsStorage;
			_archetypes = new Dictionary<ArchetypeId, Table>();
		}

		public Table GetOrCreateArchetypeFor(SortedSet<EcsId> type)
		{
			ArchetypeId archetypeId = EcsIdUtils.CalculateArchetype(type);

			if (!_archetypes.TryGetValue(archetypeId, out var archetype))
			{
				var columns = _typesStorage.GetOnlyComponentIds(type).Select((componentId, columnIndex) =>
				{
					// Link component type to created archetype
					_componentsStorage.GetOrCreateInfo(componentId).ColumnInTables.Add(archetypeId, columnIndex);

					// TODO: put container creation into factory
					return new Column(UnmanagedArray.Create(16, _typesStorage.GetTypeInfo(componentId).SizeOfElement), componentId);
				}).ToArray();

				archetype = new Table(type, columns);
				_archetypes.Add(archetypeId, archetype);
			}

			return archetype;
		}

		public Table GetTableFor(ArchetypeId archetypeId)
		{
			return _archetypes[archetypeId];
		}
	}
}