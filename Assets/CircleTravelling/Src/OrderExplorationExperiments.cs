using System;
using System.Linq;
using System.Text;
using MehaMath.Math.Components;
using MehaMath.Math.RootsFinding;
using TMPro;
using UnityEngine;

namespace CircleTravelling.Src
{
    public class OrderExplorationExperiments : MonoBehaviour
    {
        [SerializeField] private TravellersCollection travellers;
        [SerializeField] private GameObject salesman;
        [SerializeField] private TextMeshProUGUI resultsMesh;

        private void Start()
        {
            var travellersArray = travellers.Travellers.ToArray();
            var permutations = travellersArray.GetPermutations();
            var reportBuilder = new StringBuilder();
            for (int i = 0; i < permutations.GetLength(0); i++)
            {
                var permutation = permutations[i];
                var gdResult = Algorithms.GradientDescent((v) => Cost(v, permutation), Vector.CreateSameValueVector(permutation.Length, 0.0001d), 0.01d);
                var resultCost = Cost(gdResult, permutation);
                reportBuilder.Append(PermutationName(permutation) + ": " + resultCost + "\n");
            }

            resultsMesh.text = reportBuilder.ToString();
        }

        private string PermutationName(Traveller[] permutation)
        {
            var stringBuilder = new StringBuilder();
            foreach (var traveller in permutation)
            {
                stringBuilder.Append(traveller.Name);
            }

            return stringBuilder.ToString();
        }
        
        private double Cost(Vector rendezvousTimes, Traveller[] travellers)
        {
            for (int i = 0; i < travellers.Length; i++)
            {
                travellers[i].RendezvousTime = (float) rendezvousTimes[i];
            }
            var cost = CostCalculator.CalculateCost(salesman, travellers.ToList());
            return cost;
        }
    }
}
