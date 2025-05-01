using System;
using MehaMath.Math.Components;
using Src.GeneticAlgorithms;
using Src.Model;
using Src.SpacecraftDynamics.CentralBodyDynamics;

namespace Src.SingleRendezvousControlGeneration.DoubleManeuverApproach
{
	public class ControlEvaluator : IGenomeEvaluator
	{
		public double MinDistance { get; set; }
		public double MaxDistance { get; set; }
		public double MaxVelocityDiff { get; set; }
		
		private readonly Spacecraft _spacecraftInitialState;
		private readonly Spacecraft _satelliteInitialState;
		private readonly int _polynomialsDegree;
		private readonly KeplerianDynamics _keplerianDynamics;
		private readonly Rkf45Dynamics _rkf45Dynamics;
		private readonly double _timeStep;

		public ControlEvaluator(int polynomialsDegree, double gravitationalParameter,
			Vector centralBodyPosition, double timeStep,
			Spacecraft spacecraftInitialState, Spacecraft satelliteInitialState, 
			double minDistance, double maxDistance, double maxVelocityDiff)
		{
			_polynomialsDegree = polynomialsDegree;
			_timeStep = timeStep;
			_keplerianDynamics = new KeplerianDynamics()
			{
				GravitationalParameter = gravitationalParameter,
				CentralBodyPosition = centralBodyPosition
			};
			_rkf45Dynamics = new Rkf45Dynamics()
			{
				GravitationalParameter = gravitationalParameter,
				CentralBodyPosition = centralBodyPosition
			};
			_spacecraftInitialState = spacecraftInitialState;
			_satelliteInitialState = satelliteInitialState;
			MinDistance = minDistance;
			MaxDistance = maxDistance;
			MaxVelocityDiff = maxVelocityDiff;
		}

		/// <summary>
		/// Returns 1 if, in the end of the maneuvers, distance between satellite and a spacecraft is exactly MinDistance or MaxDistance and velocity difference is exactly MaxVelocity difference
		/// Returns values less than 1 if distance is between MinDistance and MaxDistance and velocity difference is less than MaxVelocityDiff
		/// Returns values bigger than 1 otherwise.
		/// </summary>
		/// <param name="genome"></param>
		/// <returns></returns>
		public double Evaluate(double[] genome)
		{
			var control = GenomeConverter.FromGenome(genome, _polynomialsDegree);
			var satelliteState = _satelliteInitialState;
			var spacecraftState = _spacecraftInitialState;

			//Step 1: Skip time for initial drift, e.g. move the spacecraft and satellite for initial drift time
			var initialDriftTime = control.StartManeuver.DriftTime;
			satelliteState = _keplerianDynamics.PropagateState(satelliteState, initialDriftTime);
			spacecraftState = _keplerianDynamics.PropagateState(spacecraftState, initialDriftTime);

			//Step 2: Apply the first maneuver
			(spacecraftState, satelliteState) = ApplyManeuver(spacecraftState, satelliteState, control.StartManeuver);
			

			//Step 3: Skip time for drift before the second maneuver
			var secondDriftTime = control.EndManeuver.DriftTime;
			satelliteState = _keplerianDynamics.PropagateState(satelliteState, secondDriftTime);
			spacecraftState = _keplerianDynamics.PropagateState(spacecraftState, secondDriftTime);

			//Step 4: Apply the second maneuver
			(spacecraftState, satelliteState) = ApplyManeuver(spacecraftState, satelliteState, control.EndManeuver);

			//TODO: Modify so that goal is to get in range around satellite, not exactly to the satellite
			var velocityDiff = (spacecraftState.Velocity - satelliteState.Velocity).Magnitude();
			var positionDiff = (spacecraftState.Position - satelliteState.Position).Magnitude();

			var distanceComponent = SplineCriterion(positionDiff, MinDistance, MaxDistance);
			var velocityComponent = SplineCriterion(velocityDiff, 0, MaxVelocityDiff);
			
			return distanceComponent/2 + velocityComponent/2;
		}

		private (Spacecraft spacecraftState, Spacecraft satelliteState) ApplyManeuver(Spacecraft spacecraftInitialState,
			Spacecraft satelliteInitialState, Maneuver maneuver)
		{
			var maneuverController = new PolynomialThrustControl(
				alphaPolynomial: new Polynomial(maneuver.AlphaPolynomialCoefficients),
				betaPolynomial: new Polynomial(maneuver.BetaPolynomialCoefficients),
				gammaPolynomial: new Polynomial(maneuver.GammaPolynomialCoefficients));
			var elapsedTime = 0d;
			var spacecraft = spacecraftInitialState;
			var satellite = satelliteInitialState;
			//Moving spacecraft
			while (elapsedTime < maneuver.BurnTime)
			{
				spacecraft.ExhaustDirection = maneuverController.ThrustDirection(elapsedTime) * -1;
				spacecraft.FuelConsumptionRate =
					maneuverController.FuelConsumptionRatePercent(elapsedTime) *
					spacecraft.MaxFuelConsumptionRate;
				spacecraft = _rkf45Dynamics.PropagateState(spacecraft, _timeStep);
				elapsedTime += _timeStep;
			}

			spacecraft.FuelConsumptionRate = 0; //Don't forget to turn off the engine.
			//Moving satellite. We use elapsed time to ensure states synchronization
			//If we use BurnTime instead, we may end up with satellite state slightly from the future, or from the past, relatively to the spacecraft state.
			//As a result we have states for spacecraft and a satellite for the same moment in time
			satellite = _keplerianDynamics.PropagateState(satellite, elapsedTime);
			return (spacecraft, satellite);
		}
		
		/// <summary>
		/// Returns 1 if value is exactly min or max,
		/// returns values from 0 to 1 if value is between min and max
		/// returns values bigger than 1 if value is outside [min, max] region.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		private double SplineCriterion(double value, double min, double max)
		{
			var mid = (max + min) / 2;
			var normalized = (value - mid) / (max - mid);
			if (value < min)
			{
				return Math.Pow(normalized, 4);
			}

			if (value > max)
			{
				return Math.Pow(normalized, 4);
			}

			return Math.Pow(normalized, 2);
		}
	}
}