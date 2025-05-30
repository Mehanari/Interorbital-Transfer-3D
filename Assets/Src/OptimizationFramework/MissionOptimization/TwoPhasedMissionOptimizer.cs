using System.Linq;
using Src.OptimizationFramework.Calculators;
using Src.OptimizationFramework.MathComponents;
using Src.OptimizationFramework.ScheduleOptimizers;

namespace Src.OptimizationFramework.MissionOptimization
{
	public class TwoPhasedMissionOptimizer : IMissionOptimizer
	{
		private ScheduleOptimizer _initialGuessGenerator;
		private ScheduleOptimizer _mainOptimizer;

		private FuelCalculator _fuelCalculator;
		private KinematicCalculator _kinematicCalculator;
		private CostCalculator _costCalculator;

		public TwoPhasedMissionOptimizer(ScheduleOptimizer initialGuessGenerator, ScheduleOptimizer mainOptimizer, FuelCalculator fuelCalculator, KinematicCalculator kinematicCalculator, CostCalculator costCalculator)
		{
			_initialGuessGenerator = initialGuessGenerator;
			_mainOptimizer = mainOptimizer;
			_fuelCalculator = fuelCalculator;
			_kinematicCalculator = kinematicCalculator;
			_costCalculator = costCalculator;
		}

		public OptimizationResult Optimize(MissionParameters parameters)
		{
			_fuelCalculator.Isp = parameters.Isp;
			_fuelCalculator.StandardGrav = parameters.StandGrav;
			_kinematicCalculator.Mu = parameters.Mu;
			_costCalculator.Mu = parameters.Mu;
			_costCalculator.FuelCalculator = _fuelCalculator;
			_costCalculator.CentralBodyRadius = parameters.CentralBodyRadius;
			_costCalculator.FuelCost = parameters.FuelCost;
			_costCalculator.TimeCost = parameters.TimeCost;

			_initialGuessGenerator.CostCalculator = _costCalculator;
			_initialGuessGenerator.KinematicCalculator = _kinematicCalculator;
			_mainOptimizer.CostCalculator = _costCalculator;
			_mainOptimizer.KinematicCalculator = _kinematicCalculator;

			var permutations = parameters.Targets.GetPermutations();
			var bestPermutation = permutations[0];
			(double[] driftTimes, double[] transferTimes ) bestSchedule = (new double[]{0}, new double[]{0});
			var minCost = double.MaxValue;
			foreach (var permutation in permutations)
			{
				var (driftTimes, transferTimes, cost) = GetOptimalSchedule(permutation, parameters.ShipInitialOrbit,
					parameters.ShipFinalMass);
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
			var crushes = IntersectionsCalculator
				.CalculateIntersections(kinematics, parameters.Mu, parameters.CentralBodyRadius).Count(i => i > 0);
			var serviceData = new TargetServicing[kinematics.Length];
			var fuel = _fuelCalculator.CalculateFuelMasses(kinematics, parameters.ShipFinalMass);
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
			TargetParameters[] targets, Orbit spacecraftInitialOrbit, double spacecraftFinalMass)
		{
			var initialGuess = _initialGuessGenerator.OptimizeSchedule(targets, spacecraftInitialOrbit, spacecraftFinalMass);
			_mainOptimizer.InitialGuess = initialGuess;
			var refinedGuess = _mainOptimizer.OptimizeSchedule(targets, spacecraftInitialOrbit, spacecraftFinalMass);
			var kinematics = _kinematicCalculator.CalculateKinematics(refinedGuess.driftTimes,
				refinedGuess.transferTimes, targets, spacecraftInitialOrbit);
			var cost = _costCalculator.CalculateCost(kinematics, spacecraftFinalMass);
			return (refinedGuess.driftTimes, refinedGuess.transferTimes, cost);
		}
	}
}