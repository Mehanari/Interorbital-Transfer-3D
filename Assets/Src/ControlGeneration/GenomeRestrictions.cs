using System;

namespace Src.ControlGeneration
{
	public class GenomeRestrictions
	{
		public int PolynomialsDegree { get; } = 5;
		public double MinCoefficientValue { get;  } = -Math.PI;
		public double MaxCoefficientValue { get; } = Math.PI;
		public double MinTrueAnomaly { get;  } = 0;
		public double MaxTrueAnomaly { get; } = Math.PI * 2;
		public double MinBurnTime { get; } = 1;
		public double MaxBurnTime { get; } = 1000;

		public GenomeRestrictions(int polynomialsDegree, double minCoefficientValue, double maxCoefficientValue, double minTrueAnomaly, double maxTrueAnomaly, double minBurnTime, double maxBurnTime)
		{
			PolynomialsDegree = polynomialsDegree;
			MinCoefficientValue = minCoefficientValue;
			MaxCoefficientValue = maxCoefficientValue;
			MinTrueAnomaly = minTrueAnomaly;
			MaxTrueAnomaly = maxTrueAnomaly;
			MinBurnTime = minBurnTime;
			MaxBurnTime = maxBurnTime;
		}
	}
}