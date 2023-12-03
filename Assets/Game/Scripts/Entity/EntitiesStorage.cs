using System;
using System.Collections.Generic;

namespace ECS
{
	/// <summary>
	/// Container for all entities (EcsId) and their location (Table, RowInTable).
	/// </summary>
	public class EntitiesStorage
	{
		private readonly EcsIdGenerator _ecsIdGenerator;
		private readonly Dictionary<EcsId, EntityInfo> _entities;

		public EntitiesStorage()
		{
			_ecsIdGenerator = new EcsIdGenerator();
			_entities = new Dictionary<EcsId, EntityInfo>();
		}

		public EntityInfo this[EcsId id]
		{
			get => _entities[id];
			set => _entities[id] = value;
		}

		public EcsId CreateEntityInTable(Table table)
		{
			EcsId entity = _ecsIdGenerator.NewId();
			
			AddEntityToTable(entity, table);
			
			return entity;
		}

		public void DestroyEntity(EcsId entity)
		{
			if (!_entities.TryGetValue(entity, out var entityInfo))
			{
				return;
			}
			
			RemoveRowFromTable(entityInfo.Table, entityInfo.RowInTable);

			_entities.Remove(entity);
			_ecsIdGenerator.RecycleEntityId(entity);
		}

		public EntityInfo MoveEntity(EcsId entity, Table destination)
		{
			if (!_entities.TryGetValue(entity, out var entityInfo))
			{
				throw new ArgumentOutOfRangeException(nameof(entity));
			}

			if (entityInfo.Table == destination)
			{
				return entityInfo;
			}
			
			AddEntityToTable(entity, destination);
			
			TableUtils.CopyRow(entityInfo.Table, entityInfo.RowInTable, destination, destination.LastRowIndex);
			
			RemoveRowFromTable(entityInfo.Table, entityInfo.RowInTable);

			return new EntityInfo(destination, destination.LastRowIndex);
		}

		private void RemoveRowFromTable(Table table, int rowIndex)
		{
			table.SwapRemoveRow(rowIndex);
			
			if (rowIndex <= table.LastRowIndex)
			{
				EcsId swappedEntity = table.Entities[rowIndex];
				_entities[swappedEntity] = new EntityInfo(table, rowIndex);
			}
		}

		private void AddEntityToTable(EcsId entity, Table table)
		{
			table.AppendEntity(entity);
			_entities[entity] = new EntityInfo(table, table.LastRowIndex);
		}
	}
}