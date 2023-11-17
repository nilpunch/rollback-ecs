using System.Collections.Generic;

namespace ECS
{
	public class EcsIdGenerator
	{
		private readonly Stack<EcsId> _freeIds = new Stack<EcsId>();
		private EcsId _nextFreeId;

		public EcsId ReserveId()
		{
			if (_freeIds.Count != 0)
			{
				return _freeIds.Pop();
			}

			EcsId freeId = _nextFreeId;
			_nextFreeId = new EcsId(freeId.Id + 1);
			return freeId;
		}

		public void FreeEntityId(EcsId ecsId)
		{
			_freeIds.Push(EcsId.FromIndexGeneration(ecsId.Index, ecsId.Generation + 1));
		}
	}
}