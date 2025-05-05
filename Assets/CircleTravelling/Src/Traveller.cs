using BoatAndRafts.Src;
using UnityEngine;

namespace CircleTravelling.Src
{
    /// <summary>
    /// Traveller is an entity that travels in a circular trajectory with constant angular velocity.
    /// It is assumed that traveller moves anti-clockwise if angular velocity is positive.
    /// The scripts transform is considered to be the center of the traveller's circle.
    /// </summary>
    [ClassTooltip("All angles and velocities are measured in radians.")]
    public class Traveller : MonoBehaviour
    {
        [SerializeField] private float radius;
        [SerializeField] private float angularVelocity;
        [SerializeField] private float initialAngle;
        [SerializeField] [Min(0f)] private float rendezvousTime;

        public float Radius => radius;
        public float AngularVelocity => angularVelocity;
        public float InitialAngle => initialAngle;
        public Vector3 Center => transform.position;

        public float RendezvousTime
        {
            get => rendezvousTime;
            set => rendezvousTime = value;
        }

        public Vector3 GetRendezvousPosition()
        {
            var rendezvousAngle = InitialAngle + AngularVelocity * rendezvousTime;
            var rendezvousPosition =
                new Vector3(Mathf.Cos(rendezvousAngle) * radius, Mathf.Sin(rendezvousAngle) * radius, 0) +
                Center;
            return rendezvousPosition;
        }
    }
}
