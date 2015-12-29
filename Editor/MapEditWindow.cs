﻿using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using MetroTileEditor.Utils;
using MetroTileEditor.Generation;
using MetroTileEditor.Renderers;

namespace MetroTileEditor.Editors
{
    [InitializeOnLoad]
    public class MapEditWindow : EditorWindow
    {
        public enum EditMode
        {
            Disabled,
            PlaceBlocks,
            RotateBlocks,
            PaintBlocks,
            DeleteBlocks,
            ColourPick,
            SelectBlock
        }

        private Dictionary<KeyCode, EditMode> keyCodes = new Dictionary<KeyCode, EditMode>()
    {
        {KeyCode.C, EditMode.ColourPick },
        {KeyCode.Q, EditMode.Disabled },
        {KeyCode.V, EditMode.PaintBlocks },
        {KeyCode.U, EditMode.DeleteBlocks },
        {KeyCode.G, EditMode.PlaceBlocks },
        {KeyCode.M, EditMode.SelectBlock },
        {KeyCode.B, EditMode.RotateBlocks }
    };

        private MapObject currentMapObj;
        private bool editing;
        private bool creatingNew;
        private string newName;

        private string selectedMaterialId;

        public EditMode editMode = EditMode.PlaceBlocks;
        private GameObject[] blockPrefabs;
        private string selectedBlockType = "CubeBlock";

        [SerializeField]
        public string loadedMapPath;

        private int newX;
        private int newY;
        private int newZ;

        private Vector2 gridPoint;
        private GameObject mouseOverObj;
        private static BlockData lastPlaced;
        private Vector2 mousePoint;
        private FaceDirection hitDirection = FaceDirection.None;

        private MeshRenderer cubePreview;

        Tool lastTool = Tool.None;

        public Vector2 scrollPosition;

        [MenuItem("Window/MapEditor")]
        static void Init()
        {
            TextureManager.FindMaterials();
            EditorWindow window = GetWindow<MapEditWindow>();
            GUIContent content = new GUIContent();
            content.text = "Map Editor";
            window.titleContent = content;
            window.autoRepaintOnSceneChange = true;
            window.Show();
        }

