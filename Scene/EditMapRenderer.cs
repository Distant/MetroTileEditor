using UnityEngine;

namespace MetroTileEditor.Renderers
{
    public class EditMapRenderer : ScriptableObject
    {
        private Transform parent;

        public GameObject BlockAdded(int x, int y, int z, BlockData data)
        {
            var newBlock = GenerateCube(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), data.blockType, parent);
            newBlock.GetComponent<Block>().SetBlockData(data);
            return newBlock;
        }

        public void BlockDeleted(int x, int y, int z, GameObject block)
        {
            GameObject.DestroyImmediate(block);
        }

        public void BlockUpdated(BlockData data, GameObject block)
        {
            block.GetComponent<Block>().SetBlockData(data);
        }

        public void DrawBlocks(BlockData[,,] blockDataArray, string mapName)
        {
            Block[] blocks = FindSceneObject(mapName + "_data").GetComponentsInChildren<Block>();
            for (int i = 0; i < blocks.Length; i++) GameObject.DestroyImmediate(blocks[i].gameObject);

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
            GameObject newCube = (GameObject)GameObject.Instantiate(o ? o : Resources.Load("Blocks/Prefabs/" + "basic_cube"));
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
    }
}