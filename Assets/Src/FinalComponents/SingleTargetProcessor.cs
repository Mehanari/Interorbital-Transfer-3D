using System;
using MehaMath.Math.Components;
using Src.Helpers;

namespace Src.FinalComponents
{
	public class SingleTargetProcessor
	{
		/// <summary>
		/// Gravitational parameter
		/// </summary>
		public double Mu { get; set; } = 398600.4418d;
		public double CentralBodyRadius { get; set; }
		
		public TargetParameters Target { get; set; }
		public Orbit StartOrbit { get; set; }
		/// <summary>
		/// This parameter needed to update target's position for the cases when given Target object represents the state of the target in the past.
		/// This parameter DOES NOT affect the StartOrbit, all calculations are made with assumption that StartOrbit corresponds to the spacecraft's state after WaitTime.
		/// </summary>
		public double WaitTime { get; set; }

		

		public CostParameters CalculateCost(double driftTime, double transferTime)
		{
			return CalculateCostWithOrbit(driftTime, transferTime).cost;
		}

		public (CostParameters cost, Orbit transfer) CalculateCostWithOrbit(double driftTime, double transferTime)
		{
			if (transferTime <= 0)
			{
				throw new ArgumentException("Transfer time must be greater than zero");
			}
			if (driftTime < 0)
			{
				throw new ArgumentException("Drift time must be non-negative");
			}

			var spacecraftStartOrbit = StartOrbit;
			var satelliteInitialOrbit = Target.InitialOrbit;
			
			var keplerianPropagation = new KeplerianPropagation()
			{
				CentralBodyPosition = new Vector(0, 0, 0),
				GravitationalParameter = Mu
			};

			var startOrbit = keplerianPropagation.PropagateState(spacecraftStartOrbit, driftTime);
			var endOrbit = keplerianPropagation.PropagateState(satelliteInitialOrbit, driftTime + transferTime + WaitTime);

			var (startPos, startVel) = OrbitHelper.GetPositionAndVelocity(startOrbit, Mu);
			var (endPos, endVel) = OrbitHelper.GetPositionAndVelocity(endOrbit, Mu);

			var (vt1, vt2) = Gooding1990.FindTransfer(Mu, startPos, endPos, transferTime);
			var transferOrbit = OrbitHelper.GetOrbit(vt1, startPos, Mu);
			
			//Calculating the delta V
			var deltaVStart = startVel - vt1;
			var deltaVEnd = endVel - vt2;
			var totalDeltaV = deltaVEnd.Magnitude() + deltaVStart.Magnitude();

			//Calculating the time (simple)
			var neededTime = driftTime + transferTime + Target.ServiceTime;
			
			//Calculating the central body intersection
			var startTrueAnomaly = transferOrbit.TrueAnomaly;
			var endTransferOrbit = OrbitHelper.GetOrbit(vt2, endPos, Mu);
			var endTrueAnomaly = endTransferOrbit.TrueAnomaly;
			var minCentralBodyDistance =
				CentralBodyDistanceCalculator.MinDistanceForSection(transferOrbit, startTrueAnomaly, endTrueAnomaly);
			var centralBodyIntersection = CentralBodyRadius - minCentralBodyDistance;
			if (centralBodyIntersection < 0)
			{
				centralBodyIntersection = 0d;
			}

			var costParameters = new CostParameters()
			{
				CentralBodyIntersection = centralBodyIntersection,
				TotalTime = neededTime,
				TotalVelocityDelta = totalDeltaV
			};
			return (costParameters, transferOrbit);
		}
	}
}