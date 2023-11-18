namespace ECS
{
	public struct EcsTypeInfo
	{
		public readonly bool HasFields;
		public readonly EcsId Id;
		public readonly int SizeOfElement;

		public EcsTypeInfo(EcsId id, int sizeOfElement, bool hasFields)
		{
			Id = id;
			SizeOfElement = sizeOfElement;
			HasFields = hasFields;
		}
	}
}