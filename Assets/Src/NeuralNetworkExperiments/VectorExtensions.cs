using System;
using MehaMath.Math.Components;

namespace Src.NeuralNetworkExperiments
{
	public static class VectorExtensions
	{
		public static double WeightedSum(this Vector vector, Vector weights)
		{
			if (vector.Length != weights.Length)
			{
				throw new ArgumentException(
					"Cannot calculate the weighted sum. The length of the weights vector differs from the values vector.");
			}

			var sum = 0d;
			for (int i = 0; i < vector.Length; i++)
			{
				sum += vector[i] * weights[i];
			}

			return sum;
		}
	}
}