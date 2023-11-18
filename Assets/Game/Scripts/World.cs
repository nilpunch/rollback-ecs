using System;
using System.Collections.Generic;

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
			_typesIdStorage.Register<T>();
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
			EntityInfo entityInfo = _entities[entityId];
			Archetype entityArchetype = entityInfo.Archetype;
			int componentColumnInTable = _componentsStorage.GetColumnInTable<T>(entityArchetype.TableId);
			
			return ref entityArchetype.Table!
				.Columns[componentColumnInTable]
				.Rows.GetRef<T>(entityInfo.RowInTable);
		}
        
		public bool HasComponent<T>(EcsId entityId) where T : unmanaged
		{
			EntityInfo entityInfo = _entities[entityId];
			return _componentsStorage.HasColumnInTable<T>(entityInfo.Archetype.TableId);
		}

		public void AddComponent<T>(EcsId entityId, T component) where T : unmanaged
		{
			EntityInfo currentEntityInfo = _entities[entityId];
			EcsId componentId = _typesIdStorage.GetId<T>();
			Archetype currentArchetype = currentEntityInfo.Archetype;
			int currentEntityRow = currentEntityInfo.RowInTable;
			int currentComponentColumn = _componentsStorage.GetColumnInTable<T>(currentArchetype.TableId);

			// If current archetype already contains this component, then just update component value
			if (currentArchetype.Components.Contains(componentId))
			{
				currentArchetype.Table!
					.Columns[currentComponentColumn]
					.Rows.GetRef<T>(currentEntityRow) = component;
				return;
			}
			
			Archetype destinationArchetype = _archetypesGraph.ArchetypeAfterAddComponent(currentArchetype, componentId);
			if (destinationArchetype.Table == null)
				throw new Exception("This can not happened. After adding component to archetype, there MUST be table.");

			var destinationEntityRow = destinationArchetype.Table.ReserveRow();
			var destinationComponentColumn = _componentsStorage.GetColumnInTable<T>(destinationArchetype.TableId);

			if (currentArchetype.Table != null)
			{
				TableUtils.CopyRow(currentArchetype.Table, currentEntityRow,
					destinationArchetype.Table, destinationEntityRow);
				currentArchetype.Table.FreeRow(currentEntityRow);
			}
			
			destinationArchetype.Table
				.Columns[destinationComponentColumn]
				.Rows.GetRef<T>(destinationEntityRow) = component;

			currentArchetype.Entities.Remove(entityId);
			destinationArchetype.Entities.Add(entityId);
			
			_entities[entityId] = new EntityInfo(destinationArchetype, destinationEntityRow);
		}
	}
}