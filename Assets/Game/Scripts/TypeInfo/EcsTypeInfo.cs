namespace ECS
{
	public struct EcsTypeInfo
	{
		public readonly EcsId Id;
		public readonly int SizeOfElement;

		public EcsTypeInfo(EcsId id, int sizeOfElement)
		{
			Id = id;
			SizeOfElement = sizeOfElement;
		}
	}
}