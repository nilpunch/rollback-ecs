using System;

namespace ECS
{
	public class Table
	{
		private readonly TableIndices _indices;
		private readonly IDataContainer _dataContainer;
		
		public readonly Column[] Columns;

		public Table(Column[] columns)
		{
			Columns = columns;
			_indices = new TableIndices();
			
			// Assuming that each column is the same type of data container with same amount of elements
			_dataContainer = Columns[0].Rows;
		}

		public int ReserveRow()
		{
			int reservedRow = _indices.ReserveRow();

			if (reservedRow < _dataContainer.Capacity)
			{
				return reservedRow;
			}

			if (!_dataContainer.IsResizeable)
			{
				throw new InvalidOperationException("Table reached it's max capacity.");
			}

			int newCapacity = _dataContainer.Capacity * 2;
			foreach (var column in Columns)
			{
				column.Rows.Resize(newCapacity);
			}

			return reservedRow;
		}

		public void FreeRow(int rowIndex)
		{
			_indices.FreeRow(rowIndex);
		}
	}
}