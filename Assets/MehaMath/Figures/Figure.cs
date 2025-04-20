using UnityEngine;

namespace MehaMath.Figures
{
    public abstract class Figure : MonoBehaviour
    {
        [Tooltip("Defines center of the figure and its rotation.")]
        [SerializeField] protected Transform pivot;

        /// <summary>
        /// Touch vector is a vector pointing from closest point on this figure contour
        /// to the <paramref name="point"/>
        /// </summary>
        /// <param name="point"></param>
        /// <param name="touchVector"></param>
        /// <returns></returns>
        public abstract bool IsTouching(Vector3 point, out Touch touch);
    }
}