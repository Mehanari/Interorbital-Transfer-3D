using System.Collections.Generic;
using MehaMath.Math;
using UnityEngine;

namespace MehaMath.VisualisationTools.Plotting
{
    public class CircularPlot2D : MonoBehaviour
    {
        [SerializeField] private float lineWidth = 0.1f;

        private List<PlotParameters2D> _plots = new();
        
        public void Plot(float[] radii, float[] angles, string plotName, Color color, Vector3 shift = default)
        {
            var line = CreateLine(lineWidth, color, plotName, radii.Length);
            var plot = new PlotParameters2D()
            {
                Color = color,
                Line = line,
                Name = plotName,
                X = angles,
                Y = radii
            };
            _plots.Add(plot);
            var center = transform.position + shift;
            for (int i = 0; i < radii.Length; i++)
            {
                var radius = radii[i];
                var angle = angles[i];
                var vector = new Vector2(center.x + radius, center.y);
                vector = Vector2Utils.Rotate(vector, angle);
                line.SetPosition(i, vector);
            }
        }

        private LineRenderer CreateLine(float width, Color color, string plotName, int positionsCount)
        {
            var line = new GameObject(plotName).AddComponent<LineRenderer>();
            line.startColor = color;
            line.endColor = color;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.transform.SetParent(transform);
            line.transform.localPosition = Vector3.zero;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = color;
            line.endColor = color;
            line.positionCount = positionsCount;
            return line;
        }
        
        public void RemovePlotByName(string plotName)
        {
            var plot = _plots.Find(p => p.Name == plotName);
            if (plot != null)
            {
                _plots.Remove(plot);
                if (plot.Dots != null)
                {
                    foreach (var dot in plot.Dots)
                    {
                        Destroy(dot);
                    }
                }
                if (plot.Line != null)
                {
                    Destroy(plot.Line.gameObject);
                }
            }
        }
    }
}
