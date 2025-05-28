using MehaMath.Math.Components;
using Newtonsoft.Json;

namespace Src.FinalComponents
{
	public struct KinematicData
	{
		[JsonProperty("drift_time")]
		public double DriftTime { get; set; }
		[JsonProperty("transfer_time")]
		public double TransferTime { get; set; }
		[JsonProperty("service_time")]
		public double ServiceTime { get; set; }
		[JsonProperty("drift_start_position")]
		public Vector DriftStartPosition { get; set; }
		[JsonProperty("drift_start_velocity")]
		public Vector DriftStartVelocity { get; set; }
		[JsonProperty("drift_end_velocity")]
		public Vector DriftEndVelocity { get; set; }
		/// <summary>
		/// Transfer start position is drift end position.
		/// </summary>
		[JsonProperty("transfer_start_position")]
		public Vector TransferStartPosition { get; set; }
		[JsonProperty("transfer_start_velocity")]
		public Vector TransferStartVelocity { get; set; }
		/// <summary>
		/// Transfer end position is service start position.
		/// </summary>
		[JsonProperty("transfer_end_position")]
		public Vector TransferEndPosition { get; set; }
		[JsonProperty("transfer_end_velocity")]
		public Vector TransferEndVelocity { get; set; }
		[JsonProperty("service_start_velocity")]
		public Vector ServiceStartVelocity { get; set; }
		[JsonProperty("service_end_position")]
		public Vector ServiceEndPosition { get; set; }
		[JsonProperty("service_end_velocity")]
		public Vector ServiceEndVelocity { get; set; }
	}
}