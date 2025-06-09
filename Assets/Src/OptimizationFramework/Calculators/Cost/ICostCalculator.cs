using MehaMath.Math.Components;
using Src.OptimizationFramework.DataModels;

namespace Src.OptimizationFramework.Calculators.Cost
{
	public interface ICostCalculator
	{
		public double CalculateCost(double[] driftTimes, double[] transferTimes, TargetParameters[] targets, Orbit shipInitialOrbit);
		public double CalculateCost((double[] driftTimes, double[] transferTimes) schedule, TargetParameters[] targets , Orbit shipInitialOrbit);
		public double CalculateCost(Vector scheduleVector, TargetParameters[] targets, Orbit shipInitialOrbit);
	}
}