using System;
using System.Collections.Generic;
using System.Linq;

namespace ECS
{
	public class World
	{
		private readonly ComponentsStorage _componentsStorage;
		private readonly TypesIdStorage _typesIdStorage;
		private readonly ArchetypesGraph _archetypesGraph;
		private readonly ArchetypesStorage _archetypesStorage;
		private readonly EcsIdGenerator _ecsIdGenerator;

		private readonly Dictionary<EcsId, EntityLocation> _entities;

		private readonly ArchetypeId _defaultArchetype;

		public World()
		{
			_ecsIdGenerator = new EcsIdGenerator();
			_typesIdStorage = new TypesIdStorage(_ecsIdGenerator);
			_componentsStorage = new ComponentsStorage(_typesIdStorage);
			
			_archetypesStorage = new ArchetypesStorage(new TablesStorage(_componentsStorage));
			_archetypesGraph = new ArchetypesGraph(_archetypesStorage);
			

			// Create default archetype
			_defaultArchetype = _archetypesStorage.GetOrCreateArchetypeFor(new SortedSet<EcsId>(), new SortedSet<EcsId>()).ArchetypeId;
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

			entityLocation.Archetype.Table?.Indices.FreeRow(entityLocation.RowInTable);
			
			_entities.Remove(entityId);
			_ecsIdGenerator.FreeEntityId(entityId);
		}
        
		public ref T GetComponent<T>(EcsId entityId) where T : unmanaged
		{
			EntityLocation entityLocation = _entities[entityId];
			Archetype entityArchetype = entityLocation.Archetype;
			int componentColumnInTable = _componentsStorage.GetColumnInTable<T>(entityArchetype.TableId);
			
			return ref entityArchetype.Table!
				.Columns[componentColumnInTable]
				.Rows.GetRef<T>(entityLocation.RowInTable);
		}
        
		public bool HasComponent<T>(EcsId entityId) where T : unmanaged
		{
			EntityLocation entityLocation = _entities[entityId];
			return _componentsStorage.HasColumnInTable<T>(entityLocation.Archetype.TableId);
		}

		public void AddComponent<T>(EcsId entityId, T component) where T : unmanaged
		{
			EntityLocation entityLocation = _entities[entityId];
			EcsId componentId = _typesIdStorage.GetId<T>();
			Archetype currentArchetype = entityLocation.Archetype;

			// If current archetype contains this component, then just copy component value
			if (currentArchetype.Components.Contains(componentId))
			{
				currentArchetype.Table!
					.Columns[_componentsStorage.GetColumnInTable<T>(currentArchetype.TableId)]
					.Rows.GetRef<T>(entityLocation.RowInTable) = component;
				return;
			}
			
			Archetype targetArchetype = _archetypesGraph.ArchetypeAfterAddComponent(currentArchetype, componentId);

			if (currentArchetype.Table == null)
			{
				var targetRow = targetArchetype.Table!.Indices.ReserveRow();

				targetArchetype.Table
					.Columns[_componentsStorage.GetColumnInTable<T>(targetArchetype.TableId)]
					.Rows.GetRef<T>(targetRow) = component;
			}
			else if (currentArchetype.Table == targetArchetype.Table)
			{
				
			}
			
			//
			// if (targetArchetype.Table != null)
			// {
			// 	
			// 	if (currentArchetype.Table != null)
			// 	{
			// 		CopyComponentData(currentArchetype.Components,
			// 			currentArchetype.Table, entityLocation.RowInTable,
			// 			targetArchetype.Table, targetRow);
			// 		
			// 		currentArchetype.Table.Indices.FreeRow(entityLocation.RowInTable);
			// 	}
			//
			// 	targetArchetype.Table
			// 		.Columns[_components[componentId].ColumnInTable[targetArchetype.TableId]]
			// 		.Rows.GetRef<T>(targetRow) = component;
			// }
			//
			// currentArchetype.Entities.Remove(entityId);
			// targetArchetype.Entities.Add(entityId);
			//
			// _entities[entityId] = new EntityLocation(targetArchetype, targetRow);
		}

		
		
		private void CopyComponentData(SortedSet<EcsId> components, Table source, int sourceRow, Table destination, int destinationRow)
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