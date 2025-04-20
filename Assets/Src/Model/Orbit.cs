using System;

namespace Src.Model
{
	[Serializable]
	public struct Orbit
	{
		public double SemiMajorAxis { get; set; }
		public double Eccentricity { get; set; }
		public double Inclination { get; set; }
		public double PerigeeArgument { get; set; }
		public double AscendingNodeLongitude { get; set; }
		public double TrueAnomaly { get; set; }
	}
}