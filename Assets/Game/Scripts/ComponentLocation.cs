namespace ECS
{
	public readonly struct ComponentLocation
	{
		public readonly int ColumnInTable;

		public ComponentLocation(int columnInTable)
		{
			ColumnInTable = columnInTable;
		}
	}
}