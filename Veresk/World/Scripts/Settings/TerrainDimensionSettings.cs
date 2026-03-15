using System;
using UnityEngine;

namespace Veresk.World.Settings
{
    [Serializable]
    public class TerrainDimensionSettings
    {
        [Header("Heightmap")]
        [Min(33)] public int heightmapResolution = 513;

        [Header("Alphamap")]
        [Min(16)] public int alphamapResolution = 256;

        [Header("Detail Resolution")]
        [Min(16)] public int detailResolution = 256;

        [Header("Terrain Size")]
        [Min(64f)] public float terrainSizeX = 2048f;
        [Min(64f)] public float terrainSizeZ = 2048f;
        [Min(32f)] public float terrainHeight = 240f;

        [Header("Water Level")]
        [Range(0f, 1f)] public float normalizedSeaLevel = 0.38f;
    }
}