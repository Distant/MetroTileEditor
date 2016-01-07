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

        public static void FindMaterials()
        {
            materials = new Dictionary<string, Material>();

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
                        materials.Add(mat.name, mat);
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
    }
}