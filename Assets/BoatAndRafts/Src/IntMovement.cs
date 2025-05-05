using System;
using UnityEngine;

namespace BoatAndRafts.Src
{
	[ClassTooltip("Rounds own transform position to int on awake.")]
	public class IntMovement : MonoBehaviour
	{
		private Transform _transform;

		/// <summary>
		/// Invoked when an object moved to a new position. Passes new position.
		/// </summary>
		public event Action<Vector3> Moved;

		public Vector2Int Position => transform.position.FloorToInt2D();
		
		private void Awake()
		{
			_transform = transform;
			_transform.position = (Vector2)transform.position.FloorToInt2D();
		}

		public void Move(Direction2D direction)
		{
			var shift = direction.ToVector2Int();
			var position = _transform.position;
			position = (Vector2)(position.FloorToInt2D() + shift);
			_transform.position = position;
			Moved?.Invoke(position);
		}
		
	}
}