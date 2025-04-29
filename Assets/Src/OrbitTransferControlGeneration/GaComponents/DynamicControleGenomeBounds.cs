using System;

namespace Src.OrbitTransferControlGeneration.GaComponents
{
	public class DynamicControlGenomeBounds : IControlGenomeBounds
	{
		private double _burnMinTime;
		private double _burnMaxTime;
		private double _trueAnomalyMin;
		private double _trueAnomalyMax;
		private double _coefficientMin;
		private double _coefficientMax;
		private int _polynomialsDegree;

		public int PolynomialsDegree => _polynomialsDegree;

		public DynamicControlGenomeBounds(double burnMinTime, double burnMaxTime, double trueAnomalyMin, 
			double trueAnomalyMax, double coefficientMin, double coefficientMax, 
			int polynomialsDegree)
		{
			_burnMinTime = burnMinTime;
			_burnMaxTime = burnMaxTime;
			_trueAnomalyMin = trueAnomalyMin;
			_trueAnomalyMax = trueAnomalyMax;
			_coefficientMin = coefficientMin;
			_coefficientMax = coefficientMax;
			_polynomialsDegree = polynomialsDegree;
		}

		public (double min, double max) TrueAnomalyRange()
		{
			return (_trueAnomalyMin, _trueAnomalyMax);
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