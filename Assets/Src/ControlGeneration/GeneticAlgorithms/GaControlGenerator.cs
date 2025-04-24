using System;
using Src.Model;

namespace Src.ControlGeneration.GeneticAlgorithms
{
	public class GaControlGenerator : OrbitTransferControlGenerator
	{
		/// <summary>
		/// Values range from 0 to 1. Decides how many best specimen should go to the next sortedPopulation directly.
		/// </summary>
		public double ElitismIndex { get; set; } = 0.01;
		public int PopulationSize { get; set; } = 200;
		public int MaxGenerations { get; set; } = 100;
		public int ParentPoolSize { get; set; } = 198;

		/// <summary>
		/// Smaller -> more precise.
		/// </summary>
		public double DesirableFitness { get; set; } = 0.1d;
		public IControlGenomeBounds ControlGenomeBounds { get; set; }
		public ControlPrecisionEvaluator Evaluator { get; set; }
		public TournamentSelector ParentsPoolSelector { get; set; } 
		public ICrossoverOperator CrossoverOperator { get; set; }
		public IMutator Mutator { get; set; }

		public GaControlGenerator(double elitismIndex, int populationSize, int maxGenerations, int parentPoolSize, 
			double desirableFitness, IControlGenomeBounds controlGenomeBounds, ControlPrecisionEvaluator evaluator, 
			TournamentSelector parentsPoolSelector, ICrossoverOperator crossoverOperator, IMutator mutator)
		{
			ElitismIndex = elitismIndex;
			PopulationSize = populationSize;
			MaxGenerations = maxGenerations;
			ParentPoolSize = parentPoolSize;
			DesirableFitness = desirableFitness;
			ControlGenomeBounds = controlGenomeBounds;
			Evaluator = evaluator;
			ParentsPoolSelector = parentsPoolSelector;
			CrossoverOperator = crossoverOperator;
			Mutator = mutator;
		}

		public override ControlData GenerateControl()
		{
			var population = GenerateInitialPopulation();
			EvaluatePopulation(population);
			Array.Sort(population, new SpecimenComparer());
			var bestSpecimen = population[0];
			
			for (int i = 0; i < MaxGenerations; i++)
			{
				if (bestSpecimen.Fitness < DesirableFitness)
				{
					break;
				}
				
				var elites = SelectElite(population);
				var parentsPool = ParentsPoolSelector.SelectPool(population, ParentPoolSize);
				var children = GenerateChildren(parentsPool, PopulationSize - elites.Length);
				Mutator.MutatePopulation(children);
				ClampGenes(children);
				
				var newPopulation = new Specimen[PopulationSize];
				Array.Copy(elites, newPopulation, elites.Length);
				Array.Copy(children, 0, newPopulation, elites.Length, children.Length);

				population = newPopulation;
				EvaluatePopulation(population);
				Array.Sort(population, new SpecimenComparer());
				bestSpecimen = population[0];
			}

			return FromGenome(bestSpecimen.Genome);
		}

		private void ClampGenes(Specimen[] population)
		{
			var trueAnomalyRange = ControlGenomeBounds.TrueAnomalyRange();
			var burnTimeRange = ControlGenomeBounds.BurnTimeRange();
			for (int i = 0; i < population.Length; i++)
			{
				var specimen = population[i];
				specimen.Genome[0] = Math.Clamp(specimen.Genome[0], trueAnomalyRange.min, trueAnomalyRange.max);
				specimen.Genome[1] = Math.Clamp(specimen.Genome[1], burnTimeRange.min, burnTimeRange.max);
				var coefficientsRange = ControlGenomeBounds.CoefficientsRanges(specimen.Genome[1]);
				var coefficientsCount = coefficientsRange.min.Length;
				for (int j = 0; j < (ControlGenomeBounds.PolynomialsDegree+1)*3; j++)
				{
					var rangeIndex = j  % coefficientsCount;
					var min = coefficientsRange.min[rangeIndex];
					var max = coefficientsRange.max[rangeIndex];
					specimen.Genome[j + 2] = Math.Clamp(specimen.Genome[j + 2], min, max);
				}
			}
		}

