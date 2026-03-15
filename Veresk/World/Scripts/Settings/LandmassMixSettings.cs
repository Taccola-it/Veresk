using System;
using UnityEngine;

namespace Veresk.World.Settings
{
    [Serializable]
    public class LandmassMixSettings
    {
        [Header("Land Source Weights")]
        [Range(0f, 1f)] public float macroLandmassWeight = 0.20f;
        [Range(0f, 1f)] public float continentWeight = 0.20f;
        [Range(0f, 1f)] public float archipelagoClusterWeight = 0.60f;

        [Header("Ocean Channel Cutting")]
        [Range(0f, 1f)] public float minChannelMultiplier = 0.05f;

        [Header("Start Island Reinforcement")]
        [Range(0f, 2f)] public float startIslandMacroBlend = 0.98f;
        [Range(0f, 2f)] public float startIslandMaskBlend = 0.94f;
    }
}