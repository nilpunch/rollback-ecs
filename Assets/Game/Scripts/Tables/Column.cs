namespace ECS
{
	public readonly struct Column
	{
		public readonly IUnmanagedArray Data;
		public readonly EcsId ComponentId;

		public Column(IUnmanagedArray data, EcsId componentId)
		{
			Data = data;
			ComponentId = componentId;
		}
	}
}