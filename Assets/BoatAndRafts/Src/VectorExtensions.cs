using UnityEngine;

namespace BoatAndRafts.Src
{
	public static class VectorExtensions
	{
		public static Vector2Int FloorToInt2D(this Vector3 vector)
		{
			return new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
		}
		
		public static Vector2Int FloorToInt(this Vector2 vector)
		{
			return new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
		}

		public static Vector3[] ToVector3(this Vector2Int[] array)
		{
			var vector3Array = new Vector3[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				vector3Array[i] = (Vector2)array[i];
			}

			return vector3Array;
		}

		/// <summary>
		/// Adds the addition to all vectors in the array.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="addition"></param>
		public static void AddToAll(this Vector2Int[] array, Vector2Int addition)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] += addition;
			}
		}
	}
}