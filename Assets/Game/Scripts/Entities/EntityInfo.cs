namespace ECS
{
	public readonly struct EntityInfo
	{
		public readonly Archetype Archetype;
		public readonly int RowInTable;

		public EntityInfo(Archetype archetype, int rowInTable = -1)
		{
			Archetype = archetype;
			RowInTable = rowInTable;
		}
	}
}