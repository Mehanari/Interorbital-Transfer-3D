namespace Src.OptimizationFramework.Calculators.Fuel
{
	public abstract class FuelCalculator
	{
		public abstract double[] CalculateFuelMasses(KinematicData[] transfersKinematics);
	}
}