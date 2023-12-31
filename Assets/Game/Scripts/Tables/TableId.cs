﻿using System;

namespace ECS
{
	public readonly struct TableId : IEquatable<TableId>
	{
		public readonly ulong Id;

		public TableId(ulong id)
		{
			Id = id;
		}

		public bool Equals(TableId other)
		{
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return obj is TableId other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}