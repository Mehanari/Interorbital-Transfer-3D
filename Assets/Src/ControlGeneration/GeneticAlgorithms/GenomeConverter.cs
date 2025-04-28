using Src.Model;

namespace Src.ControlGeneration.GeneticAlgorithms
{
	public static class GenomeConverter
	{
		public static ControlData FromGenome(double[] genome, int polynomialsDegree)
		{
			var trueAnomaly = genome[0];
			var burnTime = genome[1];
			var polynomialCoefficientsCount = polynomialsDegree + 1;
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