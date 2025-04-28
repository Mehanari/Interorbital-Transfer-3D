using System.Collections.Generic;

namespace Src.GeneticAlgorithms
{
	public class SpecimenComparer : IComparer<Specimen>
	{
		public int Compare(Specimen x, Specimen y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (ReferenceEquals(null, y)) return 1;
			if (ReferenceEquals(null, x)) return -1;
			return x.Fitness.CompareTo(y.Fitness);
		}
	}
}