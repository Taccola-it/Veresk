using UnityEngine;
using Veresk.World.Settings;

namespace Veresk.World.Generation
{
    public class CoastMaskBuilder
    {
        public float[,] Build(WorldSettings settings, float[,] heightMap)
        {
            int resolution = heightMap.GetLength(0);
            float[,] coastMap = new float[resolution, resolution];
            float seaLevel = settings.terrainDimensions.normalizedSeaLevel;
            float coastWidth = settings.coastalBandWidth;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float h = heightMap[x, y];
                    float distanceFromSea = Mathf.Abs(h - seaLevel);

                    float coast = 1f - Mathf.Clamp01(distanceFromSea / Mathf.Max(0.0001f, coastWidth));
                    coastMap[x, y] = Mathf.Clamp01(coast);
                }
            }

            return Blur(coastMap, 2);
        }

        public float[,] BuildInlandDistance(WorldSettings settings, float[,] heightMap)
        {
            int resolution = heightMap.GetLength(0);
            float[,] inlandMap = new float[resolution, resolution];
            float seaLevel = settings.terrainDimensions.normalizedSeaLevel;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float h = heightMap[x, y];

                    if (h <= seaLevel)
                    {
                        inlandMap[x, y] = 0f;
                    }
                    else
                    {
                        float value = Mathf.InverseLerp(seaLevel, 1f, h);
                        inlandMap[x, y] = value;
                    }
                }
            }

            return Blur(inlandMap, 3);
        }

        private float[,] Blur(float[,] source, int radius)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            float[,] result = new float[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sum = 0f;
                    int count = 0;

                    for (int oy = -radius; oy <= radius; oy++)
                    {
                        for (int ox = -radius; ox <= radius; ox++)
                        {
                            int sx = Mathf.Clamp(x + ox, 0, width - 1);
                            int sy = Mathf.Clamp(y + oy, 0, height - 1);

                            sum += source[sx, sy];
                            count++;
                        }
                    }

                    result[x, y] = count > 0 ? sum / count : source[x, y];
                }
            }

            return result;
        }
    }

    public class SlopeMapBuilder
    {
        public float[,] Build(WorldSettings settings, float[,] heightMap)
        {
            int resolution = heightMap.GetLength(0);
            float[,] slopeMap = new float[resolution, resolution];

            float terrainSizeX = settings.terrainDimensions.terrainSizeX;
            float terrainSizeZ = settings.terrainDimensions.terrainSizeZ;
            float terrainHeight = settings.terrainDimensions.terrainHeight;

            float stepX = terrainSizeX / (resolution - 1);
            float stepZ = terrainSizeZ / (resolution - 1);

            for (int y = 1; y < resolution - 1; y++)
            {
                for (int x = 1; x < resolution - 1; x++)
                {
                    float hL = heightMap[x - 1, y] * terrainHeight;
                    float hR = heightMap[x + 1, y] * terrainHeight;
                    float hD = heightMap[x, y - 1] * terrainHeight;
                    float hU = heightMap[x, y + 1] * terrainHeight;

                    float dx = (hR - hL) / (2f * stepX);
                    float dz = (hU - hD) / (2f * stepZ);

                    float slopeRadians = Mathf.Atan(Mathf.Sqrt(dx * dx + dz * dz));
                    float slopeDegrees = slopeRadians * Mathf.Rad2Deg;

                    slopeMap[x, y] = slopeDegrees;
                }
            }

            CopyEdges(slopeMap);
            return slopeMap;
        }

        public float[,] SoftenSteepAreas(WorldSettings settings, float[,] heightMap, float[,] slopeMap)
        {
            int resolution = heightMap.GetLength(0);
            float[,] result = new float[resolution, resolution];

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    result[x, y] = heightMap[x, y];
                }
            }

            float strength = settings.slopeSofteningStrength;
            if (strength <= 0f)
                return result;

            for (int y = 1; y < resolution - 1; y++)
            {
                for (int x = 1; x < resolution - 1; x++)
                {
                    float slope = slopeMap[x, y];
                    float softFactor = Mathf.InverseLerp(18f, 40f, slope) * strength;

                    if (softFactor <= 0f)
                        continue;

                    float average =
                        result[x, y] +
                        result[x - 1, y] +
                        result[x + 1, y] +
                        result[x, y - 1] +
                        result[x, y + 1];

                    average /= 5f;

                    result[x, y] = Mathf.Lerp(result[x, y], average, softFactor);
                }
            }

            return result;
        }

        private void CopyEdges(float[,] map)
        {
            int resolution = map.GetLength(0);

            for (int i = 0; i < resolution; i++)
            {
                map[i, 0] = map[i, 1];
                map[i, resolution - 1] = map[i, resolution - 2];
                map[0, i] = map[1, i];
                map[resolution - 1, i] = map[resolution - 2, i];
            }
        }
    }

    public class ShorelineHeightProcessor
    {
        public float[,] Smooth(WorldSettings settings, float[,] heightMap, float[,] coastMask)
        {
            int resolution = heightMap.GetLength(0);
            float[,] result = new float[resolution, resolution];

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    result[x, y] = heightMap[x, y];
                }
            }

            float strength = settings.coastSmoothingStrength;
            if (strength <= 0f)
            {
                return result;
            }

            for (int y = 1; y < resolution - 1; y++)
            {
                for (int x = 1; x < resolution - 1; x++)
                {
                    float coast = coastMask[x, y];
                    if (coast <= 0.01f)
                    {
                        continue;
                    }

                    float average =
                        result[x, y] +
                        result[x - 1, y] +
                        result[x + 1, y] +
                        result[x, y - 1] +
                        result[x, y + 1] +
                        result[x - 1, y - 1] +
                        result[x + 1, y - 1] +
                        result[x - 1, y + 1] +
                        result[x + 1, y + 1];

                    average /= 9f;

                    float blend = coast * strength;
                    result[x, y] = Mathf.Lerp(result[x, y], average, blend);
                }
            }

            return result;
        }
    }
}