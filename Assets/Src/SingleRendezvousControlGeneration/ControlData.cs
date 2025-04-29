using System;
using Newtonsoft.Json;

namespace Src.SingleRendezvousControlGeneration
{
	[Serializable]
	public class ControlData
	{
		/// <summary>
		/// Maneuver to get to the transfer orbit.
		/// </summary>
		[JsonProperty("startManeuver")]
		public ManeuverData StartManeuver { get; set; }
		/// <summary>
		/// Maneuver to get from transfer orbit to the rendezvous target.
		/// </summary>
		[JsonProperty("endManeuver")]
		public ManeuverData EndManeuver { get; set; }
	}
}