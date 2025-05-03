using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace RaftAndWhales
{
	[ClassTooltip("Treats every child transform as a point for making a route.\nRounds down the coordinates values to integers.\n" +
	              "If there is no points, creates a square route between (0, 0) and (1, 1) world coordinates.\n" +
	              "If there are two points, creates a polyline between them.\n" +
	              "The route will be build from the first child (top) to the last (bottom), so the order matters.")]
	public class RouteMaker2D : MonoBehaviour
	{
		[SerializeField] private Color routeColor;
		private Vector2Int[] _helperPoints = Array.Empty<Vector2Int>();

		private Vector2Int[] ZeroSquare => new[]
		{
			Vector2Int.zero,
			Vector2Int.right,
			Vector2Int.up + Vector2Int.right,
			Vector2Int.up,
			Vector2Int.zero
		};

		private void Awake()
		{
			UpdateHelperPoints();
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = routeColor;
			UpdateHelperPoints();
			var routePoints = GetRoutePoints();
			for (int i = 0; i < routePoints.Length - 1; i++)
			{
				var current = routePoints[i];
				var next = routePoints[i + 1];
				Gizmos.DrawLine((Vector2)current, (Vector2)next);
			}
		}

		/// <summary>
		/// Returns points in clockwise order.
		/// </summary>
		/// <returns></returns>
		public Vector2Int[] GetRoutePoints()
		{
			if (_helperPoints.Length == 0)
			{
				return ZeroSquare;
			}

			if (_helperPoints.Length == 1)
			{
				var pivot = _helperPoints[0];
				var square = ZeroSquare;
				square.AddToAll(pivot);
				return square;
			}

			if (_helperPoints.Length > 1)
			{
				var points = new List<Vector2Int>();
				for (int i = 0; i < _helperPoints.Length - 1; i++)
				{
					var current = _helperPoints[i];
					var next = _helperPoints[i + 1];
					points.Add(current);
					points.AddRange(GetPointsBetweenRect(current, next));
				}
				points.Add(_helperPoints[^1]);
				points.AddRange(GetPointsBetweenRect(_helperPoints[^1], _helperPoints[0]));
				points.Add(_helperPoints[0]);
				return points.ToArray();
			}
			return Array.Empty<Vector2Int>();
		}

		private List<Vector2Int> GetPointsBetweenRect(Vector2Int start, Vector2Int end)
		{
			var points = new List<Vector2Int>();

			var xStep = start.x < end.x ? 1 : -1;
			var yStep = start.y < end.y ? 1 : -1;
			var xDistance = Math.Abs(start.x - end.x);
			var yDistance = Math.Abs(start.y - end.y);
			var xStepsLeft = xDistance;
			var yStepsLeft = yDistance;
			var currentPoint = start;
			while (xStepsLeft > 0)
			{
				currentPoint.x+=xStep;
				xStepsLeft--;
				points.Add(currentPoint);
			}

			while (yStepsLeft > 0)
			{
				currentPoint.y+=yStep;
				yStepsLeft--;
				points.Add(currentPoint);
			}

			return points;
		}

		private List<Vector2Int> GetPointsBetweenDirected(Vector2Int start, Vector2Int end)
		{
			var points = new List<Vector2Int>();

			var xStep = start.x < end.x ? 1 : -1;
			var yStep = start.y < end.y ? 1 : -1;
			var xDistance = Math.Abs(start.x - end.x);
			var yDistance = Math.Abs(start.y - end.y);
			var xPerY = Mathf.FloorToInt(xDistance / (float)yDistance); //How many x steps should I make before making one y step? May be -int.MaxValue if yDistance is 0.
			var yPerX = Mathf.FloorToInt(yDistance / (float)xDistance); //How many y steps should I make before making one x step? May be -int.MaxValue if xDistance is 0.
			var xStepsLeft = xDistance;
			var yStepsLeft = yDistance;
			var currentPoint = start;
			var xPreviousMove = false;
			var yPreviousMove = false;
			while (xStepsLeft > 0 || yStepsLeft > 0)
			{
				var xStepsPassed = xDistance - xStepsLeft;
				var yStepsPassed = yDistance - yStepsLeft;
				if (yPerX > 0 || xDistance == 0 || (xDistance == yDistance))
				{
					if (xDistance != 0 &&
					    yStepsPassed > 0 && 
					    yStepsPassed % yPerX == 0 &&
					    xStepsLeft > 0 &&
					    !xPreviousMove)
					{
						currentPoint.x += xStep;
						xStepsLeft--;
						xPreviousMove = true;
					}
					else
					{
						currentPoint.y += yStep;
						yStepsLeft--;
						xPreviousMove = false;
					}
				}
				else if(xPerY > 0 || yDistance == 0)
				{
					if (yDistance != 0 &&
					    xStepsPassed > 0 &&
					    xStepsPassed % xPerY == 0 &&
					    yStepsLeft > 0 &&
					    !yPreviousMove)
					{
						currentPoint.y += yStep;
						yStepsLeft--;
						yPreviousMove = true;
					}
					else
					{
						currentPoint.x += xStep;
						xStepsLeft--;
						yPreviousMove = false;
					}
				}
				
				points.Add(currentPoint);
			}
			
			return points;
		}

		/// <summary>
		/// Gets children of this transform and puts their positions into helper points array.
		/// </summary>
		public void UpdateHelperPoints()
		{
			var points = new HashSet<Vector2Int>();
			for (int i = 0; i < transform.childCount; i++)
			{
				var point = ((Vector2)transform.GetChild(i).position).FloorToInt();
				points.Add(point);
			}

			_helperPoints = points.ToArray();
		}
	}
}
