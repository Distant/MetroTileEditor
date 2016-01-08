using System;

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
        [NonSerialized]
        public ColliderData colliderData;
        public BlockData()
        {
            materialIDs = new string[6];
            colliderData = new ColliderData();
        }
    }
}