using System.Diagnostics;
using UnityEngine;
using Veresk.World.Debugging;
using Veresk.World.Settings;
using Veresk.World.TerrainSystem;

namespace Veresk.World.Core
{
    public class WorldGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WorldSettings worldSettings;
        [SerializeField] private Material oceanMaterial;

        [Header("Runtime State")]
        [SerializeField] private int lastGeneratedSeed;
        [SerializeField] private bool hasGenerated;

        private readonly TerrainBuilder terrainBuilder = new();
        private readonly OceanBuilder oceanBuilder = new();
        private readonly WorldDebugDisplay worldDebugDisplay = new();
        private readonly WorldGenerationPipeline pipeline = new();

        public WorldSettings Settings => worldSettings;
        public int LastGeneratedSeed => lastGeneratedSeed;
        public bool HasGenerated => hasGenerated;

        private void Start()
        {
            if (worldSettings != null && worldSettings.autoGenerateOnStart)
            {
                GenerateWorld();
            }
        }

        [ContextMenu("Generate World")]
        public void GenerateWorld()
        {
            if (worldSettings == null)
            {
                Debug.LogError("WorldGenerator: WorldSettings is not assigned.");
                return;
            }

            int seed = worldSettings.useRandomSeedOnGenerate
                ? Random.Range(int.MinValue, int.MaxValue)
                : worldSettings.seed;

            GenerateWorld(seed);
        }

        public void GenerateWorld(int seed)
        {
            if (worldSettings == null)
            {
                Debug.LogError("WorldGenerator: WorldSettings is not assigned.");
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            WorldData worldData = pipeline.Generate(worldSettings, seed);

            terrainBuilder.BuildOrUpdate(transform, worldSettings, worldData);
            oceanBuilder.BuildOrUpdateOcean(transform, worldSettings, oceanMaterial);
            worldDebugDisplay.BuildOrUpdate(worldData, transform, worldSettings.debugViewMode);

            lastGeneratedSeed = seed;
            hasGenerated = true;

            stopwatch.Stop();

            if (worldSettings.logGenerationSteps)
            {
                Debug.Log(
                    $"World generated successfully. Seed={seed}, Resolution={worldData.Resolution}, Time={stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
}