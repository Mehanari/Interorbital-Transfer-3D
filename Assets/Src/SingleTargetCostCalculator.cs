using System;
using MehaMath.Math.Components;
using Src.Helpers;
using Src.LambertProblem;
using Src.Model;
using Src.SpacecraftDynamics.CentralBodyDynamics;

namespace Src
{
	public class SingleTargetCostCalculator
	{
		/// <summary>
		/// Gravitational parameter
		/// </summary>
		public double Mu { get; set; } = 398600.4418d;
		/// <summary>
		/// Standard gravitational acceleration
		/// </summary>
		public double g0 { get; set; } = 9.80665;
		/// <summary>
		/// Mass of the spacecraft without fuel
		/// </summary>
		public double MDry { get; set; } = 100; 
		/// <summary>
		/// Engine specific impulse
		/// </summary>
		public double Isp { get; set; } = 300;

		/// <summary>
		/// Price of fuel per unit
		/// </summary>
		public double FuelPrice { get; set; } = 1d;
		/// <summary>
		/// Price of time per unit
		/// </summary>
		public double TimePrice { get; set; } = 1d;
		/// <summary>
		/// How much more fuel we need compared to the ideal case (instantaneous burns).
		/// </summary>
		public double FuelSurplus { get; set; } = 0.2;

		/// <summary>
		/// For the case if the spacecraft velocity in km/s and you need to convert to m/s to calculate fuel.
		/// </summary>
		public double DeltaVRatio { get; set; } = 1000;
		public Vector SpacecraftInitPos { get; set; }
		public Vector SpacecraftInitVel { get; set; }
		public Vector SatelliteInitPos { get; set; }
		public Vector SatelliteInitVel { get; set; }

		

		public double CalculateCost(double driftTime, double transferTime)
		{
			return CalculateCostWithOrbit(driftTime, transferTime).cost;
		}


		public (double cost, Orbit transfer) CalculateCostWithOrbit(double driftTime, double transferTime)
		{
			if (transferTime <= 0)
			{
				throw new ArgumentException("Transfer time must be greater than zero");
			}
			if (driftTime < 0)
			{
				throw new ArgumentException("Drift time must be non-negative");
			}

			var spacecraftInitialOrbit = OrbitHelper.GetOrbit(SpacecraftInitVel, SpacecraftInitPos, Mu);
			var satelliteInitialOrbit = OrbitHelper.GetOrbit(SatelliteInitVel, SatelliteInitPos, Mu);
			
			var keplerianPropagation = new KeplerianPropagation()
			{
				CentralBodyPosition = new Vector(0, 0, 0),
				GravitationalParameter = Mu
			};

			var startOrbit = keplerianPropagation.PropagateState(spacecraftInitialOrbit, driftTime);
			var endOrbit = keplerianPropagation.PropagateState(satelliteInitialOrbit, driftTime + transferTime);

			var (startPos, startVel) = OrbitHelper.GetPositionAndVelocity(startOrbit, Mu);
			var (endPos, endVel) = OrbitHelper.GetPositionAndVelocity(endOrbit, Mu);

			var (vt1, vt2) = Gooding1990.FindTransfer(Mu, startPos, endPos, transferTime);
			var transferOrbit = OrbitHelper.GetOrbit(vt1, startPos, Mu);
			
			//Calculating the cost
			var deltaVStart = startVel - vt1;
			var deltaVEnd = endVel - vt2;
			var totalDeltaV = deltaVEnd.Magnitude() + deltaVStart.Magnitude();
			//Lower bound of required fuel mass
			var mMin = MDry * (Math.Exp(totalDeltaV*DeltaVRatio / (Isp * g0)) - 1);

			var surplus = mMin * FuelSurplus;
			var mRequired = mMin + surplus;
			var cost = mRequired * FuelPrice + (driftTime + transferTime) * TimePrice;
			return (cost, transferOrbit);
		}

	}
}