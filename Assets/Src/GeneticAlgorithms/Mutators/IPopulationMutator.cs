namespace Src.GeneticAlgorithms.Mutators
{
	public interface IPopulationMutator
	{
		public void MutatePopulation(Specimen[] population);
	}
}