using System.Collections.Generic;
using System.Linq;

namespace ECS
{
	public struct SortedArray<T>
	{
	}

	public static class EcsIdUtils
	{
		public static TableId CombineTableId(SortedSet<EcsId> type)
		{
			return new TableId(CombineIds(type));
		}

		private static ulong CombineIds(SortedSet<EcsId> ids)
		{
			if (ids.Count == 0)
				return 0;

			return ids.Select(id => id.Id).Aggregate(CombineHash);
		}

		private static ulong CombineHash(ulong a, ulong b)
		{
			unchecked
			{
				ulong hash = 17;
				hash = hash * 31 + a;
				hash = hash * 31 + b;
				return hash;
			}
		}
	}
}