namespace Src.GeneticAlgorithms
{
	public interface IMutator
	{
		public void MutatePopulation(Specimen[] population);
	}
}