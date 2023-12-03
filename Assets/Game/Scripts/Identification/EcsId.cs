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

		public static bool operator ==(EcsId a, EcsId b)
		{
			return a.Id == b.Id;
		}

		public static bool operator !=(EcsId a, EcsId b)
		{
			return !(a == b);
		}

		public bool Equals(EcsId other)
		{
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return obj is EcsId other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public int CompareTo(EcsId other)
		{
			return Id.CompareTo(other.Id);
		}
	}
}