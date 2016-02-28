using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MetroTileEditor
{
    public static class TextureManager
    {
        private static Material defaultMat;
        private static Material DefaultMat
        {
            get
            {
                if (defaultMat == null)
                {
                    var m = new Material(Shader.Find("Standard"));
                    m.SetFloat("_Glossiness", 0);
                    defaultMat = m;
                }
                return defaultMat;
            }
        }

        private static Dictionary<string, Material> materials;
        public static Dictionary<string, Material> Materials
        {
            get
            {
                if (materials == null) FindMaterials();
                return materials;
            }
        }

        private static Dictionary<string, Texture2D> previews;
        public static Dictionary<string, Texture2D> Previews
        {
            get
            {
                if (previews == null) FindMaterials();
                return previews;
            }
        }

        public static void FindMaterials()
        {
            materials = new Dictionary<string, Material>();
            previews = new Dictionary<string, Texture2D>();

            Texture2D[] sheets = Resources.LoadAll<Texture2D>("Materials/TileSheets");
            foreach (Texture2D sheet in sheets.Where(sheet => !sheet.name.Contains("_normal") && !sheet.name.Contains("_emission")))
            {
                int size;
                string[] parameters = sheet.name.Split('_');
                if (!int.TryParse(parameters.Last(), out size)) size = 16;

                for (int i = 0; i < sheet.height / size; i++)
                {
                    for (int j = 0; j < sheet.width / size; j++)
                    {

                        Material mat = new Material(DefaultMat);
                        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
                        tex.filterMode = FilterMode.Point;
                        tex.SetPixels(sheet.GetPixels(i * size, j * size, size, size));
                        tex.Apply();
                        
                        Texture2D texPrev = new Texture2D(size, size, TextureFormat.ARGB32, false, true);
                        texPrev.filterMode = FilterMode.Point;
                        texPrev.SetPixels(sheet.GetPixels(i * size, j * size, size, size));
                        texPrev.Apply();

                        Texture2D normalSheet = null;
                        for (int t = 0; t < sheets.Length; t++)
                        {
                            if (sheets[t].name.Equals(sheet.name + "_normal")) normalSheet = sheets[t];
                        }

                        if (normalSheet != null)
                        {
                            Texture2D norm = new Texture2D(size, size, TextureFormat.ARGB32, false);
                            norm.filterMode = FilterMode.Point;

                            Color colour = new Color();
                            for (int x = 0; x < norm.width; x++)
                            {
                                for (int y = 0; y < norm.height; y++)
                                {
                                    colour.r = normalSheet.GetPixel(x + (i * size), y + (j * size)).g;
                                    colour.g = colour.r;
                                    colour.b = colour.r;
                                    colour.a = normalSheet.GetPixel(x + (i * size), y + (j * size)).r;
                                    norm.SetPixel(x, y, colour);

                                }
                            }
                            norm.Apply();
                            mat.SetTexture("_BumpMap", norm);
                        }

                        Texture2D emissionSheet = null;
                        for (int t = 0; t < sheets.Length; t++)
                        {
                            if (sheets[t].name.Equals(sheet.name + "_emission")) emissionSheet = sheets[t];
                        }

                        if (emissionSheet != null)
                        {
                            if (emissionSheet.GetPixel(i * size, j * size).a != 0)
                            {
                                Texture2D emission = new Texture2D(size, size, TextureFormat.ARGB32, false);
                                emission.filterMode = FilterMode.Point;
                                emission.SetPixels(emissionSheet.GetPixels(i * size, j * size, size, size));
                                emission.Apply();
                                mat.SetTexture("_EmissionMap", emission);
                            }
                        }

                        mat.mainTexture = tex;
                        mat.shaderKeywords = new string[2] { "_NORMALMAP", "_EMISSIONMAP" };
                        mat.name = sheet.name + "_" + i + "_" + j;
                        mat.SetFloat("_Mode", 1);
                        mat.EnableKeyword("_ALPHATEST_ON");
                        materials.Add(mat.name, mat);
                        previews.Add(mat.name, texPrev);
                    }
                }
            }

            defaultMat = null;
            materials.Add(string.Empty, DefaultMat);
        }

        public static Material GetMaterialOrDefault(string materialID)
        {
            Material m;
            return Materials.TryGetValue(materialID ?? string.Empty, out m) ? m : DefaultMat;
        }

        public class TexturePacker
        {
            private struct Index
            {
                public int x;
                public int y;
                public Index(int x, int y)
                {
                    this.x = x;
                    this.y = y;
                }
            }

            private int size;
            private int minSize;
            private Dictionary<Material, Rect> positions; // in pixels
            private Texture2D final;

            private Index firstFreeIndex;
            private bool[,] tiles;

            public void Start(int initSize, int minSpriteSize)
            {
                size = initSize;
                minSize = minSpriteSize;
                positions = new Dictionary<Material, Rect>();

                final = new Texture2D(size * minSize, size * minSize, TextureFormat.ARGB32, false);
                final.filterMode = FilterMode.Point;

                tiles = new bool[size, size];
                firstFreeIndex = new Index(0, 0);
            }

            public Rect AddSprite(string matID)
            {
                Material m = materials[matID];
                if (positions.ContainsKey(m))
                {
                    return positions[m];
                }

                Texture2D mainTex = m.mainTexture as Texture2D;
                if (mainTex.width == minSize)
                {
                    Debug.Log(m.name + mainTex.width);
                    positions.Add(m, new Rect(firstFreeIndex.x, firstFreeIndex.y, 1, 1));
                    tiles[firstFreeIndex.x, firstFreeIndex.y] = true;
                    firstFreeIndex = FindNextFreeIndex(firstFreeIndex);
                    AddPixels(mainTex, final, positions[m]);
                    return positions[m];
                }
                else if (mainTex.width == minSize * 2)
                {
                    Index current = firstFreeIndex;
                    while (true)
                    {
                        if (current.y + 1 >= tiles.GetLength(1)) throw new System.Exception();
                        if (current.x + 1 < tiles.GetLength(0) &&
                            !tiles[current.x + 1, current.y] &&
                            !tiles[current.x, current.y + 1] &&
                            !tiles[current.x + 1, current.y + 1])
                        {
                            positions.Add(m, new Rect(current.x, current.y, 2, 2));
                            tiles[current.x, current.y] = true;
                            tiles[current.x + 1, current.y] = true; 
                            tiles[current.x, current.y + 1] = true;
                            tiles[current.x + 1, current.y + 1] = true;

                            firstFreeIndex = FindFirstFreeIndex();
                            AddPixels(mainTex, final, positions[m]);
                            return positions[m];
                        }
                        else { current = FindNextFreeIndex(current); }
                    }
                }

                else if (mainTex.width == minSize * 3)
                {
                    Index current = firstFreeIndex;
                    while (true)
                    {
                        if (current.y + 2 >= tiles.GetLength(1)) { throw new System.Exception(); }
                        if (current.x + 2 < tiles.GetLength(0) &&
                            !tiles[current.x, current.y] &&
                            !tiles[current.x + 1, current.y] &&
                            !tiles[current.x + 2, current.y] &&
                            !tiles[current.x, current.y + 1] &&
                            !tiles[current.x, current.y + 2] &&
                            !tiles[current.x + 1, current.y + 1] &&
                            !tiles[current.x + 1, current.y + 2] &&
                            !tiles[current.x + 2, current.y + 1] &&
                            !tiles[current.x + 2, current.y + 2])
                        {
                            positions.Add(m, new Rect(current.x, current.y, 3, 3));
                            tiles[current.x, current.y] = true;
                            tiles[current.x + 1, current.y] = true;
                            tiles[current.x + 2, current.y] = true;
                            tiles[current.x, current.y + 1] = true;
                            tiles[current.x, current.y + 2] = true;
                            tiles[current.x + 1, current.y + 1] = true;
                            tiles[current.x + 1, current.y + 2] = true;
                            tiles[current.x + 2, current.y + 1] = true;
                            tiles[current.x + 2, current.y + 2] = true;

                            firstFreeIndex = FindFirstFreeIndex();
                            AddPixels(mainTex, final, positions[m]);
                            return positions[m];
                        }
                        else { current = FindNextFreeIndex(current); }
                    }
                }
                return new Rect(0, 0, 0, 0);
            }

            private void AddPixels(Texture2D source, Texture2D dest, Rect pos)
            {
                final.SetPixels((int)pos.x * minSize, (int)pos.y * minSize, (int)pos.width * minSize, (int)pos.height * minSize, source.GetPixels(0, 0, source.width, source.height));
                final.Apply();
            }

            private Index FindFirstFreeIndex()
            {
                for (int i = 0; i < tiles.GetLength(1); i++)
                {
                    for (int j = 0; j < tiles.GetLength(0); j++)
                    {
                        if (!tiles[j, i]) { return new Index(j, i); }
                    }
                }
                Debug.Log("TexturePacker, size too small");
                return new Index(100, 100);
            }

            private Index FindNextFreeIndex(Index current)
            {
                int x = current.x;
                int y = current.y;

                while (true)
                {
                    x++;
                    if (x >= tiles.GetLength(0))
                    {
                        x = 0; y++;
                    }
                    if (y >= tiles.GetLength(1)) { Debug.Log("TexturePacker, size too small"); throw new System.Exception(); }
                    if (!tiles[x, y]) break;
                }
                return new Index(x, y);
            }

            public Material Finish()
            {
                final.Apply();
                Material mat = new Material(DefaultMat);
                mat.mainTexture = final;
                mat.name = "tex_atlas";
                mat.SetFloat("_Mode", 1);
                mat.EnableKeyword("_ALPHATEST_ON");
                return mat;
            }
        }
    }
}