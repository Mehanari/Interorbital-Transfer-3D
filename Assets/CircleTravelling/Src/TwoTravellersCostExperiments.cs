using System.Collections.Generic;
using MehaMath.Math.Components;
using MehaMath.Math.RootsFinding;
using MehaMath.VisualisationTools.Plotting;
using UnityEngine;

namespace CircleTravelling.Src
{
    public class TwoTravellersCostExperiments : MonoBehaviour
    {
        [SerializeField] private Plotter3D plotter;
        [SerializeField] private Traveller traveller1;
        [SerializeField] private Traveller traveller2;
        [SerializeField] private GameObject salesman;
        
        private List<Traveller> Travellers => new List<Traveller>()
        {
            traveller1,
            traveller2
        };

        private void Start()
        {
            var step = 0.2f;
            var maxT = 15f;
            var z = new float[(int)(maxT/step),(int)(maxT/step)];
            var travellers = Travellers;
            var min = float.MaxValue;
            var minT1 = 0f;
            var minT2 = 0f;
            for (int t1 = 0; t1 < (int)(maxT/step); t1++)
            {
                for (int t2 = 0; t2 < (int)(maxT / step); t2++)
                {
                    var rendezvous1 = (t1 + 1) * step;
                    var rendezvous2 = (t2 + 1) * step;
                    traveller1.RendezvousTime = rendezvous1;
                    traveller2.RendezvousTime = rendezvous2;
                    var cost = CostCalculator.CalculateCost(salesman, travellers);
                    if (cost < min)
                    {
                        min = cost;
                        minT1 = traveller1.RendezvousTime;
                        minT2 = traveller2.RendezvousTime;
                    }
                    z[t1, t2] = cost;
                }
            }
            Debug.Log("Best times for T1 and T2: " + minT1 + "; " + minT2);
            traveller1.RendezvousTime = minT1;
            traveller2.RendezvousTime = minT2;
            plotter.PlotDots(maxT, maxT, z, "Costs", Color.yellow);

            var gdResult = Algorithms.GradientDescent(Goal, new Vector(0.001d, 0.001d), 0.01d);
            var resultCost = Goal(gdResult);
            Debug.Log("Gradient descent results.\nCost: " + resultCost + "\nT1: " + gdResult[0] + "\nT2: " + gdResult[1]);
            plotter.PlotSingleDot(new Vector3((float)gdResult[0],(float)resultCost, (float)gdResult[1]), "Gd Result", Color.red);
        }

        private double Goal(Vector rendezvousTimes)
        {
            traveller1.RendezvousTime = (float) rendezvousTimes[0];
            traveller2.RendezvousTime = (float)rendezvousTimes[1];
            var cost = CostCalculator.CalculateCost(salesman, Travellers);
            return cost;
        }
    }
}
