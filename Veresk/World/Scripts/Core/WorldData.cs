using Veresk.World.Biomes;
using Veresk.World.Generation;

namespace Veresk.World.Core
{
    public sealed class WorldData
    {
        public int Seed { get; }
        public int Resolution { get; }

        public float[,] WorldDistanceMap { get; set; }
        public WorldRingType[,] WorldRingMap { get; set; }

        public float[,] StartIslandMap { get; set; }
        public float[,] ContinentMap { get; set; }
        public float[,] ArchipelagoClusterMap { get; set; }
        public float[,] OceanChannelMap { get; set; }
        public float[,] MacroLandmassMap { get; set; }

        public float[,] BaseHeightMap { get; set; }
        public float[,] FinalHeightMap { get; set; }
        public float[,] IslandMask { get; set; }
        public float[,] CoastMask { get; set; }
        public float[,] InlandDistanceMap { get; set; }
        public float[,] SlopeMapDegrees { get; set; }
        public float[,] BiomeSuitabilityMap { get; set; }
        public BiomeType[,] BiomeMap { get; set; }

        public TerrainModificationData TerrainModificationData { get; }

        public WorldData(int seed, int resolution)
        {
            Seed = seed;
            Resolution = resolution;
            TerrainModificationData = new TerrainModificationData();
        }
    }
}