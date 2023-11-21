using System;

namespace ECS
{
	public readonly struct ArchetypeId : IEquatable<ArchetypeId>
	{
		public readonly long Id;

		public ArchetypeId(long id)
		{
			Id = id;
		}

		public bool Equals(ArchetypeId other)
		{
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return obj is ArchetypeId other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}