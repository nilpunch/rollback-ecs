namespace ECS
{
	public static class TableUtils
	{
		public static void CopyRow(Table source, int sourceRow, Table destination, int destinationRow)
		{
			for (var i = 0; i < source.Columns.Length; i++)
			{
				var sourceColumn = source.Columns[i];
				var destinationColumn = destination.Columns[i];
				DataContainerUtils.CopyElement(sourceColumn.Rows, sourceRow, destinationColumn.Rows, destinationRow);
			}
		}
	}
}