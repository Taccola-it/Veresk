using UnityEngine;
using Veresk.World.Settings;

namespace Veresk.World.Generation
{
    public class HeightMapBuilder
    {
        public float[,] Build(WorldSettings settings, int seed, int resolution, float[,] islandMask)
        {
            float[,] map = new float[resolution, resolution];
            float seaLevel = settings.terrainDimensions.normalizedSeaLevel;

            float center = (resolution - 1) * 0.5f;
            float maxDistance = center;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x - center) / maxDistance;
                    float ny = (y - center) / maxDistance;
                    float radial01 = Mathf.Clamp01(Mathf.Sqrt(nx * nx + ny * ny));

                    float macro = NoiseUtility.FractalNoise(
                        x, y, seed, settings.macroNoise, 101);

                    float medium = NoiseUtility.FractalNoise(
                        x, y, seed, settings.mediumNoise, 202);

                    float detail = NoiseUtility.FractalNoise(
                        x, y, seed, settings.detailNoise, 303);

                    float zoneHeightMultiplier = EvaluateZoneHeightMultiplier(settings, radial01);

                    float combined =
                        (macro * settings.macroWeight) +
                        (medium * settings.mediumWeight) +
                        (detail * settings.detailWeight);

                    combined *= zoneHeightMultiplier;
                    combined += settings.heightBias;
                    combined = Mathf.Clamp01(combined);
                    combined = Mathf.Pow(combined, Mathf.Max(0.01f, settings.heightCurvePower));

                    float island = islandMask[x, y];

                    float landHeight = Mathf.Lerp(seaLevel - 0.02f, 0.82f, combined);
                    float maskedHeight = Mathf.Lerp(seaLevel - 0.06f, landHeight, island);

                    float startZoneFactor = EvaluateStartZoneFactor(settings, radial01);
                    float safeHeight = Mathf.Lerp(seaLevel + 0.03f, seaLevel + 0.16f, combined);
                    maskedHeight = Mathf.Lerp(maskedHeight, safeHeight, startZoneFactor * settings.worldZoneSettings.startZoneFlattening);

                    maskedHeight += startZoneFactor * settings.worldZoneSettings.startZoneHeightBias * 0.05f;

                    float flattening = settings.playableFlattening;
                    float flattened = Mathf.Lerp(maskedHeight, SmoothPlayableHeight(maskedHeight, seaLevel), flattening);

                    map[x, y] = Mathf.Clamp01(flattened);
                }
            }

            return map;
        }

        private float SmoothPlayableHeight(float value, float seaLevel)
        {
            if (value <= seaLevel)
                return value;

            float aboveSea = value - seaLevel;
            float softened = Mathf.Pow(aboveSea, 0.85f);
            return seaLevel + softened;
        }

        private float EvaluateStartZoneFactor(WorldSettings settings, float radial01)
        {
            float radius = Mathf.Max(0.0001f, settings.worldZoneSettings.startZoneRadius01);
            float value = 1f - Mathf.InverseLerp(0f, radius, radial01);
            return Mathf.Clamp01(value);
        }

        private float EvaluateZoneHeightMultiplier(WorldSettings settings, float radial01)
        {
            float inner = settings.worldZoneSettings.innerWorldRadius01;
            float mid = settings.worldZoneSettings.midWorldRadius01;

            if (radial01 <= inner)
                return 0.95f;

            if (radial01 <= mid)
                return 1.00f;

            float outerT = Mathf.InverseLerp(mid, 1f, radial01);
            return Mathf.Lerp(1.0f, 0.82f, outerT);
        }
    }
}