using UnityEngine;

namespace RaftAndWhales
{
	public static class Vector2Extensions
	{
		public static Vector2Int FloorToInt(this Vector2 vector)
		{
			return new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
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