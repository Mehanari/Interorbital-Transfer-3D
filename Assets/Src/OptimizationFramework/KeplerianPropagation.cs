using System;
using MehaMath.Math.Components;

namespace Src.OptimizationFramework
{
	public class KeplerianPropagation
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
		public Vector CentralBodyPosition { get; set; }
		public double GravitationalParameter { get; set; }

		public Orbit PropagateState(Orbit start, double time)
		{
			var mu = GravitationalParameter;
			var meanMotion = Math.Sqrt(mu/ Math.Pow(start.SemiMajorAxis, 3)); //Formula from Bate, Muller, and White "Fundamentals of astrodynamics", page 185.

			var cosE0 = (start.Eccentricity + Math.Cos(start.TrueAnomaly)) /
			            (1 + start.Eccentricity * Math.Cos(start.TrueAnomaly)); //E0 is initial eccentric anomaly. This and next formulae are from "Fundamentals of astrodynamics and applications" by David A. Vallado. Page 47 equations (2-9).
			var sinE0 = Math.Sqrt(1 - start.Eccentricity * start.Eccentricity) * Math.Sin(start.TrueAnomaly) /
			            (1 + start.Eccentricity * Math.Cos(start.TrueAnomaly));
			var E0 = Math.Atan2(sinE0, cosE0);
			var meanAnomaly0 = E0 - start.Eccentricity * Math.Sin(E0); //Formula from Bate, Muller, and White "Fundamentals of astrodynamics", page 185, equation (4.2-6)
			var newMeanAnomaly = meanAnomaly0 + meanMotion * time;
			var newEccentricAnomaly = SolveKeplerEquation(newMeanAnomaly, start.Eccentricity);
			
			var cosTrueAnomaly = (Math.Cos(newEccentricAnomaly) - start.Eccentricity) /
			                     (1 - start.Eccentricity * Math.Cos(newEccentricAnomaly)); //This and next formulae are from Bate, Muller, and White "Fundamentals of astrodynamics", page 406, equations (9.4-36) and (9.4-37).
			var sinTrueAnomaly = Math.Sqrt(1 - start.Eccentricity * start.Eccentricity) *
				Math.Sin(newEccentricAnomaly) / (1 - start.Eccentricity * Math.Cos(newEccentricAnomaly));
			var trueAnomaly = Math.Atan2(sinTrueAnomaly, cosTrueAnomaly);

			start.TrueAnomaly = trueAnomaly;
			return start;
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