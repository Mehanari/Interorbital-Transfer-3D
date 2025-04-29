using System;
using MehaMath.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.ManualTests
{
    public class DistanceCriteriaTest : MonoBehaviour
    {
        [SerializeField] private Plotter2D plotter;

        private float _maxDistance = 10f;
        private float _minDistance = 1f;
        
        void Start()
        {
            _maxDistance = 10f;
            _minDistance = 1f;
            var startDistance = 0f;
            var endDistance = 20f;
            
            plotter.Plot(startDistance, endDistance, (x) => Criterion(x), 1000, "Criterion", Color.green);
        }

        private float Criterion(float distance)
        {
            var middle = (_maxDistance + _minDistance) / 2;

            var normalized = (distance - middle) / (_maxDistance - middle);
            if (distance < _minDistance)
            {
                return (float)Math.Pow(normalized, 4);
            }

            if (distance > _maxDistance)
            {
                return (float)Math.Pow(normalized, 3);
            }
            return (float)Math.Pow(normalized, 2);
        }
    }
}
