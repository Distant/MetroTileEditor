using System;
using UnityEngine;

namespace MetroTileEditor
{
    [Serializable]
    public class BlockDataArray
    {
        [SerializeField]
        private BlockData[] dataArray;
        public BlockData[] Data { get { return dataArray; } set { dataArray = value; Clean(); } }

        [SerializeField]
        private int width;
        public int Width { get { return width; } }

        [SerializeField]
        private int height;
        public int Height { get { return height; } }

        [SerializeField]
        private int depth;
        public int Depth { get { return depth; } }

        public BlockDataArray(int width, int height, int depth)
        {
            dataArray = new BlockData[width * height * depth];
            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        public void SetBlock(int x, int y, int z, BlockData data)
        {
            dataArray[z + y * depth + x * depth * height] = data;
        }

        public void DeleteBlock(int x, int y, int z)
        {
            dataArray[z + y * depth + x * depth * height] = null;
        }

        public BlockData GetBlock(int x, int y, int z)
        {
            return dataArray[z + y * depth + x * depth * height];
        }

        public void Clean()
        {
            int p = 0;
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (dataArray[i] != null && !dataArray[i].placed) {
                    p++;
                    dataArray[i] = null;
                }
            }
            Debug.Log(p);
        }
    }
}