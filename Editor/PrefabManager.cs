using UnityEngine;
using System.Collections;
using UnityEditor;

public static class PrefabManager
{
    public static GameObject[] blockPrefabs;
    public static Texture2D[] blockPreviews;

    public static void LoadPrefabs()
    {
        blockPrefabs = Resources.LoadAll<GameObject>("Blocks/Prefabs") as GameObject[];
        blockPreviews = new Texture2D[blockPrefabs.Length];

        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            Texture2D init = null; 
            while(init == null) init = AssetPreview.GetAssetPreview(blockPrefabs[i]);
            blockPreviews[i] = new Texture2D(init.width, init.height, init.format, false, true);
            blockPreviews[i].SetPixels(init.GetPixels());
            blockPreviews[i].Apply();
        }
    }
}