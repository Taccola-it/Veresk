using System;

namespace Veresk.World.Core
{
    public static class SeedUtility
    {
        public static Random CreateRandom(int seed)
        {
            return new Random(seed);
        }

        public static int Combine(int seed, int salt)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + seed;
                hash = (hash * 31) + salt;
                return hash;
            }
        }

        public static int Combine(int seed, string salt)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + seed;
                hash = (hash * 31) + (salt != null ? salt.GetHashCode() : 0);
                return hash;
            }
        }
    }
}