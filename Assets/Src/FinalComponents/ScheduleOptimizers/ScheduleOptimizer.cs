using System;
using MehaMath.Math.Components;

namespace Src.FinalComponents.ScheduleOptimizers
{
	public abstract class ScheduleOptimizer
	{
		public double MinTransferTime { get; set; } = 1d;
		
		public abstract (double[] driftTimes, double[] transferTimes) OptimizeSchedule(double[] driftTimesInit,
			double[] transferTimesInit,
			TargetParameters[] targets, Orbit spacecraftInitialOrbit, double spacecraftFinalMass);
		
		protected CostCalculator CostCalculator;
		protected KinematicCalculator KinematicCalculator;
		
		protected ScheduleOptimizer(CostCalculator costCalculator, KinematicCalculator kinematicCalculator)
		{
			CostCalculator = costCalculator;
			KinematicCalculator = kinematicCalculator;
		}
		
		/// <summary>
		/// Clamps drift times to [0, infinity) and transfer times to [MinTransferTime, infinity)
		/// </summary>
		/// <param name="times"></param>
		/// <returns></returns>
		protected Vector Project(Vector times)
		{
			if (times.Length%2 != 0)
			{
				throw new ArgumentException("Times vector length must be divisible by 2");
			}
			
			var projected = new Vector(times); //Copying the input vector
			for (int i = 0; i < projected.Length; i+= 2)
			{
				if (times[i] < 0)
				{
					projected[i] = 0d;
				}

				if (times[i+1] < MinTransferTime)
				{
					projected[i + 1] = MinTransferTime;
				}
			}

			return projected;
		}

		protected static Vector ToVector(double[] driftTimes, double[] transferTimes)
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

		protected static (double[] driftTimes, double[] transferTimes) FromVector(Vector times)
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