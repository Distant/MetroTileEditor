using UnityEngine;
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
            FillTexture,
            ReplaceTexture,
            RotateTexture,
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
        {KeyCode.B, EditMode.RotateBlocks },
        {KeyCode.T, EditMode.RotateTexture }
    };

        private Map currentMapObj;
        private bool editing;
        private bool creatingNew;
        private string newName;

        private string selectedMaterialId;

        public EditMode editMode = EditMode.PlaceBlocks;
        public EditMode lastPaintMode;
        private string selectedBlockType = "CubeBlock";

        [SerializeField]
        public string loadedMapPath;

        private int newX;
        private int newY;
        private int newZ;

        private Vector2 gridPoint;
        private GameObject mouseOverObj;
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
                foreach (Map map in FindObjectsOfType<Map>()) map.OnBeforeEnterPlayMode();
            }

            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                Debug.Log("Exited Play Mode");
                TextureManager.FindMaterials();
                foreach (Map map in FindObjectsOfType<Map>()) { map.OnAfterExitPlayMode(); }
                OnEnable();
            }
        }

        void OnEnable()
        {
            EditorApplication.playmodeStateChanged += OnPlayModeChanged;

            PrefabManager.RefreshPrefabs();

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
            if (newMode == EditMode.PaintBlocks || newMode == EditMode.ReplaceTexture || newMode == EditMode.FillTexture)
            {
                lastPaintMode = newMode;
            }
        }

        public void CheckMapSelection()
        {
            if (Selection.activeObject != null && Selection.activeObject is GameObject)
            {
                if (currentMapObj == null || Selection.activeObject != currentMapObj.gameObject)
                {
                    Map map = Selection.activeGameObject.GetComponent<Map>();
                    if (map)
                    {
                        if (currentMapObj) currentMapObj.SetInactive();
                        map.SetActive();
                        currentMapObj = map;
                        loadedMapPath = string.Empty;
                        editing = true;
                        BlockEditWindow.currentData = null;
                    }
                    else { if (currentMapObj) currentMapObj.SetInactive(); currentMapObj = null; }
                    editing = false;
                    BlockEditWindow.currentData = null;
                }

                if (currentMapObj != null && Selection.activeObject == currentMapObj.gameObject)
                {
                    editing = true;
                }
            }
            else
            {
                if (currentMapObj != null) currentMapObj.SetInactive();
                currentMapObj = null;
                editing = false;
                BlockEditWindow.currentData = null;
            }
        }

        void OnGUI()
        {
            CheckMapSelection();

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
                    MeshGenerator.GenerateColliders(currentMapObj.blockArray, currentMapObj.mapName);
                }

                if (GUILayout.Button("Generate Mesh"))
                {
                    MeshGenerator.GenerateMesh(currentMapObj.blockArray, currentMapObj.mapName);
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

                if (GUILayout.Button("Undo"))
                {
                    currentMapObj.Undo();
                }

                if (GUILayout.Button("Redo"))
                {
                    currentMapObj.Redo();
                }

                if (GUILayout.Button("Trim Map"))
                {
                    currentMapObj.TrimMap();
                }

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

                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.padding = new RectOffset(0, 0, 0, 0);

                // texture stuff
                if (editMode == EditMode.PaintBlocks || editMode == EditMode.ReplaceTexture || editMode == EditMode.FillTexture)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Selected Texture");
                    GUILayout.Label(TextureManager.GetMaterialPreviewOrDefault(selectedMaterialId));
                    GUILayout.EndHorizontal();

                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                    GUILayout.BeginHorizontal();
                    int index = 0;
                    foreach (string id in TextureManager.Materials.Keys)
                    {
                        GUIContent c = new GUIContent();
                        if (TextureManager.Previews.ContainsKey(id))
                        {
                            c.image = TextureManager.Previews[id];
                        }
                        if (GUILayout.Button(c, style, GUILayout.Width(25), GUILayout.Height(25))) { selectedMaterialId = id; editMode = EditMode.PaintBlocks; }
                        index++;
                        if (index % 11 == 0) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                    }
                    GUILayout.EndHorizontal();

                    EditorGUILayout.EndScrollView();
                }

                // block stuff
                if (editMode == EditMode.PlaceBlocks)
                {
                    GUILayout.Label("Selected Block: " + selectedBlockType);
                    GUILayout.BeginHorizontal();
                    int i = 0;
                    foreach (GameObject g in PrefabManager.blockPrefabs)
                    {
                        GUIContent c = new GUIContent();
                        c.image = PrefabManager.blockPreviews[i];
                        if (GUILayout.Button(c, style, GUILayout.Width(25), GUILayout.Height(25))) { selectedBlockType = g.name; editMode = EditMode.PlaceBlocks; }
                        i++;
                    }
                    GUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Refresh Resources")) { TextureManager.FindMaterials(); PrefabManager.RefreshPrefabs(); }

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
                if (GUILayout.Button("Edit")) { editing = true; currentMapObj.SetActive(); }
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
            CheckMapSelection();

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
                                        case EditMode.PaintBlocks: block.SetMaterial(hit, selectedMaterialId); currentMapObj.UpdateBlock(block.gameObject); break;
                                        case EditMode.FillTexture: block.SetAllMaterials(selectedMaterialId, false); currentMapObj.UpdateBlock(block.gameObject); break;
                                        case EditMode.ReplaceTexture: block.ReplaceMaterial(hit, selectedMaterialId); currentMapObj.UpdateBlock(block.gameObject); break;
                                        case EditMode.RotateTexture: block.RotateTexture(hit); break;
                                        case EditMode.PlaceBlocks: PlaceBlock(hitDirection, mouseOverObj); break;
                                        case EditMode.DeleteBlocks: currentMapObj.DeleteBlock(mouseOverObj); break;
                                        case EditMode.ColourPick: selectedMaterialId = GetMaterialId(hit, mouseOverObj); editMode = lastPaintMode; break;
                                        case EditMode.SelectBlock: SelectBlock(block); break;
                                    }
                                }
                            }

                            else if (editMode == EditMode.PlaceBlocks && mouseOverObj == currentMapObj.GridRenderer.gameObject && currentMapObj.GridEnabled)
                            {
                                if (Event.current.type == EventType.MouseDown)
                                {
                                    gridPoint = new Vector2((int)(hit.point.x), (int)(hit.point.y));
                                    currentMapObj.AddBlock(new Vector3(gridPoint.x, gridPoint.y, currentMapObj.SelectedLayer), selectedBlockType);
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
            currentMapObj.AddBlock(new Vector3(newRaw.x, newRaw.y, (newRaw.z + 1)), selectedBlockType);
        }

        private string GetMaterialId(RaycastHit hit, GameObject g)
        {
            return g.GetComponent<Block>().GetMaterialID(hit);
        }

        public void New(string name)
        {
            GameObject map = (GameObject)Instantiate(Resources.Load("MapObject"));
            map.name = name;
            map.GetComponent<Map>().init();
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
                        "new_map" + ".map2",
                        "map2");

            SaveFile(path);
        }

        public void SaveFile(string path)
        {
            if (path.Length != 0)
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(path);
                currentMapObj.blockArray.Clean();
                bf.Serialize(file, currentMapObj.blockArray);
                file.Close();
                loadedMapPath = path;
            }
        }

        public void Load()
        {
            var path = EditorUtility.OpenFilePanel(
                        "Select Map",
                        Application.persistentDataPath,
                        "map2");

            if (File.Exists(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(path, FileMode.Open);
                BlockDataArray saveData = (BlockDataArray)bf.Deserialize(file);
                file.Close();

                loadedMapPath = path;
                try
                {
                    saveData.Clean();
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