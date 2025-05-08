using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CircleTravelling.Src
{
    public class CostCalculator : MonoBehaviour
    {
        [Tooltip("Travellers order matter")]
        [SerializeField] private TravellersCollection travellers;
        [SerializeField] private GameObject salesman;
        [SerializeField] private TextMeshProUGUI textMesh;

        private void Update()
        {
            textMesh.text = "Cost: " + CalculateCost();
        }

        private float CalculateCost()
        {
            var cost = 0f;
            var position = salesman.transform.position;
            foreach (var traveller in travellers.Travellers)
            {
                var radius = traveller.Radius;
                var rendezvousTime = traveller.RendezvousTime;
                var rendezvousAngle = traveller.InitialAngle + traveller.AngularVelocity * rendezvousTime;
                var rendezvousPosition =
                    new Vector3(Mathf.Cos(rendezvousAngle) * radius, Mathf.Sin(rendezvousAngle) * radius, 0) +
                    traveller.Center;
                var distance = (rendezvousPosition - position).magnitude;
                var requiredVelocity = distance / rendezvousTime;
                cost += (requiredVelocity + rendezvousTime);
                position = rendezvousPosition;
            }

            return cost;
        }

        public static float CalculateCost(GameObject salesman, List<Traveller> travellers)
        {
            var cost = 0f;
            var position = salesman.transform.position;
            foreach (var traveller in travellers)
            {
                var radius = traveller.Radius;
                var rendezvousTime = traveller.RendezvousTime;
                var rendezvousAngle = traveller.InitialAngle + traveller.AngularVelocity * rendezvousTime;
                var rendezvousPosition =
                    new Vector3(Mathf.Cos(rendezvousAngle) * radius, Mathf.Sin(rendezvousAngle) * radius, 0) +
                    traveller.Center;
                var distance = (rendezvousPosition - position).magnitude;
                var requiredVelocity = distance / rendezvousTime;
                cost += (requiredVelocity + rendezvousTime);
                position = rendezvousPosition;
            }

            return cost;
        }
    }
}
