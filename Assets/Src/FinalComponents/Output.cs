using Newtonsoft.Json;

namespace Src.FinalComponents
{
	public struct Output
	{
		[JsonProperty("service_order")]
		public TargetParameters[] ServiceOrder { get; set; }
		[JsonProperty("service_data")]
		public TargetServicing[] ServiceData { get; set; }
		[JsonProperty("total_cost")]
		public double TotalCost { get; set; }
		[JsonProperty("total_time")]
		public double TotalTime { get; set; }
		[JsonProperty("total_fuel")]
		public double TotalFuel { get; set; }
		[JsonProperty("crushes")]
		public int Crushes { get; set; }
	}
}