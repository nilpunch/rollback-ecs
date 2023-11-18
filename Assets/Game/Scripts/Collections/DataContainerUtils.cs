using System;

namespace ECS
{
	public static class DataContainerUtils
	{
		public static unsafe void CopyElement(IDataContainer source, int sourceIndex, IDataContainer destination, int destinationIndex)
		{
			if (source.SizeOfElement != destination.SizeOfElement)
				throw new InvalidOperationException("Element sizes not match!");

			if (sourceIndex >= source.Capacity)
				throw new ArgumentOutOfRangeException(nameof(sourceIndex));

			if (destinationIndex >= destination.Capacity)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			IntPtr sourceElement = IntPtr.Add(source.Pointer, sourceIndex * source.SizeOfElement);
			IntPtr destinationElement = IntPtr.Add(destination.Pointer, destinationIndex * destination.SizeOfElement);
			int sizeOfElement = source.SizeOfElement;

			Buffer.MemoryCopy(sourceElement.ToPointer(), destinationElement.ToPointer(), sizeOfElement, sizeOfElement);
		}

		public static unsafe Span<T> AsSpan<T>(this IDataContainer source) where T : unmanaged
		{
			return new Span<T>(source.Pointer.ToPointer(), source.Capacity);
		}

		public static unsafe ref T GetRef<T>(this IDataContainer source, int index) where T : unmanaged
		{
			return ref ((T*)source.Pointer.ToPointer())[index];
		}
	}
}