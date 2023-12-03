using System;
using System.Collections.Generic;

namespace ECS
{
	public class Table
	{
		public readonly ArchetypeId ArchetypeId;
		public readonly SortedSet<EcsId> Type;
		public readonly List<EcsId> Entities;
		public readonly Column[] Columns;

		public int LastRowIndex => Entities.Count - 1;
		
		public Table(SortedSet<EcsId> type, Column[] columns)
		{
			Entities = new List<EcsId>();
			Columns = columns;
			ArchetypeId = EcsIdUtils.CalculateArchetype(type);
			Type = type;
		}

		public void AppendEntity(EcsId entityIndex)
		{
			Entities.Add(entityIndex);

			EnsureCapacity(Entities.Count);
		}

		public void SwapRemoveRow(int rowIndex)
		{
			foreach (var column in Columns)
			{
				column.Data.Swap(Entities.Count - 1, rowIndex);
			}
			
			Entities.RemoveBySwap(rowIndex);
		}

		private void EnsureCapacity(int capacity)
		{
			if (Columns.Length == 0 || Columns[0].Data.Capacity >= capacity)
			{
				return;
			}
			
			if (!Columns[0].Data.IsResizeable)
			{
				throw new Exception("Entities limit reached.");
			}
			
			foreach (var column in Columns)
			{
				column.Data.Resize(capacity * 2);
			}
		}
	}
}