using System;

namespace Src.ControlGeneration.GeneticAlgorithms
{
	/// <summary>
	/// Formulae for this crossover are taken from "Self-Adaptive Genetic Algorithms with Simulated Binary Crossover" by Kalyanmoy Deb
	/// </summary>
	public class SbxCrossoverOperator : ICrossoverOperator
	{
		private double _crossoverProbability;
		
		/// <summary>
		/// For each gene in a genome determines whether to generate a new gene with a crossover, or copy it directly to the offsprings.
		/// </summary>
		public double CrossoverProbability 
		{ 
			get => _crossoverProbability;
			set => _crossoverProbability = Math.Clamp(value, 0, 1);
		}
		/// <summary>
		/// Large value of distribution index leads to higher probability of creating offspring that are closer to parents, stimulating exploitation.
		/// Smaller values lead to offspring being further from parents, stimulating exploration.
		/// </summary>
		public double DistributionIndex { get; set; }

		public SbxCrossoverOperator(double crossoverProbability, double distributionIndex)
		{
			CrossoverProbability = crossoverProbability;
			DistributionIndex = distributionIndex;
		}

		public (double[] offspringA, double[] offspringB) Crossover(double[] parentA, double[] parentB)
		{
			if (parentA.Length != parentB.Length)
			{
				throw new InvalidOperationException("Cannot crossover parents with genomes of different length. Length of parent A: " + parentA.Length + ", length of parent B: " + parentB.Length);
			}

			var rnd = new Random();
			var offspringA = new double[parentA.Length];
			var offspringB = new double[parentB.Length];
			for (int i = 0; i < parentA.Length; i++)
			{
				if (rnd.NextDouble() > _crossoverProbability) continue;

				var sum = parentA[i] + parentB[i]; //Sum of chromosomes values
				var difference = Math.Abs(parentA[i] - parentB[i]);
				var spreadFactor = GetSpreadFactor();
				offspringA[i] = 0.5 * (sum - spreadFactor * difference);
				offspringB[i] = 0.5 * (sum + spreadFactor * difference);
			}

			return (offspringA, offspringB);
		}

		private double GetSpreadFactor()
		{
			var rnd = new Random();
			var u = rnd.NextDouble();
			if (u <= 0.5)
			{
				return Math.Pow(2 * u, 1 / (DistributionIndex + 1));
			}
			else
			{
				return Math.Pow(1 / (2 * (1 - u)), 1 / (DistributionIndex + 1));
			}
		}
	}
}