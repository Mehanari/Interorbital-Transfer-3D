using System;
using System.Linq;
using MehaMath.Math.Components;
using Src.Helpers;

namespace Src.OptimizationFramework.Calculators
{
	public class CostCalculator
	{
		public double Mu { get; set; }
		public double CentralBodyRadius { get; set; }
		public FuelCalculator FuelCalculator { get; set; }
		public double CrushPenaltyLambda { get; set; } = 100000;
		public double CrushPenaltyPower { get; set; } = 2;
		public double FuelCost { get; set; }
		public double TimeCost { get; set; }
		

		public CostCalculator(FuelCalculator fuelCalculator, double mu, double centralBodyRadius, double fuelCost, double timeCost)
		{
			FuelCalculator = fuelCalculator;
			Mu = mu;
			CentralBodyRadius = centralBodyRadius;
			FuelCost = fuelCost;
			TimeCost = timeCost;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="transfersKinematics">Kinematics of transfers in a chronological order.</param>
		/// <param name="finalSpacecraftMass">How much the spacecraft should weight after all transfers.</param>
		/// <returns></returns>
		public double CalculateCost(KinematicData[] transfersKinematics, double finalSpacecraftMass)
		{
			var fuel = FuelCalculator.CalculateFuelMasses(transfersKinematics, finalSpacecraftMass);
			var intersections =
				IntersectionsCalculator.CalculateIntersections(transfersKinematics, Mu, CentralBodyRadius);

			var totalTime = transfersKinematics.Sum(k => k.ServiceTime + k.TransferTime + k.DriftTime);
			var totalFuel = fuel.Sum();
			var totalIntersection = intersections.Sum();
			

			return totalFuel * FuelCost + totalTime * TimeCost +
			       Math.Pow(totalIntersection, CrushPenaltyPower) * CrushPenaltyLambda;
		}
	}
}