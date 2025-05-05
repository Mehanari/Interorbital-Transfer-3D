using System;
using UnityEngine;

namespace BoatAndRafts.Src
{
	public static class Direction2DExtensions
	{
		public static Vector2Int ToVector2Int(this Direction2D direction)
		{
			return direction switch
			{
				Direction2D.Up => Vector2Int.up,
				Direction2D.Down => Vector2Int.down,
				Direction2D.Left => Vector2Int.left,
				Direction2D.Right => Vector2Int.right,
				_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
		}
	}
}