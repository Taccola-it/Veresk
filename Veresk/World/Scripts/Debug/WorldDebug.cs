using UnityEngine;
using Veresk.World.Biomes;
using Veresk.World.Core;

namespace Veresk.World.Debugging
{
    public enum DebugViewMode
    {
        None = 0,
        HeightMap = 1,
        IslandMask = 2,
        CoastMask = 3,
        InlandMap = 4,
        SlopeMap = 5,
        BiomeMap = 6,
        BiomeSuitability = 7
    }

    public class WorldDebugDisplay
    {
        private const string DebugPlaneName = "WorldDebugPlane";

        public void BuildOrUpdate(WorldData worldData, Transform parent, DebugViewMode mode)
        {
            Transform existing = parent.Find(DebugPlaneName);
            GameObject plane;

            if (existing == null)
            {
                plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.name = DebugPlaneName;
                plane.transform.SetParent(parent, false);

                Collider collider = plane.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }
            else
            {
                plane = existing.gameObject;
            }

            if (mode == DebugViewMode.None || worldData == null)
            {
                plane.SetActive(false);
                return;
            }

            plane.SetActive(true);

            Texture2D texture = BuildTexture(worldData, mode);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;

            MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
            Material mat = renderer.sharedMaterial;
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                renderer.sharedMaterial = mat;
            }

            mat.mainTexture = texture;

            int resolution = worldData.Resolution;
            float size = Mathf.Max(1f, resolution / 10f);

            plane.transform.localScale = new Vector3(size, 1f, size);
            plane.transform.localPosition = new Vector3(0f, 300f, 0f);
            plane.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        private Texture2D BuildTexture(WorldData worldData, DebugViewMode mode)
        {
            int resolution = worldData.Resolution;
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Color color = GetColor(worldData, x, y, mode);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private Color GetColor(WorldData worldData, int x, int y, DebugViewMode mode)
        {
            switch (mode)
            {
                case DebugViewMode.HeightMap:
                    return Grayscale(worldData.FinalHeightMap[x, y]);

                case DebugViewMode.IslandMask:
                    return Grayscale(worldData.IslandMask[x, y]);

                case DebugViewMode.CoastMask:
                    return Grayscale(worldData.CoastMask[x, y]);

                case DebugViewMode.InlandMap:
                    return Grayscale(worldData.InlandDistanceMap[x, y]);

                case DebugViewMode.SlopeMap:
                    return SlopeColor(worldData.SlopeMapDegrees[x, y]);

                case DebugViewMode.BiomeSuitability:
                    return Grayscale(worldData.BiomeSuitabilityMap[x, y]);

                case DebugViewMode.BiomeMap:
                    return BiomeColor(worldData.BiomeMap[x, y]);

                default:
                    return Color.black;
            }
        }

        private Color Grayscale(float value)
        {
            value = Mathf.Clamp01(value);
            return new Color(value, value, value, 1f);
        }

        private Color SlopeColor(float slopeDegrees)
        {
            float t = Mathf.InverseLerp(0f, 45f, slopeDegrees);
            return Color.Lerp(new Color(0.15f, 0.8f, 0.15f), new Color(0.8f, 0.15f, 0.15f), t);
        }

        private Color BiomeColor(BiomeType biomeType)
        {
            switch (biomeType)
            {
                case BiomeType.SacredGrove:
                    return new Color(0.35f, 0.65f, 0.35f);
                default:
                    return new Color(0.05f, 0.1f, 0.25f);
            }
        }
    }
}