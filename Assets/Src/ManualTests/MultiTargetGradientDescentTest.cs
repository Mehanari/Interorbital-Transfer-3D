using MehaMath.Math.Components;
using Src.FinalComponents;
using Src.FinalComponents.ScheduleOptimizers;
using Src.Helpers;
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
		
		private Vector _shipInitialPos = new Vector(7500.0, 1200.0, 1800.0);
		private Vector _shipInitialVel = new Vector(-1.8, 7.2, 1.2);

		private Vector[] _targetsInitialPositions = new[]
		{
			new Vector(6800.0, 3500.0, -1200.0),
			new Vector(5500.0, -2800.0, 2600.0)
		};
		private Vector[] _targetsInitialVelocities = new[]
		{
			new Vector(-3.5, 5.8, 2.4),
			new Vector(2.9, 6.4, -1.5)
		};

		private double[] _targetsServiceTimes = new[]
		{
			100d,
			100d
		};

		private void Start()
		{
			var sequentialOptimizer = InitializeSequentialOptimizer();
			var globalOptimizer = InitializeOptimizer();
			var spacecraftInitialOrbit = GetSpacecraftInitialOrbit();
			var targets = GetTargets();
			var initialGuess = sequentialOptimizer.OptimizeSchedule(1000, 36000, targets, spacecraftInitialOrbit, _mDry);
			var optimum = globalOptimizer.OptimizeSchedule(initialGuess.driftTimes, initialGuess.transferTimes, targets, spacecraftInitialOrbit, _mDry);

		}

		private Orbit GetSpacecraftInitialOrbit()
		{
			return OrbitHelper.GetOrbit(_shipInitialVel, _shipInitialPos, _mu);
		}

		private TargetParameters[] GetTargets()
		{
			var targets = new TargetParameters[_targetsInitialPositions.Length];
			for (int i = 0; i < _targetsInitialPositions.Length; i++)
			{
				var pos = _targetsInitialPositions[i];
				var vel = _targetsInitialVelocities[i];
				var orbit = OrbitHelper.GetOrbit(vel, pos, _mu);
				var serviceTime = _targetsServiceTimes[i];
				var targetName = i.ToString();

				targets[i] = new TargetParameters()
				{
					TargetName = targetName,
					ServiceTime = serviceTime,
					InitialOrbit = orbit
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
	}
}