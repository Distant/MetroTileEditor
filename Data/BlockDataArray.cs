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
        public Vector3 TrimArray()
        {
            int xmin = width, xmax = 0, ymin = height, ymax = 0, zmin = depth, zmax = 0;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        if (dataArray[k + j * depth + i * depth * height] != null && dataArray[k + j * depth + i * depth * height].placed)
                        {
                            if (i < xmin) xmin = i;
                            if (i > xmax) xmax = i;
                            if (j < ymin) ymin = j;
                            if (j > ymax) ymax = j;
                            if (k < zmin) zmin = k;
                            if (k > zmax) zmax = k;
                        }
                    }
                }
            }

            int nWidth = (xmax - xmin + 1);
            int nHeight = (ymax - ymin + 1);
            int nDepth = (zmax - zmin + 1);
            BlockData[] resized = new BlockData[ nWidth * nHeight * nDepth];

            BlockData curData;

            for (int i = 0; i < nWidth; i++)
            {
                for (int j = 0; j < nHeight; j++)
                {
                    for (int k = 0; k < nDepth; k++)
                    {
                        curData = dataArray[(k + zmin) + (j + ymin) * depth + (i + xmin) * depth * height];
                        if ( curData != null && curData.placed)
                        {
                            resized[k + j * nDepth + i * nDepth * nHeight] = dataArray[(k + zmin) + (j + ymin) * depth + (i + xmin) * depth * height];
                        }
                    }
                }
            }

            width = nWidth;
            height = nHeight;
            depth = nDepth;
            dataArray = resized;

            return new Vector3(xmin, ymin, zmin);
        }
    }
}