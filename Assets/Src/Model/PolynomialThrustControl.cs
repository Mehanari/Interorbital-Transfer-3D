using System;
using MehaMath.Math.Components;

namespace Src.Model
{
	/// <summary>
	/// This class implements the idea of a spacecraft control that was introduced in "Mathematical modeling of spacecraft guidance and control system in 3D space orbit transfer mission" papper
	/// by Adolfazl Shirazi and A. H. Mazinan.
	/// It was modified so that not only the thrust direction is determined by a polynomial, but also the fuel consumption rate.
	/// </summary>
	public class PolynomialThrustControl
	{
		//Alpha and beta parameters control the direction of spacecraft's thrust
		public Polynomial AlphaPolynomial { get; set; }
		public Polynomial BetaPolynomial { get; set; }
		//Gamma polynomial controls the fuel consumption rate.
		public Polynomial GammaPolynomial { get; set; }

		public Vector ThrustDirection(double time)
		{
			var alpha = AlphaPolynomial.Compute(time);
			var beta = BetaPolynomial.Compute(time);
			var cosAlpha = Math.Cos(alpha);
			var cosBeta = Math.Cos(beta);
			var sinBeta = Math.Sin(beta);
			var sinAlpha = Math.Sin(alpha);
			return new Vector(cosAlpha * cosBeta, cosAlpha * sinBeta, sinAlpha);
		}

		//Returns how many percent of the spacecraft's maximum fuel consumption rate to use at a given moment in time.
		public double FuelConsumptionRatePercent(double time)
		{
			var gamma = GammaPolynomial.Compute(time);
			var percent = 1 + Math.Sin(gamma);
			return percent;
		}
	}
}