using UnityEngine;

namespace MehaMath.ForceFields
{
    public class RectVerticalForce : ForceField
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private float height;
        [SerializeField] private float width;
        [SerializeField] private float force;
        
        public override Vector3 GetForce(Vector3 point)
        {
            var toLocal = pivot.worldToLocalMatrix;
            var toWorld = pivot.localToWorldMatrix;
            var localOffset = new Vector3(0, height/2, 0);
            var worldOffset = toWorld.MultiplyVector(localOffset);
            var center = pivot.position + worldOffset;
            var vector = point - center;
            var localVector = toLocal.MultiplyVector(vector);
            var inXBounds = localVector.x > -width / 2 && localVector.x < width / 2;
            var inYBounds = localVector.y > -height/2 && localVector.y < height/2;
            if (inXBounds && inYBounds)
            {
                var forceVector = Vector3.up * force;
                forceVector = toWorld.MultiplyVector(forceVector);
                return forceVector;
            }
            return Vector3.zero;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            var rotationMatrix = pivot.localToWorldMatrix;
            Gizmos.matrix = rotationMatrix;
            var center = new Vector3(0, height / 2, 0);
            Gizmos.DrawWireCube(center, new Vector3(width, height, 0));
        }
    }
}