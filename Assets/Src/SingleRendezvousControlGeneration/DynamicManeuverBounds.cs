using System;

namespace Src.SingleRendezvousControlGeneration
{
	public class DynamicManeuverBounds : IManeuverBounds
	{
		private double _burnMinTime;
		private double _burnMaxTime;
		private double _driftTimeMin;
		private double _driftTimeMax;
		private double _coefficientMin;
		private double _coefficientMax;
		private int _polynomialsDegree;


		public int PolynomialsDegree => _polynomialsDegree;

		public DynamicManeuverBounds(double burnMinTime, double burnMaxTime, double driftTimeMin, double driftTimeMax, double coefficientMin, double coefficientMax, int polynomialsDegree)
		{
			_burnMinTime = burnMinTime;
			_burnMaxTime = burnMaxTime;
			_driftTimeMin = driftTimeMin;
			_driftTimeMax = driftTimeMax;
			_coefficientMin = coefficientMin;
			_coefficientMax = coefficientMax;
			_polynomialsDegree = polynomialsDegree;
		}

		public (double min, double max) DriftTimRange()
		{
			return (_driftTimeMin, _driftTimeMax);
		}

		public (double min, double max) BurnTimeRange()
		{
			return (_burnMinTime, _burnMaxTime);
		}

		public (double[] min, double[] max) CoefficientsRanges(double burnTime)
		{
			var lowerBounds = new double[_polynomialsDegree + 1];
			var upperBounds = new double[_polynomialsDegree + 1];
			for (int i = 0; i < _polynomialsDegree+1; i++)
			{
				var min = _coefficientMin / Math.Pow(burnTime, i);
				var max = _coefficientMax / Math.Pow(burnTime, i);
				lowerBounds[i] = min;
				upperBounds[i] = max;
			}

			return (lowerBounds, upperBounds);
		}
	}
}