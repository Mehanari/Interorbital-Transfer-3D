using Src.OptimizationFramework.Calculators.Cost;
using Src.OptimizationFramework.DataModels;

namespace Src.OptimizationFramework.ScheduleOptimizers
{
	public abstract class ScheduleOptimizer
	{
		public abstract (double[] driftTimes, double[] transferTimes) OptimizeSchedule(TargetParameters[] targets, Orbit spacecraftInitialOrbit);
		
		public ICostCalculator CostCalculator { get; set; }
	}
}