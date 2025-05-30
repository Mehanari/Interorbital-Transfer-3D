using System;
using MehaMath.Math.Components;

namespace Src.OptimizationFramework.MathComponents
{
	public static class GradientDescent
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objective"></param>
		/// <param name="initialGuess"></param>
		/// <param name="initialStepSize"></param>
		/// <param name="tolerance"></param>
		/// <param name="iterationsLimit"></param>
		/// <param name="projection">Functions that clamps the input vector into a feasible region</param>
		/// <param name="useLineSearch"></param>
		/// <returns></returns>
		public static Vector Minimize(Func<Vector, 
				double> objective, Vector initialGuess, double initialStepSize = 0.1d,
			double tolerance = 0.1d, int iterationsLimit = 1000, Func<Vector, Vector> projection = null, bool useLineSearch = true)
		{
			var change = double.MaxValue; //When the change is low we stop the algorithm
			var iteration = 0;
			var x = initialGuess;
			var stepSize = initialStepSize;
			var currentCost =  objective(x);
			while (iteration < iterationsLimit && change > tolerance)
			{
				iteration++;
				var gradient = Gradient(objective, x);
				if (useLineSearch)
				{
					stepSize = ArmijoLineSearch(objective, x, gradient, projection);
				}
				
				x = x - gradient*stepSize;
				//Applying projection
				if (projection is not null)
				{
					x = projection(x);
				}
				var newCost = objective(x);
				change = Math.Abs(currentCost - newCost);
				currentCost = newCost;
			}

			return x;
		}

		/// <summary>
		/// Perform a single step in the steepest slope direction.
		/// </summary>
		/// <param name="objective"></param>
		/// <param name="currentGuess"></param>
		/// <param name="maxStepSize"></param>
		/// <param name="projection"></param>
		/// <param name="useLineSearch"></param>
		/// <returns></returns>
		public static Vector Step(Func<Vector, double> objective, Vector currentGuess, double maxStepSize,
			Func<Vector, Vector> projection = null, bool useLineSearch = true)
		{
			var x = currentGuess;
			var gradient = Gradient(objective, x);
			var stepSize = maxStepSize;
			if (useLineSearch)
			{
				stepSize = ArmijoLineSearch(objective, x, gradient, projection, maxStepSize: maxStepSize);
			}
			x = x - gradient * stepSize;
			if (projection is not null)
			{
				x = projection(x);
			}
			return x;
		}
		
		// Armijo Line Search (Backtracking)
		// The step size choosing algorithm from "Numerical Optimization" by J. Nocedal and S. Wright. Chapter 3. 
		private static double ArmijoLineSearch(Func<Vector, double> objective, Vector x, 
			Vector gradient, Func<Vector, Vector> projection = null, double c1 = 1e-4, 
			double rho = 0.5, double maxStepSize = 1.0)
		{
			var stepSize = maxStepSize;
			var currentValue = objective(x);
			var gradientNormSquared = Vector.DotProduct(gradient, gradient);
        
			while (stepSize > 1e-10)
			{
				var newX = x - gradient * stepSize;
				if (projection != null)
					newX = projection(newX);
                
				var newValue = objective(newX);
            
				// Armijo condition
				if (newValue <= currentValue - c1 * stepSize * gradientNormSquared)
				{
					return stepSize;
				}
            
				stepSize *= rho; // Reduce step size
			}
        
			return stepSize;
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