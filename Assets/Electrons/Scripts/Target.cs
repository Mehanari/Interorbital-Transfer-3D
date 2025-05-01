using System;
using UnityEngine;

namespace Electrons.Scripts
{
    public class Target : MonoBehaviour
    {
        [SerializeField] private ElectronParameters2D electronParameters;
        [SerializeField] private float minDistance = 0.5f;
        [SerializeField] private float maxDistance = 1.5f;
        [SerializeField] private float minDistancingRate = 0.1f;
        [SerializeField] private Ring ring;

        private ElectronState _targetState;
        private ElectronState _carrierState;

        public ElectronParameters2D ElectronParameters => electronParameters;

        private void Start()
        {
            ring.SetInnerRadius(minDistance);
            ring.SetOuterRadius(maxDistance);
        }

        public void SetTargetState(ElectronState newState)
        {
            _targetState = newState;
        }

        public void SetCarrierState(ElectronState newState)
        {
            _carrierState = newState;
        }

        public void UpdateRingColor()
        {
            var distancingRate = Math.Abs(CalculateDistancingRate(_carrierState, _targetState));
            var color = GetColorBasedOnDifference((float)distancingRate, minDistancingRate);
            ring.SetColor(color);
        }

        /// <summary>
        /// Answers the question "How fast does the distance between two electrons change?"
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <returns></returns>
        private double CalculateDistancingRate(ElectronState e1, ElectronState e2)
        {
            var v1 = e1.GetVelocity2D();
            var v2 = e2.GetVelocity2D();
            var pos1 = e1.Position;
            var pos2 = e2.Position;
            var distance = (pos1 - pos2).Magnitude();
            var x1 = pos1[0];
            var y1 = pos1[1];
            var x2 = pos2[0];
            var y2 = pos2[1];
            var dx1 = v1[0];
            var dy1 = v1[1];
            var dx2 = v2[0];
            var dy2 = v2[1];
            var distancingRate = ((x1 - x2) * (dx1 - dx2) + (y1 - y2) * (dy1 - dy2)) / distance;
            return distancingRate;
        }
        
        private static Color GetColorBasedOnDifference(float diff, float minDiff)
        {
            Color green = Color.green;       // RGB: (0, 1, 0)
            Color yellow = Color.yellow;     // RGB: (1, 1, 0)
            Color orange = new Color(1.0f, 0.5f, 0.0f); // RGB: (1, 0.5, 0)
            Color red = Color.red;           // RGB: (1, 0, 0)
            
            if (diff <= 0.0001f) 
                return green;

            if (diff > minDiff)
            {
                float remappedDiff = Mathf.Clamp01((diff - minDiff) / minDiff);
                return Color.Lerp(orange, red, remappedDiff);
            }
            else
            {
                float normalizedDiff = Mathf.Clamp01(diff / minDiff);
                return Color.Lerp(green, yellow, normalizedDiff);
            }
        }
    }
}
