using System;

namespace Src.ControlGeneration.GeneticAlgorithms
{
	public class ControlGenomeBounds
	{
		public int PolynomialsDegree { get; } = 5;
		public double MinCoefficientValue { get;  } = -Math.PI;
		public double MaxCoefficientValue { get; } = Math.PI;
		public double MinTrueAnomaly { get;  } = 0;
		public double MaxTrueAnomaly { get; } = Math.PI * 2;
		public double MinBurnTime { get; } = 1;
		public double MaxBurnTime { get; } = 1000;

		public ControlGenomeBounds(int polynomialsDegree, double minCoefficientValue, double maxCoefficientValue, double minTrueAnomaly, double maxTrueAnomaly, double minBurnTime, double maxBurnTime)
		{
			PolynomialsDegree = polynomialsDegree;
			MinCoefficientValue = minCoefficientValue;
			MaxCoefficientValue = maxCoefficientValue;
			MinTrueAnomaly = minTrueAnomaly;
			MaxTrueAnomaly = maxTrueAnomaly;
			MinBurnTime = minBurnTime;
			MaxBurnTime = maxBurnTime;
		}

		public double[] MaximumPermissibleChanges(int polynomialsCount)
		{
			var changes = new double[2 + (PolynomialsDegree + 1) * polynomialsCount];
			changes[0] = Math.Abs(MaxTrueAnomaly - MinTrueAnomaly);
			changes[1] = Math.Abs(MaxBurnTime - MinBurnTime);
			var coefficientChange = Math.Abs(MaxCoefficientValue - MinCoefficientValue);
			for (int i = 2; i < changes.Length; i++)
			{
				changes[i] = coefficientChange;
			}

			return changes;
		}
	}
}