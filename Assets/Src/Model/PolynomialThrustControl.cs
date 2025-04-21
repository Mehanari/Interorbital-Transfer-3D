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
		private readonly Polynomial _alphaPolynomial;

		private readonly Polynomial _betaPolynomial;
		//Gamma polynomial controls the fuel consumption rate.
		private readonly Polynomial _gammaPolynomial;

		public PolynomialThrustControl(Polynomial alphaPolynomial, Polynomial betaPolynomial, Polynomial gammaPolynomial)
		{
			_alphaPolynomial = alphaPolynomial;
			_betaPolynomial = betaPolynomial;
			_gammaPolynomial = gammaPolynomial;
		}

		public Vector ThrustDirection(double time)
		{
			var alpha = _alphaPolynomial.Compute(time);
			var beta = _betaPolynomial.Compute(time);
			var cosAlpha = Math.Cos(alpha);
			var cosBeta = Math.Cos(beta);
			var sinBeta = Math.Sin(beta);
			var sinAlpha = Math.Sin(alpha);
			return new Vector(cosAlpha * cosBeta, cosAlpha * sinBeta, sinAlpha);
		}

		//Returns how many percent of the spacecraft's maximum fuel consumption rate to use at a given moment in time.
		public double FuelConsumptionRatePercent(double time)
		{
			var gamma = _gammaPolynomial.Compute(time);
			var percent = 1 + Math.Sin(gamma);
			return percent;
		}
	}
}