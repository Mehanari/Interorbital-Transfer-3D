using System;
using MehaMath.Math.Components;

namespace Electrons.Scripts
{
	public class Rkf45
	{
		public double Tolerance { get; set; } = 1e-6;
		public double SafetyFactor { get; set; } = 0.84;
		public double MinStepSize { get; set; } = 0.001d;
		public double MaxStepSize { get; set; } = 1.0;
		
		public Vector PropagateState(Vector state, double deltaT, Func<Vector, Vector> dynamics)
		{
			var t = 0d;
			var dt = deltaT;
			Vector current = state;

			while (t < deltaT)
			{
				var step = Math.Min(dt, deltaT - t);
				var (newState, newDt, appliedStepSize) = RKF45Step(state, step, dynamics);
				current = newState;
				dt = newDt;
				t += appliedStepSize;
			}

			return current;
		}

		private (Vector newState, double newDt, double appliedStepSize) RKF45Step(Vector state, double deltaT, Func<Vector, Vector> dynamics)
		{
			var error = double.MaxValue;
			var solution = state;
			var s = 0d;
			while (error > Tolerance)
			{
				var x = state;
				var k1 = dynamics(x);
				var k2 = dynamics(x + k1 * deltaT / 4);
				var k3 = dynamics(x + k1 * 3 * deltaT / 32 + k2 * 9 * deltaT / 32);
				var k4 = dynamics(x + k1 * 1932 * deltaT / 2197 - k2 * 7200 * deltaT / 2197 + k3 * 7296 * deltaT / 2197);
				var k5 = dynamics(
					x + k1 * 439 * deltaT / 216 - k2 * 8 * deltaT + k3 * 3680 * deltaT / 513 - k4 * 845 * deltaT / 4104);
				var k6 = dynamics(
					x - k1 * 8 * deltaT / 27 + k2 * 2 * deltaT - k3 * 3544 * deltaT / 2565 + k4 * deltaT * 1859 / 4104 -
					k5 * 11 * deltaT / 40);
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
						solution = xrk5;
						break;
					}
					deltaT = Math.Max(MinStepSize, s * deltaT);
				}
			}
			
			double nextStepSize = Math.Max(MinStepSize, Math.Min(MaxStepSize, s * deltaT));
			return (solution, nextStepSize, deltaT);
		}

		private double ComputeError(Vector rk4Solution, Vector rk5Solution)
		{
			var diff = rk5Solution - rk4Solution;
			var error = diff.Magnitude();
			return error;
		}
	}
}