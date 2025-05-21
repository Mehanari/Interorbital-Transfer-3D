using System;
using MehaMath.Math.Components;
using MehaMath.VisualisationTools.Plotting;
using Src.FinalComponents;
using Src.Helpers;
using UnityEngine;

namespace Src
{
	public class GradientDescentTest : MonoBehaviour
	{
		[SerializeField] private Plotter3D plotter;
		[SerializeField] private Vector3 scale = new Vector3(0.001f, 0.001f, 0.00001f);
		
		//Constants
		private double _mu = 398600.4418d;
		private double _g0 = 9.80665; //Standard gravitational acceleration
		private double _mDry = 100; //Mass of the spacecraft without fuel
		private double _isp = 300; //Engine specific impulse
        
		//Ship initial state
		private Vector _r1 = new Vector(8000.0, 1000.0, 2000.0); //Initial position of the ship
		private Vector _v1 = new Vector(-1.5, 7.0, 2.5); //Initial velocity of the ship
        
		//Satellite initial state
		private Vector _v2 = new Vector(-5.3, -3.2, 4.1); //Satellite initial velocity
		private Vector _r2 = new Vector(-5000.0, 7000.0, -3000.0); //Satellite initial position

		private double _minDriftTime = 0;
		private double _maxDriftTime = 0;
		private double _minTransferTime = 1000;
		private double _maxTransferTime = 5000;
		private int _timePoints = 51;
		private double _serviceTime = 10d;
		private double _centralBodyRadius = 6500d; //Earth's radius + very low Earth orbit height
		private double _crushLambda = 10;
		private double _fuelCost = 1;
		private double _timeCost = 2;
		private SingleTargetCostCalculator _costCalculator;

		private void Start()
		{
			var targetInitialOrbit = OrbitHelper.GetOrbit(_v2, _r2, _mu);
			var targetParameters = new TargetParameters()
			{
				InitialOrbit = targetInitialOrbit,
				ServiceTime = _serviceTime
			};
			var shipStartOrbit = OrbitHelper.GetOrbit(_v1, _r1, _mu);
			_costCalculator = new SingleTargetCostCalculator()
			{
				Mu = _mu,
				Target = targetParameters,
				StartOrbit = shipStartOrbit,
				CentralBodyRadius = _centralBodyRadius
			};
			var initialGuess = new Vector(3000d, 10000d);
			var min = Minimize(Objective, initialGuess, iterationsLimit: 2000);
			Debug.Log("Min: " + min);
			Debug.Log("Cost: " + Objective(min));
		}

		public double Objective(Vector times)
		{
			var driftTime = times[0];
			var transferTime = times[1];
			var negativeDriftPenalty = 0d;
			if (driftTime < 0)
			{
				negativeDriftPenalty += driftTime * driftTime;
				driftTime = 0d;
			}
			var costParameters = _costCalculator.CalculateCost(driftTime, transferTime);
			
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

		private Vector Minimize(Func<Vector, 
			double> objective, Vector initialGuess, double stepSize = 0.1d,
			double tolerance = 0.1d, int iterationsLimit = 1000)
		{
			var change = double.MaxValue; //When the change is low we stop the algorithm
			var iteration = 0;
			var x = initialGuess;
			while (iteration < iterationsLimit || change > tolerance)
			{
				iteration++;
				var gradient = Gradient(objective, x);
				var oldCost = objective(x);
				x = x - gradient*stepSize;
				//Applying projection
				var projectedX = new Vector(x.Length);
				for (int i = 0; i < projectedX.Length; i++)
				{
					projectedX[i] = Math.Max(x[i], 0);
				}
				x = projectedX;
				var newCost = objective(x);
				change = Math.Abs(oldCost - newCost);
			}

			return x;
		}

		private Vector Gradient(Func<Vector, double> objective, Vector x, double h = 1e-5)
		{
			var deriv = new Vector(x.Length);
			for (int i = 0; i < x.Length; i++)
			{
				var variableIndex = i;
				Func<double, double> oneDimensionalObjective = (input) =>
				{
					var inputVector = new Vector(x);
					inputVector[variableIndex] = input;
					return objective(inputVector);
				};
				deriv[i] = CenteredDerivative(oneDimensionalObjective, x[variableIndex], h);
			}

			return deriv;
		}

		private static double CenteredDerivative(Func<double, double> objective, double x, double h = 1e-5)
		{
			var forward = objective(x + h);
			var backward = objective(x - h);
			var derivative = (forward - backward)/(2*h);
			return derivative;
		}
	}
}