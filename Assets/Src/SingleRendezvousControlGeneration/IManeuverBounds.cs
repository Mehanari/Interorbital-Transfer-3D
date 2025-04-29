namespace Src.SingleRendezvousControlGeneration
{
	/// <summary>
	/// Defines values ranges for maneuver characteristics.
	/// </summary>
	public interface IManeuverBounds
	{
		public int PolynomialsDegree { get; }
		public (double min, double max) DriftTimRange();
		public (double min, double max) BurnTimeRange();
		public (double[] min, double[] max) CoefficientsRanges(double burnTime);
	}
}