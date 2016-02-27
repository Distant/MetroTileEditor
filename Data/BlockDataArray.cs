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
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;
            if (depth <= 0) depth = 1;

            dataArray = new BlockData[width * height * depth];
            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        public void SetBlock(int x, int y, int z, BlockData data)
        {
            if (ValidIndex(x, y, z))
            {
                dataArray[z + y * depth + x * depth * height] = data;
            }
        }

        public void DeleteBlock(int x, int y, int z)
        {
            if (ValidIndex(x, y, z))
            {
                dataArray[z + y * depth + x * depth * height] = null;
            }
        }

        public BlockData GetBlockData(int x, int y, int z)
        {
            if (ValidIndex(x, y, z))
            {
                return dataArray[z + y * depth + x * depth * height];
            }
            else throw new IndexOutOfRangeException("No such index in array");
        }

        // Remove empty objects created by the serialization process
        public void Clean()
        {
            int p = 0;
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (dataArray[i] != null && !dataArray[i].placed)
                {
                    p++;
                    dataArray[i] = null;
                }
            }
            Debug.Log(p);
        public Vector3 TrimArray()
        {
            int xmin = width, xmax = 0, ymin = height, ymax = 0, zmin = depth, zmax = 0;
            bool blockFound = false;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        if (dataArray[k + j * depth + i * depth * height] != null && dataArray[k + j * depth + i * depth * height].placed)
                        {
                            blockFound = true;
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

            if (!blockFound)
            {
                width = 1;
                height = 1;
                depth = 1;
                dataArray = new BlockData[1];
                return Vector3.zero;
            }

            int nWidth = (xmax - xmin + 1);
            int nHeight = (ymax - ymin + 1);
            int nDepth = (zmax - zmin + 1);
            BlockData[] resized = new BlockData[nWidth * nHeight * nDepth];

            BlockData curData;

            for (int i = 0; i < nWidth; i++)
            {
                for (int j = 0; j < nHeight; j++)
                {
                    for (int k = 0; k < nDepth; k++)
                    {
                        curData = dataArray[(k + zmin) + (j + ymin) * depth + (i + xmin) * depth * height];
                        if (curData != null && curData.placed)
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

        private bool ValidIndex(int x, int y, int z)
        {
            return (x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth);
        }
    
    }
}