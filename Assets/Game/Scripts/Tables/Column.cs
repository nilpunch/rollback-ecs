namespace ECS
{
	public class Column
	{
		public readonly IDataContainer Rows;

		public Column(IDataContainer rowsContainer)
		{
			Rows = rowsContainer;
		}
	}
}