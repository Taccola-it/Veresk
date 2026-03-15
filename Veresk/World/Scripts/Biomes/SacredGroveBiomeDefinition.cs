using UnityEngine;
using Veresk.World.Generation;

namespace Veresk.World.Biomes
{
    [CreateAssetMenu(
        fileName = "SacredGroveBiomeDefinition",
        menuName = "Veresk/World/Biomes/Sacred Grove Definition")]
    public class SacredGroveBiomeDefinition : BiomeDefinition
    {
        [Header("Sacred Grove Tuning")]
        [Range(0f, 1f)] public float preferredLowlandHeight = 0.48f;
        [Range(0f, 1f)] public float lowlandBias = 0.18f;
        [Range(0f, 1f)] public float coastFriendliness = 0.15f;

        [Header("World Radius Limit")]
        [Range(0f, 1f)] public float maxWorldRadius01 = 0.486f;
        [Range(0f, 1f)] public float fadeStartRadius01 = 0.40f;

        public override float EvaluateRingAffinity(WorldRingType ringType)
        {
            switch (ringType)
            {
                case WorldRingType.StartZone:
                    return 1f;
                case WorldRingType.InnerWorld:
                    return 0.95f;
                case WorldRingType.MidWorld:
                    return 0.75f;
                case WorldRingType.OuterWorld:
                    return 0f;
                default:
                    return 0f;
            }
        }

        public override float EvaluateWorldRadiusAffinity(float worldRadius01)
        {
            if (worldRadius01 >= maxWorldRadius01)
                return 0f;

            if (worldRadius01 <= fadeStartRadius01)
                return 1f;

            float t = Mathf.InverseLerp(fadeStartRadius01, maxWorldRadius01, worldRadius01);
            return Mathf.Lerp(1f, 0.15f, t);
        }

        public override float EvaluateSuitability(
            float normalizedHeight,
            float slopeDegrees,
            float inlandness01,
            float coastFactor01)
        {
            float baseScore = base.EvaluateSuitability(
                normalizedHeight,
                slopeDegrees,
                inlandness01,
                coastFactor01);

            if (baseScore <= 0f)
                return 0f;

            float lowlandAffinity = 1f - Mathf.Abs(normalizedHeight - preferredLowlandHeight);
            float gentleCoastBonus = 1f - Mathf.Clamp01(coastFactor01);

            float result =
                baseScore +
                (lowlandAffinity * lowlandBias) +
                (gentleCoastBonus * coastFriendliness * 0.5f);

            return Mathf.Clamp01(result);
        }
    }
}