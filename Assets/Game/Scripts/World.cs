using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ECS
{
	public class World
	{
		private readonly ComponentsStorage _componentsStorage;
		private readonly TypesIdStorage _typesIdStorage;
		private readonly ArchetypesGraph _archetypesGraph;
		private readonly ArchetypesStorage _archetypesStorage;
		private readonly EcsIdGenerator _ecsIdGenerator;

		private readonly Dictionary<EcsId, EntityInfo> _entities;

		private readonly ArchetypeId _defaultArchetype;

		public World()
		{
			_ecsIdGenerator = new EcsIdGenerator();
			_typesIdStorage = new TypesIdStorage(_ecsIdGenerator);
			_componentsStorage = new ComponentsStorage(_typesIdStorage);
			
			_archetypesStorage = new ArchetypesStorage(new TablesStorage(_componentsStorage));
			_archetypesGraph = new ArchetypesGraph(_archetypesStorage);
			
			_defaultArchetype = _archetypesStorage.GetOrCreateArchetypeFor(new SortedSet<EcsId>(), new SortedSet<EcsId>()).ArchetypeId;
		}

		public void RegisterType<T>() where T : unmanaged
		{
			_typesIdStorage.EnsureRegistered<T>();
		}
		
		public EcsId CreateEntity()
		{
			Archetype archetype = _archetypesStorage.GetArchetypeFor(_defaultArchetype);
			EcsId entityId = _ecsIdGenerator.ReserveId();
			_entities.Add(entityId, new EntityInfo(archetype));
			return entityId;
		}
		
		public void FreeEntity(EcsId entityId)
		{
			if (!_entities.TryGetValue(entityId, out var entityLocation))
			{
				return;
			}

			entityLocation.Archetype.Table?.FreeRow(entityLocation.RowInTable);
			
			_entities.Remove(entityId);
			_ecsIdGenerator.FreeEntityId(entityId);
		}
        
		public ref T GetComponent<T>(EcsId entityId) where T : unmanaged
		{
			var typeInfo = _typesIdStorage.EnsureRegistered<T>();

			if (!typeInfo.HasFields)
			{
				throw new InvalidOperationException("Specified type has no associated data.");
			}
			
			EntityInfo entityInfo = _entities[entityId];
			Archetype entityArchetype = entityInfo.Archetype;
			int componentColumnInTable = _componentsStorage.GetColumnInTable(typeInfo.Id, entityArchetype.TableId);
			
			return ref entityArchetype.Table!
				.Columns[componentColumnInTable]
				.Rows.GetRef<T>(entityInfo.RowInTable);
		}
        
		public bool HasComponent<T>(EcsId entityId) where T : unmanaged
		{
			var typeInfo = _typesIdStorage.EnsureRegistered<T>();
			
			if (!typeInfo.HasFields)
			{
				return false;
			}
			
			EntityInfo entityInfo = _entities[entityId];
			return _componentsStorage.HasColumnInTable<T>(entityInfo.Archetype.TableId);
		}
		
		public bool Has<T>(EcsId entityId) where T : unmanaged
		{
			_typesIdStorage.EnsureRegistered<T>();
			
			EntityInfo entityInfo = _entities[entityId];
			return _componentsStorage.HasColumnInTable<T>(entityInfo.Archetype.TableId);
		}

		public void Remove<T>(EcsId entityId) where T : unmanaged
		{
			EcsTypeInfo typeInfo = _typesIdStorage.EnsureRegistered<T>();
			
			if (typeInfo.HasFields)
			{
				RemoveComponent<T>(entityId, typeInfo);
			}
			else
			{
				RemoveThing(entityId, typeInfo.Id);
			}
		}

		public void Set<T>(EcsId entityId, T value = default) where T : unmanaged
		{
			EcsTypeInfo typeInfo = _typesIdStorage.EnsureRegistered<T>();
			
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
			EcsTypeInfo typeInfo = _typesIdStorage.EnsureRegistered<T>();
			
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
			
			EntityInfo currentEntityInfo = _entities[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;
			int currentEntityRow = currentEntityInfo.RowInTable;
			
			// Check if current archetype already contains this component,
			// then just update existed component value
			if (currentArchetype.Components.Contains(componentId))
			{
				int currentComponentColumn = _componentsStorage.GetColumnInTable<T>(currentArchetype.TableId);
				currentArchetype.Table!
					.Columns[currentComponentColumn]
					.Rows.GetRef<T>(currentEntityRow) = value;
				return;
			}
			
			Archetype destinationArchetype = _archetypesGraph.ArchetypeAfterAddComponent(currentArchetype, componentId);
			
			Assert.IsNotNull(destinationArchetype.Table, "This can not happened. After adding component to archetype, there MUST be table.");

			// Reserve row in destination archetype
			var destinationEntityRow = destinationArchetype.Table.ReserveRow();
			var destinationComponentColumn = _componentsStorage.GetColumnInTable<T>(destinationArchetype.TableId);

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
				.Rows.GetRef<T>(destinationEntityRow) = value;

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);
			
			_entities[entityId] = new EntityInfo(destinationArchetype, destinationEntityRow);
		}
		
		private void RemoveComponent<T>(EcsId entityId, EcsTypeInfo typeInfo) where T : unmanaged
		{
			EcsId componentId = typeInfo.Id;
			
			EntityInfo currentEntityInfo = _entities[entityId];
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
			
			_entities[entityId] = new EntityInfo(destinationArchetype, destinationEntityRow);
		}

		private void AddThing(EcsId entityId, EcsId thingId)
		{
			EntityInfo currentEntityInfo = _entities[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;

			// Check if current archetype already contains this thing
			if (currentArchetype.Things.Contains(thingId))
			{
				return;
			}
			
			Archetype destinationArchetype = _archetypesGraph.ArchetypeAfterAddTag(currentArchetype, thingId);

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);
			
			_entities[entityId] = new EntityInfo(destinationArchetype, currentEntityInfo.RowInTable);
		}
		
		private void RemoveThing(EcsId entityId, EcsId thingId)
		{
			EntityInfo currentEntityInfo = _entities[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;
			
			// Check if current archetype already NOT contains this thing
			if (!currentArchetype.Things.Contains(thingId))
			{
				return;
			}
			
			Archetype destinationArchetype = _archetypesGraph.ArchetypeAfterRemoveThing(currentArchetype, thingId);

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);
			
			_entities[entityId] = new EntityInfo(destinationArchetype, currentEntityInfo.RowInTable);
		}
	}
}