		private Specimen[] GenerateChildren(Specimen[] parents, int childrenCount)
		{
			var rnd = new Random();
			var children = new Specimen[childrenCount];
			for (int i = 0; i < childrenCount; i+=2)
			{
				var parentA = parents[rnd.Next(0, parents.Length)];
				var parentB = parents[rnd.Next(0, parents.Length)];
				var (childGenomeA, childGenomeB) = CrossoverOperator.Crossover(parentA.Genome, parentB.Genome);
				children[i] = new Specimen()
				{
					Genome = childGenomeA
				};
				if (i + 1 < childrenCount)
				{
					children[i + 1] = new Specimen()
					{
						Genome = childGenomeB
					};
				}
			}

			return children;
		}

		private Specimen[] SelectElite(Specimen[] sortedPopulation)
		{
			var elitesCount = (int)(sortedPopulation.Length * ElitismIndex);
			var elites = new Specimen[elitesCount];
			for (int i = 0; i < elitesCount; i++)
			{
				elites[i] = sortedPopulation[i];
			}

			return elites;
		}

		private Specimen[] GenerateInitialPopulation()
		{
			var population = new Specimen[PopulationSize];
			for (int i = 0; i < PopulationSize; i++)
			{
				var genome = GenerateGenome();
				population[i] = new Specimen
				{
					Genome = genome
				};
			}

			return population;
		}

		private void EvaluatePopulation(Specimen[] population)
		{
			foreach (var specimen in population)
			{
				specimen.Fitness = Evaluator.EvaluateControl(FromGenome(specimen.Genome));
			}
		}

		private double[] GenerateGenome()
		{
			var rnd = new Random();
			var genome = new double[2 + (ControlGenomeBounds.PolynomialsDegree+1) * 3];
			var trueAnomalyRange = ControlGenomeBounds.TrueAnomalyRange();
			var burnTimeRange = ControlGenomeBounds.BurnTimeRange();
			genome[0] = trueAnomalyRange.min + (trueAnomalyRange.max - trueAnomalyRange.min) * rnd.NextDouble();
			genome[1] = burnTimeRange.min + (burnTimeRange.max - burnTimeRange.min) * rnd.NextDouble();
			var burnTime = genome[1];

			var coefficientsRange = ControlGenomeBounds.CoefficientsRanges(burnTime);
			var coefficientsCount = coefficientsRange.min.Length;
			for (int i = 0; i < (ControlGenomeBounds.PolynomialsDegree+1)*3; i++)
			{
				var rangeIndex = i  % coefficientsCount;
				genome[i + 2] = coefficientsRange.min[rangeIndex] + (coefficientsRange.max[rangeIndex] - coefficientsRange.min[rangeIndex]) * rnd.NextDouble();
			}

			return genome;
		}

		private ControlData FromGenome(double[] genome)
		{
			var trueAnomaly = genome[0];
			var burnTime = genome[1];
			var polynomialCoefficientsCount = ControlGenomeBounds.PolynomialsDegree + 1;
			var alphaCoefs = new double[polynomialCoefficientsCount];
			var betaCoefs = new double[polynomialCoefficientsCount];
			var gammaCoefs = new double[polynomialCoefficientsCount];
			for (int i = 0; i < polynomialCoefficientsCount; i++)
			{
				alphaCoefs[i] = genome[i + 2];
			}

			for (int i = 0; i < polynomialCoefficientsCount; i++)
			{
				betaCoefs[i] = genome[i + 2 + polynomialCoefficientsCount];
			}

			for (int i = 0; i < polynomialCoefficientsCount; i++)
			{
				gammaCoefs[i] = genome[i + 2 + polynomialCoefficientsCount * 2];
			}

			return new ControlData
			{
				IgnitionTrueAnomaly = trueAnomaly,
				BurnTime = burnTime,
				AlphaPolynomialCoefficients = alphaCoefs,
				BetaPolynomialCoefficients = betaCoefs,
				GammaPolynomialCoefficients = gammaCoefs
			};
		}
	}
}