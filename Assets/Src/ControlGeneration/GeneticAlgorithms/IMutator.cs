namespace Src.ControlGeneration.GeneticAlgorithms
{
	public interface IMutator
	{
		public void MutatePopulation(Specimen[] population);
	}
}