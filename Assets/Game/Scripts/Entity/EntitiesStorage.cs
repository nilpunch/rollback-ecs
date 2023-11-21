using System.Collections.Generic;

namespace ECS
{
    /// <summary>
    /// Container for all entities (EcsId) and their location (Archetype, RowInTable).
    /// </summary>
    public class EntitiesStorage
    {
        private readonly Dictionary<EcsId, EntityInfo> _entities;
        private readonly EcsIdGenerator _ecsIdGenerator;

        public EntitiesStorage()
        {
            _entities = new Dictionary<EcsId, EntityInfo>();
            _ecsIdGenerator = new EcsIdGenerator();
        }

        public EntityInfo this[EcsId id]
        {
            get => _entities[id];
            set => _entities[id] = value;
        }

        public EcsId CreateInArchetype(Archetype archetype)
        {
            EcsId entityId = _ecsIdGenerator.ReserveId();
            int entityRow = archetype.Table?.ReserveRow() ?? -1;
            archetype.Entities.Add(entityId);
            _entities.Add(entityId, new EntityInfo(archetype, entityRow));
            return entityId;
        }

        public void Destroy(EcsId entityId)
        {
            if (!_entities.TryGetValue(entityId, out var entityInfo))
            {
                return;
            }

            entityInfo.Archetype.Table?.FreeRow(entityInfo.RowInTable);
			
            _entities.Remove(entityId);
            _ecsIdGenerator.FreeEntityId(entityId);
        }
    }
}