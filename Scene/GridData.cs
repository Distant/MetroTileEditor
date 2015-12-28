using System;

namespace MetroTileEditor.Renderers
{
    [Serializable]
    public class GridData
    {
        public bool gridEnabled;
        public int gridX;
        public int gridY;
        public int layers;
        private int selectedLayer;
        public int SelectedLayer
        {
            get { return selectedLayer; }
            set
            {
                selectedLayer = value;
                if (selectedLayer < 0) selectedLayer = 0;
                else if (selectedLayer > layers - 1) selectedLayer = layers - 1;
            }
        }

        public GridData(bool gridEnabled, int gridX, int gridY, int layers, int selectedLayer)
        {
            this.gridEnabled = gridEnabled;
            this.gridX = gridX;
            this.gridY = gridY;
            this.layers = layers;
            this.selectedLayer = selectedLayer;
        }
    }
}