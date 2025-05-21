using UnityEngine;

namespace Src.Visualisation
{
    public class Arrow3D : MonoBehaviour
    {
        [SerializeField] private float length = 1;
        [SerializeField] private Vector3 direction;
        
        [SerializeField] private Transform head;
        [SerializeField] private Transform stem;
        [SerializeField] private Transform pivot;
        
        private const float DEFAULT_STEM_LENGTH = 2;
        private readonly Vector3 DEFAULT_DIRECTION = Vector3.up;
        private readonly Vector3 REFERENCE_EULER = new Vector3(90, 0, 0);

        public void SetLength(float newLength)
        {
            if (newLength < 0)
            {
                newLength = 0;
            }

            length = newLength;
            UpdateLength();
        }

        public void SetDirection(Vector3 newDirection)
        {
            direction = newDirection;
            UpdateRotation();
        }
        
        private void OnValidate()
        {
            if (head == null || stem == null || pivot == null)
            {
                return;
            }
            UpdateLength();
            UpdateRotation();
        }

        private void UpdateRotation()
        {
            if (head == null || stem == null || pivot == null)
            {
                return;
            }

            var directionToApply = direction;
            if (direction.magnitude <= Vector3.kEpsilon)
            {
                directionToApply = DEFAULT_DIRECTION;
                direction = DEFAULT_DIRECTION;
            }

            var eulerAngles = DirectionToEuler(directionToApply.normalized, Vector3.up);
            pivot.localEulerAngles = eulerAngles + REFERENCE_EULER;
        }

        private void UpdateLength()
        {
            if (head == null || stem == null || pivot == null)
            {
                return;
            }
            var stemScale = stem.localScale;
            var stemPosition = stem.localPosition;
            var stemCurrentHeight = stemScale.y;
            var change = Mathf.Abs(stemCurrentHeight - length);
            //Rescale stem
            stemScale.y = length;
            stem.localScale = stemScale;
            //Move stem up or down by the change amount
            if (stemCurrentHeight > length)
            {
                //Move down if stem got shorter
                stemPosition.y -= change;
            }
            else
            {
                //Move up if stem got higher
                stemPosition.y += change;
            }
            stem.localPosition = stemPosition;
            
            //Move head up or down
            var headPosition = head.localPosition;
            if (stemCurrentHeight > length)
            {
                headPosition.y -= change*DEFAULT_STEM_LENGTH;
            }
            else
            {
                headPosition.y += change*DEFAULT_STEM_LENGTH;
            }
            head.localPosition = headPosition;
        }
        
        public static Vector3 DirectionToEuler(Vector3 direction, Vector3 upVector)
        {
            direction.Normalize();
            var rotation = Quaternion.LookRotation(direction, upVector);
            var eulerAngles = rotation.eulerAngles;
            return eulerAngles;
        }
    }
}
