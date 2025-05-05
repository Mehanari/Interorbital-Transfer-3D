using BoatAndRafts.Src.RouteMaking;
using UnityEngine;

namespace BoatAndRafts.Src
{
	/// <summary>
	/// Gets points from given route in Awake or OnValidate method and caches them.
	/// Use UpdateRoutePoints to do what the method name suggests if you modified the route in runtime.
	/// </summary>
	public class IntRouteMovement2D : MonoBehaviour
	{
		[SerializeField] private int startPointIndex = 0;
		[SerializeField] private IntRoute2D route;

		private Vector2Int[] _routePoints;
		private int _currentPointIndex;

		private void Awake()
		{
			UpdateRoutePoints();
			SetToStart();
		}

		public void UpdateRoutePoints()
		{
			_routePoints = route.GetRoutePoints();
		}

		private void SetToStart()
		{
			var startIndexClamped = _routePoints.LoopIndex(startPointIndex);
			var point = _routePoints[startIndexClamped];
			
			_currentPointIndex = startIndexClamped;
			transform.position = (Vector2)point;
		}

		private void OnValidate()
		{
			if (route is null)
			{
				return;
			}
			UpdateRoutePoints();
			SetToStart();
		}

		public void Move(bool backward = false)
		{
			var nextPointIndex = _routePoints.LoopIndex(_currentPointIndex + 1);
			if (backward)
			{
				nextPointIndex = _routePoints.LoopIndex(_currentPointIndex - 1);
			}
			var nextPoint = _routePoints[nextPointIndex];
			_currentPointIndex = nextPointIndex;
			transform.position = (Vector2)nextPoint;
		}
	}
}