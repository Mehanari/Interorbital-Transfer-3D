using System;
using System.Linq;
using MehaMath.Math.Components;
using Src.OptimizationFramework.Calculators.Fuel;

namespace Src.OptimizationFramework.Calculators.Cost
{
	public class WeightedCostCalculator : CostCalculator
	{
		public FuelCalculator FuelCalculator { get; set; }
		public KinematicCalculator KinematicCalculator { get; set; }
		public IntersectionsCalculator IntersectionsCalculator { get; set; }
		public double CrushPenaltyLambda { get; set; } = 100000;
		public double CrushPenaltyPower { get; set; } = 2;
		public double FuelCost { get; set; }
		public double TimeCost { get; set; }
		

		public override double CalculateCost(double[] driftTimes, double[] transferTimes, TargetParameters[] targets, Orbit shipInitialOrbit)
		{
			var transfersKinematics =
				KinematicCalculator.CalculateKinematics(driftTimes, transferTimes, targets, shipInitialOrbit);
			var fuel = FuelCalculator.CalculateFuelMasses(transfersKinematics);
			var intersections =
				IntersectionsCalculator.CalculateIntersections(transfersKinematics);

			var totalTime = transfersKinematics.Sum(k => k.ServiceTime + k.TransferTime + k.DriftTime);
			var totalFuel = fuel.Sum();
			var totalIntersection = intersections.Sum();
			

			return totalFuel * FuelCost + totalTime * TimeCost +
			       Math.Pow(totalIntersection, CrushPenaltyPower) * CrushPenaltyLambda;
		}

		public override double CalculateCost((double[] driftTimes, double[] transferTimes) schedule, TargetParameters[] targets, Orbit shipInitialOrbit)
		{
			return CalculateCost(schedule.driftTimes, schedule.transferTimes, targets, shipInitialOrbit);
		}

		public override double CalculateCost(Vector scheduleVector, TargetParameters[] targets, Orbit shipInitialOrbit)
		{
			var schedule = ScheduleVectorUtils.FromVector(scheduleVector);
			return CalculateCost(schedule, targets, shipInitialOrbit);
		}
	}
}