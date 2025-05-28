using System;
using MehaMath.Math.Components;
using Src.Helpers;

namespace Src.FinalComponents
{
	public class KinematicCalculator
	{
		public int MaxRevolutions { get; set; } = 10;
		
		/// <summary>
		/// Gravitational parameter
		/// </summary>
		public double Mu { get; set; } = 398600.4418d;

		public KinematicCalculator(double mu)
		{
			Mu = mu;
		}

		public KinematicData[] CalculateKinematics(double[] driftTimes, double[] transferTimes,
			TargetParameters[] targets, Orbit startOrbit)
		{
			var kinematics = new KinematicData[targets.Length];
			var spacecraftCurrentOrbit = startOrbit;
			var elapsedTime = 0d;
			for (int i = 0; i < targets.Length; i++)
			{
				var target = targets[i];
				var driftTime = driftTimes[i];
				var transferTime = transferTimes[i];
				var kinematicData = CalculateKinematics(driftTime, transferTime, elapsedTime,
					target, spacecraftCurrentOrbit);
				kinematics[i] = kinematicData;
				spacecraftCurrentOrbit = OrbitHelper.GetOrbit(kinematicData.ServiceEndVelocity,
					kinematicData.ServiceEndPosition, Mu);
				elapsedTime += driftTime + transferTime + target.ServiceTime;
			}

			return kinematics;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="driftTime"></param>
		/// <param name="transferTime"></param>
		/// <param name="waitTime">This parameter needed to update target's position for the cases when given TargetParameters object represents the state of the target in the past.
		/// This parameter DOES NOT affect the startOrbit, all calculations are made with assumption that startOrbit corresponds to the spacecraft's state after waitTime.</param>
		/// <param name="target"></param>
		/// <param name="startOrbit"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public KinematicData CalculateKinematics(double driftTime, double transferTime, double waitTime, TargetParameters target, Orbit startOrbit)
		{
			if (transferTime <= 0)
			{
				throw new ArgumentException("Transfer time must be greater than zero");
			}

			if (driftTime < 0)
			{
				throw new ArgumentException("Drift time must be non-negative");
			}

			var keplerianPropagation = new KeplerianPropagation()
			{
				CentralBodyPosition = new Vector(0, 0, 0),
				GravitationalParameter = Mu
			};

			//Where is the spacecraft initially?
			var driftStartOrbit = startOrbit;
			var (driftStartPos, driftStartVel) = OrbitHelper.GetPositionAndVelocity(driftStartOrbit, Mu);
			//Where is the spacecraft after drift?
			var driftEndOrbit = keplerianPropagation.PropagateState(driftStartOrbit, driftTime);
			var (driftEndPos, driftEndVel) = OrbitHelper.GetPositionAndVelocity(driftEndOrbit, Mu);
			//Where do we need to meet the target?
			var rendezvousOrbit =
				keplerianPropagation.PropagateState(target.InitialOrbit, driftTime + transferTime + waitTime);
			var (rendezvousPos, rendezvousVel) = OrbitHelper.GetPositionAndVelocity(rendezvousOrbit, Mu);
			//How to get from driftEndPos (transfer start position) to rendezvousPos in a given transfer time?
			var (transferStartVel, transferEndVel) = FindCheapestTransfer(driftEndPos, driftEndVel, rendezvousPos, rendezvousVel, transferTime);
			//Where the spacecraft will be after servicing the target?
			var serviceEndOrbit = keplerianPropagation.PropagateState(rendezvousOrbit, target.ServiceTime);
			var (serviceEndPos, serviceEndVel) = OrbitHelper.GetPositionAndVelocity(serviceEndOrbit, Mu);

			return new KinematicData()
			{
				ServiceTime = target.ServiceTime,
				DriftTime = driftTime,
				TransferTime = transferTime,
				DriftStartPosition = driftStartPos,
				DriftStartVelocity = driftStartVel,
				DriftEndVelocity = driftEndVel,
				TransferStartPosition = driftEndPos,
				TransferStartVelocity = transferStartVel,
				TransferEndPosition = rendezvousPos,
				TransferEndVelocity = transferEndVel,
				ServiceStartVelocity = rendezvousVel,
				ServiceEndPosition = serviceEndPos,
				ServiceEndVelocity = serviceEndVel
			};
		}

		private (Vector transferStarVel, Vector transferEndVel) FindCheapestTransfer(Vector driftEndPos, Vector driftEndVel,
			Vector transferEndPos, Vector rendezvousVel, double transferTime)
		{
			//Can we get lower delta V if we do several revolutions along the transfer orbit before meeting the satellite?
			var minDeltaV = double.MaxValue;
			var optimalStartVel = new Vector();
			var optimalEndVel = new Vector();
			for (int i = 0; i < MaxRevolutions; i++)
			{
				//Gooding's algorithm implementation may throw an exception if no feasible solution exist for the given amount of revolutions.
				//It does not throw an exception if the number of revolutions is zero.
				try
				{
					var (currentStartVel, currentEndVel) = Gooding1990.FindTransfer(Mu, driftEndPos, transferEndPos, transferTime, revolutions: i);
					var startDeltaV = (driftEndVel - currentStartVel).Magnitude();
					var endDeltaV = (currentEndVel - rendezvousVel).Magnitude();
					var totalDeltaV = startDeltaV + endDeltaV;
					if (totalDeltaV < minDeltaV)
					{
						minDeltaV = totalDeltaV;
						optimalStartVel = currentStartVel;
						optimalEndVel = currentEndVel;
					}
				}
				catch (Exception e)
				{
					return (optimalStartVel, optimalEndVel);
				}
			}

			return (optimalStartVel, optimalEndVel);
		}
	}
}