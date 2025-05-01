using Src.GeneticAlgorithms.Mutators;

namespace Src.SingleRendezvousControlGeneration.DoubleManeuverApproach
{
	public class DoubleManeuverMutator : PolynomialPopulationMutator
	{
		private readonly IManeuverBounds _maneuverBounds;
		private readonly int _polynomialsCount;
		
		public DoubleManeuverMutator(PolynomialGeneMutator geneMutator, IManeuverBounds maneuverBounds, int polynomialsCount) : base(geneMutator)
		{
			_maneuverBounds = maneuverBounds;
			_polynomialsCount = polynomialsCount;
		}

		protected override (double min, double max) GetRangeForGene(int geneIndex, double[] genome)
		{
			var maneuverGeneLength = 2 + (_maneuverBounds.PolynomialsDegree + 1) * _polynomialsCount;
			geneIndex %= maneuverGeneLength;
			if (geneIndex == 0)
			{
				return _maneuverBounds.DriftTimRange();
			}

			if (geneIndex == 1)
			{
				return _maneuverBounds.BurnTimeRange();
			}

			//Remember: genome contains data for two maneuvers, and each has its own burn time.
			var burnTimeIndex = geneIndex < maneuverGeneLength ? 1 : maneuverGeneLength + 1; 
			var burnTime = genome[burnTimeIndex];
			var coefficientsRanges = _maneuverBounds.CoefficientsRanges(burnTime);
			var coefIndex = (geneIndex - 2) % (_maneuverBounds.PolynomialsDegree + 1);
			var min = coefficientsRanges.min[coefIndex];
			var max = coefficientsRanges.max[coefIndex];
			return (min, max);
		}
	}
}