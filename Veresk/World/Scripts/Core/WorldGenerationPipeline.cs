using UnityEngine;
using Veresk.World.Generation;
using Veresk.World.Settings;

namespace Veresk.World.Core
{
    public sealed class WorldGenerationPipeline
    {
        private readonly WorldDistanceMapBuilder worldDistanceMapBuilder = new();
        private readonly WorldRingMapBuilder worldRingMapBuilder = new();
        private readonly StartIslandBuilder startIslandBuilder = new();
        private readonly ContinentMapBuilder continentMapBuilder = new();
        private readonly ArchipelagoClusterBuilder archipelagoClusterBuilder = new();
        private readonly OceanChannelBuilder oceanChannelBuilder = new();
        private readonly MacroLandmassBuilder macroLandmassBuilder = new();
        private readonly IslandMaskBuilder islandMaskBuilder = new();
        private readonly HeightMapBuilder heightMapBuilder = new();
        private readonly CoastMaskBuilder coastMaskBuilder = new();
        private readonly SlopeMapBuilder slopeMapBuilder = new();
        private readonly BiomeMapBuilder biomeMapBuilder = new();
        private readonly ShorelineHeightProcessor shorelineHeightProcessor = new();
        private readonly TerrainDeformationHook terrainDeformationHook = new();

        public WorldData Generate(WorldSettings settings, int seed)
        {
            int resolution = settings.terrainDimensions.heightmapResolution;
            WorldData data = new WorldData(seed, resolution);

            data.WorldDistanceMap = worldDistanceMapBuilder.Build(resolution);
            data.WorldRingMap = worldRingMapBuilder.Build(settings, data.WorldDistanceMap);

            data.StartIslandMap = startIslandBuilder.Build(settings, seed, resolution);
            data.ContinentMap = continentMapBuilder.Build(settings, seed, resolution);
            data.ArchipelagoClusterMap = archipelagoClusterBuilder.Build(settings, seed, resolution);
            data.OceanChannelMap = oceanChannelBuilder.Build(settings, seed, resolution);

            data.MacroLandmassMap = BuildMacroLandmass(
                settings,
                seed,
                resolution,
                data.StartIslandMap,
                data.ContinentMap,
                data.ArchipelagoClusterMap,
                data.OceanChannelMap);

            data.IslandMask = islandMaskBuilder.Build(settings, seed, resolution, data.MacroLandmassMap);
            ReinforceStartIsland(data.IslandMask, data.StartIslandMap, settings.landmassMixSettings.startIslandMaskBlend);

            data.BaseHeightMap = heightMapBuilder.Build(settings, seed, resolution, data.IslandMask);

            float[,] deformationReadyHeight = terrainDeformationHook.ApplyRuntimeModifications(data);
            if (deformationReadyHeight == null)
            {
                deformationReadyHeight = data.BaseHeightMap;
            }

            float[,] slopeMap = slopeMapBuilder.Build(settings, deformationReadyHeight);
            float[,] softenedHeight = slopeMapBuilder.SoftenSteepAreas(settings, deformationReadyHeight, slopeMap);

            float[,] initialCoastMap = coastMaskBuilder.Build(settings, softenedHeight);
            float[,] shorelineSmoothedHeight = shorelineHeightProcessor.Smooth(settings, softenedHeight, initialCoastMap);

            data.SlopeMapDegrees = slopeMapBuilder.Build(settings, shorelineSmoothedHeight);
            data.CoastMask = coastMaskBuilder.Build(settings, shorelineSmoothedHeight);
            data.InlandDistanceMap = coastMaskBuilder.BuildInlandDistance(settings, shorelineSmoothedHeight);

            data.BiomeMap = biomeMapBuilder.BuildBiomeMap(
                settings,
                shorelineSmoothedHeight,
                data.SlopeMapDegrees,
                data.InlandDistanceMap,
                data.CoastMask,
                data.WorldDistanceMap,
                data.WorldRingMap,
                out float[,] suitabilityMap);

            data.BiomeSuitabilityMap = suitabilityMap;
            data.FinalHeightMap = shorelineSmoothedHeight;

            return data;
        }

        private float[,] BuildMacroLandmass(
            WorldSettings settings,
            int seed,
            int resolution,
            float[,] startIslandMap,
            float[,] continentMap,
            float[,] clusterMap,
            float[,] channelMap)
        {
            float[,] macroLandmass = macroLandmassBuilder.Build(settings, seed, resolution);
            LandmassMixSettings mix = settings.landmassMixSettings;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float land =
                        (macroLandmass[x, y] * mix.macroLandmassWeight) +
                        (continentMap[x, y] * mix.continentWeight) +
                        (clusterMap[x, y] * mix.archipelagoClusterWeight);

                    float channelCut = Mathf.Lerp(1f, mix.minChannelMultiplier, channelMap[x, y]);
                    land *= channelCut;

                    land = Mathf.Max(land, startIslandMap[x, y] * mix.startIslandMacroBlend);
                    macroLandmass[x, y] = Mathf.Clamp01(land);
                }
            }

            return macroLandmass;
        }

        private void ReinforceStartIsland(float[,] islandMask, float[,] startIslandMap, float strength)
        {
            int resolution = islandMask.GetLength(0);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    islandMask[x, y] = Mathf.Max(islandMask[x, y], startIslandMap[x, y] * strength);
                }
            }
        }
    }
}