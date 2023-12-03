using System.Collections.Generic;

namespace ECS
{
	public static class TableUtils
	{
		public static void CopyRow(Table source, int sourceRow, Table destination, int destinationRow)
		{
			int sourceColumnIndex = 0;
			int destinationColumnIndex = 0;

			while (sourceColumnIndex < source.Columns.Length && destinationColumnIndex < destination.Columns.Length)
			{
				Column sourceColumn = source.Columns[sourceColumnIndex];
				Column destinationColumn = destination.Columns[destinationColumnIndex];

				EcsId sourceComponentId = sourceColumn.ComponentId;
				EcsId destinationComponentId = destinationColumn.ComponentId;
				
				if (sourceComponentId == destinationComponentId)
				{
					UnmanagedUtils.CopyElement(sourceColumn.Data, sourceRow, destinationColumn.Data, destinationRow);
					sourceColumnIndex += 1;
					destinationColumnIndex += 1;
				}
				else if (sourceComponentId.Id < destinationComponentId.Id)
				{
					sourceColumnIndex += 1;
				}
				else
				{
					destinationColumnIndex += 1;
				}
			}
		}
	}
}