using System;
using System.Collections.Generic;
using System.Linq;

namespace ECS
{
	public static class EcsIdUtils
	{
		public static TableId CalculateTableId(SortedSet<EcsId> components)
		{
			return new TableId(CombineIds(components));
		}

		public static ArchetypeId CalculateArchetypeId(SortedSet<EcsId> type)
		{
			return new ArchetypeId(CombineIds(type));
		}

		private static long CombineIds(SortedSet<EcsId> ids)
		{
			if (ids.Count == 0)
				return 0;

			return ids.Select(id => id.Id)
				.Aggregate((current, next) => HashCode.Combine(current, next));
		}
	}
}