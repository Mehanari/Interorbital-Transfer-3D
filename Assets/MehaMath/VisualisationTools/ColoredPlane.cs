using UnityEngine;

namespace MehaMath.VisualisationTools
{
    public class ColoredPlane : MonoBehaviour
    {
        [SerializeField] private Color defaultVertexColor;
        [SerializeField] private MeshFilter meshFilter;
        private Mesh _mesh;
        private int _xDots;
        private int _yDots;

        public void Initialize(float length, float width, int xDots, int yDots)
        {
            _xDots = xDots;
            _yDots = yDots;
            meshFilter.mesh = MeshMaker.GetPlaneMesh(length, width, xDots, yDots, defaultVertexColor);
        }
        
        public void SetDotColor(int x, int y, Color color)
        {
            if (x < 0 || x >= _xDots || y < 0 || y >= _yDots)
            {
                Debug.LogError("PlotSingleDot index must be in [0, dots count)");
                return;
            }
            var colors = _mesh.colors;
            colors[x * _yDots + y] = color;
            _mesh.colors = colors;
        }
    }
}
