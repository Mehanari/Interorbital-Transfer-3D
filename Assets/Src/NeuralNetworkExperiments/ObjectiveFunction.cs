using System;
using MehaMath.Math.Components;
using Src.OptimizationFramework;
using Src.OptimizationFramework.Calculators;
using Src.OptimizationFramework.Calculators.Cost;
using Src.OptimizationFramework.Calculators.Fuel;
using Src.OptimizationFramework.DataModels;
using Src.OptimizationFramework.ScheduleOptimizers;

public class ObjectiveFunction 
{
   public double Isp { get; set; }
   public double StandGrav { get; set; }
   public double Mu { get; set; }
   public double CentralBodyRadius { get; set; }
   public double FuelCost { get; set; }
   public double TimeCost { get; set; }
   public double FuelSurplus { get; set; }
   public double ShipFinalMass { get; set; }
   public double MinDriftTime { get; set; } = 1000;
   public double MaxDriftTime { get; set; } = 20000;
   public double MinTransferTime { get; set; } = 1000;
   public double MaxTransferTime { get; set; } = 80000;

   public double SemiMajorAxisMin { get; set; } = 6650d;
   public double SemiMajorAxisMax { get; set; } = 8400d;
   public double MinEccentricity { get; set; } = 0d;
   public double MaxEccentricity { get; set; } = 0.25d;
   public double MinServiceTime { get; set; } = 1200;
   public double MaxServiceTime { get; set; } = 3600;
   public int PointsPerDimension { get; set; } = 40;

   /// <summary>
   /// The input vector structure:
   /// 0 - ship start orbit semi major axis
   /// 1 - ship start orbit eccentricity
   /// 2 - ship start orbit inclination
   /// 3 - ship start orbit perigee argument
   /// 4 - ship start orbit ascending node longitude
   /// 5 - ship start orbit true anomaly
   /// 6 - satellite orbit semi major axis
   /// 7 - satellite orbit eccentricity
   /// 8 - satellite orbit inclination
   /// 9 - satellite orbit perigee argument
   /// 10 - satellite orbit ascending node longitude
   /// 11 - satellite orbit true anomaly
   /// 12 - drift time
   /// 13 - transfer time
   /// 14 - satellite service time
   /// </summary>
   private (Orbit shipOrbit, Orbit satelliteOrbit, double driftTime, double transferTime, double serviceTime)
      FromVector(Vector normalizedVector)
   {
      var shipOrbit = new Orbit()
      {
         SemiMajorAxis = SemiMajorAxisMin + normalizedVector[0] * (SemiMajorAxisMax - SemiMajorAxisMin),
         Eccentricity = MinEccentricity + normalizedVector[1] * (MaxEccentricity - MinEccentricity),
         AscendingNodeLongitude = Math.PI * 2 * normalizedVector[2],
         Inclination = Math.PI * normalizedVector[3],
         PerigeeArgument = Math.PI * 2 * normalizedVector[4],
         TrueAnomaly = Math.PI * 2 * normalizedVector[5]
      };
      var satelliteOrbit = new Orbit()
      {
         SemiMajorAxis = SemiMajorAxisMin + normalizedVector[6] * (SemiMajorAxisMax - SemiMajorAxisMin),
         Eccentricity = MinEccentricity + normalizedVector[7] * (MaxEccentricity - MinEccentricity),
         AscendingNodeLongitude = Math.PI * 2 * normalizedVector[8],
         Inclination = Math.PI * normalizedVector[9],
         PerigeeArgument = Math.PI * 2 * normalizedVector[10],
         TrueAnomaly = Math.PI * 2 * normalizedVector[11]
      };
      var driftTime = MinDriftTime + normalizedVector[12] * (MaxDriftTime - MinDriftTime);
      var transferTime = MinTransferTime + normalizedVector[13] * (MaxTransferTime - MinTransferTime);
      var serviceTime = MinServiceTime + normalizedVector[14] * (MaxServiceTime - MinServiceTime);

      return (shipOrbit, satelliteOrbit, driftTime, transferTime, serviceTime);
   }
   
   public Vector Calculate(Vector inputNormalized)
   {
      var (shipOrbit, satelliteOrbit, driftTime, transferTime, serviceTime) = FromVector(inputNormalized);
      var kinematicCalculator = new KinematicCalculator(Mu);
      var intersectionCalculator = new IntersectionsCalculator()
      {
         Mu = Mu,
         CentralBodyRadius = CentralBodyRadius
      };
      var fuelCalculator = new SurplusFuelCalculator()
      {
         Surplus = FuelSurplus,
         ShipFinalMass = ShipFinalMass,
         Isp = Isp,
         StandardGrav = StandGrav
      };
      var costCalculator = new WeightedCostCalculator()
      {
         FuelCalculator = fuelCalculator,
         IntersectionsCalculator = intersectionCalculator,
         KinematicCalculator = kinematicCalculator,
         FuelCost = FuelCost,
         TimeCost = TimeCost
      };

      var gridOptimizer = new GridDescentSequentialOptimizer()
      {
         CostCalculator = costCalculator,
         GdIterationsLimit = 10000,
         KinematicCalculator = kinematicCalculator,
         MinDriftTime = MinDriftTime,
         MaxDriftTime = MaxDriftTime,
         MinTransferTime = MinTransferTime,
         MaxTransferTime = MaxTransferTime,
         Mu = Mu,
         PointsPerDimension = PointsPerDimension
      };

      var target = new TargetParameters()
      {
         TargetName = "Target",
         ServiceTime = serviceTime,
         Orbit = satelliteOrbit
      };
      var optimum = gridOptimizer.OptimizeSchedule(new TargetParameters[] { target }, shipOrbit);
      var cost = costCalculator.CalculateCost(optimum, new TargetParameters[] { target }, shipOrbit);
      return new Vector(optimum.driftTimes[0], optimum.transferTimes[0], cost);
   }
}
