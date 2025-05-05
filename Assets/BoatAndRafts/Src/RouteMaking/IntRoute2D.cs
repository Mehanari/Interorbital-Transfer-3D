using UnityEngine;

namespace BoatAndRafts.Src.RouteMaking
{
	public abstract class IntRoute2D : MonoBehaviour
	{
		public abstract Vector2Int[] GetRoutePoints();
	}
}