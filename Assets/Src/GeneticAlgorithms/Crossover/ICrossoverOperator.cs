namespace Src.GeneticAlgorithms.Crossover
{
	public interface ICrossoverOperator
	{
		public (double[] offspringA, double[] offspringB) Crossover(double[] parentA, double[] parentB);
	}
}