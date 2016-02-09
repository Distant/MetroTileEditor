using System;
using UnityEngine;

namespace MetroTileEditor
{
    [Serializable]
    public class BlockData
    {
        public string blockType = "basic_cube";
        public bool placed;
        public string[] materialIDs; 
        public bool breakable;
        public bool isTriggerOnly;
        public bool excludeFromMesh;
        [Range(0, 3)]
        public int[] rotations;

        [NonSerialized]
        public ColliderData colliderData;
        public BlockData()
        {
            materialIDs = new string[6];
            colliderData = new ColliderData();
            rotations = new int[6];
        }

        public BlockData(string blockType)
        {
            this.blockType = blockType;
            materialIDs = new string[6];
            colliderData = new ColliderData();
            rotations = new int[6];
        }
    }
}