using System;
using Newtonsoft.Json;

namespace Src.SingleRendezvousControlGeneration.DoubleManeuverApproach
{
	[Serializable]
	public class DoubleManeuverControl
	{
		/// <summary>
		/// Maneuver to get to the transfer orbit.
		/// </summary>
		[JsonProperty("startManeuver")]
		public Maneuver StartManeuver { get; set; }
		/// <summary>
		/// Maneuver to get from transfer orbit to the rendezvous target.
		/// </summary>
		[JsonProperty("endManeuver")]
		public Maneuver EndManeuver { get; set; }
	}
}