        void OnPlayModeChanged()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                Debug.Log("Entering Play Mode");
                foreach (MapObject map in FindObjectsOfType<MapObject>()) map.OnBeforeEnterPlayMode();
            }

            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                Debug.Log("Exited Play Mode");
                TextureManager.FindMaterials();
                foreach (MapObject map in FindObjectsOfType<MapObject>()) { map.OnAfterExitPlayMode(); }
                OnEnable();
            }
        }

        void OnEnable()
        {
            EditorApplication.playmodeStateChanged += OnPlayModeChanged;

            blockPrefabs = Resources.LoadAll<GameObject>("Blocks/Prefabs") as GameObject[];

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;

            lastTool = Tools.current;
            Tools.current = Tool.None;

            //foreach (MapObject map in FindObjectsOfType<MapObject>()) { Debug.Log("starting up"); map.OnStartup(); }
        }

        void OnDisable()
        {
            EditorApplication.playmodeStateChanged -= OnPlayModeChanged;
            Tools.current = lastTool;
            editing = false;
        }

        private void GetCubePreivew()
        {
            cubePreview = SceneUtils.GetScenePrefabComponent("BlockShadow", cubePreview);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnEditModeChanged(EditMode newMode)
        {
            if (newMode == EditMode.Disabled) Tools.current = lastTool;
            else lastTool = Tools.current;
        }

        void OnGUI()
        {
            if (Selection.activeObject != null && Selection.activeObject is GameObject)
            {
                if (currentMapObj == null || Selection.activeObject != currentMapObj.gameObject)
                {
                    MapObject map = Selection.activeGameObject.GetComponent<MapObject>();
                    if (map)
                    {
                        if (currentMapObj) currentMapObj.SetInactive();
                        map.SetActive();
                        currentMapObj = map;
                        loadedMapPath = string.Empty;
                        editing = false;
                        BlockEditWindow.currentData = null;
                    }
                    else currentMapObj = null;
                    editing = false;
                    BlockEditWindow.currentData = null;
                }
            }
            else
            {
                if (currentMapObj != null) currentMapObj.SetInactive();
                currentMapObj = null;
                editing = false;
                BlockEditWindow.currentData = null;
            }

            if (editing && currentMapObj != null)
            {
                if (Event.current.type == EventType.keyDown)
                {
                    foreach (KeyCode key in keyCodes.Keys)
                    {
                        if (Event.current.keyCode == key) { editMode = keyCodes[key]; }
                    }
                }

                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Place Tiles", EditorStyles.boldLabel);
                if (GUILayout.Button("Reset"))
                {
                    currentMapObj.DrawBlocks();
                }

                if (GUILayout.Button("Clear Array"))
                {
                    currentMapObj.Clear();
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Generate Colliders"))
                {
                    MeshGenerator.GenerateColliders(currentMapObj.blockDataArray, currentMapObj.mapName);
                }

                if (GUILayout.Button("Generate Mesh"))
                {
                    MeshGenerator.GenerateMesh(currentMapObj.blockDataArray, currentMapObj.mapName);
                }

                if (GUILayout.Button("Recreate Map"))
                {
                    currentMapObj.RecreateMap();
                }

                GUILayout.EndHorizontal();

                // block placement stuff
                EditorGUILayout.Vector2Field("Mouse Location", mousePoint);

                EditMode newEditMode = (EditMode)GUILayout.SelectionGrid((int)editMode, Enum.GetNames(typeof(EditMode)), 3);
                if (newEditMode != editMode)
                {
                    OnEditModeChanged(newEditMode);
                    editMode = newEditMode;
                }

                EditorGUILayout.TextField("Selected Object", mouseOverObj == null ? "Nothing Selected" : mouseOverObj.name);

                //grid stuff
                currentMapObj.GridEnabled = EditorGUILayout.BeginToggleGroup("Enable Grid", currentMapObj.GridEnabled);

                EditorGUILayout.Vector2Field("Grid Point", gridPoint);
                EditorGUILayout.IntField("Layer", currentMapObj.SelectedLayer);

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();

                newX = EditorGUILayout.IntField(newX);
                newY = EditorGUILayout.IntField(newY);
                newZ = EditorGUILayout.IntField(newZ);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Set Grid Size"))
                {
                    currentMapObj.SetGridSize(newX, newY, newZ);
                }

                EditorGUILayout.EndToggleGroup();

                // texture stuff
                GUILayout.BeginHorizontal();
                GUILayout.Label("Selected Texture");
                GUILayout.Label(TextureManager.GetMaterialOrDefault(selectedMaterialId).mainTexture);
                GUILayout.EndHorizontal();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.padding = new RectOffset(0, 0, 0, 0);

                GUILayout.BeginHorizontal();
                int index = 0;
                foreach (string id in TextureManager.Materials.Keys)
                {
                    GUIContent c = new GUIContent();
                    c.image = TextureManager.Materials[id].mainTexture;
                    if (GUILayout.Button(c, style, GUILayout.Width(25), GUILayout.Height(25))) { selectedMaterialId = id; editMode = EditMode.PaintBlocks; }
                    index++;
                    if (index % 11 == 0) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();

                // block stuff
                GUILayout.Label("Selected Block: " + selectedBlockType);
                GUILayout.BeginHorizontal();
                foreach (GameObject g in blockPrefabs)
                {
                    GUIContent c = new GUIContent();
                    c.image = AssetPreview.GetAssetPreview(g);
                    if (GUILayout.Button(c, style, GUILayout.Width(25), GUILayout.Height(25))) { selectedBlockType = g.name; editMode = EditMode.PlaceBlocks; }
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Refresh Materials")) TextureManager.FindMaterials();

                // saving/loading stuff

                EditorGUILayout.Space();
                //if (mapNames.Length != 0) loadedMapPath = mapNames[EditorGUILayout.Popup("Map: ", Array.IndexOf(mapNames, loadedMapPath), mapNames)];
                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();

                //if (GUILayout.Button("New")) creatingNew = true;

                if (GUILayout.Button("Save Map")) Save();
                if (GUILayout.Button("Save As...")) SaveAs();
                if (GUILayout.Button("Load Map")) Load();

                GUILayout.EndHorizontal();
            }

            else if (currentMapObj != null)
            {
                if (GUILayout.Button("Edit")) editing = true;
            }
            else
            {
                if (GUILayout.Button("New")) creatingNew = true;
                if (creatingNew)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Enter map name: ");
                    newName = EditorGUILayout.TextField(newName);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Okay")) { New(newName); creatingNew = false; }
                    if (GUILayout.Button("Cancel")) creatingNew = false;
                    GUILayout.EndHorizontal();
                } 
            }
            GUILayout.Label("Loaded Map: " + Path.GetFileName(loadedMapPath));
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (Selection.activeObject != null && Selection.activeObject is GameObject)
            {
                if (currentMapObj == null || Selection.activeObject != currentMapObj.gameObject)
                {
                    MapObject map = Selection.activeGameObject.GetComponent<MapObject>();
                    if (map)
                    {
                        if (currentMapObj) currentMapObj.SetInactive();
                        map.SetActive();
                        currentMapObj = map;
                        loadedMapPath = string.Empty;
                        editing = false;
                        BlockEditWindow.currentData = null;
                    }
                    if (currentMapObj) currentMapObj.SetInactive();
                    else currentMapObj = null;
                    editing = false;
                    BlockEditWindow.currentData = null;
                }
            }
            else
            {
                if (currentMapObj != null) currentMapObj.SetInactive();
                currentMapObj = null;
                editing = false;
                BlockEditWindow.currentData = null;
            }

            if (editing)
            {
                if (Event.current.type == EventType.keyDown)
                {
                    foreach (KeyCode key in keyCodes.Keys)
                    {
                        if (Event.current.keyCode == key) { editMode = keyCodes[key]; }
                    }
                }

                currentMapObj.EnsureGridModel();

                GetCubePreivew();

                mousePoint = Event.current.mousePosition;
                if (currentMapObj.GridEnabled)
                {
                    if (Event.current.type == EventType.KeyDown)
                    {
                        if (Event.current.keyCode == KeyCode.Period) currentMapObj.SelectedLayer++;
                        if (Event.current.keyCode == KeyCode.Comma) currentMapObj.SelectedLayer--;
                    }
                }
                if (editMode != EditMode.Disabled)
                {
                    Tools.current = Tool.None;
                    cubePreview.enabled = false;

                    var ray = HandleUtility.GUIPointToWorldRay(mousePoint);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 1000))
                    {
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                        hitDirection = SceneUtils.GetHitDirection(hit);
                        mouseOverObj = hit.collider.gameObject;
                        Block block = mouseOverObj.GetComponent<Block>();

                        switch (editMode)
                        {
                            case EditMode.PaintBlocks:
                                {
                                    if (block)
                                    {
                                        MouseOverPreviewRenderer.MouseOverPaint(hit, hitDirection);
                                    }

                                    break;
                                }

                            case EditMode.PlaceBlocks:
                                {
                                    if (block)
                                    {
                                        MouseOverPreviewRenderer.MouseOverPlace(cubePreview, hit, hitDirection);
                                    }

                                    else if (mouseOverObj == currentMapObj.GridRenderer.gameObject && currentMapObj.GridEnabled)
                                    {
                                        MouseOverGrid(hit.point);
                                        gridPoint = new Vector2((int)(hit.point.x - mouseOverObj.transform.position.x), (int)(hit.point.y - mouseOverObj.transform.position.y));
                                    }

                                    break;
                                }

                            case EditMode.DeleteBlocks:
                                {
                                    if (block)
                                    {
                                        MouseOverPreviewRenderer.MouseOverDelete(hit, hitDirection);
                                    }
                                    break;
                                }

                            case EditMode.ColourPick:
                                {
                                    if (block)
                                    {
                                        MouseOverPreviewRenderer.MouseOverPick(hit, hitDirection);
                                    }
                                    break;
                                }
                        }

                        if (Event.current.button == 0)
                        {
                            if (block)
                            {
                                if (Event.current.type == EventType.MouseDown || (Event.current.type == EventType.MouseDrag && editMode == EditMode.PaintBlocks))
                                {
                                    switch (editMode)
                                    {
                                        case EditMode.PaintBlocks: block.SetMaterial(hit, selectedMaterialId); currentMapObj.UpdateGrid(block.gameObject); break;
                                        case EditMode.PlaceBlocks: PlaceBlock(hitDirection, mouseOverObj); break;
                                        case EditMode.DeleteBlocks: currentMapObj.DeleteBlock(mouseOverObj); break;
                                        case EditMode.ColourPick: selectedMaterialId = GetMaterialId(hit, mouseOverObj); editMode = EditMode.PaintBlocks; break;
                                        case EditMode.SelectBlock: SelectBlock(block); break;
                                    }
                                }
                            }

                            else if (mouseOverObj == currentMapObj.GridRenderer.gameObject && currentMapObj.GridEnabled)
                            {
                                if (editMode == EditMode.PlaceBlocks && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag))
                                {
                                    lastPlaced = currentMapObj.AddBlock(hit.point.x, hit.point.y, currentMapObj.SelectedLayer, selectedBlockType);
                                }
                            }
                        }
                    }

                    else
                    {
                        mouseOverObj = null;
                        hitDirection = FaceDirection.None;
                    }
                }
            }
        }

        private void SelectBlock(Block block)
        {
            if (block != null)
                BlockEditWindow.currentData = block.data;
        }

        private void MouseOverGrid(Vector3 point)
        {
            int x = (int)point.x;
            int y = (int)point.y;
            Handles.color = Color.red;
            cubePreview.enabled = true;
            cubePreview.transform.position = new Vector3(x + 0.5f, y + 0.5f, currentMapObj.SelectedLayer - 0.5f);
        }

        private GameObject GetSelected()
        {
            return Selection.activeGameObject;
        }

        private void PlaceBlock(FaceDirection direction, GameObject g)
        {
            Vector3 newRaw = g.transform.position + SceneUtils.GetOffset(direction) * 2;
            int x = (int)newRaw.x;
            int y = (int)newRaw.y;
            int z = (int)(newRaw.z + 1);
            lastPlaced = currentMapObj.AddBlock(x, y, z, selectedBlockType);
        }

        private string GetMaterialId(RaycastHit hit, GameObject g)
        {
            return g.GetComponent<Block>().GetMaterialID(hit);
        }

        public void New(string name)
        {
            GameObject map = (GameObject)Instantiate(Resources.Load("MapObject"));
            map.name = name;
            map.GetComponent<MapObject>().init();
            Selection.activeObject = map;
        }

        public void Save()
        {
            string path;

            if (File.Exists(loadedMapPath))
            {
                path = loadedMapPath;
                SaveFile(path);
            }
            else
            {
                SaveAs();
            }
        }

        public void SaveAs()
        {
            var path = EditorUtility.SaveFilePanel(
                        "Save Map Data",
                        Application.persistentDataPath,
                        "new_map" + ".map",
                        "map");

            SaveFile(path);
        }

        public void SaveFile(string path)
        {
            if (path.Length != 0)
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(path);
                MapSaveData saveData = currentMapObj.GenerateSaveData();
                bf.Serialize(file, saveData);
                file.Close();
                loadedMapPath = path;
            }
        }

        public void Load()
        {
            var path = EditorUtility.OpenFilePanel(
                        "Select Map",
                        Application.persistentDataPath,
                        "map");

            if (File.Exists(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(path, FileMode.Open);
                MapSaveData saveData = (MapSaveData)bf.Deserialize(file);
                file.Close();

                loadedMapPath = path;
                try
                {
                    currentMapObj.Load(saveData, true);
                }
                catch (Exception e)
                {
                    Debug.Log(e.StackTrace);
                    loadedMapPath = string.Empty;
                }
            }
        }
    }
}