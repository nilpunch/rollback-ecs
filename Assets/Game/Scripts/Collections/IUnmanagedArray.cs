using System;

namespace ECS
{
	public interface IUnmanagedArray
	{
		bool IsResizeable { get; }
		int Capacity { get; }
		int SizeOfElement { get; }
		IntPtr Pointer { get; }
		
		void Resize(int newCapacity);
	}
}