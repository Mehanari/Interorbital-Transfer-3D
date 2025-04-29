namespace Src.OrbitTransferControlGeneration
{
	public class OrbitWeightedCoefficients
	{
		public double SemiMajorAxisWeight { get; }

		public double EccentricityWeight { get; }

		public double InclinationWeight { get; }

		public double PerigeeArgumentWeight { get; }

		public double AscendingNodeLongitudeWeight { get; }

		public OrbitWeightedCoefficients(double semiMajorAxisWeight, double eccentricityWeight, double inclinationWeight, double perigeeArgumentWeight, double ascendingNodeLongitudeWeight)
		{
			SemiMajorAxisWeight = semiMajorAxisWeight;
			EccentricityWeight = eccentricityWeight;
			InclinationWeight = inclinationWeight;
			PerigeeArgumentWeight = perigeeArgumentWeight;
			AscendingNodeLongitudeWeight = ascendingNodeLongitudeWeight;
		}
	}
}