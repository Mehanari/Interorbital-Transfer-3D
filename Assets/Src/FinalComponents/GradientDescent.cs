using System;
using MehaMath.Math.Components;

namespace Src.FinalComponents
{
	public static class GradientDescent
	{
		public static Vector Minimize(Func<Vector, 
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

		private static Vector Gradient(Func<Vector, double> objective, Vector x, double h = 1e-5)
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