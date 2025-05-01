namespace Src.GeneticAlgorithms.Mutators
{
	public abstract class PolynomialPopulationMutator : IPopulationMutator
	{
		private readonly PolynomialGeneMutator _geneMutator;

		protected PolynomialPopulationMutator(PolynomialGeneMutator geneMutator)
		{
			_geneMutator = geneMutator;
		}

		public void MutatePopulation(Specimen[] population)
		{
			foreach (var specimen in population)
			{
				MutateSpecimen(specimen);
			}
		}

		private void MutateSpecimen(Specimen specimen)
		{
			for (int i = 0; i < specimen.Genome.Length; i++)
			{
				var gene = specimen.Genome[i];
				var range = GetRangeForGene(i, specimen.Genome);
				var mutated = _geneMutator.MutateGene(gene, range.min, range.max);
				specimen.Genome[i] = mutated;
			}
		}

		protected abstract (double min, double max) GetRangeForGene(int geneIndex, double[] genome);
	}
}