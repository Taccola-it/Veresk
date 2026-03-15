using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veresk.World.Core
{
    [Serializable]
    public class TerrainModificationData
    {
        [Serializable]
        public struct HeightDelta
        {
            public int x;
            public int y;
            public float delta;
        }

        [SerializeField] private List<HeightDelta> heightDeltas = new List<HeightDelta>();

        public IReadOnlyList<HeightDelta> HeightDeltas => heightDeltas;

        public void Clear()
        {
            heightDeltas.Clear();
        }

        public void AddDelta(int x, int y, float delta)
        {
            HeightDelta entry = new HeightDelta
            {
                x = x,
                y = y,
                delta = delta
            };

            heightDeltas.Add(entry);
        }
    }
}