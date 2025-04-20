using System;
using MehaMath.Math.Components;
using Src.Model;
using UnityEngine;

namespace Src.SpacecraftDynamics.CentralBodyDynamics
{
	public class Rkf45Dynamics : CentralBodyDynamics
	{
		public double Tolerance { get; set; } = 1e-6;
		public double SafetyFactor { get; set; } = 0.84;
		public double MinStepSize { get; set; } = 0.001d;
		public double MaxStepSize { get; set; } = 1.0;
		
		/// <summary>
		/// This is a Runge-Kutta-Fehlberg method. Google the name if you want to know what are those magic numbers.
		/// This is a standard algorithm though.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <param name="deltaT"></param>
		/// <returns></returns>
		public override Spacecraft PropagateState(Spacecraft spacecraft, double deltaT)
		{
			var t = 0d;
			var dt = deltaT;
			Spacecraft current = spacecraft;

			while (t < deltaT)
			{
				var step = Math.Min(dt, deltaT - t);
				var (newState, newDt, appliedStepSize) = RKF45Step(spacecraft, step);
				current = newState;
				dt = newDt;
				t += appliedStepSize;
			}

			return current;
		}

		private (Spacecraft state, double nextStepSize, double appliedStepSize) RKF45Step(Spacecraft spacecraft, double deltaT)
		{
			var error = double.MaxValue;
			var solution = spacecraft.ToStateVector();
			var s = 0d;
			while (error > Tolerance)
			{
				var x = spacecraft.ToStateVector();
				var k1 = Derivative(x, spacecraft);
				var k2 = Derivative(x + k1 * deltaT / 4, spacecraft);
				var k3 = Derivative(x + k1 * 3 * deltaT / 32 + k2 * 9 * deltaT / 32, spacecraft);
				var k4 = Derivative(x + k1 * 1932 * deltaT / 2197 - k2 * 7200 * deltaT / 2197 + k3 * 7296 * deltaT / 2197,
					spacecraft);
				var k5 = Derivative(
					x + k1 * 439 * deltaT / 216 - k2 * 8 * deltaT + k3 * 3680 * deltaT / 513 - k4 * 845 * deltaT / 4104,
					spacecraft);
				var k6 = Derivative(
					x - k1 * 8 * deltaT / 27 + k2 * 2 * deltaT - k3 * 3544 * deltaT / 2565 + k4 * deltaT * 1859 / 4104 -
					k5 * 11 * deltaT / 40, spacecraft);
				var xrk4 = x + (k1 * 25 / 216 + k3 * 1408 / 2565 + k4 * 2197 / 4104 - k5 / 5)*deltaT;
				var xrk5 = x + (k1 * 16 / 135 + k3 * 6656 / 12825 + k4 * 28561 / 56430 - k5 * 9 / 50 + k6 * 2 / 55) *
					deltaT;
				error = ComputeError(xrk4, xrk5);
				s = SafetyFactor * Math.Pow(Tolerance * deltaT / error, 0.25);
				if (error <= Tolerance)
				{
					solution = xrk5;
				}
				else
				{
					var newDeltaT = s * deltaT;
					if (newDeltaT < MinStepSize)
					{
						Debug.LogWarning("The recommended new step size is too small: " + newDeltaT + ".\nAdjusting tolerance and min step size is advised.");
						solution = xrk5;
						break;
					}
					deltaT = Math.Max(MinStepSize, s * deltaT);
				}
			}
			
			Spacecraft newState = spacecraft.FromStateVector(solution, spacecraft.Position.Length);
			if (newState.FuelMass < 0) newState.FuelMass = 0;
			double nextStepSize = Math.Max(MinStepSize, Math.Min(MaxStepSize, s * deltaT));
			return (newState, nextStepSize, deltaT);
		}

		private double ComputeError(Vector rk4Solution, Vector rk5Solution)
		{
			var diff = rk5Solution - rk4Solution;
			var error = diff.Magnitude();
			return error;
		}
		
		private Vector Derivative(Vector stateVector, Spacecraft spacecraft)
		{
			return Derivative(spacecraft.FromStateVector(stateVector, spacecraft.Position.Length));
		}

		/// <summary>
		/// The first n values are the position rate of change. Next n values are velocity rate of change.
		/// The last value is fuel mass rate of change.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		private Vector Derivative(Spacecraft spacecraft)
		{
			return Vector.Combine(spacecraft.Velocity, GetAcceleration(spacecraft),
				new Vector(-spacecraft.FuelConsumptionRate));
		}

		/// <summary>
		/// The t is passed in case if some parameters depend on time, like exhaust direction or fuel consumption rate due to control.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		private Vector GetAcceleration(Spacecraft spacecraft)
		{
			var displacement = CentralBodyPosition - spacecraft.Position;
			var gravitationalComponent = displacement.Normalized() * GravitationalParameter / displacement.MagnitudeSquare();
			var engineComponent = spacecraft.ExhaustDirection * (-1) * spacecraft.ExhaustVelocityModule *
				spacecraft.FuelConsumptionRate / spacecraft.TotalMass;
			engineComponent /= spacecraft.ExhaustVelocityConversionRate;
			if (spacecraft.FuelMass <= 0)
			{
				engineComponent *= 0;
			}

			return gravitationalComponent + engineComponent;
		}
	}
}