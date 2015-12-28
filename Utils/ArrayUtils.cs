using System;

namespace MetroTileEditor.Utils
{
    public class ArrayUtils
    {
        public static T[,,] CopyArray<T>(T[,,] source, T[,,] dest)
        {
            int x = Math.Min(source.GetLength(0), dest.GetLength(0));
            int y = Math.Min(source.GetLength(1), dest.GetLength(1));
            int z = Math.Min(source.GetLength(2), dest.GetLength(2));
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    for (int k = 0; k < z; k++)
                    {
                        dest[i, j, k] = source[i, j, k];
                    }
                }
            }
            return dest;
        }

        public static T[] Array3DTo1D<T>(T[,,] data, int x, int y, int z)
        {
            T[] data1D = new T[data.Length];

            int index = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    for (int k = 0; k < z; k++)
                    {
                        data1D[index] = data[i, j, k];
                        index++;
                    }
                }
            }
            return data1D;
        }

        public static T[,,] Array1DTo3D<T>(T[] data, int x, int y, int z)
        {
            T[,,] data3D = new T[x, y, z];
            int index = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    for (int k = 0; k < z; k++)
                    {
                        data3D[i, j, k] = data[index];
                        index++;
                    }
                }
            }
            return data3D;
        }
    }
}