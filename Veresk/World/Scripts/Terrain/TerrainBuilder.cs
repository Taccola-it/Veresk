using UnityEngine;
using Veresk.World.Biomes;
using Veresk.World.Core;
using Veresk.World.Settings;

namespace Veresk.World.TerrainSystem
{
    public sealed class TerrainBuilder
    {
        private const string TerrainObjectName = "GeneratedTerrain";

        public Terrain BuildOrUpdate(
            Transform parent,
            WorldSettings settings,
            WorldData worldData)
        {
            TerrainLayer[] terrainLayers = BuildDefaultLayers(settings);
            Terrain terrain = BuildOrUpdateTerrain(parent, settings, worldData, terrainLayers);
            ApplyTextures(terrain, settings, worldData, terrainLayers);
            return terrain;
        }

        private Terrain BuildOrUpdateTerrain(
            Transform parent,
            WorldSettings settings,
            WorldData worldData,
            TerrainLayer[] terrainLayers)
        {
            Terrain existingTerrain = FindExistingTerrain(parent);
            Terrain terrainComponent;

            if (existingTerrain == null)
            {
                TerrainData terrainData = new TerrainData
                {
                    heightmapResolution = settings.terrainDimensions.heightmapResolution,
                    alphamapResolution = settings.terrainDimensions.alphamapResolution,
                    size = new Vector3(
                        settings.terrainDimensions.terrainSizeX,
                        settings.terrainDimensions.terrainHeight,
                        settings.terrainDimensions.terrainSizeZ),
                    terrainLayers = terrainLayers
                };

                terrainData.SetDetailResolution(settings.terrainDimensions.detailResolution, 8);

                GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
                terrainObject.name = TerrainObjectName;
                terrainObject.transform.SetParent(parent, false);

                terrainComponent = terrainObject.GetComponent<Terrain>();
            }
            else
            {
                terrainComponent = existingTerrain;
                TerrainData terrainData = terrainComponent.terrainData;

                terrainData.heightmapResolution = settings.terrainDimensions.heightmapResolution;
                terrainData.alphamapResolution = settings.terrainDimensions.alphamapResolution;
                terrainData.size = new Vector3(
                    settings.terrainDimensions.terrainSizeX,
                    settings.terrainDimensions.terrainHeight,
                    settings.terrainDimensions.terrainSizeZ);
                terrainData.terrainLayers = terrainLayers;
            }

            ApplyHeights(terrainComponent.terrainData, worldData.FinalHeightMap);
            return terrainComponent;
        }

        private void ApplyHeights(TerrainData terrainData, float[,] heights)
        {
            int width = heights.GetLength(0);
            int height = heights.GetLength(1);

            float[,] unityHeights = new float[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    unityHeights[y, x] = Mathf.Clamp01(heights[x, y]);
                }
            }

            terrainData.SetHeights(0, 0, unityHeights);
        }

        private TerrainLayer[] BuildDefaultLayers(WorldSettings settings)
        {
            TerrainLayer[] layers = new TerrainLayer[5];

            layers[0] = CreateLayer(
                settings.terrainLayerSettings.underwaterLayerName,
                new Color(0.20f, 0.27f, 0.23f),
                new Vector2(12f, 12f));

            layers[1] = CreateLayer(
                settings.terrainLayerSettings.coastLayerName,
                new Color(0.54f, 0.47f, 0.34f),
                new Vector2(10f, 10f));

            layers[2] = CreateLayer(
                settings.terrainLayerSettings.lowlandLayerName,
                new Color(0.41f, 0.52f, 0.30f),
                new Vector2(16f, 16f));

            layers[3] = CreateLayer(
                settings.terrainLayerSettings.grasslandLayerName,
                new Color(0.33f, 0.46f, 0.24f),
                new Vector2(18f, 18f));

            layers[4] = CreateLayer(
                settings.terrainLayerSettings.uplandLayerName,
                new Color(0.42f, 0.43f, 0.35f),
                new Vector2(20f, 20f));

            return layers;
        }

        private TerrainLayer CreateLayer(string layerName, Color tint, Vector2 tileSize)
        {
            TerrainLayer layer = new TerrainLayer
            {
                name = layerName,
                tileSize = tileSize,
                diffuseRemapMax = new Vector4(tint.r, tint.g, tint.b, 1f)
            };

            return layer;
        }

