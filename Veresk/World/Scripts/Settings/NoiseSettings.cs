using System;
using UnityEngine;

namespace Veresk.World.Settings
{
    [Serializable]
    public class NoiseSettings
    {
        [Header("Base Noise")]
        [Min(0.0001f)] public float baseScale = 180f;
        [Range(1, 12)] public int octaves = 4;
        [Range(0f, 1f)] public float persistence = 0.5f;
        [Min(1f)] public float lacunarity = 2f;

        [Header("Offsets")]
        public Vector2 offset = Vector2.zero;

        [Header("Weight")]
        [Range(0f, 3f)] public float amplitude = 1f;

        [Header("Domain Warp")]
        public bool useDomainWarp = false;
        [Min(0.0001f)] public float warpScale = 90f;
        [Range(0f, 200f)] public float warpStrength = 20f;
    }
}