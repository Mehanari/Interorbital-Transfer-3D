using System;
using Src.Helpers;
using Src.Model;

namespace Src.SpacecraftDynamics.CentralBodyDynamics
{
	/// <summary>
	/// This dynamics calculation script uses Keplerian propagation to calculate next spacecraft state.
	/// This type of propagation has a high precision, but works only when the spacecraft`s engine is off, e.g. there is no thrust.
	/// Also, it doesn't work well with certain types of orbits (Parabolic, Rectilinear, Hyperbolic), so use CanUseKeplerianPropagation method to check if this dynamics is applicable.
	/// </summary>
	public class KeplerianDynamics : CentralBodyDynamics
	{
		/// <summary>
		/// This tolerance is used for numerical solution of the Kepler's equation.
		/// </summary>
		public double Tolerance { get; set; } = 1e-10;
		/// <summary>
		/// Iterations limit for Kepler's equation numerical solution.
		/// It usually should not take more than 20 iteration though.
		/// </summary>
		public int IterationsLimit { get; set; } = 1000;
		
		/// <summary>
		/// Checks if spacecraft's orbit is neither parabolic, nor rectilinear, nor hyperbolic. Also checks if engines are off or there is no fuel left.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <returns></returns>
		public bool CanUseKeplerianDynamics(Spacecraft spacecraft)
		{
			//Checking for unsupported orbits
			if (OrbitHelper.IsParabolic(spacecraft, GravitationalParameter) || 
			    OrbitHelper.IsRectilinear(spacecraft) ||
			    OrbitHelper.IsHyperbolic(spacecraft, GravitationalParameter))
			{
				return false;
			}
			
			//Checking if spacecraft's engines are off.
			if (spacecraft.FuelConsumptionRate > 0 && spacecraft.FuelMass > 0)
			{
				return false;
			}

			return true;
		}
		
		//TODO: Currently this implementation sucks. I'll use Rkf45 instead.
		public override Spacecraft PropagateState(Spacecraft spacecraft, double deltaT)
		{
			if (!CanUseKeplerianDynamics(spacecraft))
			{
				throw new InvalidOperationException(
					"Cannot apply keplerian propagation for given spacecraft. CanUseKeplerianDynamics returns false.");
			}

			var mu = GravitationalParameter;
			var orbit = OrbitHelper.GetOrbit(spacecraft.Velocity, spacecraft.Position, mu);
			var meanMotion = Math.Sqrt(mu / Math.Pow(orbit.SemiMajorAxis, 3)); //Formula from Bate, Muller, and White "Fundamentals of astrodynamics", page 185.

			
			var cosE0 = (orbit.Eccentricity + Math.Cos(orbit.TrueAnomaly)) /
			            (1 + orbit.Eccentricity * Math.Cos(orbit.TrueAnomaly)); //E0 is initial eccentric anomaly. This and next formulae are from "Fundamentals of astrodynamics and applications" by David A. Vallado. Page 47 equations (2-9).
			var sinE0 = Math.Sqrt(1 - orbit.Eccentricity * orbit.Eccentricity) * Math.Sin(orbit.TrueAnomaly) /
			            (1 + orbit.Eccentricity * Math.Cos(orbit.TrueAnomaly));
			var E0 = Math.Atan2(sinE0, cosE0);
			var meanAnomaly0 = E0 - orbit.Eccentricity * Math.Sin(E0); //Formula from Bate, Muller, and White "Fundamentals of astrodynamics", page 185, equation (4.2-6)
			var newMeanAnomaly = meanAnomaly0 + meanMotion * deltaT;
			var newEccentricAnomaly = SolveKeplerEquation(newMeanAnomaly, orbit.Eccentricity);
			
			
			var cosTrueAnomaly = (Math.Cos(newEccentricAnomaly) - orbit.Eccentricity) /
			                     (1 - orbit.Eccentricity * Math.Cos(newEccentricAnomaly)); //This and next formulae are from Bate, Muller, and White "Fundamentals of astrodynamics", page 406, equations (9.4-36) and (9.4-37).
			var sinTrueAnomaly = Math.Sqrt(1 - orbit.Eccentricity * orbit.Eccentricity) *
				Math.Sin(newEccentricAnomaly) / (1 - orbit.Eccentricity * Math.Cos(newEccentricAnomaly));
			var trueAnomaly = Math.Atan2(sinTrueAnomaly, cosTrueAnomaly);

			
			orbit.TrueAnomaly = trueAnomaly;
			var (newPosition, newVelocity) = OrbitHelper.GetPositionAndVelocity(orbit, mu);
			var newState = spacecraft.Clone();
			newState.Position = newPosition;
			newState.Velocity = newVelocity;
			return newState;
		}

		/// <summary>
		/// Solves M = E - e * sin(E) and return E.
		/// Uses Newton's numerical method.
		/// </summary>
		/// <param name="meanAnomaly"></param>
		/// <param name="eccentricity"></param>
		/// <returns></returns>
		private double SolveKeplerEquation(double meanAnomaly, double eccentricity)
		{
			var guess = meanAnomaly + eccentricity * Math.Sin(meanAnomaly);
			var previousGuess = double.MaxValue;
			var iteration = 0;
			while (Math.Abs(guess - previousGuess) > Tolerance && iteration < IterationsLimit)
			{
				iteration++;
				previousGuess = guess;
				guess = previousGuess + (meanAnomaly - (previousGuess - eccentricity * Math.Sin(previousGuess))) /
					(1 - eccentricity * Math.Cos(previousGuess));
			}

			return guess;
		}
	}
}