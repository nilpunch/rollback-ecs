using System;
using System.Collections.Generic;
using System.Linq;

namespace ECS
{
	public static class EcsIdUtils
	{
		public static TypeId CalculateType(SortedSet<EcsId> components)
		{
			return new TypeId(components.Select(id => id.Id).Aggregate((current, next) => HashCode.Combine(current, next)));
		}
		
		public static TypeId CalculateType(IEnumerable<EcsId> components)
		{
			return CalculateType(new SortedSet<EcsId>(components));
		}
		
		public static TypeId CalculateArchetype(IEnumerable<EcsId> components, IEnumerable<EcsId> tags)
		{
			return CalculateType(components.Concat(tags));
		}
	}
}