using System.Collections.Generic;
using UnityEngine;

namespace BoatAndRafts.Src.RouteMaking
{
	[ClassTooltip("Treats its own position, rounded to Vector2Int, as a corner of a rectangular route.\n" +
	              "Gives one-point route if width and height are zeros.")]
	public class RectIntRoute2D : IntRoute2D
	{
		[SerializeField] private int width;
		[SerializeField] private int height;
		
		/// <summary>
		/// Returns points in a clockwise order.
		/// Always starts from a lower left corner.
		/// </summary>
		/// <returns></returns>
		public override Vector2Int[] GetRoutePoints()
		{
			var pivot = transform.position.FloorToInt2D();
			var upperLeft = Vector2Int.zero;
			var upperRight = Vector2Int.zero;
			var lowerLeft = Vector2Int.zero;
			var lowerRight = Vector2Int.zero;
			
			if (height >= 0 && width > 0)
			{
				lowerLeft = pivot;
				lowerRight = pivot + Vector2Int.right * width;
				upperLeft = pivot + Vector2Int.up * height;
				upperRight = upperLeft + Vector2Int.right * width;
			}
			else if (height < 0 && width >= 0)
			{
				upperLeft = pivot;
				upperRight = pivot + Vector2Int.right * width;
				lowerLeft = pivot + Vector2Int.down * Mathf.Abs(height);
				lowerRight = lowerLeft + Vector2Int.right * width;
			}
			else if (height >= 0 && width <= 0)
			{
				lowerRight = pivot;
				lowerLeft = pivot + Vector2Int.left * Mathf.Abs(width);
				upperLeft = lowerLeft + Vector2Int.up * height;
				upperRight = lowerRight + Vector2Int.up * height;
			}
			else
			{
				upperRight = pivot;
				upperLeft = upperRight + Vector2Int.left * Mathf.Abs(width);
				lowerRight = upperRight + Vector2Int.down * Mathf.Abs(height);
				lowerLeft = upperLeft + Vector2Int.down * Mathf.Abs(height);
			}

			var points = new List<Vector2Int> { lowerLeft };
			for (int y = lowerLeft.y + 1; y <= upperLeft.y; y++)
			{
				points.Add(new Vector2Int(lowerLeft.x, y));
			}
			for (int x = upperLeft.x + 1; x <= upperRight.x; x++)
			{
				points.Add(new Vector2Int(x, upperLeft.y));
			}
			for (int y = upperRight.y - 1; y >= lowerRight.y; y--)
			{
				points.Add(new Vector2Int(upperRight.x, y));
			}
			for (int x = lowerRight.x - 1; x > lowerLeft.x; x--)
			{
				points.Add(new Vector2Int(x, lowerRight.y));
			}
			return points.ToArray();
		}
		
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			var routePoints = GetRoutePoints();
			for (int i = 0; i < routePoints.Length - 1; i++)
			{
				var current = routePoints[i];
				var next = routePoints[i + 1];
				Gizmos.DrawLine((Vector2)current, (Vector2)next);
			}

			if (routePoints.Length > 0)
			{
				Gizmos.DrawLine((Vector2)routePoints[^1], (Vector2)routePoints[0]);
			}
		}
	}
}