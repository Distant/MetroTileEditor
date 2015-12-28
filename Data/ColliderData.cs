using System;
using UnityEngine;

namespace MetroTileEditor
{
    [Serializable]
    public class ColliderData
    {
        // Colliders
        public Vector3[] orientations;
        public ColliderType[] types;
        public int[] vertCount;
        public Vector2[] verts;
        public Vector2[] dimensions;

        public enum ColliderType
        {
            Box,
            Edge,
            Polygon
        }
    }
}