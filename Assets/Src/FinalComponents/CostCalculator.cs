using System;
using System.Linq;
using MehaMath.Math.Components;
using Src.Helpers;

namespace Src.FinalComponents
{
	public class CostCalculator
	{
		public double Mu { get; set; }
		public double CentralBodyRadius { get; set; }
		public FuelCalculator FuelCalculator { get; set; }
		public double CrushPenaltyLambda { get; set; } = 100;
		public double CrushPenaltyPower { get; set; } = 4;
		public double FuelCost { get; set; }
		public double TimeCost { get; set; }

		private readonly KeplerianPropagation _keplerianPropagation;

		public CostCalculator(FuelCalculator fuelCalculator, double mu, double centralBodyRadius, double fuelCost, double timeCost)
		{
			FuelCalculator = fuelCalculator;
			Mu = mu;
			CentralBodyRadius = centralBodyRadius;
			FuelCost = fuelCost;
			TimeCost = timeCost;

			_keplerianPropagation = new KeplerianPropagation()
			{
				GravitationalParameter = Mu,
				CentralBodyPosition = new Vector(0, 0, 0)
			};
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

			var totalTime = 0d;
			var totalFuel = fuel.Sum();
			var totalIntersection = 0d;

			foreach (var kinematics in transfersKinematics)
			{
				totalTime += (kinematics.ServiceTime + kinematics.DriftTime + kinematics.TransferTime);

				var transferOrbitStart = OrbitHelper.GetOrbit(kinematics.TransferStartVelocity,
					kinematics.TransferStartPosition, Mu);
				var transferOrbitEnd =
					_keplerianPropagation.PropagateState(transferOrbitStart, kinematics.TransferTime);
				var startTrueAnomaly = transferOrbitStart.TrueAnomaly;
				var endTrueAnomaly = transferOrbitEnd.TrueAnomaly;
				var distance =
					CentralBodyDistanceCalculator.MinDistanceForSection(transferOrbitStart, startTrueAnomaly,
						endTrueAnomaly);
				//If distance to the central body center is bigger than central body radius, then there is no intersection.
				var intersection = distance > CentralBodyRadius ? 0 : CentralBodyRadius - distance;
				totalIntersection += intersection;
			}

			return totalFuel * FuelCost + totalTime * TimeCost +
			       Math.Pow(totalIntersection, CrushPenaltyPower) * CrushPenaltyLambda;
		}
	}
}