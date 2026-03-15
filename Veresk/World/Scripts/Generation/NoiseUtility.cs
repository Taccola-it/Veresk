using UnityEngine;
using Veresk.World.Core;
using Veresk.World.Settings;

namespace Veresk.World.Generation
{
    public static class NoiseUtility
    {
        public static float FractalNoise(
            float x,
            float y,
            int seed,
            NoiseSettings settings,
            int salt = 0)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float value = 0f;
            float amplitudeSum = 0f;

            int noiseSeed = SeedUtility.Combine(seed, salt);

            float sampleX = x;
            float sampleY = y;

            if (settings.useDomainWarp)
            {
                float warpX = Mathf.PerlinNoise(
                    (x / settings.warpScale) + 0.123f + noiseSeed * 0.0001f,
                    (y / settings.warpScale) + 0.456f + noiseSeed * 0.0001f);

                float warpY = Mathf.PerlinNoise(
                    (x / settings.warpScale) + 0.789f + noiseSeed * 0.0001f,
                    (y / settings.warpScale) + 0.321f + noiseSeed * 0.0001f);

                sampleX += (warpX - 0.5f) * settings.warpStrength;
                sampleY += (warpY - 0.5f) * settings.warpStrength;
            }

            for (int i = 0; i < settings.octaves; i++)
            {
                float px = ((sampleX + settings.offset.x) / settings.baseScale) * frequency;
                float py = ((sampleY + settings.offset.y) / settings.baseScale) * frequency;

                float perlin = Mathf.PerlinNoise(
                    px + noiseSeed * 0.0001f,
                    py + noiseSeed * 0.0001f);

                value += perlin * amplitude;
                amplitudeSum += amplitude;

                amplitude *= settings.persistence;
                frequency *= settings.lacunarity;
            }

            if (amplitudeSum <= 0f)
                return 0f;

            return (value / amplitudeSum) * settings.amplitude;
        }
    }
}