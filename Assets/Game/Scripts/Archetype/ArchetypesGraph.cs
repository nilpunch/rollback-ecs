using System.Collections.Generic;

namespace ECS
{
	/// <summary>
	/// Container to find linked archetypes.
	/// </summary>
	public class ArchetypesGraph
	{
		private readonly ArchetypesStorage _archetypesStorage;
		private readonly Dictionary<Archetype, Dictionary<EcsId, ArchetypesLink>> _graph;

		public ArchetypesGraph(ArchetypesStorage archetypesStorage)
		{
			_archetypesStorage = archetypesStorage;
			_graph = new Dictionary<Archetype, Dictionary<EcsId, ArchetypesLink>>();
		}

		public Archetype ArchetypeAfterAddComponent(Archetype archetype, EcsId component)
		{
			var link = GetOrCreateLink(archetype, component);

			if (link.OnAdd == null)
			{
				link.OnAdd = _archetypesStorage.GetOrCreateArchetypeFor(archetype.Components.CloneAdd(component), archetype.Things);
			}

			return link.OnAdd;
		}

		public Archetype ArchetypeAfterRemoveComponent(Archetype archetype, EcsId component)
		{
			var link = GetOrCreateLink(archetype, component);

			if (link.OnRemove == null)
			{
				link.OnRemove = _archetypesStorage.GetOrCreateArchetypeFor(archetype.Components.CloneRemove(component), archetype.Things);
			}

			return link.OnRemove;
		}

		public Archetype ArchetypeAfterAddThing(Archetype archetype, EcsId thing)
		{
			var link = GetOrCreateLink(archetype, thing);

			if (link.OnAdd == null)
			{
				link.OnAdd = _archetypesStorage.GetOrCreateArchetypeFor(archetype.Components, archetype.Things.CloneAdd(thing));
			}

			return link.OnAdd;
		}

		public Archetype ArchetypeAfterRemoveThing(Archetype archetype, EcsId thing)
		{
			var link = GetOrCreateLink(archetype, thing);

			if (link.OnRemove == null)
			{
				link.OnRemove = _archetypesStorage.GetOrCreateArchetypeFor(archetype.Components, archetype.Things.CloneRemove(thing));
			}

			return link.OnRemove;
		}

		private ArchetypesLink GetOrCreateLink(Archetype archetype, EcsId component)
		{
			ArchetypesLink link = null;
			bool hasArchetype = _graph.TryGetValue(archetype, out var connections);
			bool hasComponentLink = hasArchetype && connections.TryGetValue(component, out link);

			if (hasComponentLink)
			{
				return link;
			}

			if (!hasArchetype)
			{
				connections = new Dictionary<EcsId, ArchetypesLink>();
				_graph.Add(archetype, connections);
			}

			link = new ArchetypesLink();
			connections.Add(component, link);

			return link;
		}

		private class ArchetypesLink
		{
			public Archetype OnAdd;
			public Archetype OnRemove;
		}
	}
}