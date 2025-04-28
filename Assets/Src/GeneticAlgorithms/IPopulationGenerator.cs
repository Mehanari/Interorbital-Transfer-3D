namespace Src.GeneticAlgorithms
{
	public interface IPopulationGenerator
	{
		public Specimen[] GeneratePopulation(int populationSize);
	}
}