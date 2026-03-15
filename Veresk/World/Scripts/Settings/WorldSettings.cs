using System.Collections.Generic;
using UnityEngine;
using Veresk.World.Biomes;
using Veresk.World.Debugging;

namespace Veresk.World.Settings
{
    [CreateAssetMenu(
        fileName = "WorldSettings",
        menuName = "Veresk/World/World Settings")]
    public class WorldSettings : ScriptableObject
    {
        [Header("Seed")]
        public bool useRandomSeedOnGenerate = false;
        public int seed = 12345;

        [Header("World")]
        public TerrainDimensionSettings terrainDimensions = new();
        public WorldZoneSettings worldZoneSettings = new();

        [Header("Landmass Mixing")]
        public LandmassMixSettings landmassMixSettings = new();

        [Header("Shape")]
        [Range(0f, 2f)] public float islandFalloffStrength = 0.55f;
        [Range(0f, 1f)] public float islandCoreBias = 0.36f;
        [Range(0f, 2f)] public float shorelineSharpness = 0.90f;

        [Header("Noise")]
        public NoiseSettings macroNoise = new()
        {
            baseScale = 500f,
            octaves = 4,
            persistence = 0.5f,
            lacunarity = 2f,
            amplitude = 1f
        };

        public NoiseSettings mediumNoise = new()
        {
            baseScale = 180f,
            octaves = 4,
            persistence = 0.5f,
            lacunarity = 2f,
            amplitude = 0.45f
        };

        public NoiseSettings detailNoise = new()
        {
            baseScale = 70f,
            octaves = 3,
            persistence = 0.45f,
            lacunarity = 2.2f,
            amplitude = 0.12f
        };

        [Header("Height")]
        [Range(0f, 1f)] public float macroWeight = 0.50f;
        [Range(0f, 1f)] public float mediumWeight = 0.38f;
        [Range(0f, 1f)] public float detailWeight = 0.12f;
        [Range(0f, 1f)] public float heightBias = 0f;
        [Range(0.1f, 8f)] public float heightCurvePower = 1.10f;
        [Range(0f, 1f)] public float playableFlattening = 0.42f;

        [Header("Coast & Slope")]
        [Range(0.001f, 0.3f)] public float coastalBandWidth = 0.15f;
        [Range(0f, 1f)] public float coastSmoothingStrength = 0.75f;
        [Range(0f, 1f)] public float slopeSofteningStrength = 0.80f;

        [Header("Terrain Texturing")]
        public TerrainLayerSettings terrainLayerSettings = new();

        [Header("Biomes")]
        public List<BiomeDefinition> biomeDefinitions = new();

        [Header("Debug")]
        public bool autoGenerateOnStart = false;
        public bool logGenerationSteps = true;
        public DebugViewMode debugViewMode = DebugViewMode.None;

        public BiomeDefinition GetBiomeDefinition(BiomeType biomeType)
        {
            for (int i = 0; i < biomeDefinitions.Count; i++)
            {
                BiomeDefinition biome = biomeDefinitions[i];
                if (biome != null && biome.BiomeType == biomeType)
                {
                    return biome;
                }
            }

            return null;
        }
    }
}