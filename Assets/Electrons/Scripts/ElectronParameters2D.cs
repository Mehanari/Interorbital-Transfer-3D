using MehaMath.Math.Components;
using UnityEngine;

namespace Electrons.Scripts
{
	public class ElectronParameters2D : MonoBehaviour
	{
		private const double BASE_ANGULAR_VELOCITY = 5d;

		[SerializeField] private ElectronOrbitDrawer2D orbitDrawer;
		[SerializeField] private Vector3 centralBodyPosition;
		[SerializeField] private double leaveSpeed;

		private ElectronState _initialState;
		private bool _initialized = false;
		
		private void Awake()
		{
			InitializeState();
		}

		public GameObject GetGo()
		{
			return gameObject;
		}

		public ElectronOrbitDrawer2D GetOrbitDrawer()
		{
			return orbitDrawer;
		}
		
		public ElectronState GetInitialState()
		{
			InitializeState();
			return _initialState;
		}

		private void InitializeState()
		{
			if (!_initialized)
			{
				_initialState.Position = new Vector((Vector2)(transform.position - centralBodyPosition));
				_initialState.LeaveSpeed = leaveSpeed;
				_initialState.AngularVelocity = BASE_ANGULAR_VELOCITY / _initialState.Position.Magnitude();
				_initialized = true;
			}
		}
	}
}