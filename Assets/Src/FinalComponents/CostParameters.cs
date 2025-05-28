namespace Src.FinalComponents
{
	public struct CostParameters
	{
		public double TotalVelocityDelta { get; set; }
		public double TotalTime { get; set; }
		/// <summary>
		/// How much the orbit of the maneuver intersects with the central body.
		/// Ideally it should be zero.
		/// We need it to calculate penalties.
		/// </summary>
		public double CentralBodyIntersection { get; set; }

		public static CostParameters operator +(CostParameters a, CostParameters b)
		{
			return new CostParameters()
			{
				CentralBodyIntersection = a.CentralBodyIntersection + b.CentralBodyIntersection,
				TotalTime = a.TotalTime + b.TotalTime,
				TotalVelocityDelta = a.TotalVelocityDelta + b.TotalVelocityDelta
			};
		}
	}
}