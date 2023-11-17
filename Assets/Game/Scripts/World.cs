using System;
using System.Collections.Generic;
using System.Linq;

namespace ECS
{
	public class World
	{
		private readonly ArchetypesGraph _archetypesGraph;
		private readonly ArchetypesStorage _archetypesStorage;
		private readonly EcsIdGenerator _ecsIdGenerator;

		private readonly Dictionary<EcsId, EntityLocation> _entities;
		private readonly Dictionary<EcsId, ComponentArchetypes> _components;

		private readonly TypeId _defaultArchetype;

		public World()
		{
			_ecsIdGenerator = new EcsIdGenerator();
			_entities = new Dictionary<EcsId, EntityLocation>();
			_components = new Dictionary<EcsId, ComponentArchetypes>();
			_archetypesStorage = new ArchetypesStorage(new TablesStorage());
			_archetypesGraph = new ArchetypesGraph(_archetypesStorage);
			
			// Create default archetype
			_defaultArchetype = _archetypesStorage.GetOrCreateArchetypeFor(new SortedSet<EcsId>(), new SortedSet<EcsId>()).Id;
		}
		
		public EcsId CreateEntity()
		{
			Archetype archetype = _archetypesStorage.GetArchetypeFor(_defaultArchetype);
			EcsId entityId = _ecsIdGenerator.ReserveId();
			_entities.Add(entityId, new EntityLocation(archetype));
			return entityId;
		}
		
		public void FreeEntity(EcsId entityId)
		{
			if (!_entities.TryGetValue(entityId, out var entityLocation))
			{
				return;
			}

			// Free the row, if table exists
			entityLocation.Archetype.Table?.Rows.FreeRow(entityLocation.RowInTable);
			
			_entities.Remove(entityId);
			_ecsIdGenerator.FreeEntityId(entityId);
		}
        
		public Component GetComponent(EcsId entityId, EcsId componentId)
		{
			EntityLocation entityLocation = _entities[entityId];
			Archetype entityArchetype = entityLocation.Archetype;
			ComponentArchetypes componentArchetypes = _components[componentId];

			if (componentArchetypes.LocationInArchetype.TryGetValue(entityArchetype.Id, out var componentLocation))
			{
				return entityArchetype.Table!.Columns[componentLocation.ColumnInTable].Rows[entityLocation.RowInTable];
			}

			throw new Exception();
		}
        
		public bool HasComponent(EcsId entityId, EcsId componentId)
		{
			EntityLocation entityLocation = _entities[entityId];
			return _components[componentId].LocationInArchetype.ContainsKey(entityLocation.Archetype.Id);
		}

		public void AddComponent(EcsId entityId, EcsId componentId, Component component)
		{
			EntityLocation entityLocation = _entities[entityId];
			
			Archetype currentArchetype = entityLocation.Archetype;
			Archetype targetArchetype = _archetypesGraph.ArchetypeAfterAddComponent(currentArchetype, componentId);

			int targetRow = -1;
			
			// TODO: Handle case, where targetTable == currentTable
			if (targetArchetype.Table != null)
			{
				targetRow = targetArchetype.Table.Rows.ReserveRow();
				
				if (currentArchetype.Table != null)
				{
					CopyComponentData(currentArchetype.Components,
						currentArchetype.Table, entityLocation.RowInTable,
						targetArchetype.Table, targetRow);
					
					currentArchetype.Table.Rows.FreeRow(entityLocation.RowInTable);
				}

				targetArchetype.Table
					.Columns[_components[componentId].LocationInArchetype[targetArchetype.Id].ColumnInTable]
					.Rows[targetRow] = component;
			}

			currentArchetype.Entities.Remove(entityId);
			targetArchetype.Entities.Add(entityId);

			_entities[entityId] = new EntityLocation(targetArchetype, targetRow);
		}

		private void CopyComponentData(SortedSet<EcsId> components, Table fromTable, int fromRow, Table toTable, int toRow)
		{
			// foreach (var componentToCopy in currentArchetype.Components)
			// {
			// 	ComponentArchetypes componentArchetypes = _components[componentId];
			// 	componentArchetypes.LocationInArchetype[currentArchetype.Id]
			// }
			// 		
			// currentArchetype.Table.Columns[componentArchetypes.LocationInArchetype[currentArchetype.Id].ColumnInTable]
		}
	}
}