using System;
using UnityEngine;

namespace Veresk.World.Settings
{
    [Serializable]
    public class TerrainLayerSettings
    {
        [Header("Layer Names")]
        public string underwaterLayerName = "Underwater";
        public string coastLayerName = "Coast";
        public string lowlandLayerName = "Lowland";
        public string grasslandLayerName = "Grassland";
        public string uplandLayerName = "Upland";

        [Header("Height Bands")]
        [Range(0f, 1f)] public float coastHeightBlendRange = 0.03f;
        [Range(0f, 1f)] public float lowlandStart01 = 0.40f;
        [Range(0f, 1f)] public float grasslandStart01 = 0.48f;
        [Range(0f, 1f)] public float uplandStart01 = 0.62f;

        [Header("Slope Influence")]
        [Range(0f, 1f)] public float slopeToUplandInfluence = 0.35f;

        [Header("Coast Influence")]
        [Range(0f, 1f)] public float coastTextureInfluence = 0.75f;
    }
}