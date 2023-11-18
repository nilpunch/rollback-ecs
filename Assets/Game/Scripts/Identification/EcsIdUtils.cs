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
		
		public static ArchetypeId CalculateArchetypeId(SortedSet<EcsId> components, SortedSet<EcsId> tags)
		{
			return new ArchetypeId(CombineIds(components.CloneAdd(tags)));
		}

		private static long CombineIds(SortedSet<EcsId> ids)
		{
			return ids.Select(id => id.Id)
				.Aggregate((current, next) => HashCode.Combine(current, next));
		}
	}
}