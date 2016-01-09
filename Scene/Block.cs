using System;
using System.Collections.Generic;
using UnityEngine;
namespace MetroTileEditor
{
    public class Block : MonoBehaviour
    {
        [SerializeField]
        private ColliderData colliderData;
        public BlockData data;
        private bool initialised;
        public Vector2[] initialUVs;
         
        public void SetBlockData(BlockData newData)
        {
            if (!initialised) init();
            MeshRenderer mesh = GetComponent<MeshRenderer>();
            if (mesh.sharedMaterials.Length > 1)
            {
                Material[] copy = mesh.sharedMaterials;
                for (int i = 0; i < copy.Length; i++)
                {
                    copy[i] = TextureManager.GetMaterialOrDefault(newData.materialIDs[i]);
                }
                mesh.sharedMaterials = copy; 
            }
            else mesh.material = TextureManager.GetMaterialOrDefault(newData.materialIDs[0]);
            data = newData;
            newData.colliderData = colliderData;
            for (int i = 0; i < 6; i++)
            {
                if (data.rotations[i] != 0)
                {
                    RotateTexture(i);
                }
            }
        }

        private void init()
        {
            initialised = true;
            initialUVs = GetComponent<MeshFilter>().sharedMesh.uv;
        }

        public void SetMaterial(RaycastHit hit, string materialID)
        {
            int index = GetSubMeshIndex(hit);
            if (data != null && index != -1)
            {
                data.materialIDs[index] = materialID;
            }
        }

        public string GetMaterialID(RaycastHit hit)
        {
            int index = GetSubMeshIndex(hit);
            if (data != null && index != -1)
            {
                return data.materialIDs[index];
            }
            return string.Empty;
        }

        public int GetSubMeshIndex(RaycastHit hit)
        {
            if (hit.collider is MeshCollider)
            {
                Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

                int subMeshIndex = -1;

                int triangleIndex = hit.triangleIndex;
                int lookupIndex1 = mesh.triangles[triangleIndex * 3];
                int lookupIndex2 = mesh.triangles[triangleIndex * 3 + 1];
                int lookupIndex3 = mesh.triangles[triangleIndex * 3 + 2];

                int subMeshCount = mesh.subMeshCount;
                for (int i = 0; i < subMeshCount; i++)
                {
                    var triangles = mesh.GetTriangles(i);
                    for (var j = 0; j < triangles.Length; j += 3)
                    {
                        if (triangles[j] == lookupIndex1 && triangles[j + 1] == lookupIndex2 && triangles[j + 2] == lookupIndex3)
                        {
                            subMeshIndex = i;
                            break;
                        }
                    }
                    if (subMeshIndex != -1) break;
                }
                return subMeshIndex;
            }
            return -1;
        }

        public void RotateTexture(RaycastHit hit)
        {
            int subMesh = GetSubMeshIndex(hit);
            data.rotations[subMesh]++;
            if (data.rotations[subMesh] > 3) data.rotations[subMesh] = 0;

            RotateTexture(subMesh);
        }

        public void RotateTexture(int subMesh)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;

            List<Vector2> newUVs = new List<Vector2>(mesh.uv);
            int[] triangles;

            triangles = mesh.GetTriangles(subMesh);

            HashSet<int> trianglesIndexSet = new HashSet<int>();
            for (int v = 0; v < triangles.Length; v++)
            {
                trianglesIndexSet.Add(triangles[v]);
            }

            foreach (int vertIndex in trianglesIndexSet)
            {
                Vector2 offset = new Vector2(0.5f, 0.5f);
                var newUV = initialUVs[vertIndex] - offset;

                float sin = Mathf.Sin(data.rotations[subMesh] * 90 * Mathf.Deg2Rad);
                float cos = Mathf.Cos(data.rotations[subMesh] * 90 * Mathf.Deg2Rad);

                float tx = newUV.x;
                float ty = newUV.y;
                newUV.x = (cos * tx) - (sin * ty);
                newUV.y = (sin * tx) + (cos * ty);

                newUVs[vertIndex] = newUV + offset;
            }

            mesh.uv = newUVs.ToArray();
            meshFilter.mesh = mesh;
        }
    }
}