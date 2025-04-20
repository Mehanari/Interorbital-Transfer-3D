using System.Collections.Generic;
using UnityEngine;

namespace MehaMath.VisualisationTools
{
    public class ArrowsChain : MonoBehaviour
    {
        [SerializeField] private Arrow arrowPrefab;
        [SerializeField] private float defaultLength;
        [SerializeField] private float defaultAngleRad;

        private readonly List<Arrow> _arrows = new List<Arrow>();
        
        public int ArrowsCount => _arrows.Count;
        
        /// <summary>
        /// Generates arrows chain with given count.
        /// If there are already some arrows, new arrows will be added or redundant arrows will be deleted.
        /// </summary>
        /// <param name="count"></param>
        public void SetArrowsCount(int count)
        {
            if (count < 0)
            {
                count = 0;
            }
            if (count < _arrows.Count)
            {
                for (int i = _arrows.Count - 1; i >= count; i--)
                {
                    Destroy(_arrows[i].gameObject);
                    _arrows.RemoveAt(i);
                }
            }
            else if (count > _arrows.Count)
            {
                for (int i = _arrows.Count; i < count; i++)
                {
                    var arrow = Instantiate(arrowPrefab, transform);
                    if (i > 0)
                    {
                        _arrows[i-1].AttachToTip(arrow.gameObject);
                    }
                    arrow.SetLength(defaultLength);
                    arrow.SetAngleRadians(defaultAngleRad);
                    _arrows.Add(arrow);
                }
            }
        }

        /// <summary>
        /// The very first angle is the angle between the first arrow and the x-axis.
        /// Each next angle is the angle between the current arrow and the previous one.
        /// The length of the angles array must be equal to the number of arrows.
        /// </summary>
        /// <param name="angles"></param>
        public void SetAngles(float[] angles)
        {
            if (angles.Length != _arrows.Count)
            {
                Debug.LogError("The length of the angles array must be equal to the number of arrows.");
                return;
            }
            for (int i = 0; i < _arrows.Count; i++)
            {
                _arrows[i].SetAngleRadians(angles[i]);
                if (i > 0)
                {
                    _arrows[i].SetAngleRadians(angles[i] + Mathf.PI/2);
                }
            }
        }
        
        /// <summary>
        /// The length of the lengths array must be equal to the number of arrows.
        /// </summary>
        /// <param name="lengths"></param>
        public void SetLengths(float[] lengths)
        {
            if (lengths.Length != _arrows.Count)
            {
                Debug.LogError("The length of the lengths array must be equal to the number of arrows.");
                return;
            }
            for (int i = 0; i < _arrows.Count; i++)
            {
                _arrows[i].SetLength(lengths[i]);
            }
        }
        
        public void SetAngle(int arrowIndex, float angle)
        {
            if (arrowIndex < 0 || arrowIndex >= _arrows.Count)
            {
                Debug.LogError("Arrow index is out of range.");
                return;
            }
            _arrows[arrowIndex].SetAngleRadians(angle);
            if (arrowIndex > 0)
            {
                _arrows[arrowIndex].SetAngleRadians(angle + Mathf.PI/2);
            }
        }
        
        public void SetLength(int arrowIndex, float length)
        {
            if (arrowIndex < 0 || arrowIndex >= _arrows.Count)
            {
                Debug.LogError("Arrow index is out of range.");
                return;
            }
            _arrows[arrowIndex].SetLength(length);
        }
    }
}