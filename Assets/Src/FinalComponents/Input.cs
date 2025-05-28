using Newtonsoft.Json;

namespace Src.FinalComponents
{
	public struct Input
	{
		[JsonProperty("targets")]
		public TargetParameters[] Targets { get; set; }
		[JsonProperty("central_body_radius")]
		public double CentralBodyRadius { get; set; }
		[JsonProperty("mu")]
		public double Mu { get; set; }
		[JsonProperty("isp")]
		public double Isp { get; set; }
		[JsonProperty("stand_grav")]
		//Standard gravity acceleration
		public double StandGrav { get; set; }
		[JsonProperty("ship_dry_mass")]
		public double ShipDryMass { get; set; }
		[JsonProperty("ship_initial_orbit")]
		public Orbit ShipInitialOrbit { get; set; }
	}
}