        private void ApplyTextures(
            Terrain terrain,
            WorldSettings settings,
            WorldData worldData,
            TerrainLayer[] layers)
        {
            if (terrain == null || terrain.terrainData == null || worldData == null || layers == null || layers.Length < 5)
            {
                return;
            }

            TerrainData terrainData = terrain.terrainData;
            terrainData.terrainLayers = layers;

            int alphaWidth = terrainData.alphamapWidth;
            int alphaHeight = terrainData.alphamapHeight;
            int layerCount = layers.Length;

            float[,,] splatmap = new float[alphaHeight, alphaWidth, layerCount];

            float seaLevel = settings.terrainDimensions.normalizedSeaLevel;
            TerrainLayerSettings textureSettings = settings.terrainLayerSettings;
            int worldResolution = worldData.Resolution;

            for (int ay = 0; ay < alphaHeight; ay++)
            {
                for (int ax = 0; ax < alphaWidth; ax++)
                {
                    float nx = (float)ax / (alphaWidth - 1);
                    float ny = (float)ay / (alphaHeight - 1);

                    int wx = Mathf.Clamp(Mathf.RoundToInt(nx * (worldResolution - 1)), 0, worldResolution - 1);
                    int wy = Mathf.Clamp(Mathf.RoundToInt(ny * (worldResolution - 1)), 0, worldResolution - 1);

                    float h = worldData.FinalHeightMap[wx, wy];
                    float coast = worldData.CoastMask[wx, wy];
                    float slope = worldData.SlopeMapDegrees[wx, wy];
                    BiomeType biome = worldData.BiomeMap[wx, wy];

                    float[] weights = BuildWeights(textureSettings, seaLevel, h, coast, slope, biome);
                    Normalize(weights);

                    for (int layer = 0; layer < layerCount; layer++)
                    {
                        splatmap[ay, ax, layer] = weights[layer];
                    }
                }
            }

            terrainData.SetAlphamaps(0, 0, splatmap);
        }

        private float[] BuildWeights(
            TerrainLayerSettings textureSettings,
            float seaLevel,
            float normalizedHeight,
            float coastMask,
            float slopeDegrees,
            BiomeType biome)
        {
            float[] weights = new float[5];

            float underwater = normalizedHeight <= seaLevel ? 1f : 0f;

            float coastHeightFactor = 1f - Mathf.Clamp01(
                Mathf.Abs(normalizedHeight - seaLevel) /
                Mathf.Max(0.0001f, textureSettings.coastHeightBlendRange));

            float coast = Mathf.Max(coastMask, coastHeightFactor) * textureSettings.coastTextureInfluence;
            if (normalizedHeight <= seaLevel)
            {
                coast *= 0.2f;
            }

            float lowland = 0f;
            float grassland = 0f;
            float upland = 0f;

            if (normalizedHeight > seaLevel)
            {
                lowland = 1f - Mathf.InverseLerp(
                    textureSettings.lowlandStart01,
                    textureSettings.grasslandStart01,
                    normalizedHeight);
                lowland = Mathf.Clamp01(lowland);

                grassland =
                    Mathf.InverseLerp(textureSettings.lowlandStart01, textureSettings.grasslandStart01, normalizedHeight) *
                    (1f - Mathf.InverseLerp(textureSettings.grasslandStart01, textureSettings.uplandStart01, normalizedHeight));
                grassland = Mathf.Clamp01(grassland);

                upland = Mathf.InverseLerp(
                    textureSettings.grasslandStart01,
                    textureSettings.uplandStart01,
                    normalizedHeight);
                upland = Mathf.Clamp01(upland);
            }

            float slopeUplandBoost = Mathf.InverseLerp(14f, 32f, slopeDegrees) * textureSettings.slopeToUplandInfluence;
            upland = Mathf.Clamp01(upland + slopeUplandBoost);

            if (biome == BiomeType.SacredGrove)
            {
                grassland += 0.20f;
                lowland += 0.10f;
            }

            weights[0] = underwater;
            weights[1] = coast;
            weights[2] = lowland;
            weights[3] = grassland;
            weights[4] = upland;

            return weights;
        }

        private void Normalize(float[] values)
        {
            float sum = 0f;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Mathf.Max(0f, values[i]);
                sum += values[i];
            }

            if (sum <= 0f)
            {
                values[0] = 1f;
                return;
            }

            for (int i = 0; i < values.Length; i++)
            {
                values[i] /= sum;
            }
        }

        private Terrain FindExistingTerrain(Transform parent)
        {
            Transform child = parent.Find(TerrainObjectName);
            if (child == null)
            {
                return null;
            }

            return child.GetComponent<Terrain>();
        }
    }
}