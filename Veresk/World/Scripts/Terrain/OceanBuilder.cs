using UnityEngine;
using Veresk.World.Settings;

namespace Veresk.World.TerrainSystem
{
    public class OceanBuilder
    {
        private const string OceanObjectName = "Ocean";

        public GameObject BuildOrUpdateOcean(
            Transform parent,
            WorldSettings settings,
            Material oceanMaterial)
        {
            Transform existing = parent.Find(OceanObjectName);
            GameObject oceanObject;

            if (existing != null)
            {
                oceanObject = existing.gameObject;
            }
            else
            {
                oceanObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                oceanObject.name = OceanObjectName;
                oceanObject.transform.SetParent(parent, false);

                Collider collider = oceanObject.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }

            float sizeX = settings.terrainDimensions.terrainSizeX;
            float sizeZ = settings.terrainDimensions.terrainSizeZ;
            float seaY = settings.terrainDimensions.normalizedSeaLevel * settings.terrainDimensions.terrainHeight;

            oceanObject.transform.position = new Vector3(sizeX * 0.5f, seaY, sizeZ * 0.5f);
            oceanObject.transform.localScale = new Vector3(sizeX / 10f, 1f, sizeZ / 10f);

            MeshRenderer renderer = oceanObject.GetComponent<MeshRenderer>();
            if (renderer != null && oceanMaterial != null)
            {
                renderer.sharedMaterial = oceanMaterial;
            }

            return oceanObject;
        }
    }
}