using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircleTravelling.Src
{
	[RequireComponent(typeof(LineRenderer))]
	public class RouteDrawer : MonoBehaviour
	{
		[SerializeField] private TravellersCollection travellersCollection;
		[SerializeField] private GameObject salesman;
		[SerializeField] private Color lineColor;
		[SerializeField] private float lineWidth;
		private LineRenderer _lineRenderer;

		private void Awake()
		{
			_lineRenderer = GetComponent<LineRenderer>();
			_lineRenderer.startColor = _lineRenderer.endColor = lineColor;
			_lineRenderer.startWidth = _lineRenderer.endWidth = lineWidth;
			_lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		}

		private void Update()
		{
			DrawRoute();
		}

		private void DrawRoute()
		{
			var points = new List<Vector3>();
			var salesmanPosition = salesman.transform.position;
			points.Add(salesmanPosition);
			foreach (var traveller in travellersCollection.Travellers)
			{
				points.Add(traveller.GetRendezvousPosition());
			}

			_lineRenderer.positionCount = points.Count;
			_lineRenderer.SetPositions(points.ToArray());
		}
	}
}