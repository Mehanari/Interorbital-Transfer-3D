using System;
using MehaMath.Math.Components;
using Src.Helpers;
using Src.OptimizationFramework.Calculators;
using Src.OptimizationFramework.MathComponents;

namespace Src.OptimizationFramework.ScheduleOptimizers
{
    public class SequentialScheduleOptimizer : ScheduleOptimizer
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
        
        public double DriftTimeInitialGuess { get; set; }
        public double TransferTimeInitialGuess { get; set; }
        
        private readonly double _mu;
		
        public SequentialScheduleOptimizer(CostCalculator costCalculator, KinematicCalculator kinematicCalculator, double mu)
        {
            _mu = mu;
        }

        public override (double[] driftTimes, double[] transferTimes) OptimizeSchedule(TargetParameters[] targets, Orbit spacecraftInitialOrbit, double spacecraftFinalMass)
        {
            var driftTimes = new double[targets.Length];
            var transferTimes = new double[targets.Length];
            
            var elapsedTime = 0d;
            var spacecraftCurrentOrbit = spacecraftInitialOrbit;
            for (int i = 0; i < targets.Length; i++)
            {
                var target = targets[i];
                var (drift, transfer, nextOrbit) = OptimizeForOne(DriftTimeInitialGuess, TransferTimeInitialGuess, elapsedTime,
                    spacecraftFinalMass, target, spacecraftCurrentOrbit);
                
                driftTimes[i] = drift;
                transferTimes[i] = transfer;
                elapsedTime += drift + transfer + target.ServiceTime;
                spacecraftCurrentOrbit = nextOrbit;
            }

            return (driftTimes, transferTimes);
        }

        /// <summary>
        /// Finds cost-optimal drift and transfer time for one target.
        /// </summary>
        /// <param name="driftTimeInit"></param>
        /// <param name="transferTimeInit"></param>
        /// <param name="elapsedTime">How much time have passed before the spacecraft got to its current orbit (spacecraftCurrentOrbit)</param>
        /// <param name="spacecraftFinalMass">How much the spacecraft should weight after servicing the target</param>
        /// <param name="target"></param>
        /// <param name="spacecraftCurrentOrbit">Where is the spacecraft at the beginning of this transfer drift time</param>
        /// <returns>
        /// driftTime - optimal drift time for given target,
        /// transferTime - optimal transfer time for given target,
        /// spacecraftFinalOrbit - where the spacecraft will be after servicing the given target
        /// </returns>
        private (double driftTime, double transferTime, Orbit spacecraftFinalOrbit) OptimizeForOne(double driftTimeInit, double transferTimeInit, double elapsedTime, double spacecraftFinalMass,
            TargetParameters target, Orbit spacecraftCurrentOrbit)
        {
            var initialGuess = new Vector(driftTimeInit, transferTimeInit);
            var min = GradientDescent.Minimize(Objective, initialGuess, GdStepSize, GdTolerance, GdIterationsLimit,
                projection: Project);
            var kinematic =
                KinematicCalculator.CalculateKinematics(min[0], min[1], elapsedTime, target, spacecraftCurrentOrbit);
            var spacecraftFinalOrbit =
                OrbitHelper.GetOrbit(kinematic.ServiceEndVelocity, kinematic.ServiceEndPosition, _mu);
            return (min[0], min[1], spacecraftFinalOrbit);
            
            double Objective(Vector times)
            {
                var driftTime = times[0];
                var transferTime = times[1];
                var invalidTimes = 0d;
                
                if (driftTime < 0)
                {
                    invalidTimes += driftTime;
                    driftTime = 0d;
                }
                if (transferTime < MinTransferTime)
                {
                    invalidTimes += Math.Abs(transferTime - MinTransferTime);
                    transferTime = MinTransferTime;
                }

                var kinematics = KinematicCalculator.CalculateKinematics(driftTime, transferTime, elapsedTime, target,
                    spacecraftCurrentOrbit);
                var cost = CostCalculator.CalculateCost(new KinematicData[] { kinematics }, spacecraftFinalMass);
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

            return new Vector(driftTime, transferTime);
        }
    }
}
