using UnityEngine;

namespace MetroTileEditor.Utils
{
    public class SceneUtils
    {
        public static T GetScenePrefabComponent<T>(string name, T obj, out bool didCreate) where T : Component
        {
            GameObject go = FindScenePrefab(name);
            if (obj == null || !obj.gameObject.activeInHierarchy)
            {
                didCreate = true;
                return go.GetComponent<T>();
            }
            else
            {
                didCreate = false; return obj;
            }
        }

        public static T GetScenePrefabComponent<T>(string name, T obj) where T : Component
        {
            GameObject go = FindScenePrefab(name);
            if (obj == null || !obj.gameObject.activeInHierarchy)
            {
                return go.GetComponent<T>();
            }
            else
            {
                return obj;
            }
        }

        public static T GetChildComponent<T>(string name, T obj, GameObject parent) where T : Component
        {
            if (obj == null || !obj.gameObject.activeInHierarchy)
            {
                Transform[] children = parent.GetComponentsInChildren<Transform>();
                foreach (Transform t in children)
                {
                    if (t.name == name)
                    {
                        T found = t.GetComponent<T>();
                        if (found) return found;
                    }
                }
                GameObject go;
                go = (GameObject)GameObject.Instantiate(Resources.Load(name));
                go.name = name;
                go.transform.parent = parent.transform;
                return go.GetComponent<T>();
            }
            else
            {
                return obj;
            }
        }

        public static GameObject FindScenePrefab(string name)
        {
            GameObject go;
            if (!(go = GameObject.Find(name)))
            {
                go = (GameObject)GameObject.Instantiate(Resources.Load(name));
                go.name = name;
            }
            return go;
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

        public static FaceDirection GetHitDirection(RaycastHit hit)
        {
            float test = Vector3.Dot(hit.normal, new Vector3(0, 0, 1));
            if (test > 0.9f)
                return FaceDirection.Back;
            else if (test < -0.9f)
                return FaceDirection.Front;

            test = Vector3.Dot(hit.normal, new Vector3(1, 0, 0));
            if (test > 0.9f)
                return FaceDirection.Right;
            else if (test < -0.9f)
                return FaceDirection.Left;

            test = Vector3.Dot(hit.normal, new Vector3(0, 1, 0));
            if (test > 0.9f)
                return FaceDirection.Top;
            else if (test < -0.9f)
                return FaceDirection.Bottom;

            return FaceDirection.Top;
        }

        public static Vector3 GetOffset(FaceDirection dir)
        {
            switch (dir)
            {
                case FaceDirection.Bottom:
                    return new Vector3(0, -0.5f, 0);

                case FaceDirection.Top:
                    return new Vector3(0, 0.5f, 0);

                case FaceDirection.Right:
                    return new Vector3(0.5f, 0, 0);

                case FaceDirection.Left:
                    return new Vector3(-0.5f, 0, 0);

                case FaceDirection.Back:
                    return new Vector3(0, 0, 0.5f);

                case FaceDirection.Front:
                    return new Vector3(0, 0, -0.5f);
            }
            return new Vector3(0, -0.5f, 0);
        }
    }
}