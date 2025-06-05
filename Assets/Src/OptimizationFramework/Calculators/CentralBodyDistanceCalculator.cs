using System;
using Src.OptimizationFramework.DataModels;

namespace Src.OptimizationFramework.Calculators
{
	/// <summary>
	/// This class contains methods for calculating distances to the central body.
	/// </summary>
	public static class CentralBodyDistanceCalculator
	{
		/// <summary>
		/// Given start and end true anomaly values, and a 'shortPath' flag indicating the section,
		/// calculates the minimum distance connecting the central body center and the given orbit section.
		/// </summary>
		/// <param name="orbit"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="shortPath"></param>
		/// <param name="mu">Gravitational parameter</param>
		/// <returns></returns>
		public static double MinDistanceForSection(Orbit orbit, double start, double end)
		{
			var startDistance = DistanceToCentralBody(start, orbit);
			var endDistance = DistanceToCentralBody(end, orbit);
			var periapsisDistance = DistanceToCentralBody(0, orbit);
			//If the signs of start and end true anomalies are different, then boyd goes through 0 true anomaly, which means it goes through the periapsis
			if (start*end < 0)
			{
				return periapsisDistance;
			}

			var angularDistance = Math.Abs(start - end);
			//If the signs of both start and end true anomalies are the same, but
			//angular distance is bigger than the full circle, than the body goes through the periapsis
			if (angularDistance >= Math.PI*2)
			{
				return periapsisDistance;
			}
			
			return startDistance < endDistance ? startDistance : endDistance;
		}

		public static double DistanceToCentralBody(double trueAnomaly, Orbit orbit)
		{
			return (orbit.SemiMajorAxis * (1 - orbit.Eccentricity * orbit.Eccentricity)) /
			       (1 + orbit.Eccentricity * Math.Cos(trueAnomaly));
		}
	}
}