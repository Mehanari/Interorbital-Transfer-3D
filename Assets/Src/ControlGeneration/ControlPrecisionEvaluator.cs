using System;
using MehaMath.Math.Components;
using Src.Helpers;
using Src.Model;
using Src.SpacecraftDynamics;

namespace Src.ControlGeneration
{
	/// <summary>
	/// Applies a control, represented by ControlData object, to a spacecraft, represented by its initial state.
	/// Calculates the difference between the goal orbit and result orbit, reached after applying control. 
	/// </summary>
	public class ControlPrecisionEvaluator
	{
		private readonly Spacecraft _initialState;
		private readonly Orbit _goalOrbit;
		private readonly double _gravitationalParameter;
		private readonly double _timeStep;
		private readonly ISpacecraftDynamics _dynamics;
		private readonly OrbitWeightedCoefficients _coefficients;

		public ControlPrecisionEvaluator(Spacecraft initialState, Orbit goalOrbit, ISpacecraftDynamics dynamics, 
			double gravitationalParameter, double timeStep, OrbitWeightedCoefficients coefficients)
		{
			_initialState = initialState;
			_goalOrbit = goalOrbit;
			_dynamics = dynamics;
			_gravitationalParameter = gravitationalParameter;
			_timeStep = timeStep;
			_coefficients = coefficients;
		}

		public double EvaluateControl(ControlData control)
		{
			//Step 0: Prepare controller
			var controller = new PolynomialThrustControl(
				alphaPolynomial: new Polynomial(control.AlphaPolynomialCoefficients),
				betaPolynomial: new Polynomial(control.BetaPolynomialCoefficients),
				gammaPolynomial: new Polynomial(control.GammaPolynomialCoefficients));
			
			//Step 1: Move our spaceship to the engines ignition position
			var startOrbit =
				OrbitHelper.GetOrbit(_initialState.Velocity, _initialState.Position, _gravitationalParameter);
			startOrbit.TrueAnomaly = control.IgnitionTrueAnomaly;
			var ignitionState = _initialState.Clone();
			var (ignitionPosition, ignitionVelocity) =
				OrbitHelper.GetPositionAndVelocity(startOrbit, _gravitationalParameter);
			ignitionState.Position = ignitionPosition;
			ignitionState.Velocity = ignitionVelocity;

			//Step 2: Simulate movement with engines 
			var elapsedTime = 0d;
			var currentState = ignitionState;
			while (elapsedTime < control.BurnTime)
			{
				currentState.ExhaustDirection = controller.ThrustDirection(elapsedTime) * -1;
				currentState.FuelConsumptionRate =
					controller.FuelConsumptionRatePercent(elapsedTime) * currentState.MaxFuelConsumptionRate;
				var nextState = _dynamics.PropagateState(currentState, _timeStep);

				currentState = nextState;
				elapsedTime += _timeStep;
			}

			//Step 4: Evaluate deviation from the goal orbit
			var finalOrbit =
				OrbitHelper.GetOrbit(currentState.Velocity, currentState.Position, _gravitationalParameter);
			var semiMajorAxisDiff = finalOrbit.SemiMajorAxis - _goalOrbit.SemiMajorAxis;
			var eccentricityDiff = finalOrbit.Eccentricity - _goalOrbit.Eccentricity;
			var inclinationDiff = finalOrbit.Inclination - _goalOrbit.Inclination;
			var perigeeArgumentDiff = finalOrbit.PerigeeArgument - _goalOrbit.PerigeeArgument;
			var ascendingNodeLongitudeDiff = finalOrbit.AscendingNodeLongitude - _goalOrbit.AscendingNodeLongitude;
			return Math.Pow(semiMajorAxisDiff/_coefficients.SemiMajorAxisWeight, 2) + Math.Pow(eccentricityDiff/_coefficients.EccentricityWeight, 2) + Math.Pow(inclinationDiff/_coefficients.InclinationWeight, 2)
			       + Math.Pow(perigeeArgumentDiff/_coefficients.PerigeeArgumentWeight, 2) + Math.Pow(ascendingNodeLongitudeDiff/_coefficients.AscendingNodeLongitudeWeight, 2);
		}
		
	}
}