using System;
using System.Runtime.InteropServices;

namespace ECS
{
	public class ResizableDataContainer : IDataContainer
	{
		private ResizableDataContainer(IntPtr data, GCHandle handle, int capacity, int sizeOfElement)
		{
			Handle = handle;
			Pointer = data;
			Capacity = capacity;
			SizeOfElement = sizeOfElement;
		}

		public bool IsResizeable => true;

		public int Capacity { get; private set; }

		public int SizeOfElement { get; }

		public IntPtr Pointer { get; private set; }

		private GCHandle Handle { get; set; }

		public static IDataContainer Create<T>(int capacity) where T : unmanaged
		{
			return Create(capacity, Marshal.SizeOf<T>());
		}

		public static IDataContainer Create(int capacity, int sizeOfElement)
		{
			var data = new byte[capacity * sizeOfElement];
			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var pointer = handle.AddrOfPinnedObject();

			return new ResizableDataContainer(pointer, handle, capacity, sizeOfElement);
		}

		public unsafe void Resize(int newCapacity)
		{
			var newData = new byte[newCapacity * SizeOfElement];
			var newHandle = GCHandle.Alloc(newData, GCHandleType.Pinned);
			var newPointer = newHandle.AddrOfPinnedObject();

			Buffer.MemoryCopy(Pointer.ToPointer(), newPointer.ToPointer(),
				Capacity * SizeOfElement,
				Capacity * SizeOfElement);

			Handle.Free();
			Handle = newHandle;
			Pointer = newPointer;
			Capacity = newCapacity;
		}

		~ResizableDataContainer()
		{
			Handle.Free();
		}
	}
}