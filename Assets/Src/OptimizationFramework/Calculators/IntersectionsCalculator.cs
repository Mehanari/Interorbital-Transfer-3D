using MehaMath.Math.Components;
using Src.Helpers;

namespace Src.OptimizationFramework.Calculators
{
	public static class IntersectionsCalculator
	{
		public static double[] CalculateIntersections(KinematicData[] transfers, double mu, double centralBodyRadius)
		{
			var keplerianPropagation = new KeplerianPropagation()
			{
				GravitationalParameter = mu,
				CentralBodyPosition = new Vector(0, 0, 0)
			};

			var intersections = new double[transfers.Length];
			for (int i = 0; i < transfers.Length; i++)
			{
				var transfer = transfers[i];
				var transferOrbitStart = OrbitHelper.GetOrbit(transfer.TransferStartVelocity,
					transfer.TransferStartPosition, mu);
				var transferOrbitEnd =
					keplerianPropagation.PropagateState(transferOrbitStart, transfer.TransferTime);
				var startTrueAnomaly = transferOrbitStart.TrueAnomaly;
				var endTrueAnomaly = transferOrbitEnd.TrueAnomaly;
				var distance =
					CentralBodyDistanceCalculator.MinDistanceForSection(transferOrbitStart, startTrueAnomaly,
						endTrueAnomaly);
				//If distance to the central body center is bigger than central body radius, then there is no intersection.
				intersections[i] = distance > centralBodyRadius ? 0 : centralBodyRadius - distance;
			}

			return intersections;
		}
	}
}