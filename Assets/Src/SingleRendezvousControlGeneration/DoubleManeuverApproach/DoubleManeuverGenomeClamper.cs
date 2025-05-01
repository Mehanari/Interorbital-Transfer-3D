using System;
using Src.GeneticAlgorithms;

namespace Src.SingleRendezvousControlGeneration.DoubleManeuverApproach
{
	public class DoubleManeuverGenomeClamper : GenomeClamper
	{
		private readonly IManeuverBounds _maneuverBounds;
		private readonly int _polynomialsCount;

		public DoubleManeuverGenomeClamper(IManeuverBounds maneuverBounds, int polynomialsCount)
		{
			_maneuverBounds = maneuverBounds;
			_polynomialsCount = polynomialsCount;
		}

		protected override double GetClamped(int geneIndex, double[] genome)
		{
			var geneValue = genome[geneIndex];
			var maneuverGenomeLength = 2 + (_maneuverBounds.PolynomialsDegree + 1) * _polynomialsCount;
			var maneuverGeneIndex = geneIndex % maneuverGenomeLength;
			var min = 0d;
			var max = 0d;
			if (maneuverGeneIndex == 0)
			{
				(min, max) = _maneuverBounds.DriftTimRange();
			}
			else if (maneuverGeneIndex == 1)
			{
				(min, max) = _maneuverBounds.BurnTimeRange();
			}
			else
			{
				var burnTimeIndex = geneIndex < maneuverGenomeLength ? 1 : maneuverGenomeLength + 1; 
				var burnTime = genome[burnTimeIndex];
				var coefficientsRanges = _maneuverBounds.CoefficientsRanges(burnTime);
				var coefIndex = (geneIndex - 2) % (_maneuverBounds.PolynomialsDegree + 1);
				min = coefficientsRanges.min[coefIndex];
				max = coefficientsRanges.max[coefIndex];
			}

			return Math.Clamp(geneValue, min, max);
		}
	}
}