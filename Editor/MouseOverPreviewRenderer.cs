using MetroTileEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace MetroTileEditor.Renderers
{
    public static class MouseOverPreviewRenderer
    {
        public static void MouseOverPaint(RaycastHit hit, FaceDirection hitDirection)
        {
            Handles.color = Color.green;
            Handles.RectangleCap(0, hit.collider.transform.position + SceneUtils.GetOffset(hitDirection), Quaternion.LookRotation(hit.normal, Vector3.up), 0.5f);
        }

        public static void MouseOverPlace(MeshRenderer cubePreview, RaycastHit hit, FaceDirection hitDirection)
        {
            Handles.color = Color.red;
            cubePreview.enabled = true;
            cubePreview.transform.position = hit.collider.transform.position + SceneUtils.GetOffset(hitDirection) * 2;
        }

        public static void MouseOverDelete(RaycastHit hit, FaceDirection hitDirection)
        {
            Handles.color = Color.red;
            Handles.RectangleCap(0, hit.collider.transform.position + SceneUtils.GetOffset(hitDirection), Quaternion.LookRotation(hit.normal, Vector3.up), 0.5f);
        }

        public static void MouseOverPick(RaycastHit hit, FaceDirection hitDirection)
        {
            Handles.color = Color.blue;
            Handles.RectangleCap(0, hit.collider.transform.position + SceneUtils.GetOffset(hitDirection), Quaternion.LookRotation(hit.normal, Vector3.up), 0.5f);
        }
    }
}