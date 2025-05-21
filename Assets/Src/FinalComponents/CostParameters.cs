namespace Src.FinalComponents
{
	public struct CostParameters
	{
		public double TotalVelocityDelta { get; set; }
		public double TotalTime { get; set; }
		/// <summary>
		/// How much the orbit of the maneuver intersects with the central body.
		/// In the final solution this parameter must be zero.
		/// We need it to calculate penalties.
		/// </summary>
		public double CentralBodyIntersection { get; set; }
	}
}