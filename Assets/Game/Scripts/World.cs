using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ECS
{
	public delegate void ActionRef<T>(ref T value);
	
	public class World
	{
		public World()
		{
			EntitiesStorage = new EntitiesStorage();
			ComponentsStorage = new ComponentsStorage();
			TypesStorage = new TypesStorage();

			TablesStorage = new TablesStorage(TypesStorage, ComponentsStorage);
			TableGraph = new TableGraph(TablesStorage);

			DefaultTable = TablesStorage.GetOrCreateArchetypeFor(type: new SortedSet<EcsId>());

			EcsComponentId = EntitiesStorage.CreateEntityInTable(DefaultTable);
			TypesStorage.RegisterTypeWithId<EcsComponent>(EcsComponentId);
			ComponentsTable = TableGraph.TableAfterAdd(DefaultTable, EcsComponentId);
		}

		public ComponentsStorage ComponentsStorage { get; }
		public TypesStorage TypesStorage { get; }
		public TablesStorage TablesStorage { get; }
		public TableGraph TableGraph { get; }
		public EntitiesStorage EntitiesStorage { get; }

		public Table DefaultTable { get; }
		public Table ComponentsTable { get; }
		public EcsId EcsComponentId { get; }

		public EcsId CreateEntity()
		{
			return EntitiesStorage.CreateEntityInTable(DefaultTable);
		}

		public void DestroyEntity(EcsId entityId)
		{
			EntitiesStorage.DestroyEntity(entityId);
		}

		public EcsTypeInfo EnsureTypeRegistered<T>() where T : unmanaged
		{
			if (!TypesStorage.TryGetTypeInfo<T>(out var typeInfo))
			{
				EcsId componentId = EntitiesStorage.CreateEntityInTable(ComponentsTable);
				typeInfo = TypesStorage.RegisterTypeWithId<T>(componentId);
			}

			return typeInfo;
		}

		public ref T GetComponent<T>(EcsId entityId) where T : unmanaged
		{
			var typeInfo = EnsureTypeRegistered<T>();

			if (!typeInfo.HasFields)
			{
				throw new InvalidOperationException("Specified type is not component.");
			}

			EntityInfo entityInfo = EntitiesStorage[entityId];
			Table entityTable = entityInfo.Table;

			if (!ComponentsStorage.TryGetColumnInTable(typeInfo.Id, entityTable.ArchetypeId, out var componentColumnInTable))
			{
				throw new Exception("Entity doesn't have this component.");
			}

			return ref entityTable
				.Columns[componentColumnInTable].Data
				.GetElementRef<T>(entityInfo.RowInTable);
		}

		public bool Has<T>(EcsId entityId) where T : unmanaged
		{
			EcsTypeInfo typeInfo = EnsureTypeRegistered<T>();
			EntityInfo entityInfo = EntitiesStorage[entityId];
			return ComponentsStorage.HasColumnInTable(typeInfo.Id, entityInfo.Table.ArchetypeId);
		}

		public void Remove<T>(EcsId entityId) where T : unmanaged
		{
			EcsTypeInfo typeInfo = EnsureTypeRegistered<T>();

			RemoveComponent(entityId, typeInfo);
		}

		public void Add<T>(EcsId entityId, T value = default) where T : unmanaged
		{
			EcsTypeInfo typeInfo = EnsureTypeRegistered<T>();

			AddComponent(entityId, typeInfo, value);
		}

		private void AddComponent<T>(EcsId entity, EcsTypeInfo typeInfo, T value) where T : unmanaged
		{
			EcsId componentId = typeInfo.Id;

			EntityInfo currentEntityInfo = EntitiesStorage[entity];
			Table currentTable = currentEntityInfo.Table;
			int currentEntityRow = currentEntityInfo.RowInTable;

			// Check if current archetype already contains this component,
			// then just update existed component value
			if (currentTable.Type.Contains(componentId))
			{
				int currentComponentColumn = ComponentsStorage.GetColumnInTable(componentId, currentTable.ArchetypeId);
				currentTable
					.Columns[currentComponentColumn].Data
					.GetElementRef<T>(currentEntityRow) = value;
				return;
			}

			Table destinationTable = TableGraph.TableAfterAdd(currentTable, componentId);
			EntityInfo updatedEntityInfo = EntitiesStorage.MoveEntity(entity, destinationTable);
			
			// Copy added component value to destination archetype
			if (typeInfo.HasFields)
			{
				int destinationComponentColumn = ComponentsStorage.GetColumnInTable(componentId, destinationTable.ArchetypeId);
				destinationTable
					.Columns[destinationComponentColumn].Data
					.GetElementRef<T>(updatedEntityInfo.RowInTable) = value;
			}
		}

		private void RemoveComponent(EcsId entity, EcsTypeInfo typeInfo)
		{
			EcsId componentId = typeInfo.Id;

			EntityInfo currentEntityInfo = EntitiesStorage[entity];
			Table currentTable = currentEntityInfo.Table;

			// Check if current archetype already NOT contains this component,
			// then just do nothing
			if (!currentTable.Type.Contains(componentId))
			{
				return;
			}

			Table destinationTable = TableGraph.ArchetypeAfterRemove(currentTable, componentId);
			EntitiesStorage.MoveEntity(entity, destinationTable);
		}
	}
}