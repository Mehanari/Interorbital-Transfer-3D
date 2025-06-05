using System.Collections.Generic;
using MehaMath.Math.Components;
using MehaMath.Math.Components.Json;
using Newtonsoft.Json;
using Src.OptimizationFramework;
using Src.OptimizationFramework.Calculators;
using Src.OptimizationFramework.Calculators.Cost;
using Src.OptimizationFramework.Calculators.Fuel;
using Src.OptimizationFramework.DataModels;
using Src.OptimizationFramework.MathComponents;
using UnityEngine;

namespace Src.ManualTests
{
    public class RandomExperiments : MonoBehaviour
    {
        void Start()
        {
            var jsonIo = new JsonIO<MissionParameters>()
            {
                FileName = "missionParameters.json",
                Converters = new JsonConverter[] { new VectorJsonConverter()}
            };
            var missionParameters = jsonIo.Load();
            var mu = missionParameters.Mu;
            var kinematicsCalculator = new KinematicCalculator(mu);
            var fuelCalculator = new SurplusFuelCalculator()
            {
                Surplus = 0.2,
                ShipFinalMass = missionParameters.ShipFinalMass,
                Isp = missionParameters.Isp,
                StandardGrav = missionParameters.StandGrav
            };
            var costCalculator = new WeightedCostCalculator()
            {
                FuelCalculator = fuelCalculator,
                FuelCost = missionParameters.FuelCost,
                TimeCost = missionParameters.TimeCost,
                KinematicCalculator = kinematicsCalculator,
                IntersectionsCalculator = new IntersectionsCalculator()
                {
                    Mu = mu,
                    CentralBodyRadius = missionParameters.CentralBodyRadius
                }
            };
            var minDriftTime = 1000d;
            var maxDriftTime = 20000d;
            var minTransferTime = 10000d;
            var maxTransferTime = 80000d;
            var pointsPerDimension = 40;
            var zeroPoint = new Vector(minDriftTime, minTransferTime);
            var difference = new Vector((maxDriftTime- minDriftTime) / (pointsPerDimension - 1),
                (maxTransferTime - minTransferTime) / (pointsPerDimension - 1));
            var grid = GridSearcher.GenerateGrid(zeroPoint, difference, pointsPerDimension);
            var minCost = double.MaxValue;
            var results = new List<Vector>();
            foreach (var point in grid)
            {
                var cost = costCalculator.CalculateCost(point, new[] { missionParameters.Targets[0] },
                    missionParameters.ShipInitialOrbit);
                if (cost < minCost)
                {
                    minCost = cost;
                }
                results.Add(new Vector(point, cost));
            }

            var listJson = new JsonIO<List<Vector>>()
            {
                FileName = "costFunction.json",
                Converters = new JsonConverter[] { new VectorJsonConverter() }
            };
            listJson.Save(results);
            Debug.Log("Min cost: " + minCost);
        }
    }
}
