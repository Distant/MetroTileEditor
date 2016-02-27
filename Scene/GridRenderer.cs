using UnityEngine;

namespace MetroTileEditor.Renderers
{
    public class GridRenderer : MonoBehaviour
    {
        private Material lineMaterial;
        private GridData model;
        private BoxCollider col;
        public bool HasModel()
        {
            return model != null;
        }

        void Awake()
        {
            col = GetComponent<BoxCollider>();
        }

        public void SetDataModel(GridData model)
        {
            col = GetComponent<BoxCollider>();
            this.model = model;
            DataModelChanged();
        }

        public void DataModelChanged()
        {
            col.size = new Vector3(model.sizeX, 0, model.sizeY);
            col.center = new Vector3(((float)model.sizeX) / 2, -1 * ((float)model.SelectedLayer), ((float)model.sizeY) / 2);
            if (!model.gridEnabled) col.enabled = false;
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
            if (model != null && model.gridEnabled)
            {
                CreateLineMaterial();
                // Apply the line material
                lineMaterial.SetPass(0);

                // Draw lines
                GL.Begin(GL.LINES);
                GL.Color(Color.grey);
                for (int i = 0; i < model.sizeX + 1; ++i)
                {
                    GL.Vertex3(i + transform.position.x, model.sizeY + transform.position.y, model.SelectedLayer);
                    GL.Vertex3(i + transform.position.x, transform.position.y, model.SelectedLayer);
                }

                for (int i = 0; i < model.sizeY + 1; ++i)
                {
                    GL.Vertex3(model.sizeX + transform.position.x, i + transform.position.y, model.SelectedLayer);
                    GL.Vertex3(0 + transform.position.x, i + transform.position.y, model.SelectedLayer);
                }

                GL.End();
            }
        }
    }
}