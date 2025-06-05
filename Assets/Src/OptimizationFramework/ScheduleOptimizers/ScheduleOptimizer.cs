using Src.OptimizationFramework.Calculators.Cost;
using Src.OptimizationFramework.DataModels;

namespace Src.OptimizationFramework.ScheduleOptimizers
{
	public abstract class ScheduleOptimizer
	{
		protected double MinTransferTime { get; set; } = 1d;
		
		public abstract (double[] driftTimes, double[] transferTimes) OptimizeSchedule(TargetParameters[] targets, Orbit spacecraftInitialOrbit);
		
		public CostCalculator CostCalculator { get; set; }
	}
}