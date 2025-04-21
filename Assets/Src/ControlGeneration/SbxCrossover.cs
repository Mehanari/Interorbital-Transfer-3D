namespace Src.ControlGeneration
{
	public class SbxCrossover : ICrossover
	{
		/// <summary>
		/// For each gene in a genome determines whether to generate a new gene with a crossover, or copy it directly to the offsprings.
		/// </summary>
		public double CrossoverProbability { get; set; }
		
		public (Specimen offspringA, Specimen offspringB) Crossover(Specimen parentA, Specimen parentB)
		{
			throw new System.NotImplementedException();
		}
	}
}