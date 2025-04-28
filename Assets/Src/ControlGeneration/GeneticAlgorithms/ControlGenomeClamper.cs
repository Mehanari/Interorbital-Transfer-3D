using System;
using Src.GeneticAlgorithms;

namespace Src.ControlGeneration.GeneticAlgorithms
{
	public class ControlGenomeClamper : IGenomeClamper
	{
		private readonly IControlGenomeBounds _bounds;

		public ControlGenomeClamper(IControlGenomeBounds bounds)
		{
			_bounds = bounds;
		}

		public void ClampGenomeFor(Specimen specimen)
		{
			var trueAnomalyRange = _bounds.TrueAnomalyRange();
			var burnTimeRange = _bounds.BurnTimeRange();
			specimen.Genome[0] = Math.Clamp(specimen.Genome[0], trueAnomalyRange.min, trueAnomalyRange.max);
			specimen.Genome[1] = Math.Clamp(specimen.Genome[1], burnTimeRange.min, burnTimeRange.max);
			var coefficientsRange = _bounds.CoefficientsRanges(specimen.Genome[1]);
			var coefficientsCount = coefficientsRange.min.Length;
			for (int j = 0; j < (_bounds.PolynomialsDegree+1)*3; j++)
			{
				var rangeIndex = j  % coefficientsCount;
				var min = coefficientsRange.min[rangeIndex];
				var max = coefficientsRange.max[rangeIndex];
				specimen.Genome[j + 2] = Math.Clamp(specimen.Genome[j + 2], min, max);
			}
		}
	}
}