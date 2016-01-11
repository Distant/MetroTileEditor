using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MetroTileEditor.Generation
{
    public static class MeshGenerator
    {
        public static void GenerateMesh(BlockDataArray blockDataArray, string mapName)
        {
            GameObject mapParent = FindSceneObject(mapName + "_meshTemp");
            Block[] cols = mapParent.GetComponentsInChildren<Block>();
            for (int c = 0; c < cols.Length; c++) GameObject.DestroyImmediate(cols[c].gameObject);

            GameObject finalMesh = new GameObject();
            List<CombineInstance> combines = new List<CombineInstance>();

            TextureManager.TexturePacker texPacker = new TextureManager.TexturePacker();
            int texsize = 32;
            int mintex = 8;
            texPacker.Start(texsize, mintex);

            for (int i = 0; i < blockDataArray.Width; i++)
            {
                for (int j = 0; j < blockDataArray.Height; j++)
                {
                    for (int k = 0; k < blockDataArray.Depth; k++)
                    {
                        BlockData curBlockData = blockDataArray.GetBlock(i, j, k);
                        if (curBlockData != null && curBlockData.placed)
                        {
                            GameObject g = GenerateCube(new Vector3(i + 0.5f, j + 0.5f, k - 0.5f), curBlockData.blockType, mapParent.transform);
                            g.GetComponent<Block>().SetBlockData(curBlockData);

                            MeshFilter gMeshFilter = g.GetComponent<MeshFilter>();
                            Mesh gMesh = gMeshFilter.mesh;

                            List<Vector2> newUVs = new List<Vector2>(gMesh.uv);
                            int[] triangles;
                            for (int x = 0; x < 6; x++)
                            {
                                string s = curBlockData.materialIDs[x];
                                if (!string.IsNullOrEmpty(s))
                                {
                                    Rect pos = texPacker.AddSprite(s);

                                    triangles = gMesh.GetTriangles(x);

                                    HashSet<int> trianglesIndexSet = new HashSet<int>();
                                    for (int v = 0; v < triangles.Length; v++)
                                    {
                                        trianglesIndexSet.Add(triangles[v]);
                                    }

                                    foreach (int vertIndex in trianglesIndexSet)
                                    {
                                        var newUV = newUVs[vertIndex];
                                        newUV.x /= texsize;
                                        newUV.x *= pos.width;
                                        newUV.y /= texsize;
                                        newUV.y *= pos.height;

                                        newUV.x += (pos.x / texsize);
                                        newUV.y += (pos.y / texsize);

                                        newUVs[vertIndex] = newUV;
                                    }
                                }
                            }

                            gMesh.uv = newUVs.ToArray();
                            gMeshFilter.mesh = gMesh;

                            if (curBlockData.excludeFromMesh)
                            {
                                List<CombineInstance> submeshes = new List<CombineInstance>();
                                for (int x = 0; x < 6; x++)
                                {
                                    string s = curBlockData.materialIDs[x];
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        CombineInstance inst = new CombineInstance();
                                        inst.mesh = gMesh;
                                        inst.subMeshIndex = x;
                                        inst.transform = g.transform.worldToLocalMatrix * gMeshFilter.transform.localToWorldMatrix;
                                        submeshes.Add(inst);
                                    }
                                }

                                Mesh combinedMesh = new Mesh();
                                combinedMesh.CombineMeshes(submeshes.ToArray(), true);
                                gMeshFilter.sharedMesh = combinedMesh;
                                GameObject.DestroyImmediate(g.GetComponent<Block>());
                                GameObject.DestroyImmediate(g.GetComponent<MeshRenderer>());
                                GameObject.DestroyImmediate(g.GetComponent<MeshCollider>());
                                g.AddComponent<MeshRenderer>();
                                g.isStatic = false;
                                g.name = i + "_" + j + "_" + k;
                                g.transform.parent = finalMesh.transform;
                            }
                            else
                            {
                                for (int x = 0; x < 6; x++)
                                {
                                    string s = curBlockData.materialIDs[x];
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        CombineInstance inst = new CombineInstance();
                                        inst.mesh = gMesh;
                                        inst.subMeshIndex = x;
                                        inst.transform = gMeshFilter.transform.localToWorldMatrix;
                                        combines.Add(inst);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combines.ToArray(), true);
            Unwrapping.GenerateSecondaryUVSet(newMesh);
            finalMesh.AddComponent<MeshFilter>().mesh = newMesh;
            finalMesh.AddComponent<MeshRenderer>().sharedMaterial = texPacker.Finish();
            finalMesh.gameObject.SetActive(true);
            finalMesh.isStatic = true;
            finalMesh.name = mapName + "_mesh";

            GameObject.DestroyImmediate(mapParent);
        }

        private static GameObject GenerateCube(Vector3 pos, string blockType, Transform parent)
        {
            var o = Resources.Load("Blocks/Prefabs/" + blockType);
            GameObject newCube = (GameObject)GameObject.Instantiate(o ? o : Resources.Load("Blocks/Prefabs/" + "basic_cube"));
            newCube.transform.parent = parent;
            newCube.transform.position = pos;
            newCube.isStatic = true;
            return newCube;
        }

        public static void GenerateColliders(BlockDataArray blockDataArray, string mapName)
        {
            GameObject colliderParent = FindSceneObject(mapName + "_colliders");
            Collider2D[] cols = colliderParent.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < cols.Length; i++) GameObject.DestroyImmediate(cols[i].gameObject);

            Debug.Log("Colliders Reset");

            int k = 3;
            for (int i = 0; i < blockDataArray.Width; i++)
            {
                for (int j = 0; j < blockDataArray.Height; j++)
                {
                    BlockData curData = blockDataArray.GetBlock(i, j, k);
                    if (curData != null && curData.placed && !curData.isTriggerOnly)
                    {
                        GameObject g = new GameObject();
                        g.name = "collider";
                        g.transform.position = new Vector3(i + 0.5f, j + 0.5f, k - 0.5f);
                        g.transform.parent = colliderParent.transform;

                        ColliderData data = curData.colliderData;
                        Collider2D collider;
                        if (data != null && data.orientations != null && data.orientations.Length > 0)
                        {
                            var poly = g.AddComponent<PolygonCollider2D>();
                            Vector2[] verts = new Vector2[data.vertCount[0]];
                            for (int v = 0; v < data.vertCount[0]; v++)
                            {
                                verts[v] = data.verts[v];
                            }
                            poly.points = verts;
                            collider = poly;
                        }
                        else
                        {
                            collider = g.AddComponent<BoxCollider2D>();
                        }

                        if (curData.isTriggerOnly) collider.isTrigger = true;

                        TileData tileInfo = g.AddComponent<TileData>();
                        LoadBlockData(tileInfo, curData);
                        g.layer = LayerMask.NameToLayer("Solid");
                    }
                }
            }
        }

        private static void LoadBlockData(TileData tileInfo, BlockData blockData)
        {
            tileInfo.blockType = blockData.blockType;
            tileInfo.breakable = blockData.breakable;
        }

        public static GameObject FindSceneObject(string name)
        {
            GameObject go;
            if (!(go = GameObject.Find(name)))
            {
                go = new GameObject();
                go.name = name;
            }
            return go;
        }
    }
}