using System.Collections.Generic;
using JetBrains.Annotations;

namespace ECS
{
	public class Archetype
	{
		public HashSet<EcsId> Entities { get; }
		public SortedSet<EcsId> Components { get; }
		public SortedSet<EcsId> Things { get; }
		public SortedSet<EcsId> FullType { get; }
		public ArchetypeId ArchetypeId { get; }
		public TableId TableId { get; }

		/// <summary>
		/// Field is null in archetypes with no components.
		/// </summary>
		[CanBeNull] public Table Table { get; }

		public Archetype(SortedSet<EcsId> components, SortedSet<EcsId> things, [CanBeNull] Table table)
		{
			Components = components;
			Things = things;
			Table = table;
			Entities = new HashSet<EcsId>();
			ArchetypeId = EcsIdUtils.CalculateArchetypeId(components, things);
			TableId = EcsIdUtils.CalculateTableId(components);
			FullType = components.CloneAdd(things);
		}
	}
}