using Veresk.World.Settings;
using UnityEngine;

namespace Veresk.World.Generation
{
    public enum WorldRingType
    {
        None = 0,
        StartZone = 1,
        InnerWorld = 2,
        MidWorld = 3,
        OuterWorld = 4,
        Edge = 5
    }

    public class WorldDistanceMapBuilder
    {
        public float[,] Build(int resolution)
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

                    float radial01 = Mathf.Clamp01(Mathf.Sqrt(nx * nx + ny * ny));
                    map[x, y] = radial01;
                }
            }

            return map;
        }
    }

    public class WorldRingMapBuilder
    {
        public WorldRingType[,] Build(WorldSettings settings, float[,] distanceMap)
        {
            int resolution = distanceMap.GetLength(0);
            WorldRingType[,] ringMap = new WorldRingType[resolution, resolution];

            float startRadius = settings.worldZoneSettings.startZoneRadius01;
            float innerRadius = settings.worldZoneSettings.innerWorldRadius01;
            float midRadius = settings.worldZoneSettings.midWorldRadius01;
            float outerFade = settings.worldZoneSettings.outerWorldFadeStart01;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float d = distanceMap[x, y];

                    if (d <= startRadius)
                    {
                        ringMap[x, y] = WorldRingType.StartZone;
                    }
                    else if (d <= innerRadius)
                    {
                        ringMap[x, y] = WorldRingType.InnerWorld;
                    }
                    else if (d <= midRadius)
                    {
                        ringMap[x, y] = WorldRingType.MidWorld;
                    }
                    else if (d <= outerFade)
                    {
                        ringMap[x, y] = WorldRingType.OuterWorld;
                    }
                    else
                    {
                        ringMap[x, y] = WorldRingType.Edge;
                    }
                }
            }

            return ringMap;
        }
    }
}