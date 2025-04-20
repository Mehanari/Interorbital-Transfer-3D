using System;
using System.Collections.Generic;
using UnityEngine;

namespace MehaMath.VisualisationTools
{
    public class CustomStraightLine : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private float length = 1;
        [SerializeField] private float thickness = 1;


        private Vector3[] _upperVertices;
        private Vector3[] _lowerVertices;
        private Color[] _upperColors;
        private Color[] _lowerColors;
        private Mesh _mesh;

        public void Initialize()
        {
            _mesh = new Mesh
            {
                name = "CustomStraightLine"
            };
            meshFilter.mesh = _mesh;

            
            _upperVertices = new Vector3[2];
            _lowerVertices = new Vector3[2];
            _upperVertices[0] = new Vector3(0, thickness, 0);
            _upperVertices[1] = new Vector3(length, thickness, 0);
            _lowerVertices[0] = new Vector3(0, 0, 0);
            _lowerVertices[1] = new Vector3(length, 0, 0);
            
            UpdateMeshGeometry();
            
            _upperColors = new Color[2];
            _lowerColors = new Color[2];
        }

        private void OnValidate()
        {
            if (_mesh == null)
            {
                return;
            }
            if (length < 0)
            {
                length = 0;
            }

            if (thickness < 0)  
            {
                thickness = 0;
            }
            SetLength(length);
            SetThickness(thickness);
        }

        public void SetLength(float length)
        {
            if (length < 0)
            {
                Debug.LogError("Length can't be negative");
                return;
            }

            var rightMostUpperVertex = _upperVertices[^1];
            var rightMostLowerVertex = _lowerVertices[^1];
            rightMostUpperVertex.x = length;
            rightMostLowerVertex.x = length;
            _upperVertices[^1] = rightMostUpperVertex;
            _lowerVertices[^1] = rightMostLowerVertex;
            UpdateMeshGeometry();
        }
        
        public void SetThickness(float thickness)
        {
            if (thickness < 0)
            {
                Debug.LogError("Thickness can't be negative");
                return;
            }

            for (int i = 0; i < _upperVertices.Length; i++)
            {
                var upperVertex = _upperVertices[i];
                upperVertex.y = thickness;
                _upperVertices[i] = upperVertex;
                var lowerVertex = _lowerVertices[i];
                lowerVertex.y = 0;
                _lowerVertices[i] = lowerVertex;
            }
            UpdateMeshGeometry();
        }

        public void AddDot(float relativeX)
        {
            if (relativeX < 0 || relativeX > length)
            {
                Debug.LogError("Relative x must be in [0, length]");
                return;
            }

            var newDotIndex = 0;
            for (int i = 0; i < _upperVertices.Length; i++)
            {
                if (_upperVertices[i].x > relativeX)
                {
                    newDotIndex = i;
                    break;
                }
            }
            var upperVertices = new Vector3[_upperVertices.Length + 1];
            var lowerVertices = new Vector3[_lowerVertices.Length + 1];
            Array.Copy(_upperVertices, upperVertices, newDotIndex);
            Array.Copy(_upperVertices, newDotIndex, upperVertices, newDotIndex + 1, _upperVertices.Length - newDotIndex);
            Array.Copy(_lowerVertices, lowerVertices, newDotIndex);
            Array.Copy(_lowerVertices, newDotIndex, lowerVertices, newDotIndex + 1, _lowerVertices.Length - newDotIndex);
            upperVertices[newDotIndex] = new Vector3(relativeX, thickness, 0);
            lowerVertices[newDotIndex] = new Vector3(relativeX, 0, 0);
            _upperVertices = upperVertices;
            _lowerVertices = lowerVertices;
            
            var newUpperColors = new Color[_upperColors.Length + 1];
            var newLowerColors = new Color[_lowerColors.Length + 1];
            Array.Copy(_upperColors, newUpperColors, newDotIndex);
            Array.Copy(_upperColors, newDotIndex, newUpperColors, newDotIndex + 1, _upperColors.Length - newDotIndex);
            Array.Copy(_lowerColors, newLowerColors, newDotIndex);
            Array.Copy(_lowerColors, newDotIndex, newLowerColors, newDotIndex + 1, _lowerColors.Length - newDotIndex);
            _upperColors = newUpperColors;
            _lowerColors = newLowerColors;
            
            UpdateMeshGeometry();
            UpdateMeshColors(); 
        }

        public void SetDotColor(int dotIndex, Color color)
        {
            if (dotIndex < 0 || dotIndex >= _upperVertices.Length)
            {
                Debug.LogError("PlotSingleDot index must be in [0, dots count)");
                return;
            }
            _upperColors[dotIndex] = color;
            _lowerColors[dotIndex] = color;
            UpdateMeshColors();
        }

        private void UpdateMeshGeometry()
        {
            var allVertices = new Vector3[_upperVertices.Length + _lowerVertices.Length];
            Array.Copy(_upperVertices, allVertices, _upperVertices.Length);
            Array.Copy(_lowerVertices, 0, allVertices, _upperVertices.Length, _lowerVertices.Length);
            _mesh.vertices = allVertices;
            List<int> triangles = new List<int>();
            for (int i = 0; i < _upperVertices.Length - 1; i++)
            {
                //Making lower left triangle
                var lowerLeftVertexIndex = _upperVertices.Length + i;
                var upperLeftVertexIndex = i;
                var lowerRightVertexIndex = _upperVertices.Length + i + 1;
                triangles.Add(lowerLeftVertexIndex);
                triangles.Add(upperLeftVertexIndex);
                triangles.Add(lowerRightVertexIndex);
                //Making upper right triangle
                var upperRightVertexIndex = i + 1;
                triangles.Add(upperLeftVertexIndex);
                triangles.Add(upperRightVertexIndex);
                triangles.Add(lowerRightVertexIndex);
            }
            _mesh.triangles = triangles.ToArray();
            var normals = new Vector3[allVertices.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.forward;
            }
            _mesh.normals = normals;
        }

        private void UpdateMeshColors()
        {
            var colors = new Color[_upperColors.Length + _lowerColors.Length];
            Array.Copy(_upperColors, colors, _upperColors.Length);
            Array.Copy(_lowerColors, 0, colors, _upperColors.Length, _lowerColors.Length);
            _mesh.colors = colors;
        }
    }
}
