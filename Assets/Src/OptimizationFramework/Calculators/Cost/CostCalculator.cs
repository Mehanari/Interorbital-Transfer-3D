using MehaMath.Math.Components;

namespace Src.OptimizationFramework.Calculators.Cost
{
	public abstract class CostCalculator
	{
		public abstract double CalculateCost(double[] driftTimes, double[] transferTimes, TargetParameters[] targets, Orbit shipInitialOrbit);
		public abstract double CalculateCost((double[] driftTimes, double[] transferTimes) schedule, TargetParameters[] targets , Orbit shipInitialOrbit);
		public abstract double CalculateCost(Vector scheduleVector, TargetParameters[] targets, Orbit shipInitialOrbit);
	}
}