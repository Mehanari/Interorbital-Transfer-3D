using System;

namespace Src.GeneticAlgorithms.Mutators
{
	public class PolynomialGeneMutator
	{
		public double DistributionIndex { get; set; }
		public double MutationProbability { get; set; }
		protected Random Rnd = new();

		public PolynomialGeneMutator(double distributionIndex, double mutationProbability)
		{
			DistributionIndex = distributionIndex;
			MutationProbability = mutationProbability;
		}

		/// <summary>
		/// Mutates given value with mutation probability.
		/// Can return the same gene value, if mutation did not occur.
		/// </summary>
		/// <param name="geneValue"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public double MutateGene(double geneValue, double min, double max)
		{
			if (Rnd.NextDouble() < MutationProbability)
			{
				var maxChange = Math.Abs(max - min);
				var perturbation = GetPerturbation();
				geneValue += perturbation * maxChange;
			}
			return geneValue;
		}
		
		private double GetPerturbation()
		{ ;
			var u = Rnd.NextDouble();
			if (u < 0.5)
			{
				return Math.Pow(2 * u, 1 / (1 + DistributionIndex)) - 1;
			}
			else
			{
				return 1 - Math.Pow(2 * (1 - u), 1 / (1 + DistributionIndex));
			}
		}
	}
}