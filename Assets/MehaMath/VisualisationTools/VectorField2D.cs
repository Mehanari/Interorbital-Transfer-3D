using System;
using System.Collections.Generic;
using UnityEngine;

namespace MehaMath.VisualisationTools
{
    public class VectorField2D : MonoBehaviour
    {
        [SerializeField] private float arrowLength;
        [SerializeField] private Arrow arrowPrefab;
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float minY;
        [SerializeField] private float maxY;
        [SerializeField] private int xSamples;
        [SerializeField] private int ySamples;
        [SerializeField] private float minMagnitudeToDrawArrow = 0f;
        
        private readonly List<Arrow> _arrows = new List<Arrow>();
        private float _maxMagnitude;
        

        public void Plot(Func<Vector2, Vector2> function)
        {
            Clear();
            var yStep = (maxY - minY) / ySamples;
            var xStep = (maxX - minX) / xSamples;
            for (var i = 0; i < xSamples; i++)
            {
                for (var j = 0; j < ySamples; j++)
                {
                    var x = minX + i * xStep;
                    var y = minY + j * yStep;
                    var vector = function(new Vector2(x, y));
                    AddArrow(x, y, vector.x, vector.y);
                }
            }
        }
        
        private void Clear()
        {
            foreach (var arrow in _arrows)
            {
                Destroy(arrow.gameObject);
            }
            _arrows.Clear();
            _maxMagnitude = 0;
        }

        private void AddArrow(float x, float y, float dx, float dy)
        {
            var direction = new Vector2(dx, dy);
            if (direction.magnitude < minMagnitudeToDrawArrow)
            {
                return;
            }
            var arrow = Instantiate(arrowPrefab, transform);
            arrow.transform.localPosition = new Vector3(x, y, 0);
            var magnitude = Mathf.Sqrt(dx * dx + dy * dy);
            if (magnitude > _maxMagnitude)
            {
                _maxMagnitude = magnitude;
            }
            arrow.SetLength(arrowLength);
            arrow.SetColor(GetMagnitudeColor(magnitude));
            var angle = GetAngleRespectiveToXAxis(direction);
            arrow.SetAngleRadians(angle);
            _arrows.Add(arrow);
        }
        
        private float GetAngleRespectiveToXAxis(Vector2 vector)
        {
            var angle = Mathf.Acos(Vector3.Dot(Vector3.right, vector)/(vector.magnitude*Vector3.right.magnitude));
            if (vector.y < 0)
            {
                angle = 2 * Mathf.PI - angle;
            }

            return angle;
        }

        private Color GetMagnitudeColor(float magnitude)
        {
            return ColorUtils.HeatToColor(magnitude, minMagnitudeToDrawArrow, _maxMagnitude);
        }
    }
}
