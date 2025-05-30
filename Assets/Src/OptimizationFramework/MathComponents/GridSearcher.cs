using System;
using System.Collections.Generic;
using MehaMath.Math.Components;

namespace Src.OptimizationFramework.MathComponents
{
	public class GridSearcher
	{
		/// <summary>
		/// Generates a grid in the n-dimensional space.
		/// </summary>
		/// <param name="zeroPoint"></param>
		/// <param name="difference"></param>
		/// <param name="pointsPerDimension"></param>
		/// <param name="objective"></param>
		/// <returns></returns>
		public static Vector Minimize(Vector zeroPoint, Vector difference, int pointsPerDimension,
			Func<Vector, double> objective)
		{
			var grid = GenerateGrid(zeroPoint, difference, pointsPerDimension);
			var minCost = double.MaxValue;
			var bestSolution = grid[0];
			for (int i = 0; i < grid.Length; i++)
			{
				var point = grid[i];
				var cost = objective(point);
				if (cost < minCost)
				{
					minCost = cost;
					bestSolution = point;
				}
			}

			return bestSolution;
		}

		public static Vector[] GenerateGrid(Vector zeroPoint, Vector difference, int pointsPerDimension)
		{
			if (pointsPerDimension <= 0)
			{
				throw new ArgumentException("Points count per dimension must be positive.");
			}
			if (zeroPoint.Length != difference.Length)
			{
				throw new ArgumentException(
					"Dimensions count of the zero point must match the dimensions count of the difference vector.");
			}
			var dimensions = zeroPoint.Length;
			var points = new List<Vector>();
			points.Add(zeroPoint);
			for (int i = 0; i < dimensions; i++)
			{
				var shift = new Vector(dimensions);
				shift[i] = difference[i];
				var newPoints = new List<Vector>();
				for (int j = 0; j < points.Count; j++)
				{
					var basePoint = points[j];
					for (int k = 1; k < pointsPerDimension; k++)
					{
						newPoints.Add(basePoint + shift*k);
					}
				}
				points.AddRange(newPoints);
			}

			return points.ToArray();
		}

		/// <summary>
		/// Do not confuse pointsPerDirection with pointsPerDimension.
		/// Points per direction is how many steps to take in each direction from the center.
		/// </summary>
		/// <param name="zeroPoint"></param>
		/// <param name="difference"></param>
		/// <param name="pointsPerDirection">How many steps to take in each direction from the center</param>
		/// <returns></returns>
		public static Vector[] GenerateGridAround(Vector center, Vector difference, int pointsPerDirection)
		{
			if (pointsPerDirection < 0)
			{
				throw new ArgumentException("Points count per direction must be non-negative!.");
			}
			if (center.Length != difference.Length)
			{
				throw new ArgumentException(
					"Dimensions count of the zero point must match the dimensions count of the difference vector.");
			}

			var dimensions = center.Length;
			var points = new List<Vector>();
			points.Add(center);
			for (int i = 0; i < dimensions; i++)
			{
				var shift = new Vector(dimensions);
				shift[i] = difference[i];
				var newPoints = new List<Vector>();
				//Going forward
				for (int j = 0; j < points.Count; j++)
				{
					var basePoint = points[j];
					for (int k = 0; k < pointsPerDirection; k++)
					{
						newPoints.Add(basePoint + shift*(k+1));
					}
				}
				//Going backward
				for (int j = 0; j < points.Count; j++)
				{
					var basePoint = points[j];
					for (int k = 0; k < pointsPerDirection; k++)
					{
						newPoints.Add(basePoint - shift*(k+1));
					}
				}
				points.AddRange(newPoints);
			}

			return points.ToArray();
		}
	}
}