using Newtonsoft.Json;

namespace Src.OptimizationFramework
{
	/// <summary>
	/// The purpose of this struct is provide all data necessary to examine a single target servicing process, regardless of other targets. 
	/// </summary>
	public struct TargetServicing
	{
		[JsonProperty("fuel_mass")]
		public double FuelMass { get; set; }
		[JsonProperty("kinematics")]
		public KinematicData Kinematics { get; set; }
	}
}