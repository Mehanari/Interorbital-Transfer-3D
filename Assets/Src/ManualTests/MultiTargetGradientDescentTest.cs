using System;
using MehaMath.Math.Components;
using Src.Helpers;
using Src.OptimizationFramework;
using Src.OptimizationFramework.Calculators;
using Src.OptimizationFramework.MathComponents;
using Src.OptimizationFramework.MissionOptimization;
using Src.OptimizationFramework.ScheduleOptimizers;
using UnityEngine;

namespace Src.ManualTests
{
	public class MultiTargetGradientDescentTest : MonoBehaviour
	{
		private double _g0 = 0.00981; //Standard gravitational acceleration in km/s^2.
		private double _mDry = 100; //Mass of the spacecraft without fuel
		private double _isp = 300; //Engine specific impulse
		private double _mu = 398600.4418;
		private double _centralBodyRadius = 6600d;
		private double _crushLambda = 10;
		private double _fuelCost = 200000;
		private double _timeCost = 100;
		private double _fuelSurplus = 0.2;

		private Orbit _spacecraftInitialOrbit = new Orbit()
		{
			AscendingNodeLongitude = 0.5411,
			Eccentricity = 0.001,
			Inclination = 0.7156,
			PerigeeArgument = 0,
			SemiMajorAxis = 6828,
			TrueAnomaly = 0.785398
		};

		private Orbit[] _targetsOrbits = new[]
		{
			new Orbit()
			{
				AscendingNodeLongitude = 0.5236,
				Eccentricity = 0.001,
				Inclination = 0.6981,
				PerigeeArgument = 0,
				SemiMajorAxis = 6878,
				TrueAnomaly = 0
			},
			new Orbit()
			{
				AscendingNodeLongitude = 0.6109,
				Eccentricity = 0.002,
				Inclination = 0.7854,
				PerigeeArgument = 0.1745,
				SemiMajorAxis = 7578,
				TrueAnomaly = 1.5708
			},
			 new Orbit()
			 {
				 AscendingNodeLongitude = 0.5759,
				 Eccentricity = 0.0015,
				 Inclination = 0.7505,
				 PerigeeArgument = 0.3491,
				 SemiMajorAxis = 7028,
				 TrueAnomaly = 2.0944
			 },
		};

		private double[] _targetsServiceTimes = new[]
		{
			1200d,
			1200d,
			1200d,
		};

		private void Start()
		{
			var kinematicsCalculator = new KinematicCalculator(_mu);
			var fuelCalculator = new FuelCalculator(_isp, _g0, _fuelSurplus);
			var costCalculator = new CostCalculator(fuelCalculator, _mu, _centralBodyRadius, _fuelCost, _timeCost);

			var initialGuessOptimizer = InitializeGridDescentOptimizer();
			var mainOptimizer = InitializeOptimizer();

			var missionOptimizer = new TwoPhasedMissionOptimizer(initialGuessOptimizer, mainOptimizer, fuelCalculator,
				kinematicsCalculator, costCalculator);

			var missionParameters = new MissionParameters()
			{
				CentralBodyRadius = _centralBodyRadius,
				FuelCost = _fuelCost,
				Isp = _isp,
				Mu = _mu,
				ShipFinalMass = _mDry,
				ShipInitialOrbit = _spacecraftInitialOrbit,
				Targets = GetTargets(),
				StandGrav = _g0,
				TimeCost = _timeCost
			};

			var result = missionOptimizer.Optimize(missionParameters);
			Debug.Log("Mission cost: " + result);
		}
		

		private TargetParameters[] GetTargets()
		{
			var targets = new TargetParameters[_targetsOrbits.Length];
			for (int i = 0; i < _targetsOrbits.Length; i++)
			{
				var serviceTime = _targetsServiceTimes[i];
				var targetName = i.ToString();

				targets[i] = new TargetParameters()
				{
					TargetName = targetName,
					ServiceTime = serviceTime,
					InitialOrbit = _targetsOrbits[i]
				};
			}

			return targets;
		}

		private GlobalScheduleOptimizer InitializeOptimizer()
		{
			var kinematicsCalculator = new KinematicCalculator(_mu);
			var fuelCalculator = new FuelCalculator(_isp, _g0, _fuelSurplus);
			var costCalculator = new CostCalculator(fuelCalculator, _mu, _centralBodyRadius, _fuelCost, _timeCost);
			var scheduleOptimizer = new GlobalScheduleOptimizer(costCalculator, kinematicsCalculator);
			scheduleOptimizer.GdIterationsLimit = 10000;
			return scheduleOptimizer;
		}

		private SequentialScheduleOptimizer InitializeSequentialOptimizer()
		{
			var kinematicsCalculator = new KinematicCalculator(_mu);
			var fuelCalculator = new FuelCalculator(_isp, _g0, _fuelSurplus);
			var costCalculator = new CostCalculator(fuelCalculator, _mu, _centralBodyRadius, _fuelCost, _timeCost);
			var optimizer = new SequentialScheduleOptimizer(costCalculator, kinematicsCalculator, _mu);
			optimizer.GdIterationsLimit = 10000;
			return optimizer;
		}

		private GridDescentSequentialOptimizer InitializeGridDescentOptimizer()
		{
			var kinematicsCalculator = new KinematicCalculator(_mu);
			var fuelCalculator = new FuelCalculator(_isp, _g0, _fuelSurplus);
			var costCalculator = new CostCalculator(fuelCalculator, _mu, _centralBodyRadius, _fuelCost, _timeCost);
			var optimizer = new GridDescentSequentialOptimizer(costCalculator, kinematicsCalculator, _mu)
			{
				MinDriftTime = 1000,
				MaxDriftTime = 20000,
				MinTransferTime = 10000,
				MaxTransferTime = 80000,
				PointsPerDimension = 40,
				CostToDifference = 1
			};
			return optimizer;
		}
	}
}