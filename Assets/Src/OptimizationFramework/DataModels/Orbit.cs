using System;
using Newtonsoft.Json;

namespace Src.OptimizationFramework.DataModels
{
	[Serializable]
	public struct Orbit
	{
		[JsonProperty("semi_major_axis")]
		public double SemiMajorAxis { get; set; }
		[JsonProperty("eccentricity")]
		public double Eccentricity { get; set; }
		[JsonProperty("inclination")]
		public double Inclination { get; set; }
		[JsonProperty("perigee_argument")]
		public double PerigeeArgument { get; set; }
		[JsonProperty("ascending_node_longitude")]
		public double AscendingNodeLongitude { get; set; }
		[JsonProperty("true_anomaly")]
		public double TrueAnomaly { get; set; }
	}
}