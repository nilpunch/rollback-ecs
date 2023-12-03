using System;

namespace ECS
{
	public static class UnmanagedUtils
	{
		private static readonly byte[] _swapBuffer = new byte[1024];

		public static unsafe void CopyElement(IUnmanagedArray source, int sourceIndex, IUnmanagedArray destination, int destinationIndex)
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

		public static unsafe void Swap(this IUnmanagedArray array, int source, int destination)
		{
			if (source < 0 || source >= array.Capacity)
				throw new ArgumentOutOfRangeException(nameof(source));

			if (destination < 0 || destination >= array.Capacity)
				throw new ArgumentOutOfRangeException(nameof(destination));

			if (array.SizeOfElement > _swapBuffer.Length)
				throw new Exception("Size of a struct is too large.");

			IntPtr sourceElement = IntPtr.Add(array.Pointer, source * array.SizeOfElement);
			IntPtr destinationElement = IntPtr.Add(array.Pointer, destination * array.SizeOfElement);
			int sizeOfElement = array.SizeOfElement;

			fixed (void* temp = _swapBuffer)
			{
				Buffer.MemoryCopy(sourceElement.ToPointer(), temp, sizeOfElement, sizeOfElement);
				Buffer.MemoryCopy(destinationElement.ToPointer(), sourceElement.ToPointer(), sizeOfElement, sizeOfElement);
				Buffer.MemoryCopy(temp, destinationElement.ToPointer(), sizeOfElement, sizeOfElement);
			}
		}

		public static unsafe Span<T> AsSpan<T>(this IUnmanagedArray source) where T : unmanaged
		{
			return new Span<T>(source.Pointer.ToPointer(), source.Capacity);
		}

		public static unsafe ref T GetElementRef<T>(this IUnmanagedArray source, int index) where T : unmanaged
		{
			return ref ((T*)source.Pointer.ToPointer())[index];
		}
	}
}