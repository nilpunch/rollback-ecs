namespace ECS
{
	public readonly struct EntityInfo
	{
		public readonly Table Table;
		public readonly int RowInTable;

		public EntityInfo(Table table, int rowInTable)
		{
			Table = table;
			RowInTable = rowInTable;
		}
	}
}