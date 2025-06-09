using Src.OptimizationFramework.DataModels;

namespace Src.OptimizationFramework.Calculators.Fuel
{
	public interface IFuelCalculator
	{
		public double[] CalculateFuelMasses(KinematicData[] transfersKinematics);
	}
}