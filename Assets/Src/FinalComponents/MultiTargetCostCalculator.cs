using System;
using System.Collections.Generic;
using MehaMath.Math.Components;

namespace Src.FinalComponents
{
	public class MultiTargetCostCalculator
	{
		public List<TargetParameters> Targets { get; set; } = new();
		public Orbit ShipStartOrbit { get; set; }
		public double Mu { get; set; } = 398600.4418d;
		public double CentralBodyRadius { get; set; }
		
		public CostParameters CalculateCost(double[] driftTimes, double[] transferTimes)
		{
			if (driftTimes.Length != Targets.Count || transferTimes.Length != Targets.Count)
			{
				throw new ArgumentException(
					"Drift times array length and transfer times array length must be equal to targets list length.");
			}
			var singleTargetCostCalculator = new SingleTargetCostCalculator()
			{
				Mu = this.Mu,
				CentralBodyRadius = this.CentralBodyRadius
			};
			var keplerianPropagation = new KeplerianPropagation()
			{
				GravitationalParameter = Mu,
				CentralBodyPosition = new Vector(0, 0, 0)
			};
			var elapsedTime = 0d;
			var currentShipOrbit = ShipStartOrbit;
			var totalCost = new CostParameters();
			for (int i = 0; i < Targets.Count; i++)
			{
				var target = Targets[i];
				var driftTime = driftTimes[i];
				var transferTime = transferTimes[i];
				singleTargetCostCalculator.Target = target;
				singleTargetCostCalculator.WaitTime = elapsedTime;
				singleTargetCostCalculator.StartOrbit = currentShipOrbit;
				var cost = singleTargetCostCalculator.CalculateCost(driftTime, transferTime);

				totalCost += cost;
				currentShipOrbit = keplerianPropagation.PropagateState(target.InitialOrbit,
					elapsedTime + driftTime + transferTime + target.ServiceTime);
				elapsedTime += cost.TotalTime;
			}

			return totalCost;
		}
	}
}