using Newtonsoft.Json;

namespace Src.OptimizationFramework
{
	public struct TargetParameters
	{
		[JsonProperty("target_name")] 
		public string TargetName { get; set; }
		[JsonProperty("initial_orbit")]
		public Orbit InitialOrbit { get; set; }
		[JsonProperty("service_time")]
		public double ServiceTime { get; set; }
	}
}