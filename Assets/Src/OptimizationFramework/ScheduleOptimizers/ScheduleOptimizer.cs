using Src.OptimizationFramework.Calculators;

namespace Src.OptimizationFramework.ScheduleOptimizers
{
	public abstract class ScheduleOptimizer
	{
		protected double MinTransferTime { get; set; } = 1d;
		
		public (double[] DriftTimes, double[] TransferTimes) InitialGuess { get; set; }
		
		public abstract (double[] driftTimes, double[] transferTimes) OptimizeSchedule(TargetParameters[] targets, Orbit spacecraftInitialOrbit, double spacecraftFinalMass);
		
		public CostCalculator CostCalculator { get; set; }
		public KinematicCalculator KinematicCalculator { get; set; }
	}
}