using System;
using Src.GeneticAlgorithms;

namespace Src.OrbitTransferControlGeneration.GaComponents
{
	/// <summary>
	/// Polynomial mutator specifically for spacecraft control genes.
	/// </summary>
	public class PolynomialControlMutator : IMutator
	{
		/// <summary>
		/// Bigger value of this parameter leads to smaller mutations being more likely.
		/// Smaller leads to bigger mutations being more likely.
		/// </summary>
		public double DistributionIndex { get; set; }
		public double MutationProbability { get; set; }
		/// <summary>
		/// Each value in this array represents a maximum change value for each gene in a genome.
		/// </summary>
		public IControlGenomeBounds ControlGenomeBounds { get; set; }

		public PolynomialControlMutator(double distributionIndex, double mutationProbability, IControlGenomeBounds controlGenomeBounds)
		{
			DistributionIndex = distributionIndex;
			MutationProbability = mutationProbability;
			ControlGenomeBounds = controlGenomeBounds;
		}

		public void MutatePopulation(Specimen[] population)
		{
			if (population.Length == 0)
			{
				throw new InvalidOperationException("Cannot mutate population with length 0, because there is no one to mutate.");
			}

			foreach (var specimen in population)
			{
				MutateSpecimen(specimen);
			}
		}

		private void MutateSpecimen(Specimen specimen)
		{
			var rnd = new Random();
			//Mutating the true anomaly
			if (rnd.NextDouble() < MutationProbability)
			{
				var trueAnomalyBounds = ControlGenomeBounds.TrueAnomalyRange();
				var maxTrueAnomalyChange = Math.Abs(trueAnomalyBounds.max - trueAnomalyBounds.min);
				var perturbation = GetPerturbation();
				specimen.Genome[0] += perturbation * maxTrueAnomalyChange;
			}

			//Mutating the burn time
			if (rnd.NextDouble() < MutationProbability)
			{
				var burnTimeBounds = ControlGenomeBounds.BurnTimeRange();
				var maxBurnTimeChange = Math.Abs(burnTimeBounds.max - burnTimeBounds.min);
				var perturbation = GetPerturbation();
				specimen.Genome[1] += perturbation * maxBurnTimeChange;
			}
			
			//Mutating the coefficients
			var newBurnTime = specimen.Genome[1];
			var coefficientsRange = ControlGenomeBounds.CoefficientsRanges(newBurnTime);
			var coefficientsCount = coefficientsRange.min.Length;
			for (int i = 2; i < specimen.Genome.Length; i++)
			{
				if(rnd.NextDouble() > MutationProbability) continue;
				var rangeIndex = (i - 2) % coefficientsCount;
				var min = coefficientsRange.min[rangeIndex];
				var max = coefficientsRange.max[rangeIndex];
				var maxChange = Math.Abs(min - max);
				var perturbation = GetPerturbation();
				specimen.Genome[i] += perturbation * maxChange;
			}
		}
		

		private double GetPerturbation()
		{
			var rnd = new Random();
			var u = rnd.NextDouble();
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