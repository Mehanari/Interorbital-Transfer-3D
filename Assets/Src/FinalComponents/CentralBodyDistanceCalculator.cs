using System;

namespace Src.FinalComponents
{
	/// <summary>
	/// This class contains methods for calculating distances to the central body.
	/// </summary>
	public class CentralBodyDistanceCalculator
	{
		/// <summary>
		/// Given start and end true anomaly values, and a 'shortPath' flag indicating the section,
		/// calculates the minimum distance connecting the central body center and the given orbit section.
		/// It is assumed that the motion of an object corresponds to the increase of true anomaly, hence
		/// the chosen segment of an ellipse will correspond to the trajectory created by gradually increasing start true anomaly
		/// up until it becomes equal to end true anomaly.
		/// </summary>
		/// <param name="orbit"></param>
		/// <param name="start">Must be in the interval [0, 2*Pi)</param>
		/// <param name="end">Must be in the interval [0, 2*Pi)</param>
		/// <param name="shortPath"></param>
		/// <param name="mu">Gravitational parameter</param>
		/// <returns></returns>
		public static double MinDistanceForSection(Orbit orbit, double start, double end)
		{
			if (start < 0 || start >= Math.PI*2)
			{
				throw new ArgumentException("Start true anomaly must be in range [0, 2*Pi). Given value: " + start);
			}
			if (end < 0 || end >= Math.PI*2)
			{
				throw new ArgumentException("End true anomaly must be in range [0, 2*Pi). Given value: " + end);
			}
			

			if (start > end)
			{
				//Is start > end, then we go through 0, which is the periapsis - the closest point to the central body.
				return DistanceToCentralBody(0, orbit);
			}
			var startDistance = DistanceToCentralBody(start, orbit);
			var endDistance = DistanceToCentralBody(end, orbit);
			return startDistance < endDistance ? startDistance : endDistance;
		}

		public static double DistanceToCentralBody(double trueAnomaly, Orbit orbit)
		{
			return (orbit.SemiMajorAxis * (1 - orbit.Eccentricity * orbit.Eccentricity)) /
			       (1 + orbit.Eccentricity * Math.Cos(trueAnomaly));
		}
	}
}