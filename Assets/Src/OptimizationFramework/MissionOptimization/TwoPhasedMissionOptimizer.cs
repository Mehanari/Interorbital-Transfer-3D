using System.Linq;
using Src.OptimizationFramework.Calculators;
using Src.OptimizationFramework.Calculators.Cost;
using Src.OptimizationFramework.Calculators.Fuel;
using Src.OptimizationFramework.DataModels;
using Src.OptimizationFramework.MathComponents;
using Src.OptimizationFramework.ScheduleOptimizers;

namespace Src.OptimizationFramework.MissionOptimization
{
	public class TwoPhasedMissionOptimizer : IMissionOptimizer
	{
		private GridDescentSequentialOptimizer _initialGuessGenerator;
		private GradientDescentScheduleOptimizer _mainOptimizer;

		private SurplusFuelCalculator _surplusFuelCalculator;
		private KinematicCalculator _kinematicCalculator;
		private WeightedCostCalculator _weightedCostCalculator;

		public TwoPhasedMissionOptimizer(GridDescentSequentialOptimizer initialGuessGenerator, GradientDescentScheduleOptimizer mainOptimizer, SurplusFuelCalculator surplusFuelCalculator, KinematicCalculator kinematicCalculator, WeightedCostCalculator weightedCostCalculator)
		{
			_initialGuessGenerator = initialGuessGenerator;
			_mainOptimizer = mainOptimizer;
			_surplusFuelCalculator = surplusFuelCalculator;
			_kinematicCalculator = kinematicCalculator;
			_weightedCostCalculator = weightedCostCalculator;
		}

		public OptimizationResult Optimize(MissionParameters parameters)
		{
			//Initializing fuel calculator
			_surplusFuelCalculator.Isp = parameters.Isp;
			_surplusFuelCalculator.StandardGrav = parameters.StandGrav;
			_surplusFuelCalculator.ShipFinalMass = parameters.ShipFinalMass;
			
			//Initializing kinematics calculator
			_kinematicCalculator.Mu = parameters.Mu;
			
			//Initializing intersections calculator
			var intersectionsCalculator = new IntersectionsCalculator()
			{
				Mu = parameters.Mu,
				CentralBodyRadius = parameters.CentralBodyRadius
			};
			
			//Initializing cost calculator
			_weightedCostCalculator.IntersectionsCalculator = intersectionsCalculator;
			_weightedCostCalculator.FuelCalculator = _surplusFuelCalculator;
			_weightedCostCalculator.KinematicCalculator = _kinematicCalculator;
			_weightedCostCalculator.FuelCost = parameters.FuelCost;
			_weightedCostCalculator.TimeCost = parameters.TimeCost;

			_initialGuessGenerator.CostCalculator = _weightedCostCalculator;
			_initialGuessGenerator.KinematicCalculator = _kinematicCalculator;
			_initialGuessGenerator.Mu = parameters.Mu;
			_mainOptimizer.CostCalculator = _weightedCostCalculator;

			var permutations = parameters.Targets.GetPermutations();
			var bestPermutation = permutations[0];
			(double[] driftTimes, double[] transferTimes ) bestSchedule = (new double[]{0}, new double[]{0});
			var minCost = double.MaxValue;
			foreach (var permutation in permutations)
			{
				var (driftTimes, transferTimes, cost) = GetOptimalSchedule(permutation, parameters.ShipInitialOrbit);
				if (cost < minCost)
				{
					minCost = cost;
					bestSchedule = (driftTimes, transferTimes);
					bestPermutation = permutation;
				}
			}
			
			//Forming optimization result
			var kinematics = _kinematicCalculator.CalculateKinematics(bestSchedule.driftTimes,
				bestSchedule.transferTimes, bestPermutation, parameters.ShipInitialOrbit);
			var crushes = intersectionsCalculator
				.CalculateIntersections(kinematics).Count(i => i > 0);
			var serviceData = new TargetServicing[kinematics.Length];
			var fuel = _surplusFuelCalculator.CalculateFuelMasses(kinematics);
			var totalTime = kinematics.Sum(t => t.DriftTime + t.TransferTime + t.ServiceTime);
			for (int i = 0; i < serviceData.Length; i++)
			{
				serviceData[i] = new TargetServicing()
				{
					FuelMass = fuel[i],
					Kinematics = kinematics[i]
				};
			}

			var optimizationResult = new OptimizationResult()
			{
				ServiceData = serviceData,
				ServiceOrder = bestPermutation,
				Crushes = crushes,
				TotalCost = minCost,
				TotalFuel = fuel.Sum(),
				TotalTime = totalTime
			};

			return optimizationResult;
		}

		private (double[] driftTimes, double[] transferTimes, double cost) GetOptimalSchedule(
			TargetParameters[] targets, Orbit spacecraftInitialOrbit)
		{
			var initialGuess = _initialGuessGenerator.OptimizeSchedule(targets, spacecraftInitialOrbit);
			_mainOptimizer.InitialGuess = initialGuess;
			var refinedGuess = _mainOptimizer.OptimizeSchedule(targets, spacecraftInitialOrbit);
			var cost = _weightedCostCalculator.CalculateCost(refinedGuess, targets, spacecraftInitialOrbit);
			return (refinedGuess.driftTimes, refinedGuess.transferTimes, cost);
		}
	}
}