using System;
using MehaMath.Math.Components;
using MehaMath.Math.Components.Json;
using Newtonsoft.Json;
using Src.GeneticAlgorithms;
using Src.GeneticAlgorithms.Crossover;
using Src.GeneticAlgorithms.Mutators;
using Src.Model;
using Src.SingleRendezvousControlGeneration;
using Src.SingleRendezvousControlGeneration.DoubleManeuverApproach;
using UnityEngine;

namespace Src.ManualTests
{
	public class GaTest : MonoBehaviour
	{
		private void Start()
		{
			var spacecraftIo = new JsonIO<Spacecraft>
			{
				Converters = new JsonConverter[] { new VectorJsonConverter() },
			};

			var carrier =
				spacecraftIo.Load(
					"C:\\Users\\User\\AppData\\LocalLow\\DefaultCompany\\Interorbital Transfer 3D\\Rendezvous\\carrier.json");
			var targetSatellite =
				spacecraftIo.Load(
					"C:\\Users\\User\\AppData\\LocalLow\\DefaultCompany\\Interorbital Transfer 3D\\Rendezvous\\satellite.json");

			var polynomialsCount = 3;
			var polynomialsDegree = 5;
			var mu = 398600.4418;

			var bounds = new DynamicManeuverBounds(10, 1000, 
				0, 10000, -Math.PI, Math.PI, polynomialsDegree);
			var initialPopulationGenerator = new UniformPopulationGenerator(bounds);
			var evaluator = new ControlEvaluator(polynomialsDegree, mu, new Vector(0, 0, 0), 0.1, carrier,
				targetSatellite,
				0.5, 1, 0.1);
			var parentsPoolSelector = new TournamentSelector(5);
			var crossoverOperator = new SbxCrossoverOperator(0.9, 15);
			var geneMutator = new PolynomialGeneMutator(15, 0.1);
			var populationMutator = new DoubleManeuverMutator(geneMutator, bounds, polynomialsCount);
			var clamper = new DoubleManeuverGenomeClamper(bounds, polynomialsCount);
			var ga = new Ga(initialPopulationGenerator, evaluator, parentsPoolSelector, crossoverOperator,
				populationMutator, clamper);
			ga.DesirableFitness = 1;

			var result = ga.Evolve();
		}
	}
}