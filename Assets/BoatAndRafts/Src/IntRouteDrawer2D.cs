using BoatAndRafts.Src.RouteMaking;
using UnityEngine;

namespace BoatAndRafts.Src
{
    [RequireComponent(typeof(IntRoute2D))]
    [RequireComponent(typeof(LineRenderer))]
    public class IntRouteDrawer2D : MonoBehaviour
    {
        [SerializeField] private Color color;
        [SerializeField] private float lineWidth;
        [SerializeField] private bool drawOnValidate;
        [SerializeField] private bool redrawOnUpdate;
        
        private LineRenderer _lineRenderer;
        private IntRoute2D _route;

        private void Awake()
        {
            Initialize();
            DrawRoute();
        }

        private void Initialize()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _route = GetComponent<IntRoute2D>();
            _lineRenderer.startColor = _lineRenderer.endColor = color;
            _lineRenderer.startWidth = _lineRenderer.endWidth = lineWidth;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void Update()
        {
            if (redrawOnUpdate)
            {
                DrawRoute();
            }
        }

        private void OnValidate()
        {
            if (drawOnValidate)
            {
                Initialize();
                DrawRoute();
            }
        }

        public void DrawRoute()
        {
            if (_route is null || _lineRenderer is null)
            {
                Debug.LogWarning("Cannot draw route, route or line renderer component are null.");
                return;
            }
            var points = _route.GetRoutePoints();
            _lineRenderer.positionCount = points.Length + 1;
            _lineRenderer.SetPositions(points.ToVector3());
            _lineRenderer.SetPosition(points.Length, (Vector2)points[0]);
        }
    }
}
