namespace Src.OrbitTransferControlGeneration.GaComponents
{
	public interface IControlGenomeBounds
	{
		public int PolynomialsDegree { get; }
		public (double min, double max) TrueAnomalyRange();
		public (double min, double max) BurnTimeRange();
		public (double[] min, double[] max) CoefficientsRanges(double burnTime);
	}
}