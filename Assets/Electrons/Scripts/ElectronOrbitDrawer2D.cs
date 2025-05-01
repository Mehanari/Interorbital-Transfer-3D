using System;
using UnityEngine;

namespace Electrons.Scripts
{
	[RequireComponent(typeof(LineRenderer))]
    public class ElectronOrbitDrawer2D : MonoBehaviour
    {
	    [SerializeField] private int samplesCount;
	    [SerializeField] private Color lineColor;
	    [SerializeField] private float lineWidth;

	    private LineRenderer _lineRenderer;
	    private ElectronState _state;
	    
	    private void Awake()
	    {
		    _lineRenderer = GetComponent<LineRenderer>();
	    }

	    private void Start()
	    {
		    _lineRenderer.startColor = _lineRenderer.endColor = lineColor;
		    _lineRenderer.startWidth = _lineRenderer.endWidth = lineWidth;
		    _lineRenderer.positionCount = samplesCount;
		    _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		    
	    }
	    

	    public void DrawOrbit(ElectronState state)
	    {
		    var radius = state.Position.Magnitude();
		    var points = GeneratePoints(radius);
		    _lineRenderer.SetPositions(points);
	    }

	    private Vector3[] GeneratePoints(double radius)
	    {
		    var result = new Vector3[samplesCount];
		    var step = Mathf.PI * 2 / (samplesCount-1);
		    for (int i = 0; i < samplesCount; i++)
		    {
			    var angle = step * i;
			    var x = Mathf.Cos(angle);
			    var y = Mathf.Sin(angle);
			    result[i] = new Vector2(x, y) * (float)radius;
		    }

		    return result;
	    }
    }
}
