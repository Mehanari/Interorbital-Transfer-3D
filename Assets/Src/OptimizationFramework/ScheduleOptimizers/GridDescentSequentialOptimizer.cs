using System;
using MehaMath.Math.Components;
using Src.Helpers;
using Src.OptimizationFramework.Calculators;
using Src.OptimizationFramework.Calculators.Cost;
using Src.OptimizationFramework.DataModels;
using Src.OptimizationFramework.MathComponents;

namespace Src.OptimizationFramework.ScheduleOptimizers
{
	public class GridDescentSequentialOptimizer : ScheduleOptimizer
	{
		/// <summary>
		/// Step size of the gradient descent.
		/// </summary>
		public double GdStepSize { get; set; } = 0.1d;

		/// <summary>
		/// Iterations limit for the gradient descent.
		/// </summary>
		public int GdIterationsLimit { get; set; } = 1000;
		
		/// <summary>
		/// Tolerance of the gradient descent search.
		/// Minimum difference between new guess cost and previous guess cost to stop iteration process.
		/// </summary>
		public double GdTolerance { get; set; } = 0.1d;
		
		public double MinTransferTime { get; set; }
		public double MaxTransferTime { get; set; }
		public double MinDriftTime { get; set; }
		public double MaxDriftTime { get; set; }
		public int PointsPerDimension { get; set; }
		
		public KinematicCalculator KinematicCalculator { get; set; }

		public double Mu { get; set; }
		

		public override (double[] driftTimes, double[] transferTimes) OptimizeSchedule(TargetParameters[] targets, Orbit spacecraftInitialOrbit)
		{
			var driftTimes = new double[targets.Length];
			var transferTimes = new double[targets.Length];
			var keplerianPropagation = new KeplerianPropagation()
			{
				CentralBodyPosition = new Vector(0, 0, 0),
				GravitationalParameter = Mu
			};
			
			var elapsedTime = 0d;
			var spacecraftCurrentOrbit = spacecraftInitialOrbit;
			for (int i = 0; i < targets.Length; i++)
			{
				var target = targets[i];
				if (elapsedTime > 0d)
				{
					target.Orbit = keplerianPropagation.PropagateState(target.Orbit, elapsedTime);
				}
				var (drift, transfer, nextOrbit) = OptimizeForOne(target, spacecraftCurrentOrbit);
                
				driftTimes[i] = drift;
				transferTimes[i] = transfer;
				elapsedTime += drift + transfer + target.ServiceTime;
				spacecraftCurrentOrbit = nextOrbit;
			}

			return (driftTimes, transferTimes);
		}
		
		/// <summary>
        /// Finds cost-optimal drift and transfer time for one targetCurrentState.
        /// </summary>
        /// <param name="elapsedTime">How much time have passed before the spacecraft got to its current orbit (spacecraftCurrentOrbit)</param>
        /// <param name="spacecraftFinalMass">How much the spacecraft should weight after servicing the targetCurrentState</param>
        /// <param name="targetCurrentState"></param>
        /// <param name="spacecraftCurrentOrbit">Where is the spacecraft at the beginning of this transfer drift time</param>
        /// <returns>
        /// driftTime - optimal drift time for given targetCurrentState,
        /// transferTime - optimal transfer time for given targetCurrentState,
        /// spacecraftFinalOrbit - where the spacecraft will be after servicing the given targetCurrentState
        /// </returns>
        private (double driftTime, double transferTime, Orbit spacecraftFinalOrbit) OptimizeForOne(
            TargetParameters targetCurrentState, Orbit spacecraftCurrentOrbit)
		{
			var zeroPoint = new Vector(MinDriftTime, MinTransferTime);
			var difference = new Vector((MaxDriftTime - MinDriftTime) / (PointsPerDimension - 1),
				(MaxTransferTime - MinTransferTime) / (PointsPerDimension - 1));
			var gridDescentOptimizer = new GridDescent(zeroPoint, difference, PointsPerDimension, MajorCost,
				GdTolerance, GdIterationsLimit, Project, true, MajorCost, GdStepSize);
			var min = gridDescentOptimizer.Minimize();
            var kinematic =
                KinematicCalculator.CalculateKinematics(min[0], min[1], targetCurrentState, spacecraftCurrentOrbit);
            var spacecraftFinalOrbit =
                OrbitHelper.GetOrbit(kinematic.ServiceEndVelocity, kinematic.ServiceEndPosition, Mu);
            
            return (min[0], min[1], spacecraftFinalOrbit);
            
            double MajorCost(Vector times)
            {
                var driftTime = times[0];
                var transferTime = times[1];
                var invalidTimes = 0d;
                
                if (driftTime < 0)
                {
                    invalidTimes += driftTime;
                    times[0] = 0d;
                }
                if (transferTime < MinTransferTime)
                {
                    invalidTimes += Math.Abs(transferTime - MinTransferTime);
                    times[1] = MinTransferTime;
                }
                
                var cost = CostCalculator.CalculateCost(times, new TargetParameters[]{targetCurrentState}, spacecraftCurrentOrbit);
                return cost + invalidTimes * invalidTimes;
            }
        }
		
		private Vector Project(Vector times)
		{
			var driftTime = times[0];
			var transferTime = times[1];
			if (driftTime < 0d)
			{
				driftTime = 0d;
			}
			if (transferTime < MinTransferTime)
			{
				transferTime = MinTransferTime;
			}

			if (driftTime > MaxDriftTime)
			{
				driftTime = MaxDriftTime;
			}

			if (transferTime > MaxTransferTime)
			{
				transferTime = MaxTransferTime;
			}
			return new Vector(driftTime, transferTime);
		}
	}
}