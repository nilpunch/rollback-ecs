using System.Collections.Generic;

namespace ECS
{
	public class TableIndices
	{
		private readonly Stack<int> _freeRows = new Stack<int>();
		private int _nextFreeRow;

		public int ReserveRow()
		{
			if (_freeRows.Count != 0)
			{
				return _freeRows.Pop();
			}

			int freeId = _nextFreeRow;
			_nextFreeRow += 1;
			return freeId;
		}

		public void FreeRow(int rowIndex)
		{
			_freeRows.Push(rowIndex);
		}
	}
}