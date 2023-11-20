using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ECS
{
	public class World
	{
		private readonly ComponentsStorage _componentsStorage;
		private readonly TypesStorage _typesStorage;
		private readonly ArchetypesGraph _archetypesGraph;
		private readonly EntitiesStorage _entitiesStorage;

		public World()
		{
			Archetype defaultArchetype = new Archetype(new SortedSet<EcsId>(), new SortedSet<EcsId>(), null);

			_entitiesStorage = new EntitiesStorage(defaultArchetype);
			_componentsStorage = new ComponentsStorage();
			_typesStorage = new TypesStorage(_entitiesStorage);

			_archetypesGraph = new ArchetypesGraph(
				new ArchetypesStorage(
					new TablesStorage(_typesStorage, _componentsStorage),
					defaultArchetype));
		}

		public void RegisterType<T>() where T : unmanaged
		{
			_typesStorage.EnsureRegistered<T>();
		}
		
		public EcsId CreateEntity()
		{
			return _entitiesStorage.Create();
		}
		
		public void DestroyEntity(EcsId entityId)
		{
			_entitiesStorage.Destroy(entityId);
		}
        
		public ref T GetComponent<T>(EcsId entityId) where T : unmanaged
		{
			var typeInfo = _typesStorage.EnsureRegistered<T>();

			if (!typeInfo.HasFields)
			{
				throw new InvalidOperationException("Specified type has no associated data.");
			}
			
			EntityInfo entityInfo = _entitiesStorage[entityId];
			Archetype entityArchetype = entityInfo.Archetype;
			int componentColumnInTable = _componentsStorage.GetColumnInTable(typeInfo.Id, entityArchetype.TableId);
			
			return ref entityArchetype.Table!
				.Columns[componentColumnInTable]
				.GetRef<T>(entityInfo.RowInTable);
		}
        
		public bool HasComponent<T>(EcsId entityId) where T : unmanaged
		{
			var typeInfo = _typesStorage.EnsureRegistered<T>();
			
			if (!typeInfo.HasFields)
			{
				return false;
			}
			
			EntityInfo entityInfo = _entitiesStorage[entityId];
			return _componentsStorage.HasColumnInTable(typeInfo.Id, entityInfo.Archetype.TableId);
		}
		
		public bool Has<T>(EcsId entityId) where T : unmanaged
		{
			EcsTypeInfo typeInfo = _typesStorage.EnsureRegistered<T>();
			EntityInfo entityInfo = _entitiesStorage[entityId];
			return _componentsStorage.HasColumnInTable(typeInfo.Id, entityInfo.Archetype.TableId);
		}

		public void Remove<T>(EcsId entityId) where T : unmanaged
		{
			EcsTypeInfo typeInfo = _typesStorage.EnsureRegistered<T>();
			
			if (typeInfo.HasFields)
			{
				RemoveComponent(entityId, typeInfo);
			}
			else
			{
				RemoveThing(entityId, typeInfo.Id);
			}
		}

		public void Set<T>(EcsId entityId, T value = default) where T : unmanaged
		{
			EcsTypeInfo typeInfo = _typesStorage.EnsureRegistered<T>();
			
			if (typeInfo.HasFields)
			{
				AddComponent(entityId, typeInfo, value);
			}
			else
			{
				AddThing(entityId, typeInfo.Id);
			}
		}
		
		public void Add<T>(EcsId entityId) where T : unmanaged
		{
			EcsTypeInfo typeInfo = _typesStorage.EnsureRegistered<T>();
			
			if (typeInfo.HasFields)
			{
				AddComponent<T>(entityId, typeInfo, default);
			}
			else
			{
				AddThing(entityId, typeInfo.Id);
			}
		}

		private void AddComponent<T>(EcsId entityId, EcsTypeInfo typeInfo, T value) where T : unmanaged
		{
			EcsId componentId = typeInfo.Id;
			
			EntityInfo currentEntityInfo = _entitiesStorage[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;
			int currentEntityRow = currentEntityInfo.RowInTable;
			
			// Check if current archetype already contains this component,
			// then just update existed component value
			if (currentArchetype.Components.Contains(componentId))
			{
				int currentComponentColumn = _componentsStorage.GetColumnInTable(componentId, currentArchetype.TableId);
				currentArchetype.Table!
					.Columns[currentComponentColumn]
					.GetRef<T>(currentEntityRow) = value;
				return;
			}
			
			Archetype destinationArchetype = _archetypesGraph.ArchetypeAfterAddComponent(currentArchetype, componentId);
			
			Assert.IsNotNull(destinationArchetype.Table, "This can not happened. After adding component to archetype, there MUST be table.");

			// Reserve row in destination archetype
			var destinationEntityRow = destinationArchetype.Table.ReserveRow();
			var destinationComponentColumn = _componentsStorage.GetColumnInTable(componentId, destinationArchetype.TableId);

			if (currentArchetype.Table != null)
			{
				// Copy current components from current archetype to destination archetype
				TableUtils.CopyRow(currentArchetype.Table, currentEntityRow,
					destinationArchetype.Table, destinationEntityRow,
					currentArchetype.Components, _componentsStorage);
				
				// Free reserved row in current archetype
				currentArchetype.Table.FreeRow(currentEntityRow);
			}
			
			// Copy added component value to destination archetype
			destinationArchetype.Table
				.Columns[destinationComponentColumn]
				.GetRef<T>(destinationEntityRow) = value;

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);
			
			_entitiesStorage[entityId] = new EntityInfo(destinationArchetype, destinationEntityRow);
		}
		
		private void RemoveComponent(EcsId entityId, EcsTypeInfo typeInfo)
		{
			EcsId componentId = typeInfo.Id;
			
			EntityInfo currentEntityInfo = _entitiesStorage[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;
			int currentEntityRow = currentEntityInfo.RowInTable;
			
			// Check if current archetype already NOT contains this component,
			// then just do nothing
			if (currentArchetype.Components.Contains(componentId))
			{
				return;
			}
			
			Assert.IsNotNull(currentArchetype.Table, "This can not happened. If there is component, there MUST be table.");
			
			Archetype destinationArchetype = _archetypesGraph.ArchetypeAfterRemoveComponent(currentArchetype, componentId);

			// Free reserved row in current archetype
			currentArchetype.Table.FreeRow(currentEntityRow);

			var destinationEntityRow = -1;
				
			if (destinationArchetype.Table != null)
			{
				// Reserve row in destination archetype
				destinationEntityRow = destinationArchetype.Table.ReserveRow();
				
				// Copy destination components from current archetype to destination archetype
				TableUtils.CopyRow(currentArchetype.Table, currentEntityRow,
					destinationArchetype.Table, destinationEntityRow,
					destinationArchetype.Components, _componentsStorage);
			}
			
			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);
			
			_entitiesStorage[entityId] = new EntityInfo(destinationArchetype, destinationEntityRow);
		}

		private void AddThing(EcsId entityId, EcsId thingId)
		{
			EntityInfo currentEntityInfo = _entitiesStorage[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;

			// Check if current archetype already contains this thing
			if (currentArchetype.Things.Contains(thingId))
			{
				return;
			}
			
			Archetype destinationArchetype = _archetypesGraph.ArchetypeAfterAddTag(currentArchetype, thingId);

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);
			
			_entitiesStorage[entityId] = new EntityInfo(destinationArchetype, currentEntityInfo.RowInTable);
		}
		
		private void RemoveThing(EcsId entityId, EcsId thingId)
		{
			EntityInfo currentEntityInfo = _entitiesStorage[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;
			
			// Check if current archetype already NOT contains this thing
			if (!currentArchetype.Things.Contains(thingId))
			{
				return;
			}
			
			Archetype destinationArchetype = _archetypesGraph.ArchetypeAfterRemoveThing(currentArchetype, thingId);

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);
			
			_entitiesStorage[entityId] = new EntityInfo(destinationArchetype, currentEntityInfo.RowInTable);
		}
	}
}