using MetroTileEditor.Utils;
using System;
using UnityEngine;

namespace MetroTileEditor
{
    [Serializable]
    public class MapSaveData
    {
        public BlockData[] blockData1D;
        public int width, height, depth;
        public int selectedLayer;

        public MapSaveData(BlockDataArray blockDataArray, int layer)
        {
            blockData1D = blockDataArray.Data;
            selectedLayer = layer;
        }

        public MapSaveData(BlockDataArray blockDataArray, Index size, int layer)
        {
            blockData1D = blockDataArray.Data;
            this.width = size.x;
            this.height = size.y;
            this.depth = size.z;
            selectedLayer = layer;
        }

        public MapSaveData(BlockDataArray blockDataArray, Index size)
        {
            blockData1D = blockDataArray.Data;

            this.width = size.x;
            this.height = size.y;
            this.depth = size.z;
            selectedLayer = 0;
        }

        public static MapSaveData Empty()
        {
            return new MapSaveData(new BlockDataArray(1, 1, 1), 0);
        }

        public BlockDataArray Get3DData()
        {
            BlockDataArray blockData = new BlockDataArray(width, height, depth);
            blockData.Data = blockData1D;
            return blockData;
        }
    }
}