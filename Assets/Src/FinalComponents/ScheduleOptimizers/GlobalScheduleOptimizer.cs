using System;
using MehaMath.Math.Components;

namespace Src.FinalComponents.ScheduleOptimizers
{
	public class GlobalScheduleOptimizer : ScheduleOptimizer
	{
		/// <summary>
		/// Step size of the gradient descent.
		/// </summary>
		public double GdStepSize { get; set; } = 0.1d;

		/// <summary>
		/// Iterations limit for the gradient descent.
		/// </summary>
		public int GdIterationsLimit { get; set; } = 1000;
		
		/// <summary>
		/// Tolerance of the gradient descent search.
		/// Minimum difference between new guess cost and previous guess cost to stop iteration process.
		/// </summary>
		public double GdTolerance { get; set; } = 0.1d;
		

		public GlobalScheduleOptimizer(CostCalculator costCalculator, KinematicCalculator kinematicCalculator) : base(costCalculator, kinematicCalculator)
		{
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="driftTimesInit">Initial guess for drift times</param>
		/// <param name="transferTimesInit">Initial guess for transfer times</param>
		/// <param name="targets"></param>
		/// <param name="spacecraftInitialOrbit"></param>
		/// <param name="spacecraftFinalMass">How much the spacecraft must weight after servicing all targets</param>
		/// <returns></returns>
		public override (double[] driftTimes, double[] transferTimes) OptimizeSchedule(double[] driftTimesInit, double[] transferTimesInit, 
			TargetParameters[] targets, Orbit spacecraftInitialOrbit, double spacecraftFinalMass)
		{
			if (driftTimesInit.Length != transferTimesInit.Length)
			{
				throw new ArgumentException(
					"Drift times and initial times initial guess arrays must be the same length.");
			}
			if (driftTimesInit.Length != targets.Length)
			{
				throw new ArgumentException(
					"The length of initial guess arrays must be the same as the length of the targets array.");
			}

			var initialGuess = ToVector(driftTimesInit, transferTimesInit);
			var min = GradientDescent.Minimize(Objective, initialGuess, GdStepSize, GdTolerance, GdIterationsLimit, projection: Project);
			return FromVector(min);

			double Objective(Vector times)
			{
				var (driftTimes, transferTimes) = FromVector(times);
				//If some time component, either drift time or transfer time, is negative, then we add its absolute value to negative time penalty
				//and replace this component value with zero.
				//By doing so we guarantee that the variant with zero time value is always cheaper than the variant with negative time.
				//Negative time values are possible when numerically calculating gradient near zero.
				var negativeTimes = 0d;
				
				//Clamping time components to [0, infinity) and calculating negative times penalty
				for (int i = 0; i < driftTimes.Length; i++)
				{
					if (driftTimes[i] < 0)
					{
						negativeTimes += driftTimes[i];
						driftTimes[i] = 0d;
					}

					if (transferTimes[i] < 0)
					{
						negativeTimes += transferTimes[i];
						transferTimes[i] = 0d;
					}
				}

				var kinematics =
					KinematicCalculator.CalculateKinematics(driftTimes, transferTimes, targets,
						spacecraftInitialOrbit);
				var cost = CostCalculator.CalculateCost(kinematics, spacecraftFinalMass);
				return cost + negativeTimes * negativeTimes;
			}
		}
	}
}