using MehaMath.Math.Components;
using Src.Helpers;

namespace Src.OptimizationFramework.Calculators
{
	public class IntersectionsCalculator
	{
		public double Mu { get; set; }
		public double CentralBodyRadius { get; set; }
		
		public double[] CalculateIntersections(KinematicData[] transfers)
		{
			var keplerianPropagation = new KeplerianPropagation()
			{
				GravitationalParameter = Mu,
				CentralBodyPosition = new Vector(0, 0, 0)
			};

			var intersections = new double[transfers.Length];
			for (int i = 0; i < transfers.Length; i++)
			{
				var transfer = transfers[i];
				var transferOrbitStart = OrbitHelper.GetOrbit(transfer.TransferStartVelocity,
					transfer.TransferStartPosition, Mu);
				var transferOrbitEnd =
					keplerianPropagation.PropagateState(transferOrbitStart, transfer.TransferTime);
				var startTrueAnomaly = transferOrbitStart.TrueAnomaly;
				var endTrueAnomaly = transferOrbitEnd.TrueAnomaly;
				var distance =
					CentralBodyDistanceCalculator.MinDistanceForSection(transferOrbitStart, startTrueAnomaly,
						endTrueAnomaly);
				//If distance to the central body center is bigger than central body radius, then there is no intersection.
				intersections[i] = distance > CentralBodyRadius ? 0 : CentralBodyRadius - distance;
			}

			return intersections;
		}
	}
}