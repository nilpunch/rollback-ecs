using System;
using System.Buffers;
using System.Collections.Generic;

namespace ECS
{
	public static class ListExtensions
	{
		public static void RemoveBySwap<T>(this List<T> list, int index)
		{
			list[index] = list[^1];
			list.RemoveAt(list.Count - 1);
		}

		public static void RemoveBySwap<T>(this List<T> list, T item)
		{
			int index = list.IndexOf(item);
			RemoveBySwap(list, index);
		}

		public static void RemoveBySwap<T>(this List<T> list, Predicate<T> predicate)
		{
			int index = list.FindIndex(predicate);
			RemoveBySwap(list, index);
		}
	}
}