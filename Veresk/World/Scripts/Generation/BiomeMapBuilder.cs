using Veresk.World.Biomes;

namespace Veresk.World.Generation
{
    public class BiomeMapBuilder
    {
        public BiomeType[,] BuildBiomeMap(
            Settings.WorldSettings settings,
            float[,] heightMap,
            float[,] slopeMap,
            float[,] inlandMap,
            float[,] coastMap,
            float[,] worldDistanceMap,
            WorldRingType[,] ringMap,
            out float[,] suitabilityMap)
        {
            int resolution = heightMap.GetLength(0);

            BiomeType[,] biomeMap = new BiomeType[resolution, resolution];
            suitabilityMap = new float[resolution, resolution];

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float bestScore = 0f;
                    BiomeType bestBiome = BiomeType.None;

                    float h = heightMap[x, y];
                    float slope = slopeMap[x, y];
                    float inland = inlandMap[x, y];
                    float coast = coastMap[x, y];
                    float worldRadius01 = worldDistanceMap[x, y];
                    WorldRingType ring = ringMap[x, y];

                    for (int i = 0; i < settings.biomeDefinitions.Count; i++)
                    {
                        BiomeDefinition biome = settings.biomeDefinitions[i];
                        if (biome == null)
                            continue;

                        float score = biome.EvaluateSuitability(h, slope, inland, coast);
                        if (score <= 0f)
                            continue;

                        score *= biome.EvaluateRingAffinity(ring);
                        if (score <= 0f)
                            continue;

                        score *= biome.EvaluateWorldRadiusAffinity(worldRadius01);
                        if (score <= 0f)
                            continue;

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestBiome = biome.BiomeType;
                        }
                    }

                    biomeMap[x, y] = bestBiome;
                    suitabilityMap[x, y] = bestScore;
                }
            }

            return biomeMap;
        }
    }
}