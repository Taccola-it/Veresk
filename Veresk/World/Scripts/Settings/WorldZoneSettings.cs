using System;
using UnityEngine;

namespace Veresk.World.Settings
{
    [Serializable]
    public class WorldZoneSettings
    {
        [Header("Start Zone")]
        [Range(0f, 1f)] public float startZoneRadius01 = 0.18f;
        [Range(0f, 1f)] public float startZoneFlattening = 0.35f;
        [Range(0f, 1f)] public float startZoneHeightBias = 0.05f;

        [Header("Macro World Rings")]
        [Range(0f, 1f)] public float innerWorldRadius01 = 0.38f;
        [Range(0f, 1f)] public float midWorldRadius01 = 0.68f;
        [Range(0f, 1f)] public float outerWorldFadeStart01 = 0.82f;

        [Header("Future Biome Expansion")]
        public bool reserveOuterBandsForFutureBiomes = true;
    }
}