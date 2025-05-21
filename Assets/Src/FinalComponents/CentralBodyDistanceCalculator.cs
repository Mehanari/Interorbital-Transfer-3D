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
		/// </summary>
		/// <param name="orbit"></param>
		/// <param name="start">Must be in the interval [0, 2*Pi)</param>
		/// <param name="end">Must be in the interval [0, 2*Pi)</param>
		/// <param name="shortPath"></param>
		/// <param name="mu">Gravitational parameter</param>
		/// <returns></returns>
		public static double MinDistanceForSection(Orbit orbit, double start, double end, bool shortPath)
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
				throw new ArgumentException(
					"Start true anomaly of the section must be smaller than end. Given start: " + start +
					". Given end: " + end);
			}

			if (ContainsPeriapsis(start, end, shortPath))
			{
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

		private static bool ContainsPeriapsis(double start, double end, bool shortPath)
		{
			//We are working from the assumption that end >= start and that both start and end are in the interval [0, 2*Pi).
			if (start == 0 || end == 0)
			{
				return true;
			}
			var forwardDistance = end - start;
			var backwardDistance = Math.PI*2 - forwardDistance;
			if (forwardDistance < backwardDistance)
			{
				return !shortPath;
			}
			return shortPath;
		}
	}
}