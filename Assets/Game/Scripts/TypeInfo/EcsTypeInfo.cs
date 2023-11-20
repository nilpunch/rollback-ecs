namespace ECS
{
	public struct EcsTypeInfo
	{
		public readonly EcsId Id;
		public readonly int SizeOfElement;
		public readonly bool HasFields;

		public EcsTypeInfo(EcsId id, int sizeOfElement, bool hasFields)
		{
			Id = id;
			SizeOfElement = sizeOfElement;
			HasFields = hasFields;
		}
	}
}