using UnityEngine;

namespace MetroTileEditor.Renderers
{
    public class GridRenderer : MonoBehaviour
    {
        private Material lineMaterial;
        private GridData m;
        private BoxCollider col;
        public bool HasModel()
        {
            return m != null;
        }

        void Awake()
        {
            col = GetComponent<BoxCollider>();
        }

        public void SetDataModel(GridData model)
        {
            col = GetComponent<BoxCollider>();
            m = model;
            DataModelChanged();
        }

        public void DataModelChanged()
        {
            col.size = new Vector3(m.gridX, 0, m.gridY);
            col.center = new Vector3(m.gridX / 2, -1 * m.SelectedLayer, m.gridY / 2);
            if (!m.gridEnabled) col.enabled = false;
            else col.enabled = true;
        }

        void CreateLineMaterial()
        {

            if (!lineMaterial)
            {
                var shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        // Will be called after all regular rendering is done
        void OnDrawGizmos()
        {
            if (m != null && m.gridEnabled)
            {
                CreateLineMaterial();
                // Apply the line material
                lineMaterial.SetPass(0);

                // Draw lines
                GL.Begin(GL.LINES);
                GL.Color(Color.grey);
                for (int i = 0; i < m.gridX + 1; ++i)
                {
                    GL.Vertex3(i + transform.position.x, m.gridY + transform.position.y, m.SelectedLayer);
                    GL.Vertex3(i + transform.position.x, transform.position.y, m.SelectedLayer);
                }

                for (int i = 0; i < m.gridY + 1; ++i)
                {
                    GL.Vertex3(m.gridX + transform.position.x, i + transform.position.y, m.SelectedLayer);
                    GL.Vertex3(0 + transform.position.x, i + transform.position.y, m.SelectedLayer);
                }

                GL.End();
            }
        }
    }
}