using System.Collections.Generic;
using JetBrains.Annotations;

namespace ECS
{
	public class Archetype
	{
		public HashSet<EcsId> Entities { get; }
		public SortedSet<EcsId> Type { get; }

		/// <summary>
		/// Field is null in archetypes with no components.
		/// </summary>
		[CanBeNull] public Table Table { get; }

		public Archetype(SortedSet<EcsId> type, [CanBeNull] Table table)
		{
			Type = type;
			Table = table;
			Entities = new HashSet<EcsId>();
		}
	}
}