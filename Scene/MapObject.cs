using MetroTileEditor.Renderers;
using MetroTileEditor.Utils;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MetroTileEditor
{
    [SelectionBase]
    [ExecuteInEditMode]
    public class MapObject : MonoBehaviour, ISerializationCallbackReceiver
    {
        private static Vector3 DEFAULT_GRID_SIZE = new Vector3(20, 20, 7);

        private bool isEnabled;
        [NonSerialized]
        public bool active;
        public string MapData;
        public string mapName;

        [NonSerialized]
        public bool inPlayMode;

        private bool attemptedDeserialize;
        private bool deserializeFailed;

        public BlockDataArray blocks;

        [SerializeField]
        public GridData gridModel;
        private GridRenderer gridRenderer;
        public GridRenderer GridRenderer
        {
            get
            {
                gridRenderer = SceneUtils.GetChildComponent("Grid", gridRenderer, this.gameObject);
                return gridRenderer;
            }
        }

        public bool GridEnabled
        {
            get { return gridModel.gridEnabled; }
            set { gridModel.gridEnabled = value; GridRenderer.SetDataModel(gridModel); }
        }

        public int SelectedLayer
        {
            get { return gridModel.SelectedLayer; }
            set { gridModel.SelectedLayer = value; gridRenderer.DataModelChanged(); }
        }

        private EditMapRenderer editorMap;
        private EditMapRenderer EditorMap
        {
            get
            {
                if (editorMap == null) editorMap = ScriptableObject.CreateInstance<EditMapRenderer>();
                return editorMap;
            }
        }

        public void init()
        {
            Debug.Log("init");
            mapName = name;
            gridModel = new GridData(false, (int)DEFAULT_GRID_SIZE.x, (int)DEFAULT_GRID_SIZE.y, (int)DEFAULT_GRID_SIZE.z, 2);
            blocks = new BlockDataArray(gridModel.gridX, gridModel.gridY, gridModel.layers);

            EditorMap.DrawBlocks(blocks, mapName);
            GridRenderer.SetDataModel(gridModel);
        }

        public void OnStartup()
        {
            EditorMap.ReAttachBlocks(blocks, transform.position, mapName);
            GridRenderer.SetDataModel(gridModel);
        }

        void Awake()
        {

        }

        public void SetGridSize(int x, int y, int z)
        {
            gridModel.gridX = x;
            gridModel.gridY = x;
            gridModel.layers = z;
            GridRenderer.DataModelChanged();

            BlockData[,,] tempData = new BlockData[x, y, z];
            // TODO
            //blockDataArray = ArrayUtils.CopyArray(blockDataArray, tempData);
            EditorMap.DrawBlocks(blocks, mapName);
        }

        public void Clear()
        {
            blocks = new BlockDataArray(gridModel.gridX, gridModel.gridY, gridModel.layers);
        }

        public void DrawBlocks()
        {
            EditorMap.DrawBlocks(blocks, mapName);
        }

        public void SetActive()
        {
            active = true;
            Debug.Log("new map selected" + active);
            GridEnabled = true;
        }

        public void SetInactive()
        {
            GridEnabled = false;
            active = false;
        }

        public BlockData AddBlock(float xpos, float ypos, int layer, string blockType)
        {
            int x = (int)(xpos - transform.position.x);
            int y = (int)(ypos - transform.position.y);
            int z = (int)(layer - transform.position.z);

            if (x >= 0 && y >= 0 && x < gridModel.gridX && y < gridModel.gridY && z >= 0 && z < gridModel.layers)
            {
                if (blocks.GetBlock(x, y, z) == null || !blocks.GetBlock(x, y, z).placed)
                {
                    BlockData data = new BlockData();
                    data.placed = true;
                    blocks.SetBlock(x, y, z, data);
                    if (blocks.GetBlock(x, y, z).placed)
                    {
                        blocks.GetBlock(x, y, z).placed = true;
                        blocks.GetBlock(x, y, z).blockType = blockType;
                        EditorMap.BlockAdded(x, y, z, blocks.GetBlock(x, y, z));
                    }
                    return blocks.GetBlock(x, y, z);
                }
            }
            return null;
        }

        public void UpdateBlock(GameObject g)
        {
            Vector3 raw = g.transform.position;
            int x = (int)(raw.x - transform.position.x);
            int y = (int)(raw.y - transform.position.y);
            int z = (int)(raw.z + 1 - transform.position.z);

            EditorMap.BlockUpdated(blocks.GetBlock(x, y, z), g);
        }

        public void DeleteBlock(GameObject g)
        {
            Vector3 raw = g.transform.position;
            int x = (int)(raw.x - transform.position.x);
            int y = (int)(raw.y - transform.position.y);
            int z = (int)(raw.z + 1 - transform.position.z);
            blocks.DeleteBlock(x, y, z);
            EditorMap.BlockDeleted(x, y, z, g);
        }

        public void Load(bool reDraw)
        {
            Debug.Log("Load(): Blockdata null: " + (blocks == null));
            if (reDraw) EditorMap.DrawBlocks(blocks, mapName);
            else EditorMap.ReAttachBlocks(blocks, transform.position, mapName);
            gridModel = new GridData(false, blocks.Width, blocks.Height, blocks.Depth, 0);
            GridRenderer.SetDataModel(gridModel);
        }

        public void Load(BlockDataArray saveData, bool reDraw)
        {
            blocks = saveData;
            Load(reDraw);
        }

        public void RecreateMap()
        {
            blocks = new BlockDataArray(gridModel.gridX, gridModel.gridY, gridModel.layers);
            EditorMap.RecreateMap(blocks, transform.position, name);
        }

        public MapSaveData GenerateSaveData()
        {
            return new MapSaveData(blocks, gridModel.SelectedLayer);
        }

        public void EnsureGridModel()
        {
            if (GridRenderer.HasModel())
            {
                GridRenderer.SetDataModel(gridModel);
            }
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            attemptedDeserialize = true;
            if (editorMap != null && gridRenderer != null)
            {
                blocks.Clean();
                Load(false);
            }
            else deserializeFailed = true;
        }

        public void OnBeforeEnterPlayMode()
        {
            blocks.Clean();
            inPlayMode = true;
        }

        public void OnAfterExitPlayMode()
        {
            inPlayMode = false;
            Debug.Log("Map: " + name + " exiting play mode");
            EditorMap.ReAttachBlocks(blocks, transform.position, mapName);
            GridRenderer.SetDataModel(gridModel);
            GridEnabled = false;
        }

        public MapSaveData LoadFile(string path)
        {
            if (File.Exists(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(path, FileMode.Open);
                MapSaveData saveData = (MapSaveData)bf.Deserialize(file);
                file.Close();
                return saveData;
            }
            Debug.Log("No temp data found, returning empty data");
            return MapSaveData.Empty();
        }

        public void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("Destroyed");
            }
        }
    }
}