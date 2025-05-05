using UnityEngine;

namespace BoatAndRafts.Src
{
	[ClassTooltip("Draws the travelled path of a given IntMovement. Adds new position to a line every move.")]
	[RequireComponent(typeof(LineRenderer))]
	public class IntMovementDrawer : MonoBehaviour
	{
		[SerializeField] private Color lineColor;
		[SerializeField] private float lineWidth;
		[SerializeField] private IntMovement movement;

		private LineRenderer _lineRenderer;
		
		private void Awake()
		{
			_lineRenderer = GetComponent<LineRenderer>();
			_lineRenderer.startColor = _lineRenderer.endColor = lineColor;
			_lineRenderer.startWidth = _lineRenderer.endWidth = lineWidth;
			_lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			_lineRenderer.positionCount = 1;
			_lineRenderer.SetPosition(0, (Vector2)movement.Position);
			movement.Moved += OnMoved;
		}

		private void OnDestroy()
		{
			movement.Moved -= OnMoved;
		}

		private void OnMoved(Vector3 newPosition)
		{
			var positionCount = _lineRenderer.positionCount;
			positionCount++;
			_lineRenderer.positionCount = positionCount;
			_lineRenderer.SetPosition(positionCount-1, newPosition);
		}
	}
}