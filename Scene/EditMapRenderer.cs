using UnityEngine;

namespace MetroTileEditor.Renderers
{
    public class EditMapRenderer : ScriptableObject
    {
        private Transform parent;

        public GameObject BlockAdded(Index index, BlockData data)
        {
            var newBlock = GenerateCube(new Vector3(index.x + 0.5f, index.y + 0.5f, index.z - 0.5f), data.blockType, parent);
            newBlock.GetComponent<Block>().SetBlockData(data);
            return newBlock;
        }

        public void BlockDeleted(Index index, GameObject block)
        {
            GameObject.DestroyImmediate(block);
        }

        public void BlockUpdated(BlockData data, GameObject block)
        {
            block.GetComponent<Block>().SetBlockData(data);
        }

        public void DrawBlocks(BlockDataArray blockDataArray, Vector3 offset, string mapName)
        {
            Block[] blocks = FindSceneObject(mapName + "_data").GetComponentsInChildren<Block>();
            for (int i = 0; i < blocks.Length; i++) GameObject.DestroyImmediate(blocks[i].gameObject);

            Debug.Log("Editor map reset");

            parent = FindSceneObject(mapName + "_data").transform;
            parent.transform.position = offset;
            SelectionBase selBase = parent.gameObject.GetComponent<SelectionBase>();
            if (selBase == null) parent.gameObject.AddComponent<SelectionBase>();

            for (int i = 0; i < blockDataArray.Width; i++)
            {
                for (int j = 0; j < blockDataArray.Height; j++)
                {
                    for (int k = 0; k < blockDataArray.Depth; k++)
                    {
                        BlockData block = blockDataArray.GetBlockData(i, j, k);
                        if (block != null && block.placed)
                        {
                            BlockAdded(new Index(i, j, k), block);
                        }
                    }
                }
            }
        }

        public void ReAttachBlocks(BlockDataArray blockDataArray, Vector3 offset, string mapName)
        {
            Debug.Log("Reattaching blocks");
            parent = FindSceneObject(mapName + "_data").transform;
            foreach (Block block in GameObject.Find(mapName + "_data").GetComponentsInChildren<Block>())
            {
                Vector3 raw = block.gameObject.transform.position;
                int x = (int)(raw.x - offset.x);
                int y = (int)(raw.y - offset.y);
                int z = (int)(raw.z + 1 - offset.z);
                block.SetBlockData(blockDataArray.GetBlockData(x, y, z));
            }
        }

        public void RecreateMap(BlockDataArray blockDataArray, Vector3 offset, string mapName)
        {
            Debug.Log("Recreating blocks");
            parent = FindSceneObject(mapName + "_data").transform;
            foreach (Block block in GameObject.Find(mapName + "_data").GetComponentsInChildren<Block>())
            {
                Vector3 raw = block.gameObject.transform.position;
                int x = (int)(raw.x - offset.x);
                int y = (int)(raw.y - offset.y);
                int z = (int)(raw.z + 1 - offset.z);
                blockDataArray.SetBlock(x, y, z, block.data);
                blockDataArray.GetBlockData(x, y, z).placed = true;
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