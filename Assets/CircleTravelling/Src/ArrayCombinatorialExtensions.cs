using System.Collections.Generic;

namespace CircleTravelling.Src
{
	public static class ArrayCombinatorialExtensions
	{
		public static T[][] GetPermutations<T>(this T[] collection)
		{
			var length = collection.Length-1;
			var indicesPermutations = PermutationGenerator.GeneratePermutations(length);
			var permutations = new T[indicesPermutations.GetLength(0)][];
			for (int i = 0; i < indicesPermutations.GetLength(0); i++)
			{
				permutations[i] = new T[indicesPermutations.GetLength(1)];
				for (int j = 0; j < indicesPermutations.GetLength(1); j++)
				{
					var index = indicesPermutations[i, j];
					permutations[i][j] = collection[indicesPermutations[i, j]];
				}
			}

			return permutations;
		}
	}
}