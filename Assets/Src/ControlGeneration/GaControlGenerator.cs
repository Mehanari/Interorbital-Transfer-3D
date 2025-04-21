using System;
using Src.Model;

namespace Src.ControlGeneration
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
		public GenomeRestrictions GenomeRestrictions { get; set; }
		public ControlPrecisionEvaluator Evaluator { get; set; }
		public TournamentSelector ParentsPoolSelector { get; set; } 
		
		public override ControlData GenerateControl()
		{
			var population = GenerateInitialPopulation();
			EvaluatePopulation(population);
			Array.Sort(population, new SpecimenComparer());

			var elites = SelectElite(population);
			var parentsPool = ParentsPoolSelector.SelectPool(population, ParentPoolSize);
			throw new System.NotImplementedException();
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
			var genome = new double[2 + (GenomeRestrictions.PolynomialsDegree+1) * 3];
			genome[0] = GenomeRestrictions.MinTrueAnomaly + (GenomeRestrictions.MaxTrueAnomaly - GenomeRestrictions.MinTrueAnomaly) * rnd.NextDouble();
			genome[1] = GenomeRestrictions.MinBurnTime + (GenomeRestrictions.MaxBurnTime - GenomeRestrictions.MinBurnTime) * rnd.NextDouble();
			
			for (int i = 0; i < (GenomeRestrictions.PolynomialsDegree+1)*3; i++)
			{
				genome[i + 2] = GenomeRestrictions.MinCoefficientValue + (GenomeRestrictions.MaxCoefficientValue - GenomeRestrictions.MinCoefficientValue) * rnd.NextDouble();
			}

			return genome;
		}

		private ControlData FromGenome(double[] genome)
		{
			var trueAnomaly = genome[0];
			var burnTime = genome[1];
			var polynomialCoefficientsCount = GenomeRestrictions.PolynomialsDegree + 1;
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