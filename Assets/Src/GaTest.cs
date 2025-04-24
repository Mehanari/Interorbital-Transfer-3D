using System;
using MehaMath.Math.Components;
using MehaMath.Math.Components.Json;
using Newtonsoft.Json;
using Src.ControlGeneration;
using Src.ControlGeneration.GeneticAlgorithms;
using Src.Model;
using Src.SpacecraftDynamics.CentralBodyDynamics;
using UnityEngine;

namespace Src
{
	public class GaTest : MonoBehaviour
	{
		private void Start()
		{
			var orbitIo = new JsonIO<Orbit>
			{
				FileName = "goalOrbit.json"
			};
			var spacecraftIo = new JsonIO<Spacecraft>
			{
				Converters = new JsonConverter[] { new VectorJsonConverter() },
				FileName = "spacecraft.json"
			};
			var goalOrbit = orbitIo.Load();
			var spacecraft = spacecraftIo.Load();

			var mu = 398600.4418;
			var bounds = new DynamicControlGenomeBounds(polynomialsDegree: 5, coefficientMin: - Math.PI, coefficientMax: Math.PI, 
				trueAnomalyMin: 0, trueAnomalyMax: Math.PI*2, burnMinTime: 10, burnMaxTime: 500);
			var dynamics = new Rkf45Dynamics()
			{
				CentralBodyPosition = new Vector(0d, 0d, 0d),
				GravitationalParameter = mu
			};
			var coefficients = new OrbitWeightedCoefficients(1, 1, 1, 1, 1);
			var parentsSelector = new TournamentSelector(3);
			var crossoverOperator = new SbxCrossoverOperator(0.9, 15);
			var mutator = new PolynomialControlMutator(15, 0.1, bounds);
			var controlEvaluator =
				new ControlPrecisionEvaluator(spacecraft, goalOrbit, dynamics, mu, 0.1, coefficients);
			var ga = new GaControlGenerator(0.01, 200, 100, 198, 1,
				bounds, controlEvaluator, parentsSelector, crossoverOperator, mutator)
			{
				InitialState = spacecraft,
				GoalOrbit = goalOrbit
			};

			var control = ga.GenerateControl();
		}
	}
}