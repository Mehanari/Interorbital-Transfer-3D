using UnityEngine;

namespace CircleTravelling.Src
{
	[RequireComponent(typeof(LineRenderer))]
	[RequireComponent(typeof(Traveller))]
	public class TravellerView : MonoBehaviour
	{
		[SerializeField] private int circlePointsSamples = 2;
		[SerializeField] private float lineWidth;
		[SerializeField] private Color lineColor;
        [SerializeField] private GameObject initialPositionMark;
        [SerializeField] private GameObject rendezvousMark;
        [SerializeField] private bool redrawOnValidate;

        private LineRenderer _lineRenderer;
        private Traveller _traveller;

        private void Awake()
        {
	        InitAndUpdateAll();
        }

        private void Initialize()
        {
	        _lineRenderer = GetComponent<LineRenderer>();
	        _traveller = GetComponent<Traveller>();
	        _lineRenderer.startColor = _lineRenderer.endColor = lineColor;
	        _lineRenderer.startWidth = _lineRenderer.endWidth = lineWidth;
	        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        [ContextMenu("InitAndUpdateAll")]
        private void InitAndUpdateAll()
        {
	        Initialize();
	        UpdateAll();
        }

        private void OnValidate()
        {
	        if (redrawOnValidate)
	        {
		        InitAndUpdateAll();
	        }
        }

        private void Update()
        {
	        SetInitialPositionMark();
	        SetRendezvousMark();
        }

        private void UpdateAll()
        {
	        DrawCircle();
	        SetInitialPositionMark();
			SetRendezvousMark();
        }

        private void DrawCircle()
        {
	        var circlePoints = SampleCirclePoints();
	        _lineRenderer.positionCount = circlePoints.Length;
	        _lineRenderer.SetPositions(circlePoints);
        }

        private void SetInitialPositionMark()
        {
	        if(initialPositionMark is  null) return;
	        var initialAngle = _traveller.InitialAngle;
	        var radius = _traveller.Radius;
	        initialPositionMark.transform.position =
		        new Vector3(Mathf.Cos(initialAngle) * radius, Mathf.Sin(initialAngle) * radius, 0) + _traveller.Center;
        }

        private void SetRendezvousMark()
        {
	        if(rendezvousMark is null) return;
	        var initialAngle = _traveller.InitialAngle;
	        var radius = _traveller.Radius;
	        var rendezvousAngle = _traveller.InitialAngle + _traveller.AngularVelocity * _traveller.RendezvousTime;
	        rendezvousMark.transform.position =
		        new Vector3(Mathf.Cos(rendezvousAngle) * radius, Mathf.Sin(rendezvousAngle) * radius, 0) +
		        _traveller.Center;
        }

        private Vector3[] SampleCirclePoints()
        {
	        var center = _traveller.Center;
	        var radius = _traveller.Radius;
	        if (circlePointsSamples < 2)
	        {
		        return new[] { new Vector3(center.x + radius, center.y, 0) };
	        }
	        var step = Mathf.PI * 2 / (circlePointsSamples - 1);
	        var points = new Vector3[circlePointsSamples];
	        for (int i = 0; i < circlePointsSamples; i++)
	        {
		        var angle = step * i;
		        points[i] = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0) + _traveller.Center;
	        }

	        return points;
        }
	}
}