using System;
using MehaMath.Math.Components;

namespace Src.OptimizationFramework.MathComponents
{
	/// <summary>
	/// This is a hybrid optimizer that combines gradient descent with grid search.
	/// It works in two phases:
	/// 1) Find the best initial guess using the grid search and the minor cost function.
	/// 2) Apply the gradient descent to the found initial guess with the major cost function.
	/// The minor and the major cost functions can be the same, but you are free to modify the minor cost.
	/// </summary>
	public class GridDescent
	{
		//Grid search parameters
		private Vector _zeroPoint;
		private Vector _difference;
		private int _pointsPerDimension;
		/// <summary>
		/// Cost function used to pick the initial guess in the grid search phase.
		/// </summary>
		private Func<Vector, double> _minorCostFunc;
		
		//Gradient descent parameters
		private double _tolerance = 0.1d;
		public double _gdInitialStepSize = 0.1d;
		private int _iterationsLimit = 1000;
		private Func<Vector, Vector> _projection;
		private bool _useLineSearch = true;
		/// <summary>
		/// Cost function used in the gradient descent phase.
		/// </summary>
		private Func<Vector, double> _majorCostFunc;

		public GridDescent(Vector zeroPoint, Vector difference, int pointsPerDimension, Func<Vector, double> minorCostFunc, 
			double tolerance, int iterationsLimit, Func<Vector, Vector> projection, bool useLineSearch, Func<Vector, double> majorCostFunc, double gdInitialStepSize)
		{
			_zeroPoint = zeroPoint;
			_difference = difference;
			_pointsPerDimension = pointsPerDimension;
			_minorCostFunc = minorCostFunc;
			_tolerance = tolerance;
			_iterationsLimit = iterationsLimit;
			_projection = projection;
			_useLineSearch = useLineSearch;
			_majorCostFunc = majorCostFunc;
			_gdInitialStepSize = gdInitialStepSize;
		}

		public Vector Minimize()
		{
			var initialGuess = GridSearcher.Minimize(_zeroPoint, _difference, _pointsPerDimension, _minorCostFunc);
			var result = GradientDescent.Minimize(_majorCostFunc, initialGuess, _gdInitialStepSize, _tolerance,
				_iterationsLimit, _projection, _useLineSearch);
			return result;
		}
	}
}