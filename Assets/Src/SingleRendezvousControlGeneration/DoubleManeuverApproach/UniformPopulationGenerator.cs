using System;
using Src.GeneticAlgorithms;

namespace Src.SingleRendezvousControlGeneration.DoubleManeuverApproach
{
	public class UniformPopulationGenerator : IPopulationGenerator
	{
		private readonly IManeuverBounds _bounds;

		public UniformPopulationGenerator(IManeuverBounds bounds)
		{
			_bounds = bounds;
		}

		public Specimen[] GeneratePopulation(int populationSize)
		{
			var population = new Specimen[populationSize];
			for (int i = 0; i < populationSize; i++)
			{
				population[i] = GenerateSpecimen();
			}

			return population;
		}

		private Specimen GenerateSpecimen()
		{
			var startManeuver = GenerateManeuverGenome();
			var endManeuver = GenerateManeuverGenome();
			var genome = new double[startManeuver.Length + endManeuver.Length];
			Array.Copy(startManeuver, genome, startManeuver.Length);
			Array.Copy(endManeuver, 0, genome, startManeuver.Length, endManeuver.Length);
			return new Specimen()
			{
				Genome = genome
			};
		}

		private double[] GenerateManeuverGenome()
		{
			var rnd = new Random();
			var genome = new double[2 + (_bounds.PolynomialsDegree+1) * 3];
			var driftTimeRange = _bounds.DriftTimRange();
			var burnTimeRange = _bounds.BurnTimeRange();
			genome[0] = driftTimeRange.min + (driftTimeRange.max - driftTimeRange.min) * rnd.NextDouble();
			genome[1] = burnTimeRange.min + (burnTimeRange.max - burnTimeRange.min) * rnd.NextDouble();
			var burnTime = genome[1];

			var coefficientsRange = _bounds.CoefficientsRanges(burnTime);
			var coefficientsCount = coefficientsRange.min.Length;
			for (int i = 0; i < (_bounds.PolynomialsDegree+1)*3; i++)
			{
				var rangeIndex = i  % coefficientsCount;
				genome[i + 2] = coefficientsRange.min[rangeIndex] + (coefficientsRange.max[rangeIndex] - coefficientsRange.min[rangeIndex]) * rnd.NextDouble();
			}

			return genome;
		}
	}
}