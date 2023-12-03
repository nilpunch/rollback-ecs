using System.Collections.Generic;

namespace ECS
{
	public static class SortedSetExtensions
	{
		public static SortedSet<T> CloneRemove<T>(this SortedSet<T> self, T element)
		{
			var set = new SortedSet<T>(self);
			set.Remove(element);
			return set;
		}

		public static SortedSet<T> CloneAdd<T>(this SortedSet<T> self, T element)
		{
			var set = new SortedSet<T>(self);
			set.Add(element);
			return set;
		}

		public static SortedSet<T> CloneAdd<T>(this SortedSet<T> self, SortedSet<T> other)
		{
			var set = new SortedSet<T>(self);
			set.UnionWith(other);
			return set;
		}
	}
}