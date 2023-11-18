using System.Collections.Generic;
using JetBrains.Annotations;

namespace ECS
{
	public class Archetype
	{
		public HashSet<EcsId> Entities { get; }
		public SortedSet<EcsId> Components { get; }
		public SortedSet<EcsId> Tags { get; }
		public ArchetypeId ArchetypeId { get; }
		public TableId TableId { get; }

		/// <summary>
		/// Field is null in archetypes with no components.
		/// </summary>
		[CanBeNull] public Table Table { get; }

		public Archetype(SortedSet<EcsId> components, SortedSet<EcsId> tags, [CanBeNull] Table table)
		{
			Components = components;
			Tags = tags;
			Table = table;
			Entities = new HashSet<EcsId>();
			ArchetypeId = EcsIdUtils.CalculateArchetypeId(components, tags);
			TableId = EcsIdUtils.CalculateTableId(components);
		}
	}
}