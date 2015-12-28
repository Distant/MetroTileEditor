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
        public bool active;
        public string MapData;
        public string mapName;
        public bool initialized;

        public bool inPlayMode;

        private bool attemptedDeserialize;
        private bool deserializeFailed;

        [NonSerialized]
        public BlockData[,,] blockDataArray;
        public MapSaveData serializedMapData;

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
            set { gridModel.gridEnabled = value; }
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
            blockDataArray = new BlockData[gridModel.gridX, gridModel.gridY, gridModel.layers];
            serializedMapData = new MapSaveData(blockDataArray, blockDataArray.GetLength(0), blockDataArray.GetLength(1), blockDataArray.GetLength(2), gridModel.SelectedLayer);

            EditorMap.DrawBlocks(blockDataArray, mapName);
            GridRenderer.SetDataModel(gridModel);
            initialized = true;
        }

        public void OnStartup()
        {
            LoadTempData();
            EditorMap.ReAttachBlocks(blockDataArray, transform.position, mapName);
            GridRenderer.SetDataModel(gridModel);
        }

        void Awake()
        {
        }

        void OnEnable()
        {
            if (initialized)
            {
                if (!inPlayMode)
                {
                    if (deserializeFailed)
                    {
                        Debug.Log("Map Object OnEnable: AfterDeserialize failed");
                        Load(serializedMapData, false);
                        attemptedDeserialize = false;
                    }
                }

                if (!attemptedDeserialize)
                {
                    Debug.Log("Map Object OnEnable: no deserializtion attempted");
                    LoadTempData();
                    EditorMap.ReAttachBlocks(blockDataArray, transform.position, mapName);
                    GridRenderer.SetDataModel(gridModel);
                }
                else attemptedDeserialize = false;
                isEnabled = true;
            }
        }

        void Start()
        {

        }

        void Update()
        {

        }

        public void SetGridSize(int x, int y, int z)
        {
            gridModel.gridX = x;
            gridModel.gridY = x;
            gridModel.layers = z;
            GridRenderer.DataModelChanged();

            BlockData[,,] tempData = new BlockData[x, y, z];
            blockDataArray = ArrayUtils.CopyArray(blockDataArray, tempData);
            EditorMap.DrawBlocks(blockDataArray, mapName);
        }

        public void Clear()
        {
            blockDataArray = new BlockData[gridModel.gridX, gridModel.gridY, gridModel.layers];
        }

        public void DrawBlocks()
        {
            EditorMap.DrawBlocks(blockDataArray, mapName);
        }


        public void SetActive()
        {
            active = true;
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
                if (blockDataArray[x, y, z] == null || !blockDataArray[x, y, z].placed)
                {
                    blockDataArray[x, y, z] = new BlockData();
                    if (!blockDataArray[x, y, z].placed)
                    {
                        blockDataArray[x, y, z].placed = true;
                        blockDataArray[x, y, z].blockType = blockType;
                        EditorMap.BlockAdded(x, y, z, blockDataArray[x, y, z]);
                    }
                    return blockDataArray[x, y, z];
                }

            }
            return null;
        }

        public void UpdateGrid(GameObject g)
        {
            Vector3 raw = g.transform.position;
            int x = (int)(raw.x - transform.position.x);
            int y = (int)(raw.y - transform.position.y);
            int z = (int)(raw.z + 1 - transform.position.z);

            EditorMap.BlockUpdated(blockDataArray[x, y, z], g);
        }

        public void GenerateColliders()
        {
            EditorMap.GenerateColliders(blockDataArray, mapName);
        }

        public void GenerateMesh()
        {
            EditorMap.GenerateMesh(blockDataArray, mapName);
        }

        public void DeleteBlock(GameObject g)
        {
            Vector3 raw = g.transform.position;
            int x = (int)(raw.x - transform.position.x);
            int y = (int)(raw.y - transform.position.y);
            int z = (int)(raw.z + 1 - transform.position.z);
            blockDataArray[x, y, z] = null;
            EditorMap.BlockDeleted(x, y, z, g);
        }


        public void OnBeforeSerialize()
        {
            if (blockDataArray != null)
                serializedMapData = GenerateSaveData();
        }

        public void OnAfterDeserialize()
        {
            attemptedDeserialize = true;
            if (editorMap != null && gridRenderer != null)
            {
                Load(serializedMapData, false);
            }
            else deserializeFailed = true;
        }

        public void Load(MapSaveData saveData, bool reDraw)
        {
            blockDataArray = saveData.Get3DData();
            Debug.Log("Load. Blockdata null? " + (blockDataArray == null));
            if (reDraw) EditorMap.DrawBlocks(blockDataArray, mapName);
            else EditorMap.ReAttachBlocks(blockDataArray, transform.position, mapName);
            gridModel = new GridData(false, saveData.x, saveData.y, saveData.z, saveData.selectedLayer);
            GridRenderer.SetDataModel(gridModel);
        }

        public void RecreateMap()
        {
            blockDataArray = new BlockData[gridModel.gridX, gridModel.gridY, gridModel.layers];
            EditorMap.RecreateMap(blockDataArray, transform.position, name);
        }

        public MapSaveData GenerateSaveData()
        {
            return new MapSaveData(blockDataArray, gridModel.SelectedLayer);
        }

        public void EnsureGridModel()
        {
            if (GridRenderer.HasModel())
            {
                GridRenderer.SetDataModel(gridModel);
            }
        }

        public void OnBeforeEnterPlayMode()
        {
            inPlayMode = true;
            Debug.Log("Map: " + name + " entering play mode");
            SaveFile(Application.persistentDataPath + "/temp/" + name + "_temp.map");
        }

        public void OnAfterExitPlayMode()
        {
            inPlayMode = false;
            Debug.Log("Map: " + name + " exiting play mode");
            LoadTempData();
            EditorMap.ReAttachBlocks(blockDataArray, transform.position, mapName);
            GridRenderer.SetDataModel(gridModel);
        }

        public void LoadTempData()
        {
            Debug.Log("Loading temp data");
            MapSaveData tempData = LoadFile(Application.persistentDataPath + "/temp/" + name + "_temp.map");
            blockDataArray = tempData.Get3DData();
            gridModel = new GridData(true, tempData.x, tempData.y, tempData.z, tempData.selectedLayer);
        }

        public void SaveFile(string path)
        {
            if (path.Length != 0)
            {
                BinaryFormatter bf = new BinaryFormatter();
                if (!Directory.Exists(Application.persistentDataPath + "/temp")) Directory.CreateDirectory(Application.persistentDataPath + "/temp");
                FileStream file = File.Create(path);
                bf.Serialize(file, GenerateSaveData());
                file.Close();
            }
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
                SaveFile(Application.persistentDataPath + "/temp/" + name + "_temp.map");
            }
        }
    }
}