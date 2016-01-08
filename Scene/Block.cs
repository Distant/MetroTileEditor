using UnityEngine;
namespace MetroTileEditor
{
    public class Block : MonoBehaviour
    {
        [SerializeField]
        private ColliderData colliderData;
        public BlockData data;
        private bool initialised;

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
            else gameObject.GetComponent<MeshRenderer>().material = TextureManager.GetMaterialOrDefault(newData.materialIDs[0]);
            data = newData;
            newData.colliderData = colliderData;
        }

        private void init()
        {
            initialised = true;
        }

        public void SetMaterial(RaycastHit hit, string materialID)
        {
            int index = GetMaterialIndex(hit);
            if (data != null && index != -1)
            {
                data.materialIDs[index] = materialID;
            }
        }

        public string GetMaterialID(RaycastHit hit)
        {
            int index = GetMaterialIndex(hit);
            if (data != null && index != -1)
            {
                return data.materialIDs[index];
            }
            return string.Empty;
        }

        public int GetMaterialIndex(RaycastHit hit)
        {
            if (hit.collider is MeshCollider)
            {
                Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

                int materialIndex = -1;

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
                            materialIndex = i;
                            break;
                        }
                    }
                    if (materialIndex != -1) break;
                }
                return materialIndex;
            }
            return -1;
        }
    }
}