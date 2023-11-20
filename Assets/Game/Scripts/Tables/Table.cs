using System;

namespace ECS
{
	public class Table
	{
		private readonly TableIndices _indices;
		private readonly IDataContainer _oneOfColumns;

		public readonly TableId TableId;
		public readonly IDataContainer[] Columns;

		public Table(TableId tableId, IDataContainer[] columns)
		{
			Columns = columns;
			TableId = tableId;
			_indices = new TableIndices();
			
			// Assuming that each column is the same type of data container with same amount of elements
			_oneOfColumns = Columns[0];
		}

		public int ReserveRow()
		{
			int reservedRow = _indices.ReserveRow();

			if (reservedRow < _oneOfColumns.Capacity)
			{
				return reservedRow;
			}

			if (!_oneOfColumns.IsResizeable)
			{
				throw new InvalidOperationException("Table reach it's max capacity.");
			}

			int newCapacity = _oneOfColumns.Capacity * 2;
			foreach (var column in Columns)
			{
				column.Resize(newCapacity);
			}

			return reservedRow;
		}

		public void FreeRow(int rowIndex)
		{
			_indices.FreeRow(rowIndex);
		}
	}
}