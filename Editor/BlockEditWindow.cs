using UnityEngine;
using UnityEditor;

namespace MetroTileEditor.Editors
{
    [InitializeOnLoad]
    public class BlockEditWindow : EditorWindow
    {
        public static BlockData currentData;

        [MenuItem("Window/BlockEditor")]
        static void Init()
        {
            EditorWindow window = GetWindow<BlockEditWindow>();
            GUIContent content = new GUIContent();
            content.text = "Block Editor";
            window.titleContent = content;
            window.autoRepaintOnSceneChange = true;
            window.Show();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnEnable()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;

        }

        void OnDisable()
        { 
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        void OnGUI()
        {
            if (currentData != null)
            {
                GUILayout.Label(currentData.blockType);
                currentData.breakable = EditorGUILayout.Toggle("Breakable", currentData.breakable);
                currentData.isTriggerOnly = EditorGUILayout.Toggle("Is Trigger (no collision)", currentData.isTriggerOnly);
                currentData.excludeFromMesh = EditorGUILayout.Toggle("Exclude from mesh", currentData.excludeFromMesh);
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {

        }
    }
}
