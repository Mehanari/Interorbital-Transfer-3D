using Src.Model;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Src.Visualisation
{
    public class TrajectoryRenderer : MonoBehaviour
    {
        [SerializeField] private float timeStep = 0.2f;
        [SerializeField] private Color lineColor;
        [SerializeField] private float lineWidth;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int trajectoryPoints;
        [SerializeField] private float kilometersPerUnit = 1000;

        private NativeArray<Vector3> _trajectoryPositions;
        private JobHandle _trajectoryJobHandle;
        private bool _jobInProgress = false;

        private SatelliteModel _model;

        public void SetModel(SatelliteModel model)
        {
            _model = model;
        }

        private void Start()
        {
            lineRenderer.startColor = lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void Update()
        {
            if (!_jobInProgress)
            {
                ScheduleTrajectoryCalculation();
            }

            if (_jobInProgress && _trajectoryJobHandle.IsCompleted)
            {
                CompleteTrajectoryJob();
            }
        }
        
        void ScheduleTrajectoryCalculation()
        {
            // Create native array that can be accessed from job thread
            _trajectoryPositions = new NativeArray<Vector3>(trajectoryPoints, Allocator.TempJob);
        
            // Clone required model data to pass to job
            Spacecraft initialState = _model.Spacecraft;
        
            // Create and schedule job
            TrajectoryCalculationJob job = new TrajectoryCalculationJob
            {
                gravitationalParameter = (float)_model.GravitationalParameter,
                earthPosition = _model.EarthPosition.ToVector3(),
                initialVelocity = initialState.Velocity.ToVector3(),
                initialPosition = initialState.Position.ToVector3(),
                deltaTime = timeStep,
                results = _trajectoryPositions
            };
        
            _trajectoryJobHandle = job.Schedule();
            _jobInProgress = true;
        }
        
        void CompleteTrajectoryJob()
        {
            // Wait for job completion (should already be done)
            _trajectoryJobHandle.Complete();
        
            // Update line renderer with calculated positions
            DrawTrajectory();
        
            // Clean up native array
            _trajectoryPositions.Dispose();
            _jobInProgress = false;
        }

        private void DrawTrajectory()
        {
            lineRenderer.positionCount = _trajectoryPositions.Length;
            for (int i = 0; i < _trajectoryPositions.Length; i++)
            {
                lineRenderer.SetPosition(i, _trajectoryPositions[i]/kilometersPerUnit);
            }
        }
        
        private void OnDestroy()
        {
            if (_jobInProgress)
            {
                CompleteTrajectoryJob();
            }
            // Make sure to dispose any allocated memory if component is destroyed
            if (_trajectoryPositions.IsCreated)
                _trajectoryPositions.Dispose();
        }
    }
}