using System;

namespace Src.SingleRendezvousControlGeneration.DoubleManeuverApproach
{
	public static class GenomeConverter
	{
		public static DoubleManeuverControl FromGenome(double[] genome, int polynomialsDegree)
		{
			//First two genes are drift time and burn time
			//Next 3 * (polynomialDegree - 1) genes are coefficients for polynomials
			//We multiply by 3 because we have 3 polynomials: alpha, beta and gamma
			var maneuverGenomeLength = 2 + 3 * (polynomialsDegree + 1);
			//We have 2 maneuvers
			if (genome.Length != 2*maneuverGenomeLength)
			{
				throw new ArgumentException("Genome length does not correspond to target data. Expected length: " +
				                            2 * maneuverGenomeLength + "" +
				                            ". Actual length: " + genome.Length);
			}

			var startManeuverGenome = new double[maneuverGenomeLength];
			Array.Copy(genome, startManeuverGenome, maneuverGenomeLength);
			var endManeuverGenome = new double[maneuverGenomeLength];
			Array.Copy(genome, maneuverGenomeLength, endManeuverGenome, 0, maneuverGenomeLength);
			var startManeuver = ManeuverFromGenome(startManeuverGenome, polynomialsDegree);
			var endManeuver = ManeuverFromGenome(endManeuverGenome, polynomialsDegree);
			return new DoubleManeuverControl()
			{
				StartManeuver = startManeuver,
				EndManeuver = endManeuver
			};
		}

		public static Maneuver ManeuverFromGenome(double[] maneuverGenome, int polynomialsDegree)
		{
			var driftTime = maneuverGenome[0];
			var burnTime = maneuverGenome[1];
			var polynomialCoefficientsCount = polynomialsDegree + 1;
			var alphaCoefs = new double[polynomialCoefficientsCount];
			var betaCoefs = new double[polynomialCoefficientsCount];
			var gammaCoefs = new double[polynomialCoefficientsCount];
			for (int i = 0; i < polynomialCoefficientsCount; i++)
			{
				alphaCoefs[i] = maneuverGenome[i + 2];
			}

			for (int i = 0; i < polynomialCoefficientsCount; i++)
			{
				betaCoefs[i] = maneuverGenome[i + 2 + polynomialCoefficientsCount];
			}

			for (int i = 0; i < polynomialCoefficientsCount; i++)
			{
				gammaCoefs[i] = maneuverGenome[i + 2 + polynomialCoefficientsCount * 2];
			}

			return new Maneuver()
			{
				DriftTime = driftTime,
				BurnTime = burnTime,
				AlphaPolynomialCoefficients = alphaCoefs,
				BetaPolynomialCoefficients = betaCoefs,
				GammaPolynomialCoefficients = gammaCoefs
			};
		}
	}
}