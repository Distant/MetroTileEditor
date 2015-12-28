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

        public MapSaveData(BlockData[,,] blockDataArray, int layer)
        {
            x = blockDataArray.GetLength(0);
            y = blockDataArray.GetLength(1);
            z = blockDataArray.GetLength(2);
            blockData1D = ArrayUtils.Array3DTo1D(blockDataArray, x, y, z);
            selectedLayer = layer;
        }

        public MapSaveData(BlockData[,,] blockDataArray, int x, int y, int z, int layer)
        {
            blockData1D = ArrayUtils.Array3DTo1D(blockDataArray, x, y, z);

            this.x = x;
            this.y = y;
            this.z = z;
            selectedLayer = layer;
        }

        public MapSaveData(BlockData[,,] blockDataArray, int x, int y, int z)
        {
            blockData1D = ArrayUtils.Array3DTo1D(blockDataArray, x, y, z);

            this.x = x;
            this.y = y;
            this.z = z;
            selectedLayer = 0;
        }

        public static MapSaveData Empty()
        {
            return new MapSaveData(new BlockData[1, 1, 1], 0);
        }

        public BlockData[,,] Get3DData()
        {
            int count = 0;
            for (int i = 0; i < blockData1D.Length; i++)
            {
                if (blockData1D[i] != null && blockData1D[i].placed)
                    count++;
            }
            Debug.Log("Get save data: " + count + " blocks placed");
            return ArrayUtils.Array1DTo3D(blockData1D, x, y, z);
        }
    }
}