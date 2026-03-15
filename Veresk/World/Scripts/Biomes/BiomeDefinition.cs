using UnityEngine;
using Veresk.World.Generation;

namespace Veresk.World.Biomes
{
    public abstract class BiomeDefinition : ScriptableObject
    {
        [Header("Common")]
        [SerializeField] private BiomeType biomeType = BiomeType.None;
        [SerializeField] private string biomeId = "biome";
        [SerializeField] private string displayName = "Biome";

        [Header("Height Rules")]
        [Range(0f, 1f)][SerializeField] private float minNormalizedHeight = 0.38f;
        [Range(0f, 1f)][SerializeField] private float maxNormalizedHeight = 0.75f;

        [Header("Slope Rules")]
        [Range(0f, 90f)][SerializeField] private float maxSlopeDegrees = 24f;

        [Header("Coast Rules")]
        [Range(0f, 1f)][SerializeField] private float minCoastDistance01 = 0.0f;
        [Range(0f, 1f)][SerializeField] private float preferredInlandness01 = 0.5f;

        [Header("Placement Weight")]
        [Range(0f, 10f)][SerializeField] private float suitabilityWeight = 1f;

        public BiomeType BiomeType => biomeType;
        public string BiomeId => biomeId;
        public string DisplayName => displayName;
        public float MinNormalizedHeight => minNormalizedHeight;
        public float MaxNormalizedHeight => maxNormalizedHeight;
        public float MaxSlopeDegrees => maxSlopeDegrees;
        public float MinCoastDistance01 => minCoastDistance01;
        public float PreferredInlandness01 => preferredInlandness01;
        public float SuitabilityWeight => suitabilityWeight;

        public virtual float EvaluateSuitability(
            float normalizedHeight,
            float slopeDegrees,
            float inlandness01,
            float coastFactor01)
        {
            if (normalizedHeight < minNormalizedHeight || normalizedHeight > maxNormalizedHeight)
                return 0f;

            if (slopeDegrees > maxSlopeDegrees)
                return 0f;

            if (inlandness01 < minCoastDistance01)
                return 0f;

            float heightMid = (minNormalizedHeight + maxNormalizedHeight) * 0.5f;
            float heightHalfRange = Mathf.Max(0.001f, (maxNormalizedHeight - minNormalizedHeight) * 0.5f);
            float heightScore = 1f - Mathf.Clamp01(Mathf.Abs(normalizedHeight - heightMid) / heightHalfRange);

            float slopeScore = 1f - Mathf.Clamp01(slopeDegrees / Mathf.Max(0.001f, maxSlopeDegrees));
            float inlandScore = 1f - Mathf.Abs(inlandness01 - preferredInlandness01);

            float combined = (heightScore * 0.45f) + (slopeScore * 0.35f) + (inlandScore * 0.20f);
            return Mathf.Clamp01(combined * suitabilityWeight);
        }

        public virtual float EvaluateWorldRadiusAffinity(float worldRadius01)
        {
            return 1f;
        }

        public virtual float EvaluateRingAffinity(WorldRingType ringType)
        {
            return 1f;
        }
    }
}