namespace ECS
{
	public readonly struct EntityLocation
	{
		public readonly Archetype Archetype;
		public readonly int RowInTable;

		public EntityLocation(Archetype archetype, int rowInTable = -1)
		{
			Archetype = archetype;
			RowInTable = rowInTable;
		}
	}
}