using System;

namespace ECS
{
	public static class DataContainerUtils
	{
		public static unsafe void CopyElement(IDataContainer source, int sourceIndex, IDataContainer destination, int destinationIndex)
		{
			if (source.SizeOfElement != destination.SizeOfElement)
			{
				throw new InvalidOperationException();
			}
			
			IntPtr sourceElement = IntPtr.Add(source.Pointer, sourceIndex * source.SizeOfElement);
			IntPtr destinationElement = IntPtr.Add(destination.Pointer, destinationIndex * destination.SizeOfElement);
			int sizeOfElement = source.SizeOfElement;

			Buffer.MemoryCopy(sourceElement.ToPointer(), destinationElement.ToPointer(), sizeOfElement, sizeOfElement);
		}
		
		public static unsafe void Copy(IDataContainer source, int sourceIndex, IDataContainer destination, int destinationIndex, int amount)
		{
			throw new NotImplementedException();
		}
	}
}