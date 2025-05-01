namespace Src.GeneticAlgorithms
{
	public abstract class GenomeClamper : IGenomeClamper
	{
		public void ClampGenomeFor(Specimen specimen)
		{
			for (int i = 0; i < specimen.Genome.Length; i++)
			{
				var clamped = GetClamped(i, specimen.Genome);
				specimen.Genome[i] = clamped;
			}
		}

		/// <summary>
		/// Must return a clamped value of gene at given index.
		/// </summary>
		/// <param name="geneIndex"></param>
		/// <param name="genome"></param>
		/// <returns></returns>
		protected abstract double GetClamped(int geneIndex, double[] genome);
	}
}