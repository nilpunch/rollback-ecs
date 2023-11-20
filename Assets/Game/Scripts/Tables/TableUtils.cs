using System.Collections.Generic;

namespace ECS
{
	public static class TableUtils
	{
		public static void CopyRow(Table source, int sourceRow, Table destination, int destinationRow,
			IEnumerable<EcsId> componentsToCopy, ComponentsStorage componentsStorage)
		{
			foreach (var componentId in componentsToCopy)
			{
				var sourceColumn = source.Columns[componentsStorage.GetColumnInTable(componentId, source.TableId)];
				var destinationColumn = destination.Columns[componentsStorage.GetColumnInTable(componentId, destination.TableId)];
				
				DataContainerUtils.CopyElement(sourceColumn, sourceRow, destinationColumn, destinationRow);
			}
		}
	}
}