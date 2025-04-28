using System;
using Src.GeneticAlgorithms.Crossover;

namespace Src.GeneticAlgorithms
{
	public class Ga
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

		private IPopulationGenerator InitialPopulationGenerator { get; set; }
		private IGenomeClamper GenomeClamper { get; set; }
		private IGenomeEvaluator Evaluator { get; set; }
		private TournamentSelector ParentsPoolSelector { get; set; }
		private ICrossoverOperator CrossoverOperator { get; set; }
		private IMutator Mutator { get; set; }

		/// <summary>
		/// Creates GA with default parameters.
		/// </summary>
		/// <param name="initialPopulationGenerator"></param>
		/// <param name="controlGenomeBounds"></param>
		/// <param name="evaluator"></param>
		/// <param name="parentsPoolSelector"></param>
		/// <param name="crossoverOperator"></param>
		/// <param name="mutator"></param>
		public Ga(IPopulationGenerator initialPopulationGenerator, IGenomeEvaluator evaluator, TournamentSelector parentsPoolSelector, ICrossoverOperator crossoverOperator, IMutator mutator, IGenomeClamper genomeClamper)
		{
			InitialPopulationGenerator = initialPopulationGenerator;
			Evaluator = evaluator;
			ParentsPoolSelector = parentsPoolSelector;
			CrossoverOperator = crossoverOperator;
			Mutator = mutator;
			GenomeClamper = genomeClamper;
		}

		public Ga(double elitismIndex, int populationSize, int maxGenerations, int parentPoolSize, 
			double desirableFitness, IGenomeEvaluator evaluator, 
			TournamentSelector parentsPoolSelector, ICrossoverOperator crossoverOperator, IMutator mutator, IPopulationGenerator initialPopulationGenerator, IGenomeClamper genomeClamper)
		{
			ElitismIndex = elitismIndex;
			PopulationSize = populationSize;
			MaxGenerations = maxGenerations;
			ParentPoolSize = parentPoolSize;
			DesirableFitness = desirableFitness;
			Evaluator = evaluator;
			ParentsPoolSelector = parentsPoolSelector;
			CrossoverOperator = crossoverOperator;
			Mutator = mutator;
			InitialPopulationGenerator = initialPopulationGenerator;
			GenomeClamper = genomeClamper;
		}

		/// <summary>
		/// Returns the best genome obtained after maxGenerations, or returns it early if reached desirable fitness.
		/// Lower fitness value -> better.
		/// </summary>
		/// <returns></returns>
		public double[] Evolve()
		{
			var population = InitialPopulationGenerator.GeneratePopulation(PopulationSize);
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

			return bestSpecimen.Genome;
		}

		private void ClampGenes(Specimen[] population)
		{
			foreach (var specimen in population)
			{
				GenomeClamper.ClampGenomeFor(specimen);
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

		private void EvaluatePopulation(Specimen[] population)
		{
			foreach (var specimen in population)
			{
				specimen.Fitness = Evaluator.Evaluate(specimen.Genome);
			}
		}
	}
}