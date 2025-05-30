using System;

namespace Src.OptimizationFramework.Calculators
{
	public class FuelCalculator
	{
		public double Isp { get; set; }
		/// <summary>
		/// Standard gravity acceleration
		/// </summary>
		public double StandardGrav { get; set; }
		public double Surplus { get; set; }


		public FuelCalculator(double isp, double standardGrav, double surplus)
		{
			Isp = isp;
			StandardGrav = standardGrav;
			Surplus = surplus;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="transfersKinematics">Must be in a chronological order</param>
		/// <param name="spacecraftFinalMass">How much should spacecraft weight after all transfers</param>
		/// <returns></returns>
		public double[] CalculateFuelMasses(KinematicData[] transfersKinematics, double spacecraftFinalMass)
		{
			var fuelMasses = new double[transfersKinematics.Length];
			var currentFinalMass = spacecraftFinalMass;
			//Starting with the last transfer
			for (int i = transfersKinematics.Length-1; i >= 0; i--)
			{
				var transfer = transfersKinematics[i];
				var startDeltaV = (transfer.TransferStartVelocity - transfer.DriftEndVelocity).Magnitude();
				var endDeltaV = (transfer.TransferEndVelocity - transfer.ServiceStartVelocity).Magnitude();
				var totalDeltaV = startDeltaV + endDeltaV;
				var fuelMass = currentFinalMass * (Math.Exp(totalDeltaV / (StandardGrav * Isp)) - 1);
				fuelMass *= (1 + Surplus);
				fuelMasses[i] = fuelMass;
				currentFinalMass += fuelMass;
			}

			return fuelMasses;
		}
	}
}