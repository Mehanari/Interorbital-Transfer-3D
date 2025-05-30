using System;
using MehaMath.Math.Components;

namespace Src.OptimizationFramework.MathComponents
{
	/// <summary>
	/// This is a gradient descent variant with application of a repeller function that helps escape the local minima.
	/// The minimize method guarantees that the result will be at least as good as for regular gradient descent.
	/// </summary>
	public class GradientDescentWithRepeller
	{
		//Repulsion strenght
		private double _amplitude;
		//Radius of the repulsion zone
		private double _width;
		
		//Gradient descent parameters
		private double _tolerance = 0.1d;
		public double _gdInitialStepSize = 0.1d;
		private int _iterationsLimit = 1000;
		private Func<Vector, Vector> _projection;
		private bool _useLineSearch = true;

		private Func<Vector, double> _costFunc;
		
		public Vector Minimize()
		{
			throw new NotImplementedException();
		}
	}
}