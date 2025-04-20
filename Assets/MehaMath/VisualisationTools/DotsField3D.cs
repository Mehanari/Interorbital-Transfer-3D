using System.Collections.Generic;
using UnityEngine;

namespace MehaMath.VisualisationTools
{
    /// <summary>
    /// This script is used to spawn and color dots in a 3D field.
    /// dotPrefab must have a MeshRenderer component.
    /// </summary>
    public class DotsField3D : MonoBehaviour
    {
        [SerializeField] private GameObject dotPrefab;
        [SerializeField] private Color defaultColor;
        
        private readonly List<GameObject> _dots = new List<GameObject>();
        private int _xDots;
        private int _yDots;
        private int _zDots;
        
        public void Initialize(float width, float height, float length, int xDots, int yDots, int zDots)
        {
            DeleteAllDots();
            _xDots = xDots;
            _yDots = yDots;
            _zDots = zDots;
            var xStep = width / (xDots - 1);
            var yStep = height / (yDots - 1);
            var zStep = length / (zDots - 1);
            for (int x = 0; x < xDots; x++)
            {
                for (int y = 0; y < yDots; y++)
                {
                    for (int z = 0; z < zDots; z++)
                    {
                        var dot = Instantiate(dotPrefab, transform);
                        dot.transform.localPosition = new Vector3(x * xStep, y * yStep, z * zStep);
                        dot.GetComponent<MeshRenderer>().material.color = defaultColor;
                        _dots.Add(dot);
                    }
                }
            }
        }
        
        public void SetDotsScale(float scale)
        {
            foreach (var dot in _dots)
            {
                dot.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
        
        public void SetDotColor(int x, int y, int z, Color color)
        {
            if (x < 0 || x >= _xDots || y < 0 || y >= _yDots || z < 0 || z >= _zDots)
            {
                return;
            }
            _dots[x * _yDots * _zDots + y * _zDots + z].GetComponent<MeshRenderer>().material.color = color;
        }
        
        public void DeleteAllDots()
        {
            foreach (var dot in _dots)
            {
                Destroy(dot);
            }
            _dots.Clear();
        }
    }
}
