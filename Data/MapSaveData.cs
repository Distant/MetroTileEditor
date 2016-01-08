using MetroTileEditor.Utils;
using System;
using UnityEngine;

namespace MetroTileEditor
{
    [Serializable]
    public class MapSaveData
    {
        public BlockData[] blockData1D;
        public int x, y, z;
        public int selectedLayer;

        public MapSaveData(BlockDataArray blockDataArray, int layer)
        {
            blockData1D = blockDataArray.Data;
            selectedLayer = layer;
        }

        public MapSaveData(BlockDataArray blockDataArray, int x, int y, int z, int layer)
        {
            blockData1D = blockDataArray.Data;
            this.x = x;
            this.y = y;
            this.z = z;
            selectedLayer = layer;
        }

        public MapSaveData(BlockDataArray blockDataArray, int x, int y, int z)
        {
            blockData1D = blockDataArray.Data;

            this.x = x;
            this.y = y;
            this.z = z;
            selectedLayer = 0;
        }

        public static MapSaveData Empty()
        {
            return new MapSaveData(new BlockDataArray(1,1,1), 0);
        }

        public BlockDataArray Get3DData()
        {
            BlockDataArray blockData = new BlockDataArray(x, y, z);
            blockData.Data = blockData1D;
            return blockData;
        }
    }
}