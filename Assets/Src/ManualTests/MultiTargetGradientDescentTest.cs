using System;
using System.Collections.Generic;
using MehaMath.Math.Components;
using Src.FinalComponents;
using Src.Helpers;
using UnityEngine;

namespace Src.ManualTests
{
	public class MultiTargetGradientDescentTest : MonoBehaviour
	{
		private double _g0 = 9.80665; //Standard gravitational acceleration
		private double _mDry = 100; //Mass of the spacecraft without fuel
		private double _isp = 300; //Engine specific impulse
		private double _mu = 398600.4418;
		private double _centralBodyRadius = 6600d;
		private double _crushLambda = 10;
		private double _fuelCost = 1;
		private double _timeCost = 2;
		
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

		private MultiTargetCostCalculator _costCalculator;

		private void Start()
		{
			InitializeCostCalculator();
			var initialGuess = new Vector(1000, 1000, 1000, 1000);
			var minimum = GradientDescent.Minimize(Objective, initialGuess, stepSize: 0.01d);
			Debug.Log("Minimum cost: " + Objective(minimum));
		}

		private double Objective(Vector times)
		{
			if (times.Length != _targetsInitialPositions.Length*2)
			{
				throw new ArgumentException("Invalid times count");
			}

			var driftTimes = new double[_targetsInitialPositions.Length];
			var transferTimes = new double[_targetsInitialPositions.Length];
			var negativeDriftPenalty = 0d;
			for (int i = 0, k = 0; i < _targetsInitialPositions.Length*2; i += 2, k++)
			{
				var drift = times[i];
				var transfer = times[i + 1];
				if (drift < 0)
				{
					negativeDriftPenalty += drift * drift;
					drift = 0;
				}
				driftTimes[k] = drift;
				transferTimes[k] = transfer;
			}

			var costParameters = _costCalculator.CalculateCost(driftTimes, transferTimes);
			
			//Calculating needed fuel mass
			var deltaVTotalMs = costParameters.TotalVelocityDelta * 1000;
			var mMin = _mDry * (Math.Exp(deltaVTotalMs / (_isp * _g0)) - 1);
			var surplus = mMin * 0.2;
			var mTotal = mMin + surplus;
			var fuelCost = mTotal * _fuelCost;
			
			//Calculating crush
			var crushPenalty = costParameters.CentralBodyIntersection * costParameters.CentralBodyIntersection * _crushLambda;
			
			//Calculating time cost
			var timeCost = costParameters.TotalTime * _timeCost; 
			
			return fuelCost + crushPenalty + timeCost + negativeDriftPenalty;
		}

		private void InitializeCostCalculator()
		{
			var targets = new List<TargetParameters>();
			for (int i = 0; i < _targetsInitialPositions.Length; i++)
			{
				var pos = _targetsInitialPositions[i];
				var vel = _targetsInitialVelocities[i];
				var orbit = OrbitHelper.GetOrbit(vel, pos, _mu);
				var target = new TargetParameters()
				{
					InitialOrbit = orbit,
					ServiceTime = _targetsServiceTimes[i]
				};
				targets.Add(target);
			}
			var shipStartOrbit = OrbitHelper.GetOrbit(_shipInitialVel, _shipInitialPos, _mu);
			_costCalculator = new MultiTargetCostCalculator()
			{
				Targets = targets,
				CentralBodyRadius = _centralBodyRadius,
				Mu = _mu,
				ShipStartOrbit = shipStartOrbit
			};
		}
	}
}