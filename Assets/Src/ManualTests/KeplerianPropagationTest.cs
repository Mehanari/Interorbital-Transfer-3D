using MehaMath.Math.Components;
using Src.Helpers;
using Src.Model;
using Src.OptimizationFramework;
using Src.Visualisation;
using UnityEngine;
using UnityEngine.UI;

namespace Src.ManualTests
{
	public class KeplerianPropagationTest : MonoBehaviour
	{
		private const double KILOMETERS_PER_UNIT = 1000;
		
		[SerializeField] private OrbitDrawer orbitDrawer;
		[SerializeField] private GameObject ship;
		[SerializeField] private Arrow3D velocityArrow;
		[SerializeField] private Slider timeSlider;

		private float _initialTime = 0f;
		private float _finalTime = 10000f;

		private double _mu = 398600.4418d;
		//Ship initial state
		private Vector R1 = new Vector(8000.0, 1000.0, 2000.0); //Initial position of the ship
		private Vector V1 = new Vector(-1.5, 7.0, 2.5); //Initial velocity of the ship

		private KeplerianPropagation _propagation;
		private Orbit _orbit;

		private void Start()
		{
			V1 *= -1;
			
			_propagation = new KeplerianPropagation()
			{
				GravitationalParameter = _mu,
				CentralBodyPosition = new Vector(0, 0, 0)
			};
			
			
			_orbit = OrbitHelper.GetOrbit(V1, R1, _mu);
			var shipScenePos = (R1 / KILOMETERS_PER_UNIT).ToVector3();
			ship.transform.position = shipScenePos;
			velocityArrow.transform.position = shipScenePos;
			velocityArrow.SetDirection(V1.ToVector3());
			
			orbitDrawer.DrawOrbit(_orbit, Vector3.zero, 1000, new OrbitLineParameters
			{
				LineColor = Color.yellow,
				LineWidth = 0.01f,
				Name = "Orbit"
			});
			
			timeSlider.minValue = _initialTime;
			timeSlider.maxValue = _finalTime;
			timeSlider.onValueChanged.AddListener(OnTimeChanged);
		}

		private void OnTimeChanged(float time)
		{
			var futureState = _propagation.PropagateState(_orbit, time);
			var (pos, vel) = OrbitHelper.GetPositionAndVelocity(futureState, _mu);
			var scenePos = (pos / KILOMETERS_PER_UNIT).ToVector3();
			ship.transform.position = scenePos;
			velocityArrow.transform.position = scenePos;
			velocityArrow.SetDirection(vel.ToVector3());
		}
	}
}