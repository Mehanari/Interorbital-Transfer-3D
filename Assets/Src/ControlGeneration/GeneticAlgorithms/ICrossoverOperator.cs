namespace Src.ControlGeneration.GeneticAlgorithms
{
	public interface ICrossoverOperator
	{
		public (double[] offspringA, double[] offspringB) Crossover(double[] parentA, double[] parentB);
	}
}