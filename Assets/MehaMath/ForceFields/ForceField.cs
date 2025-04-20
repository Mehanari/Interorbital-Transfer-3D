using UnityEngine;

namespace MehaMath.ForceFields
{
    public abstract class ForceField : MonoBehaviour
    {
        public abstract Vector3 GetForce(Vector3 point);
    }
}