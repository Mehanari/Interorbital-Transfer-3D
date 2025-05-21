using System;
using MehaMath.Math.Components;
using MehaMath.Math.RootsFinding;
using MehaMath.VisualisationTools.Plotting;
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
		private SingleTargetCostCalculator _costCalculator;

		private void Start()
		{
			_costCalculator = new SingleTargetCostCalculator()
			{
				Mu = _mu,
				g0 = _g0,
				MDry = _mDry,
				Isp = _isp,
				SpacecraftInitPos = _r1,
				SpacecraftInitVel = _v1,
				SatelliteInitPos = _r2,
				SatelliteInitVel = _v2,
				TimePrice = 1
			};
			var initialGuess = new Vector(1000d, 1000d);
			var min = Minimize(Objective, initialGuess, iterationsLimit: 2000);
			Debug.Log("Min: " + min);
			Debug.Log("Cost: " + _costCalculator.CalculateCost(min[0], min[1]));
		}

		public double Objective(Vector times)
		{
			var driftTime = times[0];
			var transferTime = times[1];
			var penalty = 0d;
			if (driftTime < 0)
			{
				penalty += driftTime * driftTime;
				driftTime = 0d;
			}
			var cost = _costCalculator.CalculateCost(driftTime, transferTime);
			return cost + penalty;
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

		private void Plot()
		{
			var costsScaled = new float[_timePoints, _timePoints];
			var timeStepDrift = (_maxDriftTime - _minDriftTime) / (_timePoints - 1);
			var timeStepTrans = (_maxTransferTime - _minTransferTime) / (_timePoints - 1);
			var costCalculator = _costCalculator;
			for (int driftInd = 0; driftInd < _timePoints; driftInd++)
			{
				for (int transInd = 0; transInd < _timePoints; transInd++)
				{
					var driftTime = _minDriftTime + driftInd * timeStepDrift;
					var transferTime = _minTransferTime + transInd * timeStepTrans;
					var cost = costCalculator.CalculateCost(driftTime, transferTime);
					costsScaled[driftInd, transInd] = (float)cost * scale.z;
				}
			}

			var driftIntervalScaled = (float)(_maxDriftTime - _minDriftTime) * scale.x;
			var transferIntervalScaled = (float)(_maxTransferTime - _minTransferTime) * scale.y;
			plotter.PlotDots(0, transferIntervalScaled, costsScaled, "Cost", Color.yellow, dotSize: 0.005f);
		}
	}
}