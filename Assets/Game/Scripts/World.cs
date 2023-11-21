using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ECS
{
	public class World
	{
		public World()
		{
			EntitiesStorage = new EntitiesStorage();
			ComponentsStorage = new ComponentsStorage();
			TypesStorage = new TypesStorage();

			TablesStorage = new TablesStorage(TypesStorage, ComponentsStorage);
			ArchetypesStorage = new ArchetypesStorage(TablesStorage);
			ArchetypesGraph = new ArchetypesGraph(ArchetypesStorage);

			DefaultArchetype = ArchetypesStorage.GetOrCreateArchetypeFor(type: new SortedSet<EcsId>());

			EcsComponentId = EntitiesStorage.CreateEntityForArchetype(DefaultArchetype);
			TypesStorage.RegisterTypeWithId<EcsComponent>(EcsComponentId);
			ComponentsArchetype = ArchetypesGraph.ArchetypeAfterAddThing(DefaultArchetype, EcsComponentId);
		}

		public ComponentsStorage ComponentsStorage { get; }
		public TypesStorage TypesStorage { get; }
		public ArchetypesStorage ArchetypesStorage { get; }
		public TablesStorage TablesStorage { get; }
		public ArchetypesGraph ArchetypesGraph { get; }
		public EntitiesStorage EntitiesStorage { get; }

		public Archetype DefaultArchetype { get; }
		public Archetype ComponentsArchetype { get; }
		public EcsId EcsComponentId { get; }

		public EcsId CreateEntity()
		{
			return EntitiesStorage.CreateEntityForArchetype(DefaultArchetype);
		}

		public void DestroyEntity(EcsId entityId)
		{
			EntitiesStorage.DestroyEntity(entityId);
		}

		public EcsTypeInfo EnsureTypeRegistered<T>() where T : unmanaged
		{
			if (!TypesStorage.TryGetTypeInfo<T>(out var typeInfo))
			{
				EcsId componentId = EntitiesStorage.CreateEntityForArchetype(ComponentsArchetype);
				typeInfo = TypesStorage.RegisterTypeWithId<T>(componentId);
			}

			return typeInfo;
		}

		public ref T GetComponent<T>(EcsId entityId) where T : unmanaged
		{
			var typeInfo = EnsureTypeRegistered<T>();

			if (!typeInfo.HasFields)
			{
				throw new InvalidOperationException("Specified type has no associated data.");
			}

			EntityInfo entityInfo = EntitiesStorage[entityId];
			Archetype entityArchetype = entityInfo.Archetype;
			Table table = entityArchetype.Table ?? throw new Exception("Entity doesn't have this component.");

			int componentColumnInTable = ComponentsStorage.GetColumnInTable(typeInfo.Id, table.TableId);

			return ref table.Columns[componentColumnInTable].GetElementRef<T>(entityInfo.RowInTable);
		}

		public bool Has<T>(EcsId entityId) where T : unmanaged
		{
			EcsTypeInfo typeInfo = EnsureTypeRegistered<T>();
			EntityInfo entityInfo = EntitiesStorage[entityId];
			return entityInfo.Archetype.Type.Contains(typeInfo.Id);
		}

		public void Remove<T>(EcsId entityId) where T : unmanaged
		{
			EcsTypeInfo typeInfo = EnsureTypeRegistered<T>();

			if (typeInfo.HasFields)
			{
				RemoveComponent(entityId, typeInfo);
			}
			else
			{
				RemoveThing(entityId, typeInfo.Id);
			}
		}

		public void Add<T>(EcsId entityId, T value = default) where T : unmanaged
		{
			EcsTypeInfo typeInfo = EnsureTypeRegistered<T>();

			if (typeInfo.HasFields)
			{
				AddComponent(entityId, typeInfo, value);
			}
			else
			{
				AddThing(entityId, typeInfo.Id);
			}
		}

		private void AddComponent<T>(EcsId entityId, EcsTypeInfo typeInfo, T value) where T : unmanaged
		{
			EcsId componentId = typeInfo.Id;

			EntityInfo currentEntityInfo = EntitiesStorage[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;
			int currentEntityRow = currentEntityInfo.RowInTable;

			// Check if current archetype already contains this component,
			// then just update existed component value
			if (currentArchetype.Type.Contains(componentId))
			{
				Table table = currentArchetype.Table ?? throw new Exception("This can't happen. If there is component in type, there must be a table.");
				int currentComponentColumn = ComponentsStorage.GetColumnInTable(componentId, table.TableId);
				table.Columns[currentComponentColumn].GetElementRef<T>(currentEntityRow) = value;
				return;
			}

			Archetype destinationArchetype = ArchetypesGraph.ArchetypeAfterAddThing(currentArchetype, componentId);
			Table destinationTable = destinationArchetype.Table ??
			                         throw new Exception("This can not happened. After adding component to archetype, there MUST be table.");

			// Reserve row in destination archetype
			var destinationEntityRow = destinationArchetype.Table.ReserveRow();
			var destinationComponentColumn = ComponentsStorage.GetColumnInTable(componentId, destinationTable.TableId);

			if (currentArchetype.Table != null)
			{
				// Copy current components from current archetype to destination archetype
				TableUtils.CopyRow(currentArchetype.Table, currentEntityRow,
					destinationTable, destinationEntityRow,
					TypesStorage.GetOnlyComponentIds(currentArchetype.Type), ComponentsStorage);

				// Free reserved row in current archetype
				currentArchetype.Table.FreeRow(currentEntityRow);
			}

			// Copy added component value to destination archetype
			destinationArchetype.Table
				.Columns[destinationComponentColumn]
				.GetElementRef<T>(destinationEntityRow) = value;

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);

			EntitiesStorage[entityId] = new EntityInfo(destinationArchetype, destinationEntityRow);
		}

		private void RemoveComponent(EcsId entityId, EcsTypeInfo typeInfo)
		{
			EcsId componentId = typeInfo.Id;

			EntityInfo currentEntityInfo = EntitiesStorage[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;
			int currentEntityRow = currentEntityInfo.RowInTable;

			// Check if current archetype already NOT contains this component,
			// then just do nothing
			if (!currentArchetype.Type.Contains(componentId))
			{
				return;
			}

			Assert.IsNotNull(currentArchetype.Table, "This can not happened. If there is component, there MUST be table.");

			Archetype destinationArchetype = ArchetypesGraph.ArchetypeAfterRemoveThing(currentArchetype, componentId);

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
					TypesStorage.GetOnlyComponentIds(destinationArchetype.Type), ComponentsStorage);
			}

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);

			EntitiesStorage[entityId] = new EntityInfo(destinationArchetype, destinationEntityRow);
		}

		private void AddThing(EcsId entityId, EcsId thingId)
		{
			EntityInfo currentEntityInfo = EntitiesStorage[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;

			// Check if current archetype already contains this thing
			if (currentArchetype.Type.Contains(thingId))
			{
				return;
			}

			Archetype destinationArchetype = ArchetypesGraph.ArchetypeAfterAddThing(currentArchetype, thingId);

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);

			EntitiesStorage[entityId] = new EntityInfo(destinationArchetype, currentEntityInfo.RowInTable);
		}

		private void RemoveThing(EcsId entityId, EcsId thingId)
		{
			EntityInfo currentEntityInfo = EntitiesStorage[entityId];
			Archetype currentArchetype = currentEntityInfo.Archetype;

			// Check if current archetype already NOT contains this thing
			if (!currentArchetype.Type.Contains(thingId))
			{
				return;
			}

			Archetype destinationArchetype = ArchetypesGraph.ArchetypeAfterRemoveThing(currentArchetype, thingId);

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);

			EntitiesStorage[entityId] = new EntityInfo(destinationArchetype, currentEntityInfo.RowInTable);
		}
	}
}