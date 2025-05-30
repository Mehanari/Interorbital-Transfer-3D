using System;
using MehaMath.Math.Components;

namespace Src.OptimizationFramework
{
	public static class ScheduleVectorUtils
	{
		public static Vector ToVector(double[] driftTimes, double[] transferTimes)
		{
			if (driftTimes.Length != transferTimes.Length)
			{
				throw new ArgumentException("Drift times array must be the same length as transfer times array.");
			}
			var vector = new Vector(driftTimes.Length + transferTimes.Length);
			for (int i = 0, k = 0; i < driftTimes.Length + transferTimes.Length; i+=2, k++)
			{
				vector[i] = driftTimes[k];
				vector[i + 1] = transferTimes[k];
			}

			return vector;
		}

		public static (double[] driftTimes, double[] transferTimes) FromVector(Vector times)
		{
			if (times.Length%2 != 0)
			{
				throw new ArgumentException("Times vector length must be divisible by 2");
			}
			var driftTimes = new double[times.Length / 2];
			var transferTimes = new double[times.Length / 2];
			for (int i = 0, k = 0; i < times.Length; i+=2, k++)
			{
				driftTimes[k] = times[i];
				transferTimes[k] = times[i + 1];
			}

			return (driftTimes, transferTimes);
		}
	}
}