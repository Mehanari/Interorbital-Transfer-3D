namespace BoatAndRafts.Src
{
	public static class ArrayExtensions
	{
		/// <summary>
		/// Loops given index according to array length.
		/// If index is bigger or equal to the array length, reduces it and returns modulus of index.
		/// If index is less than zero, returns and index from the back of an array.
		/// Example:
		/// If you have an array of length 10 and you call LoopIndex on -11, you will get 8, e.g the second element from the end.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static int LoopIndex<T>(this T[] array, int index)
		{
			var length = array.Length;
			var indexClamped = index % length;
			if (indexClamped < 0)
			{
				indexClamped = length + indexClamped;
			}

			return indexClamped;
		}
	}
}