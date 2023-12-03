using System;

namespace ECS
{
	public readonly struct EcsId : IComparable<EcsId>
	{
		public readonly ulong Id;

		public EcsId(ulong id)
		{
			Id = id;
		}

		public int ArrayIndex => (int)Index;
		
		public uint Index => (uint)(Id & 0xffffffff);

		public uint Generation => (uint)(Id >> 32);

		public static bool operator ==(EcsId a, EcsId b)
		{
			return a.Id == b.Id;
		}

		public static bool operator !=(EcsId a, EcsId b)
		{
			return !(a == b);
		}

		public static EcsId IncreaseGeneration(EcsId ecsId)
		{
			unchecked
			{
				return FromIndexGeneration(ecsId.Index, ecsId.Generation + 1);
			}
		}
		
		public static EcsId FromIndexGeneration(uint index, uint generation)
		{
			// Combine index and generation to create the EcsId
			ulong id = ((ulong)generation << 32) | index;
			return new EcsId(id);
		}

		public int CompareTo(EcsId other)
		{
			return Id.CompareTo(other.Id);
		}
	}
}