namespace Src.ControlGeneration
{
	public interface ICrossover
	{
		public (Specimen offspringA, Specimen offspringB) Crossover(Specimen parentA, Specimen parentB);
	}
}