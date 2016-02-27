using System;
using UnityEngine;

namespace MetroTileEditor.Renderers
{
    [Serializable]
    public class GridData
    {
        public bool gridEnabled;
        public int sizeX;
        public int sizeY;
        public int sizeZ;
        [SerializeField]
        private int selectedLayer;
        public int SelectedLayer
        {
            get { return selectedLayer; }
            set
            {
                selectedLayer = value;
                if (selectedLayer < 0) selectedLayer = 0;
                else if (selectedLayer > sizeZ - 1) selectedLayer = sizeZ - 1;
            }
        }

        public GridData(bool gridEnabled, int gridX, int gridY, int layers, int selectedLayer)
        {
            this.gridEnabled = gridEnabled;
            this.sizeX = gridX;
            this.sizeY = gridY;
            this.sizeZ = layers;
            this.selectedLayer = selectedLayer;
        }
    }
}