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
    public class Map : MonoBehaviour, ISerializationCallbackReceiver
    {
        private static Vector3 DEFAULT_GRID_SIZE = new Vector3(20, 20, 7);

        private bool isEnabled;
        [NonSerialized]
        public bool active;
        public string MapData;
        public string mapName;

        [NonSerialized]
        public bool inPlayMode;

        public BlockDataArray blockArray;

        [SerializeField]
        public GridData gridData;
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
            get { return gridData.gridEnabled; }
            set { gridData.gridEnabled = value; GridRenderer.SetDataModel(gridData); }
        }

        public int SelectedLayer
        {
            get { return gridData.SelectedLayer; }
            set { gridData.SelectedLayer = value; GridRenderer.DataModelChanged(); }
        }

        private EditMapRenderer mapRenderer;
        private EditMapRenderer MapRenderer
        {
            get
            {
                if (mapRenderer == null) mapRenderer = ScriptableObject.CreateInstance<EditMapRenderer>();
                return mapRenderer;
            }
        }

        private Undo undoHandler;
        public Undo UndoHandler { get { if (undoHandler == null) undoHandler = new Undo(); return undoHandler; } }

        public void init()
        {
            Debug.Log("init");
            mapName = name;
            gridData = new GridData(false, (int)DEFAULT_GRID_SIZE.x, (int)DEFAULT_GRID_SIZE.y, (int)DEFAULT_GRID_SIZE.z, 2);
            blockArray = new BlockDataArray(gridData.sizeX, gridData.sizeY, gridData.sizeZ);

            MapRenderer.DrawBlocks(blockArray, transform.position, mapName);
            GridRenderer.SetDataModel(gridData);
        }

        public void OnStartup()
        {
            MapRenderer.ReAttachBlocks(blockArray, transform.position, mapName);
            GridRenderer.SetDataModel(gridData);
        }

        public void SetGridSize(int x, int y, int z)
        {
            gridData.sizeX = x;
            gridData.sizeY = x;
            gridData.sizeZ = z;
            GridRenderer.DataModelChanged();

            MapRenderer.DrawBlocks(blockArray, transform.position, mapName);
        }

        public void Clear()
        {
            blockArray = new BlockDataArray(gridData.sizeX, gridData.sizeY, gridData.sizeZ);
        }

        public void DrawBlocks()
        {
            MapRenderer.DrawBlocks(blockArray, transform.position, mapName);
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

        public void AddBlock(Vector3 pos, string blockType)
        {
            int x = (int)(pos.x - transform.position.x);
            int y = (int)(pos.y - transform.position.y);
            int z = (int)(pos.z - transform.position.z);

            UndoHandler.RegisterUndo(new Undo.UndoCommand(() =>
            {
                blockArray.DeleteBlock(x, y, z);
                DrawBlocks();
            }, () => _AddBlock(new Index(x, y, z), blockType)));

            _AddBlock(new Index(x, y, z), blockType);
        }

        public void _AddBlock(Index index, BlockData data)
        {
            if (index.x >= 0 && index.y >= 0 && index.x < gridData.sizeX && index.y < gridData.sizeY && index.z >= 0 && index.z < gridData.sizeZ)
            {
                if (blockArray.GetBlockData(index.x, index.y, index.z) == null || !blockArray.GetBlockData(index.x, index.y, index.z).placed)
                {
                    data.placed = true;
                    blockArray.SetBlock(index.x, index.y, index.z, data);
                    if (data.placed)
                    {
                        data.placed = true;
                        MapRenderer.BlockAdded(index, data);
                    }
                }
            }
        }

        private void _AddBlock(Index index, string blockType)
        {
            _AddBlock(index, new BlockData(blockType));
        }

        public void UpdateBlock(GameObject g)
        {
            Vector3 raw = g.transform.position;
            int x = (int)(raw.x - transform.position.x);
            int y = (int)(raw.y - transform.position.y);
            int z = (int)(raw.z + 1 - transform.position.z);

            MapRenderer.BlockUpdated(blockArray.GetBlockData(x, y, z), g);
        }

        public void DeleteBlock(GameObject g)
        {
            Vector3 raw = g.transform.position;
            int x = (int)(raw.x - transform.position.x);
            int y = (int)(raw.y - transform.position.y);
            int z = (int)(raw.z + 1 - transform.position.z);

            Index index = new Index(x, y, z);

            UndoHandler.RegisterUndo(new Undo.UndoCommand(() => _AddBlock(index, g.GetComponent<Block>().data), () =>
            {
                blockArray.DeleteBlock(index.x, index.y, index.z);
                DrawBlocks();
            }));

            _DeleteBlock(index, g);
        }

        private void _DeleteBlock(Index index, GameObject g)
        {
            blockArray.DeleteBlock(index.x, index.y, index.z);
            MapRenderer.BlockDeleted(index, g);
        }

        public void TrimMap()
        {
            Vector3 offset = blockArray.TrimArray();
            transform.position += offset;
            MapRenderer.DrawBlocks(blockArray, transform.position, mapName);
            gridData = new GridData(false, blockArray.Width, blockArray.Height, blockArray.Depth, 0);
            GridRenderer.SetDataModel(gridData);
        }

        public void Undo()
        {
            UndoHandler.PerformUndo();
        }

        public void Redo()
        {
            UndoHandler.PerformRedo();
        }

        public void Load(bool reDraw)
        {
            Debug.Log("Load(): Blockdata null: " + (blockArray == null));
            if (reDraw) MapRenderer.DrawBlocks(blockArray, transform.position, mapName);
            else MapRenderer.ReAttachBlocks(blockArray, transform.position, mapName);
            gridData = new GridData(false, blockArray.Width, blockArray.Height, blockArray.Depth, 0);
            GridRenderer.SetDataModel(gridData);
        }

        public void Load(BlockDataArray saveData, bool reDraw)
        {
            blockArray = saveData;
            Load(reDraw);
        }

        public void RecreateMap()
        {
            blockArray = new BlockDataArray(gridData.sizeX, gridData.sizeY, gridData.sizeZ);
            MapRenderer.RecreateMap(blockArray, transform.position, name);
        }

        public MapSaveData GenerateSaveData()
        {
            return new MapSaveData(blockArray, gridData.SelectedLayer);
        }

        public void EnsureGridModel()
        {
            if (GridRenderer.HasModel())
            {
                GridRenderer.SetDataModel(gridData);
            }
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            if (mapRenderer != null && gridRenderer != null)
            {
                blockArray.Clean();
                Load(false);
            }
        }

        public void OnBeforeEnterPlayMode()
        {
            blockArray.Clean();
            inPlayMode = true;
        }

        public void OnAfterExitPlayMode()
        {
            inPlayMode = false;
            Debug.Log("Map: " + name + " exiting play mode");
            MapRenderer.ReAttachBlocks(blockArray, transform.position, mapName);
            GridRenderer.SetDataModel(gridData);
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