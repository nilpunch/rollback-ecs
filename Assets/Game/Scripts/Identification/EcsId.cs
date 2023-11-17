using System;

namespace ECS
{
    public readonly struct EcsId : IComparable<EcsId>
    {
        public readonly long Id;

        public EcsId(long id)
        {
            Id = id;
        }
        
        public int Index => (int)(Id & 0xffffffff);
        
        public int Generation => (int)(Id >> 32);

        public static EcsId FromIndexGeneration(int index, int generation)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be a non-negative 32-bit integer.");

            if (generation < 0)
                throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be a non-negative integer.");

            // Combine index and generation to create the EcsId
            long id = ((long)generation << 32) | (uint)index;

            return new EcsId(id);
        }

        public int CompareTo(EcsId other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}