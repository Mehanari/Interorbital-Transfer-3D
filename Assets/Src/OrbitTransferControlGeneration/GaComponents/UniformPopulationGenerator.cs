using System;
using Src.GeneticAlgorithms;

namespace Src.OrbitTransferControlGeneration.GaComponents
{
	public class UniformPopulationGenerator : IPopulationGenerator
	{
		private readonly IControlGenomeBounds _controlGenomeBounds;

		public UniformPopulationGenerator(IControlGenomeBounds controlGenomeBounds)
		{
			_controlGenomeBounds = controlGenomeBounds;
		}

		public Specimen[] GeneratePopulation(int populationSize)
		{
			var population = new Specimen[populationSize];
			for (int i = 0; i < populationSize; i++)
			{
				var genome = GenerateGenome();
				population[i] = new Specimen
				{
					Genome = genome
				};
			}

			return population;
		}
		
		private double[] GenerateGenome()
		{
			var rnd = new Random();
			var genome = new double[2 + (_controlGenomeBounds.PolynomialsDegree+1) * 3];
			var trueAnomalyRange = _controlGenomeBounds.TrueAnomalyRange();
			var burnTimeRange = _controlGenomeBounds.BurnTimeRange();
			genome[0] = trueAnomalyRange.min + (trueAnomalyRange.max - trueAnomalyRange.min) * rnd.NextDouble();
			genome[1] = burnTimeRange.min + (burnTimeRange.max - burnTimeRange.min) * rnd.NextDouble();
			var burnTime = genome[1];

			var coefficientsRange = _controlGenomeBounds.CoefficientsRanges(burnTime);
			var coefficientsCount = coefficientsRange.min.Length;
			for (int i = 0; i < (_controlGenomeBounds.PolynomialsDegree+1)*3; i++)
			{
				var rangeIndex = i  % coefficientsCount;
				genome[i + 2] = coefficientsRange.min[rangeIndex] + (coefficientsRange.max[rangeIndex] - coefficientsRange.min[rangeIndex]) * rnd.NextDouble();
			}

			return genome;
		}

	}
}