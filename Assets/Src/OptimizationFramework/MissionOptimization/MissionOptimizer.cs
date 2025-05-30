namespace Src.OptimizationFramework.MissionOptimization
{
	public interface IMissionOptimizer
	{
		public OptimizationResult Optimize(MissionParameters parameters);
	}
}