using UnityEngine;

namespace Veresk.World.Core
{
    public class TerrainDeformationHook
    {
        public float[,] ApplyRuntimeModifications(WorldData worldData)
        {
            if (worldData == null || worldData.BaseHeightMap == null)
                return null;

            int resolution = worldData.BaseHeightMap.GetLength(0);
            float[,] result = new float[resolution, resolution];

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    result[x, y] = worldData.BaseHeightMap[x, y];
                }
            }

            var deltas = worldData.TerrainModificationData.HeightDeltas;
            for (int i = 0; i < deltas.Count; i++)
            {
                var d = deltas[i];
                if (d.x < 0 || d.x >= resolution || d.y < 0 || d.y >= resolution)
                    continue;

                result[d.x, d.y] = Mathf.Clamp01(result[d.x, d.y] + d.delta);
            }

            return result;
        }
    }
}