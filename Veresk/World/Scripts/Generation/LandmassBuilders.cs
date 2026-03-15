using UnityEngine;
using Veresk.World.Core;
using Veresk.World.Settings;

namespace Veresk.World.Generation
{
    public class StartIslandBuilder
    {
        public float[,] Build(WorldSettings settings, int seed, int resolution)
        {
            float[,] map = new float[resolution, resolution];

            float center = (resolution - 1) * 0.5f;
            float maxDistance = center;

            float radius = 0.22f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x - center) / maxDistance;
                    float ny = (y - center) / maxDistance;

                    float radial01 = Mathf.Clamp01(Mathf.Sqrt(nx * nx + ny * ny));
                    float islandCore = 1f - Mathf.InverseLerp(0f, radius, radial01);
                    islandCore = Mathf.Clamp01(islandCore);

                    float shapeNoise = NoiseUtility.FractalNoise(
                        x,
                        y,
                        seed,
                        settings.mediumNoise,
                        7001);

                    shapeNoise = Mathf.Lerp(0.82f, 1.15f, shapeNoise);

                    float value = Mathf.Pow(islandCore, 1.35f) * shapeNoise;
                    map[x, y] = Mathf.Clamp01(value);
                }
            }

            return map;
        }
    }

    public class ContinentMapBuilder
    {
        public float[,] Build(WorldSettings settings, int seed, int resolution)
        {
            float[,] map = new float[resolution, resolution];

            float center = (resolution - 1) * 0.5f;
            float maxDistance = center;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x - center) / maxDistance;
                    float ny = (y - center) / maxDistance;

                    float radial = Mathf.Sqrt(nx * nx + ny * ny);

                    float noiseA = NoiseUtility.FractalNoise(
                        x, y, seed, settings.macroNoise, 3001);

                    float noiseB = NoiseUtility.FractalNoise(
                        x, y, seed, settings.macroNoise, 3002);

                    float continents = Mathf.Max(noiseA, noiseB);

                    float radialFalloff = 1f - Mathf.InverseLerp(0.65f, 1f, radial);

                    float value = continents * radialFalloff;

                    map[x, y] = Mathf.Clamp01(value);
                }
            }

            return map;
        }
    }

    public class ArchipelagoClusterBuilder
    {
        public float[,] Build(WorldSettings settings, int seed, int resolution)
        {
            float[,] map = new float[resolution, resolution];

            float center = (resolution - 1) * 0.5f;
            float maxDistance = center;

            int clusterCount = 9;

            Vector2[] centers = new Vector2[clusterCount];
            float[] radii = new float[clusterCount];

            for (int i = 0; i < clusterCount; i++)
            {
                int clusterSeed = SeedUtility.Combine(seed, $"archipelago_cluster_{i}");
                centers[i] = GetClusterCenter(clusterSeed, i == 0 ? 0.14f : 0.26f, i == 0 ? 0.24f : 0.62f);
                radii[i] = GetClusterRadius(clusterSeed, i == 0 ? 0.20f : 0.14f, i == 0 ? 0.28f : 0.24f);
            }

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x - center) / maxDistance;
                    float ny = (y - center) / maxDistance;
                    Vector2 p = new Vector2(nx, ny);

                    float best = 0f;

                    for (int i = 0; i < clusterCount; i++)
                    {
                        float d = Vector2.Distance(p, centers[i]);
                        float t = 1f - Mathf.Clamp01(d / Mathf.Max(0.0001f, radii[i]));
                        t = Mathf.Pow(t, i == 0 ? 1.4f : 1.8f);

                        if (t > best)
                            best = t;
                    }

                    float breakupNoise = NoiseUtility.FractalNoise(
                        x, y, seed, settings.mediumNoise, SeedUtility.Combine(seed, "archipelago_breakup"));

                    breakupNoise = Mathf.Lerp(0.55f, 1.30f, breakupNoise);

                    map[x, y] = Mathf.Clamp01(best * breakupNoise);
                }
            }

            return map;
        }

        private Vector2 GetClusterCenter(int seed, float minRadius, float maxRadius)
        {
            System.Random rng = SeedUtility.CreateRandom(seed);

            float angle = Mathf.Lerp(0f, Mathf.PI * 2f, (float)rng.NextDouble());
            float radius = Mathf.Lerp(minRadius, maxRadius, (float)rng.NextDouble());

            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private float GetClusterRadius(int seed, float min, float max)
        {
            System.Random rng = SeedUtility.CreateRandom(seed + 111);
            return Mathf.Lerp(min, max, (float)rng.NextDouble());
        }
    }

    public class OceanChannelBuilder
    {
        public float[,] Build(WorldSettings settings, int seed, int resolution)
        {
            float[,] map = new float[resolution, resolution];

            float center = (resolution - 1) * 0.5f;
            float maxDistance = center;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x - center) / maxDistance;
                    float ny = (y - center) / maxDistance;

                    float angle = Mathf.Atan2(ny, nx);
                    float radial = Mathf.Sqrt(nx * nx + ny * ny);

                    float lineNoise = NoiseUtility.FractalNoise(
                        x, y, seed, settings.macroNoise, 8301);

                    float lineNoiseB = NoiseUtility.FractalNoise(
                        x, y, seed, settings.mediumNoise, 8302);

                    float warpedAngle = angle
                        + (lineNoise - 0.5f) * 0.9f
                        + (lineNoiseB - 0.5f) * 0.45f;

                    float spokeA = Mathf.Abs(Mathf.Sin(warpedAngle * 2.0f));
                    float spokeB = Mathf.Abs(Mathf.Cos(warpedAngle * 3.0f));
                    float channels = Mathf.Min(spokeA, spokeB);

                    float channelWidth = Mathf.Lerp(0.15f, 0.35f, lineNoise);
                    float channelMask = 1f - Mathf.InverseLerp(0f, channelWidth, channels);

                    float radialMask = Mathf.InverseLerp(0.12f, 0.95f, radial);
                    float value = channelMask * radialMask;

                    map[x, y] = Mathf.Clamp01(value);
                }
            }

            return map;
        }
    }

    public class MacroLandmassBuilder
    {
        public float[,] Build(WorldSettings settings, int seed, int resolution)
        {
            float[,] map = new float[resolution, resolution];

            float center = (resolution - 1) * 0.5f;
            float maxDistance = center;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x - center) / maxDistance;
                    float ny = (y - center) / maxDistance;

                    float radial = Mathf.Sqrt(nx * nx + ny * ny);

                    float noiseA = NoiseUtility.FractalNoise(
                        x, y, seed, settings.macroNoise, 4101);

                    float noiseB = NoiseUtility.FractalNoise(
                        x, y, seed, settings.mediumNoise, 4102);

                    float noiseC = NoiseUtility.FractalNoise(
                        x, y, seed, settings.mediumNoise, 4103);

                    float archipelago =
                        noiseA * 0.6f +
                        noiseB * 0.3f +
                        noiseC * 0.1f;

                    float islandFalloff = 1f - Mathf.Clamp01(radial * 0.85f);

                    float breakNoise = NoiseUtility.FractalNoise(
                        x, y, seed, settings.macroNoise, 9999);

                    breakNoise = Mathf.Lerp(0.55f, 1.25f, breakNoise);

                    float value = archipelago * islandFalloff * breakNoise;

                    map[x, y] = Mathf.Clamp01(value);
                }
            }

            return map;
        }
    }

    public class IslandMaskBuilder
    {
        public float[,] Build(
            WorldSettings settings,
            int seed,
            int resolution,
            float[,] macroLandmassMap)
        {
            float[,] mask = new float[resolution, resolution];

            float center = (resolution - 1) * 0.5f;
            float maxDistance = center;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x - center) / maxDistance;
                    float ny = (y - center) / maxDistance;

                    float radialDistance = Mathf.Sqrt(nx * nx + ny * ny);
                    float radial01 = Mathf.Clamp01(radialDistance);

                    float angle = Mathf.Atan2(ny, nx);

                    float angularVariation =
                        NoiseUtility.FractalNoise(
                            x,
                            y,
                            seed,
                            settings.macroNoise,
                            SeedUtility.Combine(seed, "island_angle_noise"));

                    angularVariation = (angularVariation - 0.5f) * 0.35f;

                    float directionalWave =
                        Mathf.Sin(angle * 3f + angularVariation * 2f) * 0.04f +
                        Mathf.Cos(angle * 5f - angularVariation * 3f) * 0.025f;

                    float distortedDistance = radial01 + directionalWave;

                    float core = 1f - distortedDistance;
                    core = Mathf.Clamp01(core + settings.islandCoreBias);

                    float radialFalloff = Mathf.Pow(core, Mathf.Max(0.01f, settings.islandFalloffStrength));

                    float macro = macroLandmassMap != null ? macroLandmassMap[x, y] : 0f;
                    float combined = Mathf.Max(radialFalloff * 0.7f, macro);

                    float outerFadeStart = settings.worldZoneSettings.outerWorldFadeStart01;
                    float outerFade = 1f - Mathf.InverseLerp(outerFadeStart, 1f, radial01);
                    combined *= outerFade;

                    mask[x, y] = Mathf.Clamp01(combined);
                }
            }

            return mask;
        }
    }
}