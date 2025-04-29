using System;
using Newtonsoft.Json;

namespace Src.OrbitTransferControlGeneration
{
	[Serializable]
	public class ControlData
	{
		/// <summary>
		/// At which point on the initial orbit should engine start.
		/// </summary>
		[JsonProperty("ignitionTrueAnomaly")]
		public double IgnitionTrueAnomaly { get; set; }
		/// <summary>
		/// How much time the engine should work.
		/// </summary>
		[JsonProperty("burnTime")]
		public double BurnTime { get; set; }
		/// <summary>
		/// Determines the thrust direction dynamics along with beta polynomial.
		/// </summary>
		[JsonProperty("alphaPolynomialCoefficients")]
		public double[] AlphaPolynomialCoefficients { get; set; }
		/// <summary>
		/// determines the thrust direction dynamics along with alpha polynomial.
		/// </summary>
		[JsonProperty("betaPolynomialCoefficients")]
		public double[] BetaPolynomialCoefficients { get; set; }
		/// <summary>
		/// Determines the thrust magnitude dynamics.
		/// </summary>
		[JsonProperty("gammaPolynomialCoefficients")]
		public double[] GammaPolynomialCoefficients { get; set; }
	}
}