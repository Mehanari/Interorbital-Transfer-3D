namespace Src.Model
{
	public class ControlData
	{
		/// <summary>
		/// At which point on the initial orbit should engine start.
		/// </summary>
		public double IgnitionTrueAnomaly { get; set; }
		/// <summary>
		/// How much time the engine should work.
		/// </summary>
		public double BurnTime { get; set; }
		/// <summary>
		/// Determines the thrust direction dynamics along with beta polynomial.
		/// </summary>
		public double[] AlphaPolynomialCoefficients { get; set; }
		/// <summary>
		/// determines the thrust direction dynamics along with alpha polynomial.
		/// </summary>
		public double[] BetaPolynomialCoefficients { get; set; }
		/// <summary>
		/// Determines the thrust magnitude dynamics.
		/// </summary>
		public double[] GammaPolynomialCoefficients { get; set; }
	}
}