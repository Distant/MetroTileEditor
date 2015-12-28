using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MetroTileEditor.Renderers
{
    public class EditMapRenderer : ScriptableObject
    {
        private Transform parent;

        void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        public GameObject BlockAdded(int x, int y, int z, BlockData data)
        {
            var newBlock = GenerateCube(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), data.blockType, parent);
            newBlock.GetComponent<Block>().SetBlockData(data);
            return newBlock;
        }

        public void BlockDeleted(int x, int y, int z, GameObject block)
        {
            DestroyImmediate(block);
        }

        public void BlockUpdated(BlockData data, GameObject block)
        {
            block.GetComponent<Block>().SetBlockData(data);
        }

        public void DrawBlocks(BlockData[,,] blockDataArray, string mapName)
        {
            Block[] blocks = FindSceneObject(mapName + "_data").GetComponentsInChildren<Block>();
            for (int i = 0; i < blocks.Length; i++) DestroyImmediate(blocks[i].gameObject);

            Debug.Log("Editor map reset");

            parent = FindSceneObject(mapName + "_data").transform;

            for (int i = 0; i < blockDataArray.GetLength(0); i++)
            {
                for (int j = 0; j < blockDataArray.GetLength(1); j++)
                {
                    for (int k = 0; k < blockDataArray.GetLength(2); k++)
                    {
                        if (blockDataArray[i, j, k] != null && blockDataArray[i, j, k].placed)
                        {
                            GameObject g = BlockAdded(i, j, k, blockDataArray[i, j, k]);
                        }
                    }
                }
            }
        }

        public void ReAttachBlocks(BlockData[,,] blockDataArray, Vector3 offset, string mapName)
        {
            Debug.Log("Reattaching blocks");
            parent = FindSceneObject(mapName + "_data").transform;
            foreach (Block block in GameObject.Find(mapName + "_data").GetComponentsInChildren<Block>())
            {
                Vector3 raw = block.gameObject.transform.position;
                int x = (int)(raw.x - offset.x);
                int y = (int)(raw.y - offset.y);
                int z = (int)(raw.z + 1 - offset.z);
                block.SetBlockData(blockDataArray[x, y, z]);
            }
        }

        public void RecreateMap(BlockData[,,] blockDataArray, Vector3 offset, string mapName)
        {
            Debug.Log("Recreating blocks");
            parent = FindSceneObject(mapName + "_data").transform;
            foreach (Block block in GameObject.Find(mapName + "_data").GetComponentsInChildren<Block>())
            {
                Vector3 raw = block.gameObject.transform.position;
                int x = (int)(raw.x - offset.x);
                int y = (int)(raw.y - offset.y);
                int z = (int)(raw.z + 1 - offset.z);
                blockDataArray[x, y, z] = block.data;
                blockDataArray[x, y, z].placed = true;
            }
        }

        private GameObject GenerateCube(Vector3 pos, string blockType, Transform parent)
        {
            var o = Resources.Load("Blocks/Prefabs/" + blockType);
            GameObject newCube = (GameObject)Instantiate(o ? o : Resources.Load("Blocks/Prefabs/" + "basic_cube"));
            newCube.name = blockType;
            newCube.transform.position = pos + parent.position;
            newCube.transform.parent = parent;
            newCube.isStatic = true;
            return newCube;
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

        public void GenerateColliders(BlockData[,,] blockDataArray, string mapName)
        {
            GameObject colliderParent = FindSceneObject(mapName + "_colliders");
            Collider2D[] cols = colliderParent.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < cols.Length; i++) DestroyImmediate(cols[i].gameObject);

            Debug.Log("Colliders Reset");

            int k = 3;
            for (int i = 0; i < blockDataArray.GetLength(0); i++)
            {
                for (int j = 0; j < blockDataArray.GetLength(1); j++)
                {
                    if (blockDataArray[i, j, k] != null && blockDataArray[i, j, k].placed)
                    {
                        GameObject g = new GameObject();
                        g.name = "collider";
                        g.transform.position = new Vector3(i + 0.5f, j + 0.5f, k - 0.5f);
                        g.transform.parent = colliderParent.transform;

                        ColliderData data = blockDataArray[i, j, k].colliderData;
                        if (data != null && data.orientations != null && data.orientations.Length > 0)
                        {
                            var poly = g.AddComponent<PolygonCollider2D>();
                            Vector2[] verts = new Vector2[data.vertCount[0]];
                            for (int v = 0; v < data.vertCount[0]; v++)
                            {
                                verts[v] = data.verts[v];
                            }
                            poly.points = verts;
                        }
                        else
                        {
                            g.AddComponent<BoxCollider2D>();
                        }
                        g.layer = LayerMask.NameToLayer("Solid");
                    }
                }
            }
        }

        public void GenerateMesh(BlockData[,,] blockDataArray, string mapName)
        {
            GameObject mapParent = FindSceneObject(mapName + "_mesh");
            Block[] cols = mapParent.GetComponentsInChildren<Block>();
            for (int c = 0; c < cols.Length; c++) DestroyImmediate(cols[c].gameObject);

            List<MeshFilter> filters = new List<MeshFilter>();
            List<CombineInstance> combines = new List<CombineInstance>();

            for (int i = 0; i < blockDataArray.GetLength(0); i++)
            {
                for (int j = 0; j < blockDataArray.GetLength(1); j++)
                {
                    for (int k = 0; k < blockDataArray.GetLength(2); k++)
                    {
                        BlockData curData = blockDataArray[i, j, k];
                        if (curData != null && curData.placed)
                        {
                            GameObject g = GenerateCube(new Vector3(i + 0.5f, j + 0.5f, k - 0.5f), curData.blockType, mapParent.transform);
                            g.GetComponent<Block>().SetBlockData(curData);

                            MeshFilter gMeshFilter = g.GetComponent<MeshFilter>();
                            Mesh gMesh = gMeshFilter.mesh;

                            List<Vector2> newUVs = new List<Vector2>(gMesh.uv);
                            int[] triangles;
                            for (int x = 0; x < 6; x++)
                            {
                                string s = curData.materialIDs[x];
                                if (!string.IsNullOrEmpty(s))
                                {
                                    string[] split = s.Split('_');
                                    float UVx = int.Parse(split[2]);
                                    float UVy = int.Parse(split[3]);

                                    triangles = gMesh.GetTriangles(x);

                                    HashSet<int> trianglesIndexSet = new HashSet<int>();
                                    for (int v = 0; v < triangles.Length; v++)
                                    {
                                        trianglesIndexSet.Add(triangles[v]);
                                    }

                                    foreach (int vertIndex in trianglesIndexSet)
                                    {
                                        var newUV = newUVs[vertIndex];
                                        newUV.x /= 8;
                                        newUV.y /= 8;
                                        newUV.x += (UVx / 8);
                                        newUV.y += (UVy / 8);
                                        newUVs[vertIndex] = newUV;
                                    }

                                }
                            }

                            gMesh.uv = newUVs.ToArray();
                            gMeshFilter.mesh = gMesh;

                            for (int x = 0; x < 6; x++)
                            {
                                string s = curData.materialIDs[x];
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

            GameObject finalMesh = new GameObject();
            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combines.ToArray(), true);
            Unwrapping.GenerateSecondaryUVSet(newMesh);
            finalMesh.AddComponent<MeshFilter>().mesh = newMesh;
            finalMesh.AddComponent<MeshRenderer>();
            finalMesh.gameObject.SetActive(true);
            finalMesh.isStatic = true;
            finalMesh.name = mapName + "_mesh";

            DestroyImmediate(mapParent);
        }
    }
}