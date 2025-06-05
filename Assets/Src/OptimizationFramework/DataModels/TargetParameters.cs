using Newtonsoft.Json;

namespace Src.OptimizationFramework.DataModels
{
	public struct TargetParameters
	{
		[JsonProperty("target_name")] 
		public string TargetName { get; set; }
		[JsonProperty("orbit")]
		public Orbit Orbit { get; set; }
		[JsonProperty("service_time")]
		public double ServiceTime { get; set; }
	}
}