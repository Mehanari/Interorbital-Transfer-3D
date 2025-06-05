using Newtonsoft.Json;

namespace Src.OptimizationFramework.DataModels
{
	public struct MissionParameters
	{
		[JsonProperty("targets")]
		public TargetParameters[] Targets { get; set; }
		[JsonProperty("central_body_radius")]
		public double CentralBodyRadius { get; set; }
		[JsonProperty("mu")]
		public double Mu { get; set; }
		[JsonProperty("isp")]
		public double Isp { get; set; }
		/// <summary>
		/// Cost per fuel unit.
		/// </summary>
		public double FuelCost { get; set; }
		/// <summary>
		/// Cost per time unit.
		/// </summary>
		public double TimeCost { get; set; }
		/// <summary>
		/// Standard gravity acceleration.
		/// </summary>
		[JsonProperty("stand_grav")]
		public double StandGrav { get; set; }
		[JsonProperty("ship_dry_mass")]
		public double ShipFinalMass { get; set; }
		[JsonProperty("ship_initial_orbit")]
		public Orbit ShipInitialOrbit { get; set; }
	}
}